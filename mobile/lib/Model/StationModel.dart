

class Station{
  String name;
  String location;
  Station({required this.name,required this.location});
  factory Station.fromJson(Map<String,dynamic> json){
    return Station(name: json['name'],location: json['location']);
  }
}

class DustBin {
  String id;
  String type;
  String condition;
  DustBin({required this.id,required this.type,required this.condition});
  factory DustBin.fromJson(Map<String,dynamic> json){
    return DustBin(id: json['id'], type: json['type'], condition: json['condition']);
  }
}