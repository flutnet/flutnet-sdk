// *************************************
//         NOT EDIT THIS FILE          *
// *************************************
import 'dart:async';
import 'dart:convert';
import 'dart:developer';
import 'package:flutter/services.dart';
import 'package:meta/meta.dart';

import 'package:flutter/widgets.dart';
import 'package:synchronized/synchronized.dart';
import 'package:web_socket_channel/io.dart';
import 'package:web_socket_channel/status.dart' as status;
import 'package:web_socket_channel/web_socket_channel.dart';

///
/// The bridge communication type with native.
///
enum BridgeMode {
  Channel,
  Socket,
}

///
/// The configuration used by the [Bridge].
/// Setup this before running the flutter application.
/// [main.dart] --> void main()
///
class BridgeConfig {
  static BridgeMode mode = BridgeMode.Channel;
}

class Bridge {
  // Events from native side (Xamarin)
  static const EventChannel _events =
      EventChannel('flutterbridge.appchannel.outgoing');

  // The real event stream from navive side
  static final Stream<_FlutterEvent> _channelEvent =
      _events.receiveBroadcastStream().map(_mapEvent);

  // The event stream exposed to all the services
  final Stream<_FlutterEvent>
      _netEvent; // = _events.receiveBroadcastStream().map(_mapEvent);

  //
  // Filter the bridge event stream
  // using a specific instanceId, event
  //
  Stream<Map> events({
    String instanceId,
    String event,
  }) {
    // Filter the stream by instanceId and event name.
    return _netEvent
        .where((e) => e.instanceId == instanceId && e.event == event)
        .map((e) => e.args);
  }

  static final Bridge _instance = Bridge._internal(BridgeConfig.mode);

  Bridge._internal(BridgeMode mode)
      : invokeMethod = (buildMode == _BuildMode.release)
            ? _invokeOnChannel
            : (mode == BridgeMode.Socket) ? _invokeOnSocket : _invokeOnChannel,
        _netEvent = (buildMode == _BuildMode.release)
            ? _channelEvent
            : (mode == BridgeMode.Socket)
                ? _DebugChannel().events
                : _channelEvent;

  factory Bridge() => _instance;

  ///
  /// Invoke the message on the native channel
  ///
  final Future<Map<String, dynamic>> Function({
    @required String instanceId,
    @required String service,
    @required String operation,
    @required Map<String, dynamic> arguments,
  }) invokeMethod;

  static Future<Map<String, dynamic>> _invokeOnChannel({
    @required String instanceId,
    @required String service,
    @required String operation,
    @required Map<String, dynamic> arguments,
  }) {
    print(
      "Invoking on platform channel $operation on $service :$instanceId: build mode:$buildMode",
    );
    return _PlatformChannel().invokeMethod(
      instanceId: instanceId,
      service: service,
      operation: operation,
      arguments: arguments,
    );
  }

  static Future<Map<String, dynamic>> _invokeOnSocket({
    @required String instanceId,
    @required String service,
    @required String operation,
    @required Map<String, dynamic> arguments,
  }) {
    print(
      "Invoking on socket $operation on $service :$instanceId: build mode:$buildMode",
    );
    return _DebugChannel().invokeMethod(
      instanceId: instanceId,
      service: service,
      operation: operation,
      arguments: arguments,
    );
  }

  ///
  /// Decoding events function
  ///
  static _FlutterEvent _mapEvent(dynamic event) {
    try {
      Map json = jsonDecode(event as String);
      return _FlutterEvent.fromJson(json);
    } on Exception catch (ex) {
      print("Error decoding event: $ex");
      return null;
    }
  }

  @mustCallSuper
  void dispose() {
    // Release debug socket resources
    if (invokeMethod == _invokeOnSocket) {
      _DebugChannel().dispose();
    }
  }
}

class _FlutterEvent {
  final String instanceId; // The instance id that have the event
  final String event; // The reference for the event
  final Map args; // The data sended throw the event

  _FlutterEvent({
    @required this.instanceId,
    @required this.event,
    this.args,
  });

  Map<String, dynamic> toJson() {
    return {
      'instanceId': instanceId,
      'event': event,
      'args': args,
    };
  }

