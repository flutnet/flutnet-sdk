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

class Bridge {
  static const MethodChannel _channel = MethodChannel('xamarin_channel');

  static final Bridge _instance = Bridge._internal();

  Bridge._internal();

  factory Bridge() => _instance;

  Future<Map<String, dynamic>> invokeMethod({
    @required String id,
    @required String type,
    @required String method,
    @required Map<String, dynamic> params,
  }) async {
    bool isDebug = buildMode == _BuildMode.debug;

    if (isDebug) {
      return _DebugChannel().invokeMethod(
        id: id,
        type: type,
        methodId: method,
        params: params,
      );
    } else {
      String methodName = '$id|$type|$method';
      return _channel.invokeMethod(
        methodName,
        params,
      );
    }
  }
}

class _FNetMessageDebug {
  final int requestId;
  final String id;
  final String type;
  final String methodId;
  final Map<String, dynamic> params;
  final Map<String, dynamic> res;

  _FNetMessageDebug({
    this.requestId,
    this.id,
    this.type,
    this.methodId,
    this.params,
    this.res,
  });

  Map<String, dynamic> toJson() {
    return {
      'requestId': requestId,
      'id': id,
      'type': type,
      'methodId': methodId,
      'params': params,
      'res': res,
    };
  }

  static _FNetMessageDebug fromJson(Map<String, dynamic> json) {
    if (json == null) return null;

    return _FNetMessageDebug(
      requestId: json['requestId'],
      id: json['id'],
      type: json['type'],
      methodId: json['methodId'],
      params: json['params'],
      res: json['res'],
    );
  }
}

class _DebugChannel {
  static final _DebugChannel _instance = _DebugChannel._internal();

  _DebugChannel._internal() {
    _sendLock.synchronized(() async {
      _autoConnect(forceOpen: true);
    });
  }

  factory _DebugChannel() => _instance;

  // Dispose state
  bool _disposed = false;

  ///
  /// The url user for the connection
  ///
  String _url = "ws://127.0.0.1:8081/debug";

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
      //* IOWebSocketChannel.connect("ws://127.0.0.1:8081/debug");
      //* APRO LA CONNESSIONE
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
    await _sendLock.synchronized(() async {
      _socketChannelConnected = false;

      //! Se ho ancora messaggi da spedire allora è stato un problema di rete la chiusura
      if (_outboxMessages.length > 0) {
        try {
          await _autoConnect(delay: const Duration(seconds: 1));
        } catch (ex) {
          // Errore apertura connessione
        }
      }
    });
  }

  ///
  /// Eveto di errore della connessione.
  ///
  Future _onConnectionError(dynamic error, dynamic stacktrace) async {
    await _sendLock.synchronized(() {
      _socketChannelConnected = false;
      try {
        if (error is WebSocketChannelException) {
          log(error.message);
        } else {
          log(error.toString());
        }
        if (_onError != null) {
          _onError(error.toString());
        }
      } catch (ex) {}
    });
  }

  //void onClose() => null;

  void _onMessage(dynamic message) => null;

  void _onError(Object error) => null;

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
            Completer<dynamic> request = _sendRequestMap[msg.requestId];

            if (request != null) {
              request.complete(msg.res);
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
  Future<dynamic> invokeMethod({
    @required String id,
    @required String type,
    @required String methodId,
    @required Map<String, dynamic> params,
  }) async {
    final Completer<dynamic> completer = new Completer<dynamic>();

    int sendRequestId;

    await _sendLock.synchronized(
      () async {
        sendRequestId = ++_uniqueId;

        try {
          final _FNetMessageDebug debugMessage = _FNetMessageDebug(
            requestId: sendRequestId,
            id: id,
            type: type,
            methodId: methodId,
            params: params,
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

          // Inizializzo la connessione se chiusa
          if (_socketChannelConnected == false) {
            try {
              await _autoConnect();
            } catch (ex) {
              // Errore apertura connessione
              debugPrint(ex);
            }
          } else {
            // Se la connessione è aperta invio i dati via rete.
            _socketChannel.sink.add(jsonDegubMessage);
          }
        } catch (ex) {
          if (ex is WebSocketChannelException) {}

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
