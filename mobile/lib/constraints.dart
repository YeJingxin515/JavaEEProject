import 'package:flutter/material.dart';

const kPrimaryColor=Colors.blueAccent;
const kPrimaryLightColor=Color(0xA0C0C0C0);
const baseURL="http://220.179.227.205:6010";
const loginURL=baseURL+"/User/Login";
const getAllGarbageURL=baseURL+"/Garbage/GetAll?req=";
const getAllStationURL=baseURL+"/Facility/BinSite/GetAll";
const throwURL=baseURL+"/Throw/Add";
const getSiteAllURL=baseURL+"/Facility/TrashCan/GetSiteAll?req=";
const addGarbageURL=baseURL+"/Garbage/Add?req=";
const removeGarbageURL=baseURL+"/Garbage/Delete?req=";
const violateURL=baseURL+"/ViolateRecord/Add";