<template>
  <h1>投放记录查看</h1>
  <el-table
    :data="
      tableData.slice((currentPage - 1) * pagesize, currentPage * pagesize)
    "
    :cell-style="{ textAlign: 'center' }"
    :header-cell-style="{ textAlign: 'center' }"
    style="width: 100%"
    border
    v-loading="loading"
  >
    <el-table-column type="expand">
      <template #default="props">
        <el-form label-position="left" inline class="demo-table-expand">
          <el-form-item label="垃圾编号">
            <span>{{ props.row.gar_id }}</span>
          </el-form-item>
          <el-form-item label="垃圾类型">
            <span>{{ props.row.gar_type }}</span>
          </el-form-item>
          <el-form-item label="垃圾桶">
            <span>{{ props.row.dustbin_id }}</span>
          </el-form-item>
          <el-form-item label="投递时间">
            <span>{{ props.row.throw_time }}</span>
          </el-form-item>
        </el-form>
      </template>
    </el-table-column>
    <el-table-column label="垃圾编号" prop="gar_id"> </el-table-column>
    <el-table-column label="垃圾桶" prop="dustbin_id"> </el-table-column>
    <el-table-column label="投递时间" prop="throw_time"> </el-table-column>
    <el-table-column fixed="right" label="操作" width="100">
      <template #default="scope">
        <el-button @click="withdraw(scope.row.gar_id)" type="text"
          >撤回</el-button
        >
      </template>
    </el-table-column>
  </el-table>
  <el-pagination
    @size-change="handleSizeChange"
    @current-change="handleCurrentChange"
    :current-page="currentPage"
    :page-size="pagesize"
    :page-sizes="[5, 10, 20, 40]"
    layout="total,sizes,prev,pager,next,jumper"
    :total="tableData.length"
  ></el-pagination>
</template>

<script>
import { Base64 } from "js-base64";

export default {
  data() {
    return {
      ThrowRecord: {
        user_id: "1952108",
        gar_id: "1234567",
        gar_type: "干垃圾",
        dustbin_id: "D001",
        throw_time: "2021-07-19T17:22:36",
      },
      showDetail: false,
      tableData: [],
      currentPage: 1,
      pagesize: 5,
      loading: true,
    };
  },
  methods: {
    getData() {
      const url =
        this.$URL +
        "/Throw/GetThrowRecord?req=" +
        Base64.decode(localStorage.getItem("workingStation"));
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
          this.tableData = data;
          this.loading = false;
        });
      });
    },
    withdraw(req) {
      fetch(this.$URL + "/Throw/Delete?req=" + req, {
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
          this.getData();
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
  margin-bottom: 0;
  width: 50%;
}
</style>
