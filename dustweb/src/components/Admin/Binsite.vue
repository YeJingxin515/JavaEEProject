<template>
  <!--高德地图-->
  <el-dialog title="高德地图" v-model="isShowMap" width="70%">
    <Gaode
      :newaddress="newaddress"
      :dataObj="dataObj"
      @location="location"
    ></Gaode>
  </el-dialog>

  <!--增加垃圾站的-->
  <el-dialog title="新增垃圾站" v-model="dialogFormVisible">
    <!--:close-on-click-modal 问你点外面可不可以关-->
    <!--:show-close 问你需不需要显示x按钮-->
    <el-form>
      <el-form-item label="垃圾站名称" :label-width="formLabelWidth">
        <el-input
          v-model="Binsite.name"
          placeholder="请输入垃圾站名称"
        ></el-input>
      </el-form-item>
      <el-form-item label="垃圾站地点" :label-width="formLabelWidth">
        <el-input v-model="Binsite.location" placeholder="请输入垃圾站地点">
          <template #append>
            <el-button icon="el-icon-search" @click="isShowMap = 1"></el-button>
          </template>
        </el-input>
      </el-form-item>
    </el-form>
    <template #footer>
      <span class="dialog-footer">
        <el-button type="primary" @click="addBinsite">确 定</el-button>
      </span>
    </template>
  </el-dialog>

  <!--查看单个垃圾站属性的界面,本来打算采用这个界面,但是不方便单个垃圾站的操作,所以还是显示在表格中-->
  <!--el-dialog
          title="垃圾站属性查看"
          v-model="dialogVisible"
      >
        <el-form>
          <el-form-item label="垃圾站名称" :label-width="formLabelWidth">
            <span>{{idData.name}}</span>
          </el-form-item>
          <el-form-item label="垃圾站地点" :label-width="formLabelWidth">
            <span>{{idData.location}}</span>
          </el-form-item>
        </el-form>
      </el-dialog-->

  <!--添加与搜索功能-->
  <el-form :inline="true">
    <el-button @click="getData" style="margin: 20px" type="primary" plain
      >所有垃圾站</el-button
    >
    <el-form-item label="新增垃圾站">
      <el-button
        icon="el-icon-plus"
        type="primary"
        style="margin-right: 40px; margin-bottom: 20px"
        plain
        circle
        @click="dialogFormVisible = 1"
      ></el-button>
    </el-form-item>
    <el-form-item label="垃圾站名称">
      <el-select v-model="Name" placeholder="请选择垃圾站" @change="searchName">
        <el-option
          v-for="(item, index) in binsiteData"
          :key="index"
          :label="item.location"
          :value="item.name"
        ></el-option>
      </el-select>
    </el-form-item>
  </el-form>

  <!--主表格部分-->
  <h1>垃圾站管理</h1>
  <el-table
    :data="
      tableData.slice((currentPage - 1) * pagesize, currentPage * pagesize)
    "
    style="width: 100%"
    border
    v-loading="loading"
  >
    <el-table-column type="expand">
      <template #default="props">
        <el-form label-position="left" inline class="demo-table-expand">
          <el-form-item label="垃圾站名称">
            <span>{{ props.row.name }}</span>
          </el-form-item>
          <el-form-item label="垃圾站地点">
            <el-input v-model="Binsite.location" placeholder="请输入垃圾站地点">
              <template #append>
                <el-button
                  icon="el-icon-search"
                  @click="isShowMap = 1"
                ></el-button>
              </template>
            </el-input>
          </el-form-item>
          <el-form-item>
            <el-button type="primary" @click="submitForm(props.row.name)"
              >修改</el-button
            >
          </el-form-item>
        </el-form>
      </template>
    </el-table-column>
    <el-table-column label="垃圾站名称" prop="name"> </el-table-column>
    <el-table-column label="垃圾站地点" prop="location"> </el-table-column>
    <el-table-column fixed="right" label="操作" width="200px">
      <template #default="scope">
        <el-button @click="withdraw(scope.row.name)" type="danger" plain
          >删除</el-button
        >
      </template>
    </el-table-column>
  </el-table>
  <el-pagination
    @size-change="handleSizeChange"
    @current-change="handleCurrentChange"
    :current-page="currentPage"
    :page-size="pagesize"
    layout="total,size,prev,pager,next,jumper"
    :total="tableData.length"
  >
  </el-pagination>
</template>
    
<script>
import { Base64 } from "js-base64";
import Gaode from "./Gaode.vue";

export default {
  components: {
    Gaode,
  },
  data() {
    return {
      formLabelWidth: "120px",

      //传递给高德地图子组件
      newaddress: "",
      dataObj: "",

      Binsite: {
        name: "",
        location: "",
      },
      showDetail: false,
      dialogFormVisible: false,
      isShowMap: false,
      //dialogVisible:false,

      Name: "", //用于搜索的
      tableData: [], //展示的数据
      binsiteData: [], //所有垃圾站数据
      idData: [], //某个垃圾桶的数据
      loading: true, //转圈的标志

      currentPage: 1,
      pagesize: 5,
    };
  },
  methods: {
    location(address, isShow) {
      this.Binsite.location = address;
      this.isShowMap = isShow;
    },

    //删除某个垃圾站
    withdraw(name) {
      const req = name;
      fetch(this.$URL + "/Facility/Binsite/Delete?req=" + req, {
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

    //获取所有垃圾站信息
    getData() {
      fetch(this.$URL + "/Facility/Binsite/GetAll", {
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
          this.binsiteData = result;
          this.loading = false;
        });
      });
    },

    //更新信息
    submitForm(name) {
      console.log(name);
      const req = {
        name: name,
        location: this.Binsite.location,
      };
      fetch(this.$URL + "/Facility/BinSite/Update", {
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
          this.Binsite.location = "";
        });
      });
    },

    //添加垃圾站
    addBinsite() {
      const req = {
        name: this.Binsite.name,
        location: this.Binsite.location,
      };
      fetch(this.$URL + "/Facility/Binsite/Add", {
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
          this.Binsite.name = "";
          this.Binsite.location = "";
        });
      });
    },

    //得到单独的垃圾站
    searchName() {
      //this.dialogVisible=1;
      console.log(this.Name);
      fetch(this.$URL + "/Facility/BinSite/Get?req=" + this.Name, {
        method: "GET",
        headers: {
          Authorization:
            "Bearer " + Base64.decode(localStorage.getItem("token")),
        },
      }).then((response) => {
        let result = response.json();
        result.then((result) => {
          console.log(result);
          this.idData = result;

          this.tableData = [];
          for (let i = 0; i < this.binsiteData.length; i++) {
            //遍历所有垃圾站
            if (this.binsiteData[i].name === this.idData.name) {
              //垃圾站的名称与指定名称比较,相同则加入显示列表tableData
              this.tableData.push(this.binsiteData[i]);
            }
          }
        });
      });
    },
    handleSizeChange(size) {
      this.pagesize = size;
      console.log(this.pagesize);
    },
    handleCurrentChange(currentPage) {
      this.currentPage = currentPage;
      console.log(this.currentPage);
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
    