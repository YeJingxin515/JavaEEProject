<template>
    <div id="searchInput">
      <el-input v-model="address" @input="search">
        <template #append>
          <el-button icon="el-icon-search" @click="search"></el-button>
        </template>
      </el-input>
    </div>
    <el-container>
      <el-main>
            <el-row>
                <el-col :span="10">
                    <div id="result" class="amap_lib_placeSearch" v-show="hide">
                      <div class="amap_lib_placeSearch_list amap-pl-pc" v-for="(item,index) in poiArr"
                      @click="openMarkerTipById(index,$event)"
                      @mouseout="onmouseout_MarkerStyle(index+1,$event)"
                      :key="index">
                        <div class="poibox" style="border-bottom: 1px solid #eaeaea">
                          <h3 class="poi-title" >
                            <span>{{item.name}}</span>
                          </h3>
                          <div class="poi-info">
                            <p>地址：{{item.address}}</p>
                            <p>电话：{{item.tel}}</p>
                          </div>
                        </div>
                      </div>
                    </div>  
                </el-col>
                <el-col :span="14">
                    <div id="iCenter"></div>
                </el-col>
            </el-row>
      </el-main>
      <el-footer>
        <el-button @click="fetAddressName">获取当前位置和名字</el-button>
      </el-footer>
    </el-container>

</template>

