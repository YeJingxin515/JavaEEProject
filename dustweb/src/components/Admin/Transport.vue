//按照垃圾站查找运输记录
<template>
  <!--添加与搜索功能-->
  <el-form :inline="true">
    <el-button @click="getData" style="margin: 20px">所有运输记录</el-button>
    <el-form-item label="根据处理站筛选">
      <el-select v-model="Name" placeholder="请选择处理站" @change="searchName">
        <el-option
          v-for="(item, index) in binsiteData"
          :key="index"
          :label="item.name"
          :value="item.name"
        ></el-option>
      </el-select>
    </el-form-item>
  </el-form>

  <!--主表格部分-->
  <h1>运输记录查看</h1>
  <el-table
    :data="
      tableData.slice((currentPage - 1) * pagesize, currentPage * pagesize)
    "
    style="width: 100%"
    border
    :cell-style="{ textAlign: 'center' }"
    :header-cell-style="{ textAlign: 'center' }"
  >
    <el-table-column label="垃圾运输编号" prop="trans_id"> </el-table-column>
    <el-table-column label="垃圾桶编号" prop="dustbin_id"> </el-table-column>
    <el-table-column label="垃圾车编号" prop="truck_id"> </el-table-column>
    <el-table-column label="处理站编号" prop="plant_name"> </el-table-column>
    <el-table-column label="运输员ID" prop="carrier_id"> </el-table-column>
    <el-table-column label="开始时间" prop="start_time"> </el-table-column>
    <el-table-column label="结束时间" prop="end_time"> </el-table-column>
  </el-table>
  <el-pagination
    @size-change="handleSizeChange"
    @current-change="handleCurrentChange"
    :current-page="currentPage"
    :page-sizes="[5, 10, 20, 40]"
    :page-size="pagesize"
    layout="total,sizes,prev,pager,next,jumper"
    :total="tableData.length"
  ></el-pagination>
</template>
    
    <script>
import { Base64 } from "js-base64";

export default {
  data() {
    return {
      formLabelWidth: "120px",
      Binsite: {
        name: "",
        location: "",
      },
      showDetail: false,
      dialogFormVisible: false,
      //dialogVisible:false,

      Name: "", //用于搜索的
      tableData: [], //展示的数据
      binsiteData: [], //所有垃圾站数据
      idData: [], //某个垃圾桶的数据
      currentPage: 1,
      pagesize: 5,
    };
  },
  methods: {
    //获取所有垃圾站信息
    getData() {
      fetch(this.$URL + "/Transport/GetAll", {
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
        });
      });
    },
    getAllPlants() {
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
          this.binsiteData = result;
        });
      });
    },
    //对应垃圾站的运输记录
    searchName() {
      //this.dialogVisible=1;
      console.log(this.Name);
      fetch(this.$URL + "/Transport/StaffGet?req=" + this.Name, {
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

          this.tableData = this.idData;
          console.log(this.tableData);
        });
      });
    },
    handleSizeChange: function (size) {
      this.pagesize = size;
      console.log(this.pagesize);
    },
    handleCurrentChange: function (currentPage) {
      this.currentPage = currentPage;
      console.log(this.currentPage);
    },
  },
  mounted() {
    console.log(1);
    console.log(localStorage.token);
    console.log(Base64.decode(localStorage.username));
    this.getAllPlants();
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
    