import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter/cupertino.dart';
import 'package:qr_flutter/qr_flutter.dart';
import './text_field_container.dart';
class RoundedInputField extends StatelessWidget {
  final String hintText;
  final IconData icon;
  final String? Function(String?) validator;
  final TextEditingController controller;
  const RoundedInputField({
    Key? key,
    required this.controller,
    required this.hintText,
    this.icon=Icons.person,
    required this.validator
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return TextFieldContainer(
        child:TextFormField(
          validator: validator,
          controller: controller,
          decoration: InputDecoration(
              icon:Icon(icon),
              hintText: hintText,
              border: InputBorder.none
          ),
        )
    );
  }
}