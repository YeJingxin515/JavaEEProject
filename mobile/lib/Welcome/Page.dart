import 'package:flutter/material.dart';
import 'package:flutter/cupertino.dart';
import 'package:mobile/Component/rounded_button.dart';
import 'package:mobile/Login/Page.dart';

import '../constraints.dart';

class WelcomePage extends StatelessWidget {
  const WelcomePage({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    Size size = MediaQuery.of(context).size;
    return Scaffold(
      body: Container(
        height: size.height,
        width: double.infinity,
        alignment: Alignment.center,
        child: SingleChildScrollView(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: <Widget>[
              Text(
                "欢迎使用垃圾处理一体平台",
                style: TextStyle(fontWeight: FontWeight.bold),
              ),
              Container(
                margin: new EdgeInsets.only(
                    top: size.height * 0.03, bottom: size.height * 0.03),
                child: ClipOval(
                  child: Image.asset(
                    "assets/icons/welcome.png",
                    height: size.height * 0.2,
                    fit: BoxFit.fill,
                  ),
                ),
              ),
              RoundedButton(press: () {
                Navigator.push(
                  context,
                  new MaterialPageRoute(builder: (context)=>new LoginPage())
                );
              }, text: "登陆"),
            ],
          ),
        ),
      ),
    );
  }
}
