import 'dart:convert';

import 'package:dio/dio.dart';
import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';
import 'package:fluttertoast/fluttertoast.dart';
import 'package:mobile/Component/rounded_button.dart';
import 'package:mobile/Component/rounded_input_field.dart';
import 'package:mobile/Component/rounded_password_field.dart';
import 'package:mobile/GarbageMan/Page.dart';
import 'package:mobile/Model/StationModel.dart';
import 'package:mobile/Watcher/Page.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../Model/LoginModel.dart';
import '../constraints.dart';

class LoginPage extends StatefulWidget {
  const LoginPage({Key? key}) : super(key: key);

  @override
  _LoginPageState createState() => new _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
  final GlobalKey<FormState> _formkey = GlobalKey<FormState>();
  final TextEditingController _nameCtrl = new TextEditingController();
  final TextEditingController _pwdCtrl = new TextEditingController();

  @override
  Widget build(BuildContext context) {
    Size size = MediaQuery.of(context).size;
    return Scaffold(
      body: Center(
        child: SingleChildScrollView(
            child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Text(
              "欢迎使用垃圾一体化平台",
              style: TextStyle(fontWeight: FontWeight.bold),
            ),
            Container(
              margin: new EdgeInsets.only(top: size.height * 0.03),
              child: ClipOval(
                  child: Image.asset(
                "assets/icons/welcome.png",
                height: size.height * 0.3,
              )),
            ),
            Form(
              key: _formkey,
              child: Column(
                //crossAxisAlignment: CrossAxisAlignment.stretch,
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  RoundedInputField(
                      controller: _nameCtrl,
                      hintText: "用户名",
                      validator: emptyCheck),
                  RoundedPasswordField(
                      controller: _pwdCtrl, validator: emptyCheck),
                  RoundedButton(
                      text: "登录",
                      press: () async {
                        if (_formkey.currentState!.validate()) {
                          print(_nameCtrl.value.text);
                          print(_pwdCtrl.value.text);
                          /*
                            http请求登录信息。
                           */
                          var dio = Dio();
                          dio.options.responseType = ResponseType.plain;
                          var response = await dio.post(loginURL, data: {
                            'userID': _nameCtrl.value.text,
                            'password': _pwdCtrl.value.text
                          });
                          //print(response.data.toString());
                          var jsonStr = json.decode(response.data.toString());
                          //print(jsonStr);
                          var result = LoginResponse.fromJson(jsonStr);
                          print(result.status);
                          print(result.role);
                          print(result.token);
                          if (result.status == 1) {
                            SharedPreferences sharedPreferences =
                                await SharedPreferences.getInstance();
                            sharedPreferences.setString(
                                'username', _nameCtrl.value.text);
                            sharedPreferences.setString('token', result.token);
                            sharedPreferences.setString('role', result.role);
                            switch (result.role) {
                              case 'GarbageMan':
                                Navigator.push(
                                    context,
                                    new MaterialPageRoute(builder: (context)=>new GarbageMan())
                                );
                                print('GarbageMan Login');
                                break;
                              case 'Watcher':
                                print('Watcher Login');
                                showListDialog(context);
                                Navigator.push(
                                    context,
                                    new MaterialPageRoute(builder: (context)=>new WatcherHome())
                                );
                                break;
                              case 'Carrier':
                                print('Carrier Login');
                                break;
                              case 'StationStaff':
                                print('Staff Login');
                                break;
                            }
                          } else {
                            Fluttertoast.showToast(msg: "登录失败,请检查用户名和密码");
                          }
                        }
                      })
                ],
              ),
            )
          ],
        )),
      ),
    );
  }
  Future<void> showListDialog(BuildContext context) async {
    List<Station> items = [];
    var dio=Dio();
    dio.options.responseType=ResponseType.plain;
    var response=await dio.get(getAllStationURL);
    List<dynamic> jsonStr = json.decode(response.data.toString());
    print(jsonStr.runtimeType);
    items = jsonStr.map((i) => Station.fromJson(i)).toList();
    print(items);
    String? choice = await showDialog<String>(
        barrierDismissible: false,
        context: context,
        builder: (BuildContext context) {
          var child = Column(
            children: [
              ListTile(title: Text("请选择"),),
              Expanded(
                  child: ListView.builder(
                      itemCount: items.length,
                      itemBuilder: (BuildContext context, int index) {

                        return ListTile(
                          title: Text(items[index].name),
                          subtitle: Text(items[index].location),
                          onTap: ()=>Navigator.of(context).pop(items[index].name),
                        );
                      })
              )
            ],
          );
          return Dialog(child: child,);
        });
    if(choice!=null){
      SharedPreferences sharedPreferences =
      await SharedPreferences.getInstance();
      sharedPreferences.setString('workingStation', choice);
    }
  }
}

String? emptyCheck(String? value) {
  if (value == null || value.isEmpty) {
    return "请输入用户名";
  } else
    return null;
}
