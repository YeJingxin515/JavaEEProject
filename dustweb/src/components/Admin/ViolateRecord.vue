// 修改删除单个违规记录，定期清理存放时间很长的违规记录，用watcher—ID获取违规记录
<template>
  <div class="top">
    <div class="search">
      <el-input placeholder="请输入" v-model="searchValue" @change="search">
        <template #prepend>
          <el-select
            v-model="searchSelect"
            placeholder="请选择"
            @change="search"
          >
            <el-option label="投放人ID" value="1"></el-option>
            <el-option label="监察员ID" value="2"></el-option>
          </el-select>
        </template>
        <template #append>
          <el-button icon="el-icon-search"></el-button>
        </template>
      </el-input>
    </div>
  </div>

  <!--主表格部分-->
  <h1>违规记录管理</h1>
  <el-table
    style="width: 100%"
    border
    :cell-style="{ textAlign: 'center' }"
    :header-cell-style="{ textAlign: 'center' }"
    :data="
      tableData.slice((currentPage - 1) * pagesize, currentPage * pagesize)
    "
  >
    <el-table-column label="违规时间" prop="violate_time"> </el-table-column>
    <el-table-column label="垃圾编号" prop="gar_id"> </el-table-column>
    <!-- <el-table-column label="用户编号" prop="user_id"> </el-table-column> -->
    <el-table-column label="惩罚" prop="punishment">
      <template #default="scope">
        <el-tag disable-transitions type="danger" effect="dark">
          {{ scope.row.punishment }}
        </el-tag>
      </template>
    </el-table-column>
    <!-- <el-table-column label="值班员ID" prop="watcher_id"> </el-table-column> -->
    <el-table-column label="原因" prop="reason"> </el-table-column>
    <el-table-column fixed="right" label="操作">
      <template #default="scope">
        <el-button @click="deleteRow(scope.row)" type="text">删除</el-button>
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
import { ElMessage } from "element-plus";

export default {
  data() {
    return {
      oldTime: "1899-11-30T00:00:00", //选择要删除的时间
      tableData: [],
      searchSelect: "1", //搜索模式选择，默认为1
      searchValue: "", //搜索的值即ID
      currentPage: 1,
      pagesize: 10,
    };
  },
  methods: {
    add() {
      fetch(this.$URL + "/ViolateRecord/Add", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: "Bearer " + window.atob(localStorage.getItem("token")),
        },
        body: JSON.stringify(this.form),
      })
        .then((e) => e.json())
        .then((res) => {
          console.log(res);
          this.dialogFormVisible = false;
          this.getData();
        });
    },
    search() {
      this.tableData = [];
      this.getData();
    },
    getData() {
      let url = "";
      switch (this.searchSelect) {
        case "2":
          url = this.$URL + "/ViolateRecord/GetAll?req=" + this.searchValue;
          break;
        case "1":
          url = this.$URL + "/ViolateRecord/Get?req=" + this.searchValue;
          break;
      }

      fetch(url, {
        method: "GET",
        headers: {
          accept: "text/plain",
          Authorization: "Bearer " + window.atob(localStorage.token),
        },
      })
        .then((e) => e.json())
        .then((data) => {
          this.tableData = data;
        });
    },
    deleteRow({ gar_id }) {
      fetch(this.$URL + "/ViolateRecord/Delete?req=" + gar_id, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
          Authorization: "Bearer " + window.atob(localStorage.getItem("token")),
        },
      })
        .then((e) => e.json())
        .then((res) => {
          if (res.status == 1) {
            ElMessage.success({ message: res.deleteMessage, type: "success" });
            this.getData();
          } else {
            ElMessage.error(res.deleteMessage);
            return;
          }
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
    this.getData();
  },
};
</script>
<style scoped>
.top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 30px;
}
.search {
  width: 450px;
}
.del {
  display: flex;
  align-items: center;
}
</style>
