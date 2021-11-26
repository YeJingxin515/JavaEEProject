import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';

import '../constraints.dart';

class RoundedButton extends StatelessWidget {
  final Function() press;
  final Color color,textColor;
  final String text;
  const RoundedButton({
    Key? key,
    required this.text,
    required this.press,
    this.color=kPrimaryColor,
    this.textColor=Colors.white,
  }) : super(key: key);
  @override
  Widget build(BuildContext context) {
    Size size=MediaQuery.of(context).size;
    return Container(
      margin: EdgeInsets.symmetric(vertical: 10),
      width: size.width*0.7,
      child: CupertinoButton(
          onPressed: press,
          child: Text(text,style:TextStyle(color:textColor),),
          color: color,
        ),
    );
  }
}