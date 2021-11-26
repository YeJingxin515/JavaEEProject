<template>
  <h1>查看违规记录</h1>
  <el-table
    :cell-style="{ textAlign: 'center' }"
    :header-cell-style="{ textAlign: 'center' }"
    :data="
      tableData.slice((currentPage - 1) * pagesize, currentPage * pagesize)
    "
    style="width: 100%"
  >
    <el-table-column type="expand">
      <template #default="props">
        <el-form label-position="left" inline class="demo-table-expand">
          <el-form-item label="垃圾编号">
            <span>{{ props.row.gar_id }}</span>
          </el-form-item>
          <el-form-item label="记录人">
            <span>{{ props.row.watcher_id }}</span>
          </el-form-item>
          <!--          <el-form-item label="检查员">-->
          <!--            <span>{{ props.row.id }}</span>-->
          <!--          </el-form-item>-->
          <el-form-item label="原因">
            <span>{{ props.row.reason }}</span>
          </el-form-item>
          <el-form-item label="处罚">
            <el-tag disable-transitions effect="dark" type="danger">{{
              props.row.punishment
            }}</el-tag>
          </el-form-item>
          <el-form-item label="违规时间">
            <span>{{ props.row.violate_time }}</span>
          </el-form-item>
        </el-form>
      </template>
    </el-table-column>
    <el-table-column label="垃圾编号" prop="gar_id"> </el-table-column>
    <el-table-column label="处罚" prop="punishment">
      <template #default="scope">
        <el-tag disable-transitions effect="dark" type="danger">{{
          scope.row.punishment
        }}</el-tag>
      </template>
    </el-table-column>
    <el-table-column label="违规时间" prop="violate_time"> </el-table-column>
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
        gar_id: "11111116",
        user_id: "",
        watcher_id: "2952108",
        reason: "234",
        punishment: 2,
        violate_time: "2021-07-21T17:43:19",
      },
      showDetail: false,
      tableData: [],
      currentPage: 1,
      pagesize: 5,
    };
  },
  methods: {
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
    const url =
      this.$URL +
      "/ViolateRecord/Get?req=" +
      Base64.decode(localStorage.username);
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
      });
    });
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
