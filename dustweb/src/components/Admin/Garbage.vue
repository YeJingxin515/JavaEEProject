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
            <el-option label="垃圾站ID" value="2"></el-option>
          </el-select>
        </template>
        <template #append>
          <el-button icon="el-icon-search"></el-button>
        </template>
      </el-input>
    </div>
  </div>

  <!--主表格部分-->
  <h1>投递记录管理</h1>
  <el-table
    :data="
      tableData.slice((currentPage - 1) * pagesize, currentPage * pagesize)
    "
    style="width: 100%"
    border
    :cell-style="{ textAlign: 'center' }"
    :header-cell-style="{ textAlign: 'center' }"
    v-show="searchValue.length === 7"
  >
    <el-table-column label="垃圾运输编号" prop="gar_id"></el-table-column>
    <el-table-column label="投放人ID" prop="user_id"></el-table-column>
    <el-table-column label="垃圾类型" prop="type">
      <template #default="scope">
        <el-tag
          :type="
            scope.row.type === '干垃圾'
              ? 'info'
              : scope.row.type === '湿垃圾'
              ? 'success'
              : scope.row.type === '可回收垃圾'
              ? 'primary'
              : scope.row.type === '有害垃圾'
              ? 'danger'
              : ''
          "
          disable-transitions
          effect="dark"
        >
          {{ scope.row.type }}
        </el-tag>
      </template>
    </el-table-column>
    <el-table-column label="垃圾桶编号" prop="dustbin_id"> </el-table-column>
    <el-table-column label="投递时间" prop="latest_time"> </el-table-column>
    <el-table-column fixed="right" label="操作">
      <template #default="scope">
        <el-button @click="deleteRow(scope.row)" type="primary" plain
          >删除</el-button
        >
      </template>
    </el-table-column>
  </el-table>
  <el-table
    :data="
      tableData.slice((currentPage - 1) * pagesize, currentPage * pagesize)
    "
    style="width: 100%"
    border
    :cell-style="{ textAlign: 'center' }"
    :header-cell-style="{ textAlign: 'center' }"
    v-show="this.searchValue.length === 3"
  >
    <el-table-column label="垃圾运输编号" prop="gar_id"></el-table-column>
    <el-table-column label="垃圾类型" prop="gar_type">
      <template #default="scope">
        <el-tag
          :type="
            scope.row.gar_type === '干垃圾'
              ? 'info'
              : scope.row.gar_type === '湿垃圾'
              ? 'success'
              : scope.row.gar_type === '可回收垃圾'
              ? 'primary'
              : scope.row.gar_type === '有害垃圾'
              ? 'danger'
              : ''
          "
          disable-transitions
          effect="dark"
        >
          {{ scope.row.gar_type }}
        </el-tag>
      </template>
    </el-table-column>
    <el-table-column label="垃圾桶编号" prop="dustbin_id"> </el-table-column>
    <el-table-column label="投递时间" prop="throw_time"> </el-table-column>
    <el-table-column fixed="right" label="操作">
      <template #default="scope">
        <el-button @click="deleteRow(scope.row)" type="danger" plain
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
    :page-sizes="[5, 10, 20, 40]"
    layout="total,sizes,prev,pager,next,jumper"
    :total="tableData.length"
  ></el-pagination>
</template>

<script>
import { ElMessage } from "element-plus";
import { Base64 } from "js-base64";
export default {
  data() {
    return {
      oldTime: "",
      tableData: [],
      searchSelect: "1",
      searchValue: "",
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
    add() {
      fetch(this.$URL + "/Garbage/Add", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization:
            "Bearer " + Base64.decode(localStorage.getItem("token")),
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
        case "1":
          url = this.$URL + "/Garbage/GetAll?req=" + this.searchValue;
          break;
        case "2":
          url = this.$URL + "/Throw/GetThrowRecord?req=" + this.searchValue;
          break;
      }

      fetch(url, {
        method: "GET",
        headers: {
          accept: "text/plain",
          Authorization:
            "Bearer " + Base64.decode(localStorage.getItem("token")),
        },
      })
        .then((e) => e.json())
        .then((data) => {
          this.tableData = data;
          console.log(this.tableData);
        });
    },
    deleteRow({ gar_id }) {
      fetch(this.$URL + "/Garbage/Delete?req=" + gar_id, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization:
            "Bearer " + Base64.decode(localStorage.getItem("token")),
        },
      })
        .then((e) => e.json())
        .then((res) => {
          if (res.status !== 1) {
            ElMessage.error(res.deleteMessage);
            return;
          }
          this.getData();
        });
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