<script>
    export default {
    props:['newAddress','dataObj'],// 父组件传过来的地址和地址经纬度信息，
    data() {
    return {
      formLabelWidth: '100px',

      address:this.newAddress ? this.newAddress : '同济大学嘉定校区',//保存地址的汉字名字
      map1: '',
      map:'',//保存地址的经纬度
      poiArr: [],//左边搜索出来的数组
      windowsArr: [],//信息窗口的数组
      marker: [],
      mapObj: "",//地图对象
      selectedIndex: -1,
      hide: false,
      clickType: 1,
      location:{
        P:this.dataObj.lat,//纬度
        Q:this.dataObj.lng,//经度
      }
    };
    },

    mounted() {
      console.log(333,this.dataObj,this.location)
      this.mapInit()
      this.placeSearch(this.address)
    },

    methods: {
    showToast(address){
      this.placeSearch(address.address)
      console.log(444,address)
      this.location.P =address.lat
      this.location.Q =address.lng
      this.address = address.address
      let that = this;
      new AMap.InfoWindow({
      content:"<h3>" + '当前选中地址' + "</h3>" + that.address,
      size: new AMap.Size(300, 0),
      autoMove: true,
      offset: new AMap.Pixel(-4, -10)
      }).open(that.mapObj,that.location)
    },

    cancelSave(){
      eventBus.$emit('cancelSave')
    },

    //保存地址
    saveAddress(){
      let addressName,location;
      if(this.clickType==1){
        let address = this.poiArr[this.selectedIndex]
        addressName = address.name+address.address;
        location = address.location
        console.log(address.name+address.address,address.location)
      }else if(this.clickType==2){
        console.log(this.address,this.map)
        addressName = this.address;
        location = this.map;
      }else if(this.clickType==3){
        console.log(this.address,this.map1)
        addressName = this.address;
        location = this.map1;
      }
      eventBus.$emit('saveAddress',[addressName,location])
    },

    // 经纬度转化为详细地址
    getAddress(){
      let that = this;
      AMap.plugin('AMap.Geocoder',function(){
        let geocoder = new AMap.Geocoder({
        radius: 100,
        extensions: "all"
        });
        geocoder.getAddress([that.map1.lng,that.map1.lat], function(status, result) {
          if (status === 'complete' && result.info === 'OK') {
            let address = result.regeocode.formattedAddress;
            console.log(result.regeocode);
            that.address = result.regeocode.formattedAddress;
            // that.placeSearch(that.address)
          }
        });
      })
    },

    // 地图点击事件
    testevent(){
      let that = this;
      this.clickType = 3;
      // var map=new AMap.Map('iCenter');//重新new出一个对象，传入参数是div的id
      AMap.event.addListener(this.mapObj,'click',function (e) { //添加点击事件,传入对象名，事件名，回调函数
        that.map1 = e.lnglat;
        that.getAddress();
        setTimeout(()=>{
          new AMap.InfoWindow({
            content:"<h3>" + '当前选中地址' + "</h3>" + that.address,
            size: new AMap.Size(300, 0),
            autoMove: true,
            offset: new AMap.Pixel(-4, -10)
          }).open(that.mapObj,e.lnglat)
        },100)
      })
    },

    //创建一个map
    mapInit() {
    this.mapObj = new AMap.Map("iCenter", {
    resizeEnable: true,
    zoom: 10,
    })
    this.testevent();
    },

    //根据名字地址去搜索结果
    placeSearch(name) {
    let that = this;
    this.hide = true
    var MSearch;
    this.mapObj.plugin(
    ["AMap.PlaceSearch", "AMap.ToolBar", "AMap.Scale"],
    () => {
    this.mapObj.addControl(new AMap.ToolBar())
    this.mapObj.addControl(new AMap.Scale())
    MSearch = new AMap.PlaceSearch({
    //构造地点查询类
    city: that.address //城市
    });
    AMap.event.addListener(MSearch,"complete",this.keywordSearch_CallBack) //返回地点查询结果
    MSearch.search(name); //关键字查询
    }
    );
    },

    //结果的回调
    keywordSearch_CallBack(data) {
      console.log(111,data);
      var poiArr = data.poiList.pois;
      var resultCount = poiArr.length;
      this.poiArr = poiArr; //左边要渲染的数据
      for (var i = 0; i < resultCount; i++) {
        this.addmarker(i, poiArr[i]);
        console.log(poiArr[i]);
        this.poiArr[i].url = this.poiArr[i].photos? this.poiArr[i].photos[0]? this.poiArr[i].photos[0].url: "": ""
      }
      this.mapObj.setFitView();
    },

    //添加marker&infowindow
    addmarker(i, d) {
      var lngX = d.location.getLng();
      var latY = d.location.getLat();
      console.log(lngX,latY)
      var markerOption = {
        map: this.mapObj,
        position: new AMap.LngLat(lngX, latY)
      };
      var mar = new AMap.Marker(markerOption);
      this.marker.push(new AMap.LngLat(lngX, latY));
      var infoWindow = new AMap.InfoWindow({
        content: "<h3>" +'当前选中位置：'+ d.name + "</h3>" + this.TipContents(d.name, d.address),
        size: new AMap.Size(300, 0),
        autoMove: true,
        offset: new AMap.Pixel(0, -30)
      });
      console.log();
      this.windowsArr.push(infoWindow);
      var _this = this;
      var aa = (e) => {
        this.clickType = 2
        var obj = mar.getPosition();
        this.map = obj //这里保存的地址经纬度
        this.address = d.name//这里保存的是地址名字
        console.log(1);
        console.log(this.address);
        console.log(d.address);
        infoWindow.open(_this.mapObj, obj);
      }
      AMap.event.addListener(mar, "click", aa)
    },

    TipContents(name, address) {
      //窗体内容
      if (
        name == "" ||
        name == "undefined" ||
        name == null ||
        name == " undefined" ||
        typeof name == "undefined"
      ) {
        type = "暂无";
      }
      if (
        address == "" ||
        address == "undefined" ||
        address == null ||
        address == " undefined" ||
        typeof address == "undefined"
      ) {
        address = "暂无";
      }
      var str = `地址：${address}`;
      return str;
    },

    openMarkerTipById(pointid, event) {
    //根据id 打开搜索结果点tip
    this.clickType = 1
    event.currentTarget.style.background = "#CAE1FF";
    this.selectedIndex = pointid
    // this.map = this.marker[pointid]
    this.map1 = this.poiArr[pointid].location
    console.log(222,this.mapObj, this.marker[pointid])
    console.log(this.marker[pointid],this.poiArr[pointid])
    this.address = this.poiArr[pointid].name
    this.windowsArr[pointid].open(this.mapObj, this.marker[pointid])
    },

    onmouseout_MarkerStyle(pointid, event) {
    //鼠标移开后点样式恢复
    event.currentTarget.style.background = ""
    },

    search() {
    this.windowsArr = []
    this.marker = []
    this.mapObj=''
    this.mapInit()
    this.placeSearch(this.address)
    },

    //返回父组件，关闭地图弹窗并且返回得到的地址
    fetAddressName(){
        //console.log(1);
        this.$emit('location',this.address,false);
    }
    },
    };
</script>

<style>
    .el-col {
      border-radius: 4px;
    }

    #searchInput{
      width:500px;
      margin:auto;
    }

    #iCenter{
        height:400px;
        width:400px;
    }

    #result {
        position: absolute;
        width:300px;
        height: 400px;
        z-index: 8;
        overflow-y: scroll;
        border-right: 1px solid #ccc;
    }

    .amap_lib_placeSearch {
      height: 100%;
      overflow-y: scroll;
    }
    .poibox {
      border-bottom: 1px solid #eaeaea;
      cursor: pointer;
      padding: 5px 20px 5px 10px;
      position: relative;
    }
    .poi-title {
      margin-left: 0;
      font-size: 14px;
      overflow: hidden;
    }
    .poi-info {
      word-break: break-all;
      margin:auto;
      overflow: hidden;
    }
    p {
      color: #999;
      font-family: Tahoma;
      line-height: 16px;
      font-size: 12px;
    }
</style>
