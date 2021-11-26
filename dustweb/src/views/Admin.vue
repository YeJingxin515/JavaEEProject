<template>
  <el-header style="text-align: right; background-color: white">
    <el-button circle id="avatar" @click="choice = 1">
      <el-avatar
        shape="circle"
        fit="cover"
        v-bind:src="getAvatarUrl()"
      ></el-avatar>
    </el-button>
    <span style="color: #409eff">欢迎您！管理员{{ username }}</span>
  </el-header>
  <el-container style="height: 100%; border: 1px solid #eee">
    <el-aside width="200px" style="background-color: rgb(238, 241, 246)">
      <el-menu :default-openeds="['1']" @select="changeWindow">
        <el-submenu index="1">
          <template #title><i class="el-icon-user"></i>用户信息</template>
          <!--            <template #title>分组一</template>-->
          <el-menu-item index="1-1">查看信息</el-menu-item>
          <el-menu-item index="1-2">修改信息</el-menu-item>
          <el-menu-item index="1-3">修改密码</el-menu-item>
        </el-submenu>
        <el-submenu index="2">
          <template #title><i class="el-icon-setting"></i>设备管理</template>
          <el-menu-item index="2-1">垃圾桶管理</el-menu-item>
          <el-menu-item index="2-2">垃圾站管理</el-menu-item>
          <el-menu-item index="2-3">垃圾车管理</el-menu-item>
          <el-menu-item index="2-4">处理站管理</el-menu-item>
        </el-submenu>
        <el-submenu index="3">
          <template #title><i class="el-icon-menu"></i>工作管理</template>
          <el-menu-item index="3-1">投递记录管理</el-menu-item>
          <el-menu-item index="3-2">运输记录查看</el-menu-item>
          <el-menu-item index="3-3">违规记录管理</el-menu-item>
          <el-menu-item index="3-4">交互记录管理</el-menu-item>
          <el-menu-item index="3-5">监察员工作查看</el-menu-item>
        </el-submenu>
        <el-submenu index="4">
          <template #title><i class="el-icon-menu"></i>人事管理</template>
          <el-menu-item index="4-1">投放者信息</el-menu-item>
          <el-menu-item index="4-2">监察员信息</el-menu-item>
          <el-menu-item index="4-3">运输员信息</el-menu-item>
          <el-menu-item index="4-4">处理员信息</el-menu-item>
        </el-submenu>
        <el-menu-item index="5" route
          ><i class="el-icon-circle-close"></i>退出系统</el-menu-item
        >
      </el-menu>
    </el-aside>

    <el-container>
      <el-main>
        <!-- index=1 -->
        <GetInfo v-if="this.choice === 1"></GetInfo>
        <UpdateInfo v-if="this.choice === 2"></UpdateInfo>
        <UpdatePassword v-if="this.choice === 3"></UpdatePassword>
        <Trashcan v-if="this.choice === 4"></Trashcan>

        <!-- index=2 -->
        <Binsite v-if="this.choice === 5"></Binsite>
        <Truck v-if="this.choice === 6"></Truck>
        <Plant v-if="this.choice === 7"></Plant>
        <Garbage v-if="this.choice === 8"></Garbage>

        <!-- index=3 -->
        <Transport v-if="this.choice === 9"></Transport>
        <ViolateRecord v-if="this.choice === 10"></ViolateRecord>
        <InteractManage v-if="this.choice === 11"></InteractManage>
        <WorkRecord v-if="this.choice === 12"></WorkRecord>

        <!-- index=4 -->
        <GetThrowerInfo v-if="this.choice === 13"></GetThrowerInfo>
        <GetWatcherInfo v-if="this.choice === 14"></GetWatcherInfo>
        <GetCarrierInfo v-if="this.choice === 15"></GetCarrierInfo>
        <GetStaffInfo v-if="this.choice === 16"></GetStaffInfo>
      </el-main>
    </el-container>
  </el-container>
</template>

<script>
//index=1的组件
import GetInfo from "@/components/Admin/GetInfo";
import UpdateInfo from "@/components/Admin/UpdateInfo";
import UpdatePassword from "@/components/Admin/UpdatePassword";
import Trashcan from "@/components/Admin/Trashcan";

//index=2的组件
import Binsite from "@/components/Admin/Binsite";
import Truck from "@/components/Admin/Truck";
import Plant from "@/components/Admin/Plant";
import Garbage from "@/components/Admin/Garbage";

//index=3的组件
import Transport from "@/components/Admin/Transport";
import ViolateRecord from "@/components/Admin/ViolateRecord";
import InteractManage from "@/components/Admin/InteractManage";
import WorkRecord from "@/components/Admin/WorkRecord";

//index=4的组件
import GetThrowerInfo from "@/components/Admin/GetThrowerInfo";
import GetWatcherInfo from "@/components/Admin/GetWatcherInfo";
import GetCarrierInfo from "@/components/Admin/GetCarrierInfo";
import GetStaffInfo from "@/components/Admin/GetStaffInfo";

import { Base64 } from "js-base64";

export default {
  components: {
    GetInfo,
    UpdateInfo,
    UpdatePassword,
    Trashcan,
    GetThrowerInfo,
    GetWatcherInfo,
    GetCarrierInfo,
    GetStaffInfo,
    Binsite,
    Truck,
    WorkRecord,
    InteractManage,
    Transport,
    ViolateRecord,
    Plant,
    Garbage,
  },
  data() {
    return {
      choice: 1,
      username: Base64.decode(localStorage.getItem("username")),
      dialogFormVisible:false,
      avatarUrl: "http://220.179.227.205:6009/image/"
    };
  },
  methods: {
    getAvatarUrl() {
      return this.avatarUrl + this.username + ".jpg";
    },
    //选择左边的菜单，给每个选项编号
    changeWindow(index, indexPath) {
      console.log(1, indexPath);
      console.log(2, index);
      switch (index) {
        case "1-1":
          this.choice = 1;
          break;
        case "1-2":
          this.choice = 2;
          break;
        case "1-3":
          this.choice = 3;
          break;
        case "2-1":
          this.choice = 4;
          break;
        case "2-2":
          this.choice = 5;
          break;
        case "2-3":
          this.choice = 6;
          break;
        case "2-4":
          this.choice = 7;
          break;
        case "3-1":
          this.choice = 8;
          break;
        case "3-2":
          this.choice = 9;
          break;
        case "3-3":
          this.choice = 10;
          break;
        case "3-4":
          this.choice = 11;
          break;
        case "3-5":
          this.choice = 12;
          break;
        case "4-1":
          this.choice = 13;
          break;
        case "4-2":
          this.choice = 14;
          break;
        case "4-3":
          this.choice = 15;
          break;
        case "4-4":
          this.choice = 16;
          break;
        case "5":
          this.$router.push("/");
          break;
      }
    },
    getAvatarUrl(){
      return this.avatarUrl + this.username + ".jpg";
    }
  },
};
</script>

<style>
html,
body,
#app,
.el-container {
  /*设置内部填充为0，几个布局元素之间没有间距*/
  padding: 0px;
  /*外部间距也是如此设置*/
  margin: 0px;
  /*统一设置高度为100%*/
  height: 100%;
}

.el-header {
  background-color: #b3c0d1;
  color: #333;
  line-height: 60px;
}

.el-aside {
  color: #333;
}
</style>