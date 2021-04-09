class SerializationException implements Exception {
  String cause;
  SerializationException(this.cause);
}

class DeserializationException implements Exception {
  String cause;
  DeserializationException(this.cause);
}

