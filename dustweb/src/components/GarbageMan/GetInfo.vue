<template>
<div>
  <el-dialog title="查看大图" v-model="avatarVisible">
    <div>
      <el-image v-bind:src="getAvatarUrl()"></el-image>
    </div>
  </el-dialog>
  <div style="margin: 10px;"></div>
  <div id="main-box">
      <div class="box-title" style="margin-left: 50px;">基本信息</div>
    <div style="display: flex; flex-direction: row; margin-left: 30px; margin-top: 10px">
      <div style="margin-left: 20px; margin-top: 20px;">
        <el-button circle style="padding: 0 0 0 0" @click="avatarVisible = !avatarVisible">
          <el-avatar :size="200" fit="cover" v-bind:src="getAvatarUrl()">头像</el-avatar>
        </el-button>
      </div>
      <div class="info-box">
        <el-row style="margin-top: 10px;">
          <el-col :span="4"><div class="grid-title">账号</div></el-col>
          <el-col :span="19" :offset="1"><div style="text-align: left">{{UserInfo.account}}</div></el-col>
        </el-row>
        <el-row>
          <el-col :span="4"><div class="grid-title">姓名</div></el-col>
          <el-col :span="19" :offset="1"><div style="text-align: left">{{UserInfo.name}}</div></el-col>
        </el-row>
        <el-row>
          <el-col :span="4"><div class="grid-title">电话</div></el-col>
          <el-col :span="19" :offset="1"><div style="text-align: left">{{UserInfo.phone}}</div></el-col>
        </el-row>
        <el-row>
          <el-col :span="4"><div class="grid-title">信用积分</div></el-col>
          <el-col :span="19" :offset="1"><div style="text-align: left">{{UserInfo.credit}}</div></el-col>
        </el-row>
        <el-row>
          <el-col :span="4"><div class="grid-title">住址</div></el-col>
          <el-col :span="19" :offset="1"><div style="text-align: left">{{UserInfo.address}}</div></el-col>
        </el-row>
      </div>
    </div>
    <div style="display: flex; flex-direction: row; margin-top: 30px; margin-left: 50px; ">
      <div style="width: 20%; text-align: left">
        <div class="box-title">环保勋章</div>
        <div style="margin-top: 10px; font-size: 15px;">暂无勋章！</div>
      </div>
      <div style="margin-left: 250px; width: 70%; margin-right: 10%">
        <div class="box-title" style="margin-left: 10px">签名</div>
        <div style=" height: 200px; border-radius: 10px; background: #f3f6fa; margin-top: 10px"></div>
      </div>
    </div>
  </div>
</div>
</template>
<script>
import { Base64 } from 'js-base64';
export default {
  data() {
    return {
      //用户信息
      UserInfo: {
        account: Base64.decode(localStorage.getItem("username")),
        name: '',
        credit: '',
        phone: '',
        address: '',
        password: Base64.decode(localStorage.getItem("password")),
      },
      
      avatarVisible: false,
      updateAvatarFormVisible: false,
      avatarUrl: "http://220.179.227.205:6009/image/",
      avatarBase64: ""
    };
  },
  mounted() {
    console.log(this.UserInfo.account, this.UserInfo.password);
    fetch(this.$URL + "/User/GetInformation/GarbageMan?req=" + this.UserInfo.account, {
      method: "GET",
      headers: {
        "Authorization": "Bearer " + Base64.decode(localStorage.getItem("token")),
      },
    }).then((response) => {
      let result = response.json();
      result.then((result) => {
        this.UserInfo.name = result.name;
        this.UserInfo.credit = result.credit;
        this.UserInfo.phone = result.phonenumber;
        this.UserInfo.address = result.address;
      })
    })
  },
  methods: {
    getAvatarUrl() {
      return this.avatarUrl + this.UserInfo.account + ".jpg";
    },
  }
}
</script>
<style>
.grid-title {
  font-weight: bold;
}
.box-title{
  font-size: 22px; 
  font-weight: bold; 
  text-align: left;
}
.info-box{
  margin-left: 60px; 
  width: 70%; 
  margin-right: 10%;
  background: #f3f6fa;
  border-radius: 10px;
  font-size: 18px;
}
.info-box .el-row{
  margin-bottom: 30px;
}

.info-box .el-row:last-child{
  margin-bottom: 10px;
  }

.bg-purple-dark {
  background: #99a9bf;
}

.bg-purple {
  background: #d3dce6;
}

.bg-purple-light {
  background: #e5e9f2;
}

.grid-content {
  border-radius: 4px;
  min-height: 36px;
}

.row-bg {
  padding: 10px 0;
  background-color: #f9fafc;
}

.avatar-uploader .el-upload {
  border: 1px dashed #d9d9d9;
  border-radius: 6px;
  cursor: pointer;
  position: relative;
  overflow: hidden;
}
.avatar-uploader .el-upload:hover {
  border-color: #409EFF;
}
.avatar-uploader-icon {
  font-size: 28px;
  color: #8c939d;
  width: 178px;
  height: 178px;
  line-height: 178px;
  text-align: center;
}
.avatar {
  width: 178px;
  height: 178px;
  display: block;
}
.box{
  display: flex;
}
.box div{
  margin: 0 auto;
}
</style>