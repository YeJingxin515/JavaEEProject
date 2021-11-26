<template>
  <!--查看单个投放者的界面-->
  <el-dialog title="您查找的投放者信息" v-model="dialogVisible">
    <el-form>
      <el-form-item label="投放者编号" :label-width="formLabelWidth">
        <span>{{ idData.id }}</span>
      </el-form-item>
      <el-form-item label="姓名" :label-width="formLabelWidth">
        <span>{{ idData.name }}</span>
      </el-form-item>
      <el-form-item label="信誉积分" :label-width="formLabelWidth">
        <span>{{ idData.credit }}</span>
      </el-form-item>
      <el-form-item label="地址" :label-width="formLabelWidth">
        <span>{{ idData.address }}</span>
      </el-form-item>
      <el-form-item label="电话" :label-width="formLabelWidth">
        <span>{{ idData.phonenumber }}</span>
      </el-form-item>
    </el-form>
  </el-dialog>

  <!--搜索功能-->
  <el-form :inline="true">
    <el-form-item label="查询投放者">
      <el-autocomplete
        placeholder="请输入投放者编号"
        v-model="state"
        :fetch-suggestions="querySearch"
        @select="handleSelect"
        value-key="id"
      >
      </el-autocomplete>
      <el-form-item>
        <el-button type="primary" icon="el-icon-search" @click="searchID(state)"
          >搜索</el-button
        >
      </el-form-item>
    </el-form-item>
  </el-form>

  <!-- 修改密码 -->
  <el-dialog :model="form" title="修改该用户密码" v-model="editPasswordVisible">
    <el-form :model="form" ref="editPassForm">
      <el-form-item label="密码" :label-width="formLabelWidth">
        <el-input v-model="Thrower.password" clearable></el-input>
      </el-form-item>
      <el-form-item>
        <el-button @click="editPasswordVisible = false">取 消</el-button>
        <el-button @click="submitForm()">确 定</el-button>
      </el-form-item>
    </el-form>
  </el-dialog>

  <!-- 修改信息 -->
  <el-dialog title="修改该用户个人信息" v-model="editInfoVisible">
    <el-form :model="editForm" ref="editInfoFormRef">
      <el-form-item label="编号" :label-width="formLabelWidth">
        <el-input v-model="idData.id" disabled></el-input>
      </el-form-item>
      <el-form-item label="姓名" :label-width="formLabelWidth">
        <el-input v-model="idData.name" clearable></el-input>
      </el-form-item>
      <el-form-item label="信誉积分" :label-width="formLabelWidth">
        <el-input v-model="idData.credit" disabled></el-input>
      </el-form-item>
      <el-form-item label="地址" :label-width="formLabelWidth">
        <el-input v-model="idData.address" clearable></el-input>
      </el-form-item>
      <el-form-item label="电话" :label-width="formLabelWidth">
        <el-input v-model="idData.phonenumber" clearable></el-input>
      </el-form-item>
      <el-form-item>
        <el-button @click="editInfoVisible = false">取 消</el-button>
        <el-button @click="submitForm()">确 定</el-button>
      </el-form-item>
    </el-form>
  </el-dialog>

  <!--主表格部分-->
  <h1>投放人信息</h1>
  <el-table
    :data="
      tableData.slice((currentPage - 1) * pagesize, currentPage * pagesize)
    "
    style="width: 100%"
    :cell-style="{ textAlign: 'center' }"
    :header-cell-style="{ textAlign: 'center' }"
    border
  >
    <el-table-column type="index"> </el-table-column>
    <el-table-column label="投放者编号" prop="id"> </el-table-column>
    <el-table-column label="姓名" prop="name"> </el-table-column>
    <el-table-column label="信誉积分" prop="credit"> </el-table-column>
    <el-table-column label="地址" prop="address"> </el-table-column>
    <el-table-column label="电话" prop="phonenumber"> </el-table-column>
    <el-table-column fixed="right" label="操作" width="200px">
      <template #default="scope">
        <el-button
          type="danger"
          icon="el-icon-delete"
          circle
          @click="withdraw(scope.row.id)"
        >
        </el-button>
        <el-button
          type="warning"
          icon="el-icon-edit"
          circle
          @click="editInfo(scope.row.id)"
        ></el-button>
        <el-button
          type="success"
          icon="el-icon-key"
          circle
          @click="editPassword(scope.row.id)"
        ></el-button>
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
import { ref } from "@vue/reactivity";
export default {
  data() {
    return {
      formLabelWidth: "120px",
      Thrower: {
        id: "",
        name: "",
        credit: "",
        address: "",
        phonenumber: "",
        password: "",
      },
      dialogVisible: false, //查看个人弹窗
      editInfoVisible: false, //修改个人信息弹窗的显示与隐藏
      editPasswordVisible: false, //修改密码弹窗
      ID: "", //用于搜索
      index: [], //用于记录一条信息
      tableData: [], //展示的数据
      allData: [], //真正的所有投放者数据
      idData: [], //某个投放者的数据
      persons: ref([]),
      state: ref(""),
      timeout: null,
      currentPage: 1,
      pagesize: 5,
    };
  },

  methods: {
    //获取所有投放者信息
    loadAll() {
      return this.allData;
    },
    querySearch(queryString, cb) {
      var persons = this.persons;
      var results = queryString
        ? persons.id.filter(this.createStateFilter(queryString))
        : persons.id;

      clearTimeout(this.timeout);
      this.timeout = setTimeout(() => {
        cb(results);
      }, 3000 * Math.random());
    },
    createStateFilter(queryString) {
      return (state) => {
        return state.id.toLowerCase().indexOf(queryString.toLowerCase()) === 0;
      };
    },
    handleSelect(item) {
      console.log(item);
      this.idData = item;
    },
    getData() {
      const url = this.$URL + "/User/GetInformation/AllGarbageMan";
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
          this.allData = data;
        });
      });
    },

    //修改信息
    editInfo(id) {
      this.editInfoVisible = 1;
      this.searchID(id);
      this.dialogVisible = false;
    },

    //修改密码
    editPassword(id) {
      this.editPasswordVisible = 1;
      this.searchID(id);
      this.dialogVisible = false;
    },

    //提交修改信息
    submitForm() {
      this.editInfoVisible = false;
      this.editPasswordVisible = false;
      const req = {
        id: this.idData.id,
        name: this.idData.name,
        credit: this.idData.credit,
        password: this.idData.password,
        address: this.idData.address,
        phonenumber: this.idData.phonenumber,
      };
      fetch(this.$URL + "/User/Update/GarbageMan", {
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
        this.$message({ message: "修改成功", type: "success" });
      });
    },

    //删除某个投放者
    withdraw(id) {
      const req = {
        id: id,
      };
      this.$confirm("此操作将永久删除该用户, 是否继续?", "提示", {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning",
      })
        .then(() => {
          fetch(this.$URL + "/User/Delete?req=" + id, {
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
          this.$message({
            type: "success",
            message: "删除成功!",
          });
        })
        .catch(() => {
          this.$message({
            type: "info",
            message: "已取消删除",
          });
        });
    },

    //得到单独的投放者
    searchID(id) {
      this.dialogVisible = 1;
      const req = {
        id: id,
      };
      fetch(this.$URL + "/User/GetInformation/GarbageMan?req=" + id, {
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
          console.log(this.idData);
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
    console.log(Base64.decode(localStorage.username));
    this.getData();
    setTimeout(() => {
      this.persons.id = this.loadAll();
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
  