import "index.dart";
import 'dart:convert';

class MessageSerializer {
  static final Map<String, LibMessage Function(Map<String, dynamic>)>
      _codeToMessage = {
    "CmdHello": (Map<String, dynamic> json) => CmdHello.fromJson(json),
    "CmdHello2": (Map<String, dynamic> json) => CmdHello2.fromJson(json),
    "CmdError": (Map<String, dynamic> json) => CmdError.fromJson(json),
    "CmdFetchItems": (Map<String, dynamic> json) =>
        CmdFetchItems.fromJson(json),
    "ResFetchItems": (Map<String, dynamic> json) =>
        ResFetchItems.fromJson(json),
    "CmdTestItem": (Map<String, dynamic> json) => CmdTestItem.fromJson(json),
    "ResTestItem": (Map<String, dynamic> json) => ResTestItem.fromJson(json),
  };

  static final Map<Type, String> _typeToCode = {
    CmdHello().runtimeType: "CmdHello",
    CmdHello2().runtimeType: "CmdHello2",
    CmdError().runtimeType: "CmdError",
    CmdFetchItems().runtimeType: "CmdFetchItems",
    ResFetchItems().runtimeType: "ResFetchItems",
    CmdTestItem().runtimeType: "CmdTestItem",
    ResTestItem().runtimeType: "ResTestItem",
  };

  ///
  /// Convert a serializes netJsonMessage in a [LibMessage].
  /// When the message is formatted bad, this mathod will fail
  /// with a [DeserializationException].
  ///
  static LibMessage deserialize(String jsonMsg) {
    // Nothing to do
    if (jsonMsg == null || jsonMsg.isEmpty) return null;

    try {
      // Json decoding
      Map<String, dynamic> json = jsonDecode(jsonMsg);

      String msgTypeCode = json.keys.first;

      var fromJson = _codeToMessage.containsKey(msgTypeCode)
          ? _codeToMessage[msgTypeCode]
          : null;

      String content = json[msgTypeCode];

      Map<String, dynamic> payload = jsonDecode(content);

	  ///! *******************************
      ///! REAL DESERIALIZATION PROCESS
      ///! *******************************
      return fromJson(payload);

    } catch (e) {
      throw new DeserializationException(
          "Error during lib deserialize process: $jsonMsg");
    }
  }

  ///
  /// Convert a [LibMessage] in a string json format
  /// used in .NET Xamarin library.
  /// In case of erros thow a [SerializationException].
  ///
  static String serialize(LibMessage libMessage) {
    // Nothing to do
    if (libMessage == null) return null;

    try {
      //
      final String key = _typeToCode.containsKey(libMessage.runtimeType)
          ? _typeToCode[libMessage.runtimeType]
          : null;

      String content = jsonEncode(libMessage);

      final Map<String, String> map = {
        key: content,
      };

      // Encode the message using JSON Serielization
      final String json = jsonEncode(map);

      return json;
    } catch (e) {
      throw new SerializationException(
          "Error during lib serialize process: $libMessage");
    }
  }
}
