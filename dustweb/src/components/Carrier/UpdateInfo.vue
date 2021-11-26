<template>
  <div>
    <el-dialog title="修改头像" v-model="updateAvatarFormVisible">
      <div style="margin-bottom: 10px">选择头像上传（JPG格式，大小不超过1M）</div>
      <div class="box">
        <div>
          <el-upload
            class="avatar-uploader"
            action="none"
            accept="image/jpeg, image/jpg"
            :show-file-list="false"
            :auto-upload="false"
            :on-change="onUploadChange">
            <i class="el-icon-plus avatar-uploader-icon"></i>
          </el-upload>
        </div>
        <div>
          <el-image class="avatar" :fit="cover" v-bind:src="getAvatarBase64()" alt="">
            <template #error>
              <div style="height: 178px; line-height: 178px;">图片将显示在这里</div>
            </template>
          </el-image>
        </div>
      </div>
      <el-button @click="uploadAvatar">确认上传</el-button>
    </el-dialog>
    <transition name="el-fade-in-linear">
      <el-alert
          title="修改成功"
          type="success"
          :closable="false"
          style="width: 500px"
          show-icon v-if="UpdateStatus">
      </el-alert>
    </transition>
    <div style="margin: 10px;"></div>
    <div style="display: flex; flex-direction: row; margin-left: 20px">
      <div style="margin-right: 50px">
        <div class="update-title">修改头像</div>
        <el-button circle style="padding: 0 0 0 0;" @click="updateAvatarFormVisible=true">
          <el-avatar shape="circle" :size="150" fit="cover" v-bind:src="getAvatarUrl()"></el-avatar>
        </el-button>
      </div>
      <div style="margin-left: 50px;">
        <div class="update-title" style="margin-left: 18%; text-align: left">修改基本信息</div>
        <el-form :label-position="labelPosition" label-width="80px" ref="UserInfo" :model="UserInfo" style="width: 500px">
          <el-form-item label="姓名" prop="name">
            <el-input v-model="UserInfo.name"></el-input>
          </el-form-item>
          <el-form-item label="联系电话" prop="phone">
            <el-input v-model="UserInfo.phone"></el-input>
          </el-form-item>
          <el-form-item>
            <el-button type="primary" @click="submitForm">提交</el-button>
            <el-button @click="resetForm('UserInfo')">重置</el-button>
          </el-form-item>
        </el-form>
      </div>
    </div>
  </div>
</template>

<script>
import { Base64 } from 'js-base64';
export default {
  data() {
    return {
      labelPosition: 'right',
      //用户信息
      UserInfo: {
        account: Base64.decode(localStorage.getItem("username")),
        name: '',
        phone: '',
        password: Base64.decode(localStorage.getItem("password")),
      },
      UpdateStatus: false,
      updateAvatarFormVisible: false,
      avatarUrl: "http://220.179.227.205:6009/image/",
      avatarBase64: ""
    };
  },
  methods: {
    submitForm() {
      const req= {
        "id": this.UserInfo.account,
        "name": this.UserInfo.name,
        "password": "",
        "phonenumber": this.UserInfo.phone,
      }
      console.log(this.UserInfo.account, this.UserInfo.password);
      fetch(this.$URL + "/User/Update/Carrier", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "Authorization": "Bearer " + Base64.decode(localStorage.getItem("token"))
        },
        body: JSON.stringify(req),
      }).then((response) => {
        console.log(this.UserInfo);
        let result = response.json();
        result.then((res) => {
              console.log(res);
              this.UpdateStatus = res.status;
            }
        )
      })

    },
    resetInfo(){
      fetch(this.$URL + "/User/GetInformation/Carrier?req=" + this.UserInfo.account, {
        method: "GET",
        headers: {
          "Authorization": "Bearer " + Base64.decode(localStorage.getItem("token")),
        },
      }).then((response) => {
        let result = response.json();
        result.then((result) => {
          this.UserInfo.name = result.name;
          this.UserInfo.phone = result.phonenumber;
        })
      })
    },
    resetForm(formName) {
      this.$refs[formName].resetFields();
      this.resetInfo();
    },
    getAvatarBase64() {
      return this.avatarBase64;
    },
    getAvatarUrl() {
      return this.avatarUrl + this.UserInfo.account + ".jpg";
    },
    onUploadChange(file) {
      const isJPG = (file.raw.type === 'image/jpeg' || file.raw.type === 'image/jpg');
      const isLt1M = file.size / 1024 / 1024 < 1;
      if(!isJPG) {
        this.$message.error("上传图片只能是JPG或PNG格式");
        return false;
      }
      if(!isLt1M) {
        this.$message.error("上传图片大小不能超过1M");
        return false;
      }
      var that = this;
      var reader = new FileReader();
      reader.readAsDataURL(file.raw);
      reader.onload = function() {
        that.avatarBase64 = this.result;
      };
    },
    uploadAvatar() {
      const req = {
        "prefix": "jpg",
        "content": this.avatarBase64
      }
      fetch(this.$URL + "/User/Login/Upload", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "Authorization": "Bearer " + Base64.decode(localStorage.getItem("token"))
        },
        body: JSON.stringify(req),
      });
      this.$message({
        message: "上传成功，刷新当前页面可见",
        type: "success"
      });
    }
  },
  mounted() {
    this.UpdateStatus = false;
    this.resetInfo();

  }
}
</script>

<style>

</style>