  static _FlutterEvent fromJson(Map json) {
    if (json == null) return null;

    return _FlutterEvent(
      instanceId: json['instanceId'] as String,
      event: json['event'] as String,
      args: json['args'] as Map,
    );
  }
}

class _PlatformChannel {
  static _PlatformChannel _instance = _PlatformChannel._internal();

  factory _PlatformChannel() => _instance;

  // Send request id
  int _uniqueId = 0;

  Lock _sendLock = new Lock();

  ///
  /// All the request to be satisfied by debug WEB SOCKET.
  ///
  Map<int, Completer<dynamic>> _sendRequestMap = {};

  ///
  /// The url user for the connection
  ///
  static const String _channelName = 'flutterbridge.appchannel.incoming';

  // The real communication channel with native platform
  final _platformChannel = MethodChannel(_channelName);

  _PlatformChannel._internal() {
    _platformChannel.setMethodCallHandler(_onMessageReceived);
  }

  ///
  /// Rilascia le risorse.
  ///
  _releaseMemory() {
    try {
      _sendRequestMap?.clear();
    } catch (ex) {}
  }

  static const _emptyString = "";

  ///
  /// How manage data reception from websocket.
  ///
  Future<dynamic> _onMessageReceived(MethodCall call) async {
    // Manage message received
    try {
      String jsonMessage = call.arguments as String;

      // Json decoding
      Map<String, dynamic> json = jsonDecode(jsonMessage);
      _FlutterDebuggerMessage msg = _FlutterDebuggerMessage.fromJson(json);

      // Insert the response the the map
      await _sendLock.synchronized(() {
        if (_sendRequestMap.containsKey(msg.methodInfo.requestId)) {
          // Invoke the task completion
          Completer<Map<String, dynamic>> request =
              _sendRequestMap[msg.methodInfo.requestId];

          bool isFailed = msg.errorCode != null && msg.errorCode.isNotEmpty;
          if (isFailed) {
            //* Handle invoke error
            String errorMessage = "${msg.errorCode}, ${msg.errorMessage ?? ''}";
            request.completeError(Exception(errorMessage));
          } else {
            //* handle invoke complete
            request.complete(msg.result);
          }

          _sendRequestMap.remove(msg.methodInfo.requestId);
        }
      });
    } catch (e) {
      // Error during deserialization
      print(
        "flutter_xamarin_debug: error during _onMessageReceived deserialization.",
      );
    }

    return _emptyString;
  }

  ///
  /// Each call invoke a debug request in a local xamarin form application.
  /// The contropart appliction execute the .NET core code:
  /// the code is the same present in the plugin.dll compiled with embeddinator
  /// and configured in the native platform flutter plugin.
  /// See [FlutterXamarin]
  ///
  Future<Map<String, dynamic>> invokeMethod({
    @required String instanceId,
    @required String service,
    @required String operation,
    @required Map<String, dynamic> arguments,
  }) {
    final Completer<Map<String, dynamic>> completer =
        new Completer<Map<String, dynamic>>();

    _sendLock.synchronized(
      () async {
        int sendRequestId = ++_uniqueId;

        try {
          final _FlutterMethodInfo methodInfo = _FlutterMethodInfo(
            requestId: sendRequestId,
            instance: instanceId,
            service: service,
            operation: operation,
          );

          // Save the request
          _sendRequestMap.putIfAbsent(
            methodInfo.requestId,
            () => completer,
          );

          // Seriliaze all the method info as Json String
          final String jsonMethodInfo = jsonEncode(methodInfo);

          // Serialize all the args as Json string
          final Map<String, String> args = arguments
              .map((argName, value) => MapEntry(argName, jsonEncode(value)));

          // Send to platform channel
          await _platformChannel.invokeMethod(
            jsonMethodInfo,
            args,
          );
        } catch (ex) {
          debugPrint("Error during invokeMethod on platform channel");

          _sendRequestMap.remove(sendRequestId);

          //
          // Error during send
          //
          completer.completeError(ex);
        }
      },
    );

    //*
    //* Questa richiesta rimane in attesa fino a che non viene soddisfatta.
    //*
    return completer.future;
  }

  @mustCallSuper
  void dispose() {
    _sendLock.synchronized(() {
      _releaseMemory();
    });
  }
}

