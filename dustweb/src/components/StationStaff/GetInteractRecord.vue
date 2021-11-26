<template>
  <h1>垃圾交接记录</h1>
  <el-table
    v-loading="loading"
    border
    :data="table.slice((currentPage - 1) * pagesize, currentPage * pagesize)"
    height="500"
    :cell-style="{ textAlign: 'center' }"
    :header-cell-style="{ textAlign: 'center' }"
  >
    <el-table-column label="运输编号" prop="trans_id"> </el-table-column>
    <el-table-column label="交接时间" prop="interact_time"> </el-table-column>
    <el-table-column
      label="处理结果"
      prop="interact_result"
      :formatter="interact_finish"
      :filters="[
        { text: '未完成', value: 'F' },
        { text: '已完成', value: 'S' },
      ]"
      :filter-method="filterHandle"
      filter-placement="bottom-end"
      filter-multiple="false"
    >
      <template #default="scope">
        <el-tag
          v-if="scope.row.interact_result == 'F'"
          type="success"
          effect="dark"
          >未完成</el-tag
        >
        <el-tag v-else type="danger" effect="dark">已完成</el-tag>
      </template>
    </el-table-column>
    <el-table-column label="处理操作">
      <template #default="scope">
        <el-button
          type="primary"
          plain
          :disabled="scope.row.interact_result != 'F'"
          @click="deal(scope.row)"
          >处理</el-button
        >
      </template>
    </el-table-column>
  </el-table>
  <el-pagination
    layout="prev, pager, next"
    :total="total"
    @current-change="current_change"
  >
  </el-pagination>
</template>

<script>
import { Base64 } from "js-base64";
export default {
  methods: {
    current_change: function (currentPage) {
      this.currentPage = currentPage;
    },
    interact_finish: function (row) {
      return row.interact_result == "F" ? "未完成" : "已完成";
    },
    deal(row) {
      const req = row.trans_id;
      fetch(this.$URL + "/Interact/Update?req=" + req, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
          Authorization:
            "Bearer " + Base64.decode(localStorage.getItem("token")),
        },
      }).then((response) => {
        let result = response.json();
        result.then((res) => {
          this.$alert(
            "运输编号 " + row.trans_id + " 垃圾已处理!",
            res.updateMessage
          );
        });
      });
      this.getInteract();
    },
    getInteract() {
      const req = Base64.decode(localStorage.getItem("username"));
      fetch(this.$URL + "/Interact/Get?req=" + req, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
          Authorization:
            "Bearer " + Base64.decode(localStorage.getItem("token")),
        },
      }).then((response) => {
        let result = response.json();
        result.then((res) => {
          console.log(res);
          this.table = res;
          this.total = res.length;
          this.loading = false;
        });
      });
    },
    filterHandle(value, row) {
      return row.interact_result == value;
    },
  },
  mounted() {
    this.getInteract();
  },
  data() {
    return {
      table: [],
      loading: true,
      total: 0,
      pagesize: 10,
      currentPage: 1,
    };
  },
};
</script>

<style>
</style>