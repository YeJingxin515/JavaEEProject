import 'package:flutter/material.dart';
import 'package:flutter/cupertino.dart';
import './text_field_container.dart';
import './rounded_input_field.dart';
class RoundedPasswordField extends StatelessWidget {
  final String? Function(String?) validator;
  final TextEditingController controller;
  const RoundedPasswordField({
    Key? key,
    required this.controller,
    required this.validator,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return TextFieldContainer(
        child:TextFormField(
          controller: controller,
          validator: validator,
          decoration: InputDecoration(
              icon:Icon(Icons.lock),
              suffixIcon: Icon(Icons.visibility),
              border: InputBorder.none
          ),
        )
    );
  }
}
