<template>
  <el-header
    style="text-align: right; font-size: 12px; background-color: white"
  >
    <el-button
      icon="el-icon-setting"
      style="margin-right: 15px"
      circle
      @click="dialogFormVisible = 1"
    ></el-button>
    <!--改了一下el-icon的图标（原先是+，改成设置和别的页面统一一下）-->
    <el-button circle id="avatar" @click="choice = 1">
      <el-avatar
        shape="circle"
        fit="cover"
        v-bind:src="getAvatarUrl()"
      ></el-avatar>
    </el-button>
    <span style="font-size: 16px; color: #409eff"
      >欢迎！监察员{{ username }}</span
    >
  </el-header>
  <el-container style="height: 100%; border: 1px solid #eee">
    <el-dialog
      :close-on-click-modal="false"
      title="工作地点"
      :show-close="false"
      v-model="dialogFormVisible"
    >
      <el-form :model="workingStation">
        <el-form-item label="工作站" :label-width="formLabelWidth">
          <el-select v-model="workingStation" placeholder="请选择工作地点">
            <el-option
              v-for="(item, index) in stationData"
              :key="index"
              :label="item.name"
              :value="item.name"
            ></el-option>
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <span class="dialog-footer">
          <el-button type="primary" @click="setWorkingStation">确 定</el-button>
        </span>
      </template>
    </el-dialog>
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
          <template #title><i class="el-icon-menu"></i>记录查看</template>
          <el-menu-item index="2-1">投放记录查看</el-menu-item>
          <el-menu-item index="2-2">违规记录查看</el-menu-item>
        </el-submenu>
        <el-menu-item index="3" route
          ><i class="el-icon-circle-close"></i>退出系统</el-menu-item
        >
      </el-menu>
    </el-aside>

    <el-container>
      <el-main>
        <GetInfo v-if="this.choice === 1"></GetInfo>
        <UpdateInfo v-if="this.choice === 2"></UpdateInfo>
        <UpdatePassword v-if="this.choice === 3"></UpdatePassword>
        <GetRecord v-if="this.choice === 4"></GetRecord>
        <GetViolate v-if="this.choice === 5"></GetViolate>
        <!--          <el-table-column prop="date" label="日期" width="140">-->
        <!--          </el-table-column>-->
        <!--          <el-table-column prop="name" label="姓名" width="120">-->
        <!--          </el-table-column>-->
        <!--          <el-table-column prop="address" label="地址">-->
        <!--          </el-table-column>-->
        <!--        </el-table>-->
      </el-main>
    </el-container>
  </el-container>
</template>

<script>
import GetInfo from "@/components/Watcher/GetInfo";
import UpdateInfo from "@/components/Watcher/UpdateInfo";
import UpdatePassword from "@/components/Watcher/UpdatePassword";
import GetRecord from "@/components/Watcher/getThrowRecord";
import GetViolate from "@/components/Watcher/getViolate";
import { Base64 } from "js-base64";

export default {
  components: {
    GetInfo,
    UpdateInfo,
    UpdatePassword,
    GetRecord,
    GetViolate,
  },
  data() {
    return {
      choice: 0,
      username: Base64.decode(localStorage.getItem("username")),
      workingStation: String(),
      formLabelWidth: "120px",
      stationData: [],
      dialogFormVisible: true,
      avatarUrl: "http://220.179.227.205:6009/image/",
      s_time: String(),
    };
  },
  methods: {
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
        case "3":
          this.$router.push("/");
          break;
      }
    },
    setWorkingStation() {
      if (this.workingStation === "") return;
      localStorage.setItem(
        "workingStation",
        Base64.encode(this.workingStation)
      );
      this.dialogFormVisible = false;
      this.choice = 1;
    },
    getAvatarUrl() {
      return this.avatarUrl + this.username + ".jpg";
    },
  },
  mounted() {
    this.s_time = this.s_time + new Date().getFullYear();
    this.s_time = this.s_time + "-";
    let month = new Date().getMonth();
    if (month < 9) {
      month += 1;
      this.s_time = this.s_time + "0" + month;
    } else {
      month += 1;
      this.s_time = this.s_time + month;
    }
    this.s_time = this.s_time + "-";
    let day = new Date().getDate();
    if (day < 10) this.s_time = this.s_time + "0" + day;
    else this.s_time = this.s_time + day;
    this.s_time = this.s_time + "T";
    let hour = new Date().getHours();
    if (hour < 10) this.s_time = this.s_time + "0" + hour;
    else this.s_time = this.s_time + hour;
    this.s_time = this.s_time + ":";
    let minute = new Date().getMinutes();
    if (minute < 10) this.s_time = this.s_time + "0" + minute;
    else this.s_time = this.s_time + minute;
    this.s_time = this.s_time + ":";
    let second = new Date().getSeconds();
    if (second < 10) this.s_time = this.s_time + "0" + second;
    else this.s_time = this.s_time + second;
    let m_second = ".000Z";
    this.s_time = this.s_time + m_second;
    console.log(this.s_time);
    if (localStorage.getItem("workingStation") === null) {
      this.choice = 0;
      this.dialogFormVisible = 1;
    } else {
      this.choice = 1;
      this.dialogFormVisible = 0;
    }
    fetch(this.$URL + "/Facility/Binsite/GetAll", {
      method: "GET",
      headers: {
        Authorization: "Bearer " + Base64.decode(localStorage.getItem("token")),
      },
    }).then((response) => {
      let result = response.json();
      result.then((result) => {
        console.log(result);
        this.stationData = result;
      });
    });
    setTimeout(() => {
      const req = {
        watcher_id: Base64.decode(localStorage.getItem("username")),
        site_name: Base64.decode(localStorage.getItem("workingStation")),
        start_time: this.s_time,
        end_time: this.s_time,
      };
      fetch(this.$URL + "/User/DutyArrange/Add", {
        method: "POST",
        headers: {
          Authorization:
            "Bearer " + Base64.decode(localStorage.getItem("token")),
          "Content-Type": "application/json",
        },
        body: JSON.stringify(req),
      }).then((response) => {
        let res = response.json();
        console.log(res);
      });
    }, 10000);
  },
  unmounted() {
    const req = this.s_time;
    fetch(this.$URL + "/User/DutyArrange/Update?req=" + req, {
      method: "GET",
      headers: {
        Authorization: "Bearer " + Base64.decode(localStorage.getItem("token")),
      },
    }).then((response) => {
      let res = response.json();
      console.log(res.updateMessage);
    });
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
#avatar {
  width: 40px;
  height: 40px;
  padding: 0 0 0 0;
  position: relative;
  top: 14px;
  right: 10px;
}
</style>