class _FlutterMethodInfo {
  final int requestId;
  final String instance;
  final String service;
  final String operation;

  _FlutterMethodInfo({
    @required this.requestId,
    @required this.instance,
    @required this.service,
    @required this.operation,
  });

  Map<String, dynamic> toJson() {
    return {
      'requestId': requestId,
      'instance': instance,
      'service': service,
      'operation': operation,
    };
  }

  static _FlutterMethodInfo fromJson(Map json) {
    if (json == null) return null;

    return _FlutterMethodInfo(
      requestId: json['requestId'] as int,
      instance: json['instance'] as String,
      service: json['service'] as String,
      operation: json['operation'] as String,
    );
  }
}

class _FlutterDebuggerMessage {
  final _FlutterMethodInfo methodInfo;
  final Map<String, dynamic> arguments;
  final Map<String, dynamic> result;
  final String errorCode;
  final String errorMessage;
  final Map event;

  _FlutterDebuggerMessage({
    this.methodInfo,
    this.arguments,
    this.result,
    this.errorCode,
    this.errorMessage,
    this.event,
  });

  Map<String, dynamic> toJson() {
    return {
      'methodInfo': methodInfo?.toJson() ?? null,
      'arguments': arguments,
      'result': result,
      'errorCode': errorCode,
      'errorMessage': errorMessage,
      'event': event,
    };
  }

  static _FlutterDebuggerMessage fromJson(Map json) {
    if (json == null) return null;

    return _FlutterDebuggerMessage(
      methodInfo: json.containsKey('methodInfo')
          ? _FlutterMethodInfo.fromJson(json['methodInfo'] as Map)
          : null,
      arguments: json['arguments'],
      result: json['result'],
      errorCode: json['errorCode'],
      errorMessage: json['errorMessage'],
      event: json['event'],
    );
  }
}

class _DebugChannel {
  static _DebugChannel _instance;

  final StreamController<_FlutterEvent> _eventsController;
  final Stream<_FlutterEvent> _eventsOut;
  final Sink<_FlutterEvent> _eventsIn;

  Stream<_FlutterEvent> get events => _eventsOut;

  _DebugChannel._internal(
      this._eventsController, this._eventsIn, this._eventsOut) {
    _sendLock.synchronized(() async {
      // Wait until the connection open
      while (_socketChannelConnected == false) {
        try {
          await _autoConnect(
            delay: const Duration(seconds: 1),
            forceOpen: true,
          );
        } catch (ex) {
          // Error during connection opening
          debugPrint(ex);
        }
      }
    });
  }

  factory _DebugChannel() {
    if (_instance == null) {
      StreamController<_FlutterEvent> controller =
          StreamController<_FlutterEvent>();
      Stream<_FlutterEvent> outEvent = controller.stream.asBroadcastStream();
      _instance =
          _DebugChannel._internal(controller, controller.sink, outEvent);
    }
    return _instance;
  }

  // Dispose state
  bool _disposed = false;

  ///
  /// The url user for the connection
  ///
  String _url = "ws://127.0.0.1:12345/flutter";

  //
  // Channel used to invoke methods from Flutter to web socket native backend application.
  //
  WebSocketChannel _socketChannel;

  // Status of the debug connection
  bool _socketChannelConnected = false;

  // Send request id
  int _uniqueId = 0;

  Lock _sendLock = new Lock();

  ///
  /// All the request to be satisfied by debug WEB SOCKET.
  ///
  Map<int, Completer<dynamic>> _sendRequestMap = {};

  ///
  /// All message sended to debug server
  /// that wait a respose
  ///
  Map<int, String> _outboxMessages = {};

