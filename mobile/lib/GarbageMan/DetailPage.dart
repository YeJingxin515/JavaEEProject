import 'dart:convert';

import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';
import 'package:mobile/Model/GarbageModel.dart';
import 'package:qr_flutter/qr_flutter.dart';
class GarbageDetail extends StatelessWidget {
  final GarbageRecordModel item;
  const GarbageDetail({Key? key,required this.item}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body:Center(child: QrImage(data: json.encode(item))) ,
    );
  }
}
