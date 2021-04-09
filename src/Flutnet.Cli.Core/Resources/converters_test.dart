// *************************************
//         NOT EDIT THIS FILE          *
// *************************************
import 'dart:typed_data';

import 'package:json_annotation/json_annotation.dart';
import 'dart:convert';

class Uint8Converter implements JsonConverter<Uint8List, Object> {
  const Uint8Converter();

  @override
  Uint8List fromJson(Object json) {
    if (json == null) {
      return null;
    }

    if (json is String) {
      List<int> list = base64.decode(json);
      Uint8List bytes = Uint8List.fromList(list);
      return bytes;
    }

    throw new Exception("Invalid Uint8List conversion!!");
  }

  @override
  Object toJson(Uint8List object) {
    return object ?? base64.encode(object);
  }
}

class DateTimeConverter implements JsonConverter<DateTime, Object> {
  const DateTimeConverter();

  @override
  DateTime fromJson(Object json) {
    if (json == null) {
      return null;
    }

    if (json is String) {
      //String jsonValue = json;
      // The problem is Dart supports second fraction only 6 digits.
      // But datatime that came from api is 7 digits.
      //if (jsonValue.contains(".")) {
      //  jsonValue = jsonValue.substring(0, jsonValue.length - 1);
      //}
      return DateTime.parse(json);
    }

    throw new Exception("Invalid DateTime conversion!!");
  }

  @override
  Object toJson(DateTime object) {
    return object?.toIso8601String() ?? object;
  }
}
