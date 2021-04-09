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
  // Channel to invoke methods on native side (Xamarin)
  static const MethodChannel _channel =
      MethodChannel('flutterbridge.appchannel.incoming');

  // Events from native side (Xamarin)
  static const EventChannel _events =
      EventChannel('flutterbridge.appchannel.outgoing');

  // The real event stream from navive side
  final Stream<_FNetEvent> _netEvent =
      _events.receiveBroadcastStream().map(_mapEvent);

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
        .map((e) => e.data);
  }

  static final Bridge _instance = Bridge._internal(BridgeConfig.mode);

  Bridge._internal(BridgeMode mode)
      : invokeMethod = (buildMode == _BuildMode.release)
            ? _invokeOnChannel
            : (mode == BridgeMode.Socket) ? _invokeOnSocket : _invokeOnChannel;

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
      "Invoking on channel $operation on $service :$instanceId: build mode:$buildMode",
    );
    String methodName = '$instanceId|null|$service|$operation';
    return _channel.invokeMapMethod<String, dynamic>(
      methodName,
      arguments,
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
  static _FNetEvent _mapEvent(dynamic event) {
    try {
      return _FNetEvent.fromJson(event);
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

class _FNetEvent {
  final String instanceId; // The instance id that have the event
  final String event; // The reference for the event
  final Map<String, dynamic> data; // The data sended throw the event

  _FNetEvent({
    @required this.instanceId,
    @required this.event,
    this.data,
  });

  Map<String, dynamic> toJson() {
    return {
      'instanceId': instanceId,
      'event': event,
      'data': data,
    };
  }

  static _FNetEvent fromJson(Map<String, dynamic> json) {
    if (json == null) return null;

    return _FNetEvent(
      instanceId: json['instanceId'],
      event: json['event'],
      data: json['data'],
    );
  }
}

class _FNetMessageDebug {
  final int requestId;
  final String instanceId;
  final String service;
  final String operation;
  final Map<String, dynamic> arguments;
  final Map<String, dynamic> result;
  final String errorCode;
  final String errorMessage;

  _FNetMessageDebug({
    this.requestId,
    this.instanceId,
    this.service,
    this.operation,
    this.arguments,
    this.result,
    this.errorCode,
    this.errorMessage,
  });

  Map<String, dynamic> toJson() {
    return {
      'requestId': requestId,
      'instanceId': instanceId,
      'service': service,
      'operation': operation,
      'arguments': arguments,
      'result': result,
      'errorCode': errorCode,
      'errorMessage': errorMessage,
    };
  }

  static _FNetMessageDebug fromJson(Map<String, dynamic> json) {
    if (json == null) return null;

    return _FNetMessageDebug(
      requestId: json['requestId'],
      instanceId: json['instanceId'],
      service: json['service'],
      operation: json['operation'],
      arguments: json['arguments'],
      result: json['result'],
      errorCode: json['errorCode'],
      errorMessage: json['errorMessage'],
    );
  }
}

class _DebugChannel {
  static _DebugChannel _instance = _DebugChannel._internal();

  _DebugChannel._internal() {
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

  factory _DebugChannel() => _instance;

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
        _FNetMessageDebug msg = _FNetMessageDebug.fromJson(json);

        // Deserialize the real application message
        //FNetMessage result = FNetSerializer.deserialize(msg.fnetMessage);

        // Insert the response the the map
        await _sendLock.synchronized(() {
          if (_outboxMessages.containsKey(msg.requestId)) {
            _outboxMessages.remove(msg.requestId);
          }

          if (_sendRequestMap.containsKey(msg.requestId)) {
            // Invoke the task completion
            Completer<Map<String, dynamic>> request =
                _sendRequestMap[msg.requestId];

            bool isFailed = msg.errorCode != null && !msg.errorCode.isEmpty;
            if (isFailed) {
              //* Handle invoke error
              String errorMessage =
                  "${msg.errorCode}, ${msg.errorMessage ?? ''}";
              request.completeError(Exception(errorMessage));
            } else {
              //* handle invoke complete
              request.complete(msg.result);
            }

            _sendRequestMap.remove(msg.requestId);
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
          final _FNetMessageDebug debugMessage = _FNetMessageDebug(
            requestId: sendRequestId,
            instanceId: instanceId,
            service: service,
            operation: operation,
            arguments: arguments,
          );

          // Encode the message
          final String jsonDegubMessage = jsonEncode(debugMessage);

          // Save the request
          _sendRequestMap.putIfAbsent(
            debugMessage.requestId,
            () => completer,
          );
          _outboxMessages.putIfAbsent(
            debugMessage.requestId,
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
