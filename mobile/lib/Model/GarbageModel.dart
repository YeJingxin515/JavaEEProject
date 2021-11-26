import 'dart:convert';
import 'dart:core';

import 'package:intl/intl.dart';
/*
  用来接受返回的垃圾列表
 */
class GarbageRecordModel{
  String garId;
  String type;
  int status;
  DateTime latestTime;
  GarbageRecordModel({required this.garId,required this.type,required this.latestTime,required this.status});
  factory GarbageRecordModel.fromJson(Map<String,dynamic> json){
    DateTime temp=DateFormat('yyyy-MM-ddTHH:mm:ss').parse(json['latest_time']);
    return GarbageRecordModel(garId: json['gar_id'], type: json['type'],latestTime: temp,status:json['status']);
  }
  Map<String, dynamic> toJson() =>
      <String, dynamic>{
        'gar_id': garId,
        'type': type,
        'latest_time':DateFormat('yyyy-MM-ddTHH:mm:ss').format(latestTime),
        'status':status
      };
}