// 监察员工作记录查看
<template>
  <el-form :inline="true">
    <el-form-item label="查看监察员工作记录">
      <el-autocomplete
        placeholder="请输入监察员编号"
        v-model="state"
        :fetch-suggestions="querySearchAsync"
        @select="handleSelect"
        value-key="watcher_id"
      >
      </el-autocomplete>
      <el-form-item>
        <el-button type="primary" icon="el-icon-search" @click="searchID"
          >搜索</el-button
        >
      </el-form-item>
    </el-form-item>
  </el-form>

  <!--主表格部分-->
  <h1>监察员工作查看</h1>
  <el-table
    :data="idData.slice((currentPage - 1) * pagesize, currentPage * pagesize)"
    style="width: 100%"
    border
    :cell-style="{ textAlign: 'center' }"
    :header-cell-style="{ textAlign: 'center' }"
    v-loading="loading"
  >
    <el-table-column type="index"> </el-table-column>
    <el-table-column label="监察员编号" prop="watcher_id"> </el-table-column>
    <el-table-column label="工作地点" prop="site_name"> </el-table-column>
    <el-table-column label="工作开始时间" prop="start_time"> </el-table-column>
    <el-table-column label="工作结束时间" prop="end_time"> </el-table-column>
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
      WorkRecord: {
        watcher_id: "",
        site_name: "",
        start_time: "",
        end_time: "",
      },
      ID: "",
      idData: [], //某个人的工作记录
      allData: [], //所有的工作记录
      watchersData: ref([]), //远程搜索用
      state: ref(""),
      timeout: null,
      currentPage: 1,
      pagesize: 5,
      loading: true,
    };
  },
  methods: {
    //得到全部工作记录
    getData() {
      const url = this.$URL + "/User/DutyArrange/GetAll";
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

    //得到监察者工作记录
    searchID() {
      console.log(this.state);
      fetch(this.$URL + "/User/DutyArrange/Get?req=" + this.state, {
        method: "GET",
        headers: {
          Authorization:
            "Bearer " + Base64.decode(localStorage.getItem("token")),
        },
      }).then((response) => {
        let result = response.json();
        console.log(result);
        result.then((result) => {
          console.log("test", result);
          this.idData = result;
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
    querySearchAsync(queryString, cb) {
      var watchersData = this.watchersData;
      var results = queryString
        ? watchersData.watcher_id.filter(createFilter(queryString))
        : watchersData.watcher_id;
      clearTimeout(this.timeout);
      this.timeout = setTimeout(() => {
        cb(results);
      }, 3000 * Math.random());
    },
    createFilter(queryString) {
      return (state) => {
        return (
          state.watcher_id.toLowerCase().indexOf(queryString.toLowerCase()) ===
          0
        );
      };
    },
    handleSelect(item) {
      console.log(item);
      this.idData = item;
    },
    loadAll() {
      return this.allData;
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
    setTimeout(() => {
      this.watchersData.watcher_id = this.loadAll();
    }, 2000);
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
  