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
