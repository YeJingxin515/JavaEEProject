import 'package:dio/dio.dart';
import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';
import 'package:flutter_speed_dial/flutter_speed_dial.dart';
import 'package:intl/intl.dart';
import 'package:mobile/GarbageMan/DetailPage.dart';
import 'package:mobile/Model/GarbageModel.dart';
import 'package:mobile/constraints.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'dart:convert';

class GarbageMan extends StatefulWidget {
  const GarbageMan({Key? key}) : super(key: key);

  @override
  _GarbageManState createState() => _GarbageManState();
}

class _GarbageManState extends State<GarbageMan> {
  List<GarbageRecordModel> items = [];
  late String username;
  late String token;

  @override
  void initState() {
    _getData();
    super.initState();
  }

  /*
  注意异步延迟，最后调用setState.初始化不能是late。
  */
  void _getData() async {
    SharedPreferences sharedPreferences = await SharedPreferences.getInstance();
    username = sharedPreferences.get('username').toString();
    token = sharedPreferences.get('token').toString();
    var dio = Dio();
    dio.options.responseType = ResponseType.plain;
    dio.options.headers['Authorization'] = 'Bearer ' + token;
    var response = await dio.get(getAllGarbageURL + username);
    List<dynamic> jsonStr = json.decode(response.data.toString());
    print(jsonStr.runtimeType);
    items = jsonStr.map((i) => GarbageRecordModel.fromJson(i)).toList();
    items.sort((a, b) => (b.latestTime).compareTo(a.latestTime));
    print(items.first.latestTime);
    setState(() {});
  }

  @override
  Widget build(BuildContext context) {
    Widget divider1 = Divider(
      color: Colors.blue,
    );
    Widget divider2 = Divider(color: Colors.green);
    return Scaffold(
      floatingActionButton: SpeedDial(
        child: Icon(Icons.menu),
        onClose: () => _getData(),
        children: [
          SpeedDialChild(
            child: Icon(Icons.navigate_before),
            onTap: () {
              Navigator.pop(context);
            },
          ),
          SpeedDialChild(
            child: Icon(Icons.add),
            onTap: () async {
              _addGarbage();
              await Future.delayed(Duration(seconds: 2))
                  .then((_) => _getData());
            },
          ),
          SpeedDialChild(
            child: Icon(Icons.refresh),
            onTap: () {
              _getData();
            },
          ),
        ],
      ),

      // FloatingActionButton(
      //   child: Icon(Icons.navigate_before),
      //   onPressed: () {
      //     Navigator.pop(context);
      //   },
      // ),
      body: SafeArea(
        child: ListView.separated(
          itemCount: items.length,
          itemBuilder: (BuildContext context, int index) {
            final item = items[index];
            return Dismissible(
              key: Key(item.garId),
              onDismissed: (_) async {
                var dio = Dio();
                SharedPreferences sharedPreferences =
                    await SharedPreferences.getInstance();
                String username = sharedPreferences.get('username').toString();
                String token = sharedPreferences.get('token').toString();
                dio.options.responseType = ResponseType.plain;
                dio.options.headers['Authorization'] = 'Bearer ' + token;

                items.removeAt(index);
              },
              child: ListTile(
                title: Text(items[index].garId),
                subtitle: Text(DateFormat('yyyy-MM-dd HH:mm:ss')
                    .format(items[index].latestTime)),
                onTap: () {
                  if (items[index].status != 0) return;
                  Navigator.push(
                      context,
                      new MaterialPageRoute(
                          builder: (context) =>
                              GarbageDetail(item: items[index])));
                },
              ),
            );
          },
          separatorBuilder: (BuildContext context, int index) {
            return index % 2 == 0 ? divider1 : divider2;
          },
        ),
      ),
    );
  }

  void _addGarbage() async {
    var dio = Dio();
    SharedPreferences sharedPreferences = await SharedPreferences.getInstance();
    String username = sharedPreferences.get('username').toString();
    String token = sharedPreferences.get('token').toString();
    dio.options.responseType = ResponseType.plain;
    dio.options.headers['Authorization'] = 'Bearer ' + token;
    String? choice = await showThrowDialog(context);
    if (choice == null) return;
    var response = dio.get(addGarbageURL + choice);
    print(response);
    return;
  }

  Future<String?> showThrowDialog(BuildContext context) async {
    var size=MediaQuery.of(context).size;
    String? choice = await showDialog<String>(
        context: context,
        builder: (BuildContext context) {
          var child = Container(
            height: size.height*0.35,
              decoration: BoxDecoration(
                  borderRadius: BorderRadius.circular(10)
              ),
              child: Column(
            children: [
              ListTile(
                title: Text("请选择垃圾类型"),
              ),
              Expanded(
                  child: ListView(
                children: [
                  ListTile(
                    title: Text('干垃圾'),
                    onTap: () => Navigator.of(context).pop('干垃圾'),
                  ),
                  ListTile(
                    title: Text('湿垃圾'),
                    onTap: () => Navigator.of(context).pop('湿垃圾'),
                  ),
                  ListTile(
                    title: Text('可回收垃圾'),
                    onTap: () => Navigator.of(context).pop('可回收垃圾'),
                  ),
                  ListTile(
                    title: Text('有害垃圾'),
                    onTap: () => Navigator.of(context).pop('有害垃圾'),
                  )
                ],
              ))
            ],
          ));
          return Dialog(
            child: child,
          );
        });
    return choice;
  }
}
