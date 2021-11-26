import 'dart:convert';

import 'package:dio/dio.dart';
import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';
import 'package:mobile/Model/StationModel.dart';
import 'package:mobile/Watcher/Scan.dart';
import 'package:shared_preferences/shared_preferences.dart';

import '../constraints.dart';

class WatcherHome extends StatelessWidget {
  const WatcherHome({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) {
    Size size = MediaQuery
        .of(context)
        .size;
    return Scaffold(
        body: Center(
          child: ClipOval(
            child: TextButton(
              onPressed: () async{
                print("pressed");
                Navigator.push(context,
                    MaterialPageRoute(builder: (context) => QRViewExample()));
              },
              style: ButtonStyle(
                padding: MaterialStateProperty.all(EdgeInsets.zero),
              ),
              child: ClipOval(
                child: Container(
                    color: kPrimaryLightColor,
                    //    margin: new EdgeInsets.all(size.width * 0.1),
                    padding: new EdgeInsets.all(size.width * 0.1),
                    child: Icon(
                      Icons.camera_alt,
                      size: size.width * 0.3,
                      color: Colors.black,
                    )),
              ),
            ),
          ),
        ));
  }


}
