import 'dart:core';
import 'package:intl/intl.dart';
enum LoginStatus { FAIL, SUCCESS }

class LoginResponse {
  int status;
  String role;
  String token;


  LoginResponse(
      {required this.status,
      required this.role,
      required this.token,
      });

  factory LoginResponse.fromJson(Map<String, dynamic> json) {

    return LoginResponse(
        status: json['status'], role: json['role'], token: json['token']);
  }
}
