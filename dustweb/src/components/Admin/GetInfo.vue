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
        <div style="margin-left: 60px; width: 70%; margin-right: 10%;">
          <div id="info-box">
            <div style="height: 10px; margin-top: 50px"></div>
            <el-row>
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
          </div>
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
        address: "",
        password: Base64.decode(localStorage.getItem("password")),
      },
      avatarVisible: false,
      avatarUrl: "http://220.179.227.205:6009/image/"
    };
  },
  methods: {
    getAvatarBase64() {
      return this.avatarBase64;
    },
    getAvatarUrl() {
      return this.avatarUrl + this.UserInfo.account + ".jpg";
    },
    onUploadChange(file) {
      const isJPG =
        file.raw.type === "image/jpeg" || file.raw.type === "image/jpg";
      const isLt1M = file.size / 1024 / 1024 < 1;
      if (!isJPG) {
        this.$message.error("上传图片只能是JPG或PNG格式");
        return false;
      }
      if (!isLt1M) {
        this.$message.error("上传图片大小不能超过1M");
        return false;
      }
      var that = this;
      var reader = new FileReader();
      reader.readAsDataURL(file.raw);
      reader.onload = function () {
        that.avatarBase64 = this.result;
      };
    },
    uploadAvatar() {
      const req = {
        prefix: "jpg",
        content: this.avatarBase64,
      };
      fetch(this.$URL + "/User/Login/Upload", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization:
            "Bearer " + Base64.decode(localStorage.getItem("token")),
        },
        body: JSON.stringify(req),
      });
      this.$message({
        message: "上传成功，刷新当前页面可见",
        type: "success",
      });
    },
  },
  mounted() {
    console.log(this.UserInfo.account, this.UserInfo.password);
    fetch(
      this.$URL +
        "/User/GetInformation/Administrator?req=" +
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
      })
    })
  },
  methods: {
    getAvatarUrl() {
      return this.avatarUrl + this.UserInfo.account + ".jpg";
    }
  }
}
</script>
<style>
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
#info-box{
  background: #f3f6fa;
  border-radius: 10px;
  font-size: 18px;
}
#info-box .el-row{
  margin-bottom: 40px;
}
#info-box .el-row:last-child{
  margin-bottom: 0
}
</style>