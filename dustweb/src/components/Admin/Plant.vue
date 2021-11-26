<template>
  <el-dialog title="新增处理站" v-model="dialogFormVisible">
    <el-form>
      <el-form-item label="处理站名称" :label-width="formLabelWidth">
        <el-input
          v-model="Plant.name"
          placeholder="请输入处理站名称"
        ></el-input>
      </el-form-item>
      <el-form-item label="处理站地点" :label-width="formLabelWidth">
        <el-input
          v-model="Plant.address"
          placeholder="请输入处理站地点"
        ></el-input>
      </el-form-item>
    </el-form>
    <template #footer>
      <span class="dialog-footer">
        <el-button type="primary" @click="addPlant">确 定</el-button>
      </span>
    </template>
  </el-dialog>
  <!--增加、搜索功能-->

  <el-form :inline="true">
    <el-button @click="getData" style="margin: 20px" type="primary" plain>所有处理站</el-button>
    <el-form-item label="新增处理站">
      <el-button
        icon="el-icon-plus"
        style="margin-right: 40px; margin-bottom: 20px"
        type="primary"
        plain
        circle
        @click="dialogFormVisible = 1"
      ></el-button>
    </el-form-item>
    <el-form-item label="处理站名称">
      <el-select v-model="Name" placeholder="请选择处理站" @change="searchName">
        <el-option
          v-for="(item, index) in plantData"
          :key="index"
          :label="item.address"
          :value="item.name"
        ></el-option>
      </el-select>
    </el-form-item>
  </el-form>

  <!--主表格-->
  <h1>处理站管理</h1>
  <el-table :data="tableData" style="width: 100%" border v-loading="loading">
    <el-table-column type="expand">
      <template #default="props">
        <el-form label-position="left" inline class="demo-table-expand">
          <el-form-item label="处理站名称">
            <span>{{ props.row.name }}</span>
          </el-form-item>
          <el-form-item label="处理站地点" :label-width="formLabelWidth">
            <el-input
              v-model="Plant.address"
              placeholder="请输入处理站地点"
            ></el-input>
          </el-form-item>
          <el-form-item>
            <el-button type="primary" @click="submitForm(props.row.name)"
              >修改</el-button
            >
          </el-form-item>
        </el-form>
      </template>
    </el-table-column>
    <el-table-column label="处理站名称" prop="name"> </el-table-column>
    <el-table-column label="处理站地点" prop="address"> </el-table-column>
    <el-table-column fixed="right" label="操作" width="200px">
      <template #default="scope">
        <el-button
          @click="withdraw(scope.row.name, scope.row.address)"
          type="danger"
          plain
          >删除</el-button
        >
      </template>
    </el-table-column>
  </el-table>
</template>
    
    <script>
import { Base64 } from "js-base64";

export default {
  data() {
    return {
      formLabelWidth: "120px",
      Plant: {
        name: "",
        address: "",
      },
      showDetail: false,
      dialogFormVisible: false,
      //dialogVisible:false,

      Name: "", //用于搜索的
      tableData: [], //展示的数据
      plantData: [], //所有处理站数据
      idData: [], //某个垃圾桶的数据
      loading: true,
    };
  },
  methods: {
    //删除某个处理站
    withdraw(name, address) {
      const req = {
        name: name,
        address: address,
      };
      fetch(this.$URL + "/Facility/Plant/Delete", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization:
            "Bearer " + Base64.decode(localStorage.getItem("token")),
        },
        body: JSON.stringify(req),
      }).then((response) => {
        let result = response.json();
        result.then((res) => {
          console.log(res);
          this.getData();
        });
      });
    },

    //获取所有处理站信息
    getData() {
      fetch(this.$URL + "/Facility/Plant/GetAll", {
        method: "GET",
        headers: {
          Authorization:
            "Bearer " + Base64.decode(localStorage.getItem("token")),
        },
      }).then((response) => {
        let result = response.json();
        result.then((result) => {
          console.log(result);
          this.tableData = result;
          this.plantData = result;
          this.loading = false;
        });
      });
    },

    //更新信息
    submitForm(name) {
      console.log(name);
      const req = {
        name: name,
        address: this.Plant.address,
      };
      fetch(this.$URL + "/Facility/Plant/Update", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization:
            "Bearer " + Base64.decode(localStorage.getItem("token")),
        },
        body: JSON.stringify(req),
      }).then((response) => {
        let result = response.json();
        result.then((res) => {
          console.log(res);
          this.getData();
          this.Plant.address = "";
        });
      });
    },

    //添加处理站
    addPlant() {
      const req = {
        name: this.Plant.name,
        address: this.Plant.address,
      };
      fetch(this.$URL + "/Facility/Plant/Add", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization:
            "Bearer " + Base64.decode(localStorage.getItem("token")),
        },
        body: JSON.stringify(req),
      }).then((response) => {
        let result = response.json();
        result.then((res) => {
          console.log(res);
          this.dialogFormVisible = false;
          this.getData();
          this.Plant.name = "";
          this.Plant.address = "";
        });
      });
    },

    //得到单独的处理站
    searchName() {
      //this.dialogVisible=1;
      console.log(this.Name);
      fetch(this.$URL + "/Facility/Plant/Get?req=" + this.Name, {
        method: "GET",
        headers: {
          Authorization:
            "Bearer " + Base64.decode(localStorage.getItem("token")),
        },
      }).then((response) => {
        let result = response.json();
        result.then((result) => {
          console.log(result);
          this.idData = result; //单独垃圾桶数据

          this.tableData = [];
          for (let i = 0; i < this.plantData.length; i++) {
            //遍历所有处理站
            if (this.plantData[i].name === this.idData.name) {
              this.tableData.push(this.plantData[i]);
            }
          }
        });
      });
    },
  },
  mounted() {
    console.log(1);
    console.log(localStorage.token);
    console.log(Base64.decode(localStorage.username));
    this.getData();
  },
};
</script>
    
    <style>
.demo-table-expand {
  font-size: 0;
}

.demo-table-expand label {
  width: 90px;
  color: #99a9bf;
}

.demo-table-expand .el-form-item {
  margin-right: 0;
  margin-bottom: 10px;
  width: 50%;
}

.el-select {
  width: 160px;
}
</style>
    