  ///
  /// Apre la connessione in base al numero di messaggi da inviare
  /// e lo stato della stessa. Se la connessione è chiusa,
  /// tutti i messaggi in attesa di risposta vengono rispediti.
  ///
  Future<void> _autoConnect({Duration delay, bool forceOpen = false}) async {
    // * Se disposed, allora libero la memoria
    if (_disposed) {
      await _closeConnection();
      _socketChannelConnected = false;
      await _releaseMemory();
    }
    // * Se non ho messaggi da spedire e la connessione è aperta la chiudo
    else if (_outboxMessages.length <= 0 &&
        _socketChannelConnected == true &&
        forceOpen == false) {
      await _closeConnection();
      _socketChannelConnected = false;
      await _releaseMemory();
    }
    // * Reopen the connection
    else if ((_outboxMessages.length > 0 && _socketChannelConnected == false) ||
        (forceOpen == true && _socketChannelConnected == false)) {
      // Aspetto un po prima di collegarmi
      if (delay != null) {
        await Future.delayed(delay);
      }

      //* --------------------------------------------------------------
      //* IOWebSocketChannel.connect("ws://127.0.0.1:12345/flutter");
      //* OPEN THE CONNECCTION
      //* --------------------------------------------------------------
      _socketChannel = IOWebSocketChannel.connect(this._url);

      _socketChannel.stream.listen(
        _onMessageReceived,
        cancelOnError: false,
        onDone: _onConnectionClosed,
        //! in caso di erroe sull'apertura viene emesso l'evento qui
        onError: _onConnectionError,
      );

      //*
      //* NOTA IMPORTANTE: la connessione non fallisce mai all'apertura.
      //* Solo dopo un certo tempo viene invocato l'evento di errore,
      //* quindi suppongo sia aperta fin tanto che non ricevo l'errore esplicito.
      //*
      _socketChannelConnected = true;

      // Se sono connesso provo ad inviare i messaggi
      if (_socketChannelConnected) {
        try {
          //* Try to resend all the append messages (IN SORT ORDER)
          List<int> sortedRequests = _outboxMessages.keys.toList()..sort();

          sortedRequests.forEach((reqId) {
            String msg = _outboxMessages[reqId];

            _socketChannel.sink.add(msg);
          });
        } catch (ex) {
          debugPrint("Error sending messages");
          _socketChannelConnected = false;
          _closeConnection();
        }
      } else {
        //! Errore di connessione dopo N tentativi
        throw Exception("Error opening channel!");
      }
    }
  }

  Future _closeConnection() async {
    try {
      await _socketChannel?.sink?.close(status.normalClosure);
    } catch (ex) {}
  }

  ///
  /// Rilascia le risorse.
  ///
  _releaseMemory() async {
    // Try to resend all the append messages (IN SORT ORDER)
    List<int> sortedRequests = _sendRequestMap.keys.toList()..sort();

    sortedRequests.forEach((reqId) {
      _sendRequestMap[reqId].completeError(
        Exception("Connection closed by client."),
      );
    });

    try {
      _sendRequestMap?.clear();
    } catch (ex) {}

    try {
      _outboxMessages?.clear();
    } catch (ex) {}

    _eventsController.close();
  }

  ///
  /// Evento di chiusura della connessione.
  ///
  Future _onConnectionClosed() async {
    print("Connection closed.");
    await _sendLock.synchronized(() async {
      _socketChannelConnected = false;

      //* ----------------------------------------------------------------
      //* Wait until the connection open (IF THIS OBJECT IS NOT DISPOSED)
      //* ----------------------------------------------------------------
      while (_socketChannelConnected == false && _disposed == false) {
        try {
          print("Restoring the connection....");
          await _autoConnect(
            delay: const Duration(seconds: 1),
            forceOpen: true,
          );
        } catch (ex) {
          // Error during connection opening
          debugPrint(ex);
        }
      }

      ///! Se ho ancora messaggi da spedire allora è stato un problema di rete la chiusura
      //f (_outboxMessages.length > 0) {
      // try {
      //   await _autoConnect(delay: const Duration(seconds: 1));
      // } catch (ex) {
      //   // Errore apertura connessione
      // }
      //
    });
  }

  ///
  /// Eveto di errore della connessione.
  ///
  Future _onConnectionError(dynamic error, dynamic stacktrace) async {
    print("Connection error: closing the connection.");
    await _sendLock.synchronized(() {
      _socketChannelConnected = false;
      try {
        if (error is WebSocketChannelException) {
          log(error.message);
        } else {
          log(error.toString());
        }
        //if (_onError != null) {
        //  _onError(error.toString());
        //}
      } catch (ex) {}
    });
  }

  //void onClose() => null;

  //void _onMessage(dynamic message) => null;

  //void _onError(Object error) => null;

