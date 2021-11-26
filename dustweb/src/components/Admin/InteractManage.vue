// 监察员工作记录查看
<template>
  <!--搜索功能-->
  <el-alert title="搜索失败" type="error" show-icon center v-if="searchID == 1">
  </el-alert>
  <el-form :inline="true">
    <el-form-item label="查看交互记录">
      <el-autocomplete
        placeholder="请运输员编号"
        v-model="ID"
        :fetch-suggestions="querySearchAsync"
        @select="handleSelect"
      >
      </el-autocomplete>
      <el-form-item>
        <el-button type="primary" icon="el-icon-search" @click="searchID()"
          >搜索</el-button
        >
      </el-form-item>
    </el-form-item>
  </el-form>

  <!--主表格部分-->
  <h1>交互记录管理</h1>
  <el-table
    style="width: 100%"
    border
    :cell-style="{ textAlign: 'center' }"
    :header-cell-style="{ textAlign: 'center' }"
    :data="idData.slice((currentPage - 1) * pagesize, currentPage * pagesize)"
    v-loading="loading"
  >
    <el-table-column type="index"> </el-table-column>
    <el-table-column label="处理员编号" prop="staff_id"> </el-table-column>
    <el-table-column label="垃圾运输编号" prop="trans_id"> </el-table-column>
    <el-table-column label="交接时间" prop="interact_time"> </el-table-column>
    <el-table-column label="交接结果" prop="interact_result">
      <template #default="scope">
        <el-tag
          disable-transitions
          :type="scope.row.interact_result == 'F' ? 'danger' : 'success'"
        >
          {{ scope.row.interact_result == "F" ? "未完成" : "已完成" }}
        </el-tag>
      </template>
    </el-table-column>
  </el-table>
  <el-pagination
    @size-change="handleSizeChange"
    @current-change="handleCurrentChange"
    :current-page="currentPage"
    :page-sizes="[5, 10, 20, 40]"
    :page-size="pagesize"
    layout="total,sizes,prev,pager,next,jumper"
    :total="idData.length"
  ></el-pagination>
</template>
  
  <script>
import { Base64 } from "js-base64";
import { defineComponent, ref } from "vue";

export default {
  components: { defineComponent },
  data() {
    return {
      formLabelWidth: "120px",
      Interact: {
        trans_id: "s",
        staff_id: "",
        interact_time: "",
        interact_result: "",
      },
      ID: "",
      allData: [], //所有的交互记录
      idData: [], //某人的交互记录
      currentPage: 1,
      pagesize: 5,
      loading: true,
    };
  },
  methods: {
    //得到某人交互记录
    searchID() {
      console.log(this.ID);
      fetch(this.$URL + "/Interact/Get?req=" + this.ID, {
        method: "GET",
        headers: {
          accept: "text/plain",
          Authorization:
            "Bearer " + Base64.decode(localStorage.getItem("token")),
        },
      }).then((response) => {
        let result = response.json();
        result.then((result) => {
          console.log(result);
          this.idData = result;
        });
      });
    },
    //得到全部工作记录
    getData() {
      const url = this.$URL + "/Interact/GetAll";
      fetch(url, {
        method: "GET",
        headers: {
          accept: "text/plain",
          Authorization: "Bearer " + Base64.decode(localStorage.token),
        },
      }).then((response) => {
        console.log(response);
        let result = response.json();
        result.then((data) => {
          console.log(data);
          this.idData = data;
          this.allData = data;
          this.loading = false;
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

.el-input {
  width: 160px;
}
</style>
  