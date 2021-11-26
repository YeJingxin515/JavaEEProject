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
    <div style="display: flex; flex-direction: row; margin-left: 30px; margin-top: 30px">
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
          <el-col :span="4"><div class="grid-title">联系电话</div></el-col>
          <el-col :span="19" :offset="1"><div style="text-align: left">{{UserInfo.phone}}</div></el-col>
        </el-row>
        <el-row>
          <el-col :span="4"><div class="grid-title">工作地点</div></el-col>
          <el-col :span="19" :offset="1"><div style="text-align: left">{{UserInfo.plantname}} 垃圾收集点</div></el-col>
        </el-row>
      </div>
    </div>
      <div style="width: 20%; text-align: left; margin-top: 50px; margin-left: 50px;">
        <div class="box-title">备注</div>
        <div style="margin-top: 10px; font-size: 15px;">暂无备注</div>
      </div>
  </div>
</div>
</template>
<script>
import { Base64 } from "js-base64";
export default {
  data() {
    return {
      //用户信息
      UserInfo: {
        account: Base64.decode(localStorage.getItem("username")),
        name: "",
        phone: "",
        plant_name: "",
        password: Base64.decode(localStorage.getItem("password")),
      },
      avatarVisible: false,
      avatarUrl: "http://220.179.227.205:6009/image/",
    };
  },
  mounted() {
    console.log(this.UserInfo.account, this.UserInfo.password);
    fetch(
      this.$URL +
        "/User/GetInformation/StationStaff?req=" +
        this.UserInfo.account,
      {
        method: "GET",
        headers: {
          Authorization:
            "Bearer " + Base64.decode(localStorage.getItem("token")),
        },
      }
    ).then((response) => {
      let result = response.json();
      result.then((result) => {
        this.UserInfo.name = result.name;
        this.UserInfo.phone = result.phonenumber;
        this.UserInfo.address = result.address;
        this.UserInfo.plantname = result.plantname;
      });
    });
  },
  methods: {
    getAvatarUrl() {
      return this.avatarUrl + this.UserInfo.account + ".jpg";
    }
  },
};
</script>
<style>
.el-row {
  margin-bottom: 20px;
}

.el-row:last-child {
  margin-bottom: 0;
}

.el-col {
  border-radius: 4px;
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
  border-color: #409eff;
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
.box {
  display: flex;
}
.box div {
  margin: 0 auto;
}
</style>