  ///
  /// How manage data reception from websocket.
  ///
  void _onMessageReceived(dynamic jsonMessage) async {
    if (jsonMessage is String) {
      // Manage message received
      try {
        // Json decoding
        Map<String, dynamic> json = jsonDecode(jsonMessage);
        _FlutterDebuggerMessage msg = _FlutterDebuggerMessage.fromJson(json);

        // Handlig for event
        if (msg.event != null) {
          _eventsIn.add(_FlutterEvent.fromJson(msg.event));
        }

        // Deserialize the real application message
        //FNetMessage result = FNetSerializer.deserialize(msg.fnetMessage);

        // Insert the response the the map
        await _sendLock.synchronized(() {
          if (_outboxMessages.containsKey(msg.methodInfo.requestId)) {
            _outboxMessages.remove(msg.methodInfo.requestId);
          }

          if (_sendRequestMap.containsKey(msg.methodInfo.requestId)) {
            // Invoke the task completion
            Completer<Map<String, dynamic>> request =
                _sendRequestMap[msg.methodInfo.requestId];

            bool isFailed = msg.errorCode != null && msg.errorCode.isNotEmpty;
            if (isFailed) {
              //* Handle invoke error
              String errorMessage =
                  "${msg.errorCode}, ${msg.errorMessage ?? ''}";
              request.completeError(Exception(errorMessage));
            } else {
              //* handle invoke complete
              request.complete(msg.result);
            }

            _sendRequestMap.remove(msg.methodInfo.requestId);
          }
        });
      } catch (e) {
        // Error during deserialization
        print(
          "flutter_xamarin_debug: error during _onMessageReceived deserialization.",
        );
      }
    } else {
      // Message not managed: protocol debug error.
    }
  }

  ///
  /// Each call invoke a debug request in a local xamarin form application.
  /// The contropart appliction execute the .NET core code:
  /// the code is the same present in the plugin.dll compiled with embeddinator
  /// and configured in the native platform flutter plugin.
  /// See [FlutterXamarin]
  ///
  Future<Map<String, dynamic>> invokeMethod({
    @required String instanceId,
    @required String service,
    @required String operation,
    @required Map<String, dynamic> arguments,
  }) {
    final Completer<Map<String, dynamic>> completer =
        new Completer<Map<String, dynamic>>();

    _sendLock.synchronized(
      () async {
        int sendRequestId = ++_uniqueId;

        try {
          final _FlutterMethodInfo methodInfo = _FlutterMethodInfo(
              requestId: sendRequestId,
              instance: instanceId,
              service: service,
              operation: operation);

          final _FlutterDebuggerMessage debugMessage = _FlutterDebuggerMessage(
            methodInfo: methodInfo,
            arguments: arguments,
          );

          // Encode the message
          final String jsonDegubMessage = jsonEncode(debugMessage);

          // Save the request
          _sendRequestMap.putIfAbsent(
            methodInfo.requestId,
            () => completer,
          );
          _outboxMessages.putIfAbsent(
            methodInfo.requestId,
            () => jsonDegubMessage,
          );

          // Wait until the connection open
          while (_socketChannelConnected == false) {
            try {
              await _autoConnect(
                delay: const Duration(seconds: 1),
                forceOpen: true,
              );
            } catch (ex) {
              // Error during connection opening
              debugPrint(ex);
            }
          }

          // Se la connessione è aperta invio i dati via rete.
          _socketChannel.sink.add(jsonDegubMessage);
        } catch (ex) {
          if (ex is WebSocketChannelException) {}
          debugPrint("Error during invokeMethod on debug channel");
          //
          // Error during send
          //
          //completer.completeError(ex);
        }
      },
    );

    //*
    //* Questa richiesta rimane in attesa fino a che non viene soddisfatta.
    //*
    return completer.future;
  }

  @mustCallSuper
  void dispose() {
    _sendLock.synchronized(() {
      _disposed = true;
      _closeConnection();
      _socketChannelConnected = false;
      _releaseMemory();
    });
  }
}

enum _BuildMode {
  release,
  debug,
  profile,
}

_BuildMode buildMode = (() {
  if (const bool.fromEnvironment('dart.vm.product')) {
    return _BuildMode.release;
  }
  var result = _BuildMode.profile;
  assert(() {
    result = _BuildMode.debug;
    return true;
  }());
  return result;
}());
