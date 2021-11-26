import 'dart:convert';

import 'package:dio/dio.dart';
import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:intl/intl.dart';
import 'package:mobile/Component/rounded_button.dart';
import 'package:mobile/Component/text_field_container.dart';
import 'package:mobile/Model/GarbageModel.dart';
import 'package:mobile/Model/StationModel.dart';
import 'package:mobile/constraints.dart';
import 'package:shared_preferences/shared_preferences.dart';

class ProcessPage extends StatefulWidget {
  String jsonStr;

  ProcessPage({Key? key, required this.jsonStr}) : super(key: key);

  @override
  _ProcessPageState createState() {
    print(json.decode(jsonStr));
    return _ProcessPageState(
        item: GarbageRecordModel.fromJson(json.decode(jsonStr)));
  }
}

class _ProcessPageState extends State<ProcessPage> {
  GarbageRecordModel item;

  _ProcessPageState({required this.item});

  @override
  void initState() {
    // TODO: implement initState
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    Size size = MediaQuery.of(context).size;
    return Scaffold(
      body: Center(
        child: Container(
          decoration: BoxDecoration(
              color: Colors.greenAccent,
              borderRadius: BorderRadius.all(Radius.circular(30))),
          padding: EdgeInsets.all(size.width * 0.05),
          height: size.height * 0.4,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.center,
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              TextFieldContainer(
                child: Text(
                  '垃圾编号：' + item.garId,
                  style: TextStyle(color: Colors.white),
                ),
              ),
              TextFieldContainer(
                  child: Text(
                '垃圾类型：' + item.type,
                style: TextStyle(color: Colors.white),
              )),
              TextFieldContainer(
                  child: Text(
                '创建时间：' +
                    DateFormat('yyyy-MM-dd HH:mm:ss').format(item.latestTime),
                style: TextStyle(color: Colors.white),
              )),
              RoundedButton(
                  text: '允许投递',
                  press: () async {
                    var dio = Dio();
                    SharedPreferences sharedPreferences =
                        await SharedPreferences.getInstance();
                    String username =
                        sharedPreferences.get('username').toString();
                    String token = sharedPreferences.get('token').toString();
                    String workingStation=sharedPreferences.get('workingStation').toString();
                    print(token);
                    dio.options.responseType = ResponseType.plain;
                    dio.options.headers['Authorization'] = 'Bearer ' + token;
                    var info=await showThrowDialog(context,token,workingStation);
                    if(info.isEmpty)
                      return;
                    var response = await dio.post(throwURL, data: {
                      'gid': item.garId,
                      'bid': info,
                    });
                    var jsonStr = json.decode(response.data.toString());
                    print(jsonStr);
                  }),
              RoundedButton(
                  text: '违规投递',
                  press: () async {
                    var dio = Dio();
                    SharedPreferences sharedPreferences =
                        await SharedPreferences.getInstance();
                    String username =
                        sharedPreferences.get('username').toString();
                    String token = sharedPreferences.get('token').toString();
                    dio.options.responseType = ResponseType.plain;
                    dio.options.headers['Authorization'] = 'Bearer ' + token;
                    var info = await showViolateDialog(context);
                    if(info.isEmpty)
                      return;
                    var response = await dio.post(violateURL, data: {
                      "gar_id": item.garId,
                      "watcher_id": username,
                      "reason": info['reason'],
                      "punishment": info['punishment']
                    });
                    var jsonStr =
                        json.decode(response.data.toString());
                    print(jsonStr);
                  })
            ],
          ),
        ),
      ),
    );
  }

  Future<String> showThrowDialog(BuildContext context,String token,String workingStation) async {
    List<DustBin> items = [];
    var dio = Dio();
    dio.options.responseType = ResponseType.plain;
    dio.options.headers['Authorization'] = 'Bearer ' + token;
    print(workingStation);
    var response = await dio.get(getSiteAllURL+workingStation);
    List<dynamic> jsonStr = json.decode(response.data.toString());
    print(jsonStr.runtimeType);
    items = jsonStr.map((i) => DustBin.fromJson(i)).toList();
    print(items);
    String? choice = await showDialog<String>(
        barrierDismissible: true,
        context: context,
        builder: (BuildContext context) {
          var child = Column(
            children: [
              ListTile(
                title: Text("请选择"),
              ),
              Expanded(
                  child: ListView.builder(
                      itemCount: items.length,
                      itemBuilder: (BuildContext context, int index) {
                        return ListTile(
                          title: Text(items[index].id),
                          subtitle: Text(items[index].condition),
                          onTap: () =>
                              Navigator.of(context).pop(items[index].id),
                        );
                      }))
            ],
          );
          return Dialog(
            child: child,
          );
        });
    if (choice != null) {
      return choice;
    }
    return '';
  }

  Future<Map<String, dynamic>> showViolateDialog(BuildContext context) async {
    Map<String, dynamic>? result = await showDialog<Map<String, dynamic>>(
        barrierDismissible: true,
        context: context,
        builder: (BuildContext context) {
          var size=MediaQuery.of(context).size;
          var _reason = new TextEditingController();
          var _punishment = new TextEditingController();
          var child = Container(
            height: size.height*0.5,
            decoration: BoxDecoration(
              borderRadius: BorderRadius.circular(10)
            ),
            child:Column(
            children: [
              SizedBox(height: 10,),
              Text("输入违规记录",
              style: TextStyle(fontSize: 15),),
              Container(
                margin:EdgeInsets.symmetric(vertical: 10),
                padding: EdgeInsets.symmetric(horizontal: 20,vertical: 5),
                child: TextField(
                  controller: _punishment,
                  keyboardType: TextInputType.number, //限定数字键盘
                  // inputFormatters: <TextInputFormatter>[
                  //   FilteringTextInputFormatter.digitsOnly
                  // ], //
                  decoration: InputDecoration(
                      hintText: "请输入惩罚分数", border: OutlineInputBorder()),
                ),
              ),
              Container(
                margin:EdgeInsets.symmetric(vertical: 10),
                padding: EdgeInsets.symmetric(horizontal: 20,vertical: 5),
                child: TextField(
                  controller: _reason,
                  maxLines: 4,
                  decoration: InputDecoration(
                      hintText: "请输入违规原因", border: OutlineInputBorder()),
                ),
              ),
              RoundedButton(
                  text: '提交',
                  press: () {
                    Map temp=Map<String,dynamic>();
                    print(_reason.text);
                    print(_punishment.text);
                    temp["reason"]=_reason.text;
                    temp["punishment"]=int.parse(_punishment.text);
                    Navigator.of(context).pop(temp);
                  })
            ],
          ));
          return Dialog(
            child: child,
          );
        });
    print(result);
    if (result != null) {
      return result;
    }else
      return Map<String,dynamic>();
  }
}
