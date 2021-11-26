<template>
  <!--增加垃圾桶的-->
  <el-dialog title="新增垃圾桶" v-model="dialogFormVisible">
    <!--:close-on-click-modal 问你点外面可不可以关-->
    <!--:show-close 问你需不需要显示x按钮-->
    <el-form>
      <el-form-item label="垃圾桶编号" :label-width="formLabelWidth">
        <el-input
          v-model="Trashcan.id"
          placeholder="请输入垃圾桶编号"
        ></el-input>
      </el-form-item>
      <el-form-item label="垃圾桶类型" :label-width="formLabelWidth">
        <el-select v-model="Trashcan.type" placeholder="请选择垃圾桶类型">
          <el-option
            v-for="item in types"
            :key="item.value"
            :label="item.label"
            :value="item.value"
          ></el-option>
        </el-select>
      </el-form-item>
      <el-form-item label="垃圾桶状态" :label-width="formLabelWidth">
        <el-select v-model="Trashcan.condition" placeholder="请选择垃圾桶状态">
          <el-option
            v-for="item in states"
            :key="item.value"
            :label="item.label"
            :value="item.value"
          ></el-option>
        </el-select>
      </el-form-item>
      <el-form-item label="所属垃圾站" :label-width="formLabelWidth">
        <el-select v-model="Trashcan.siteName" placeholder="请选择所属垃圾站">
          <el-option
            v-for="(item, index) in stationData"
            :key="index"
            :label="item.name"
            :value="item.name"
          ></el-option>
        </el-select>
      </el-form-item>
    </el-form>
    <template #footer>
      <span class="dialog-footer">
        <el-button type="primary" @click="addTrashcan">确 定</el-button>
      </span>
    </template>
  </el-dialog>

  <!--查看单个垃圾桶属性的界面,本来打算采用这个界面,但是不方便单个垃圾桶的操作,所以还是显示在表格中-->
  <!--el-dialog
        title="垃圾桶属性查看"
        v-model="dialogVisible"
    >
      <el-form>
        <el-form-item label="垃圾桶编号" :label-width="formLabelWidth">
          <span>{{idData.id}}</span>
        </el-form-item>
        <el-form-item label="垃圾桶类型" :label-width="formLabelWidth">
          <span>{{idData.type}}</span>
        </el-form-item>
        <el-form-item label="垃圾桶状态" :label-width="formLabelWidth">
          <span>{{idData.condition}}</span>
        </el-form-item>
        <el-form-item label="所属垃圾站" :label-width="formLabelWidth">
          <span>{{idData.siteName}}</span>
        </el-form-item>
      </el-form>
    </el-dialog-->

  <!--添加与搜索功能-->

  <el-form :inline="true">
    <el-button @click="getData" type="primary" style="margin:20px;" plain>所有垃圾桶</el-button>
    <el-form-item label="新增垃圾桶">
      <el-button
        icon="el-icon-plus"
        style="margin-right: 40px; margin-bottom: 20px"
        type="primary"
        plain
        circle
        @click="dialogFormVisible = 1"
      ></el-button>
    </el-form-item>
    <el-form-item label="垃圾桶编号" style="margin:20px;">
      <el-select v-model="ID" placeholder="请选择垃圾桶编号" @change="searchID">
        <el-option
          v-for="(item, index) in trashcanData"
          :key="index"
          :label="item.id"
          :value="item.id"
        ></el-option>
      </el-select>
    </el-form-item>
    <el-form-item label="所属垃圾站">
      <el-select
        v-model="sitename"
        placeholder="请选择所属垃圾站"
        @change="searchSite"
      >
        <el-option
          v-for="(item, index) in stationData"
          :key="index"
          :label="item.name"
          :value="item.name"
        ></el-option>
      </el-select>
    </el-form-item>
  </el-form>
  <h1>垃圾桶管理</h1>
  <!--主表格部分-->
  <el-table :data="tableData" style="width: 100%" border v-loading="loading">
    <el-table-column type="expand">
      <template #default="props">
        <el-form label-position="left" inline class="demo-table-expand">
          <el-form-item label="垃圾桶编号">
            <span>{{ props.row.id }}</span>
          </el-form-item>
          <el-form-item label="垃圾桶状态" :label-width="formLabelWidth">
            <el-select
              v-model="Trashcan.condition"
              placeholder="请选择垃圾桶状态"
            >
              <el-option
                v-for="item in states"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              ></el-option>
            </el-select>
          </el-form-item>
          <el-form-item label="所属垃圾站" :label-width="formLabelWidth">
            <el-select
              v-model="Trashcan.siteName"
              placeholder="请选择所属垃圾站"
            >
              <el-option
                v-for="(item, index) in stationData"
                :key="index"
                :label="item.name"
                :value="item.name"
              ></el-option>
            </el-select>
          </el-form-item>
          <el-form-item>
            <el-button type="primary" @click="submitForm(props.row.id)"
              >提交</el-button
            >
          </el-form-item>
        </el-form>
      </template>
    </el-table-column>
    <el-table-column label="垃圾桶编号" prop="id"> </el-table-column>
    <el-table-column label="垃圾桶类型" prop="type">
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
    <el-table-column
      label="垃圾桶状态"
      prop="condition"
      :formatter="stateFormat"
    >
      <template #default="scope">
        <el-tag
          :type="
            scope.row.condition === 'P'
              ? 'success'
              : scope.row.condition === 'B'
              ? 'warning'
              : scope.row.condition === 'D'
              ? 'danger'
              : ''
          "
          disable-transitions
          effect="dark"
        >
          {{
            scope.row.condition === "P"
              ? "正常"
              : scope.row.condition === "B"
              ? "异常"
              : scope.row.condition === "D"
              ? "损坏"
              : ""
          }}
        </el-tag>
      </template>
    </el-table-column>
    <el-table-column label="所属垃圾站" prop="siteName"> </el-table-column>
    <el-table-column fixed="right" label="操作">
      <template #default="scope">
        <el-button
          @click="
            withdraw(
              scope.row.id,
              scope.row.type,
              scope.row.condition,
              scope.row.siteName
            )
          "
          type="danger"
          plain
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
    layout="total,sizes,prev,pager,next,jumper"
    :total="trashcanData.length"
  ></el-pagination>
</template>
  
  <script>
import { Base64 } from "js-base64";

export default {
  data() {
    return {
      formLabelWidth: "120px",
      Trashcan: {
        id: "",
        type: "",
        condition: "",
        siteName: "",
      },
      showDetail: false,
      dialogFormVisible: false,
      //dialogVisible:false,
      loading: true,
      ID: "",
      sitename: "",
      tableData: [], //展示的数据
      trashcanData: [], //真正的所有垃圾桶数据
      idData: [], //某个垃圾桶的数据

      currentPage: 1,
      pagesize: 10,

      types: [
        {
          value: "干垃圾",
          label: "干垃圾",
        },
        {
          value: "湿垃圾",
          label: "湿垃圾",
        },
        {
          value: "可回收垃圾",
          label: "可回收垃圾",
        },
        {
          value: "有害垃圾",
          label: "有害垃圾",
        },
      ],
      states: [
        {
          //垃圾桶状态的选项集合
          value: "P",
          label: "正常",
        },
        {
          value: "B",
          label: "异常",
        },
        {
          value: "D",
          label: "损坏",
        },
      ],
      stationData: [], //垃圾桶所属垃圾站的选项集合
    };
  },
  methods: {
    //获取所有垃圾桶信息
    getData() {
      const url = this.$URL + "/Facility/TrashCan/GetAll";
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
          this.trashcanData = data;
        });
      });
    },

    //删除某个垃圾桶
    withdraw(id, type, condition, siteName) {
      const req = {
        id: id,
        type: type,
        condition: condition,
        siteName: siteName,
      };
      fetch(this.$URL + "/Facility/TrashCan/Delete", {
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
    getStation() {
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
          this.stationData = result;
          this.loading = false;
        });
      });
    },

    //更新信息
    submitForm(id) {
      console.log(id);
      console.log(this.Trashcan.condition);
      const req = {
        id: id,
        type: this.Trashcan.type,
        condition: this.Trashcan.condition,
        siteName: this.Trashcan.siteName,
      };
      fetch(this.$URL + "/Facility/TrashCan/Update", {
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
          this.Trashcan.condition = "";
          this.Trashcan.type = "";
          this.Trashcan.siteName = "";
        });
      });
    },

    //添加垃圾桶
    addTrashcan() {
      const req = {
        id: this.Trashcan.id,
        type: this.Trashcan.type,
        condition: this.Trashcan.condition,
        siteName: this.Trashcan.siteName,
      };
      fetch(this.$URL + "/Facility/TrashCan/Add", {
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
          this.Trashcan.id = "";
          this.Trashcan.condition = "";
          this.Trashcan.type = "";
          this.Trashcan.siteName = "";
        });
      });
    },

    //得到单独的垃圾桶
    searchID() {
      //this.dialogVisible=1;
      console.log(this.ID);
      fetch(this.$URL + "/Facility/TrashCan/Get?req=" + this.ID, {
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
          for (let i = 0; i < this.trashcanData.length; i++) {
            //遍历所有垃圾桶
            if (this.trashcanData[i].id === this.idData.id) {
              //垃圾桶的id与指定id比较,相同则加入显示列表tableData
              this.tableData.push(this.trashcanData[i]);
            }
          }
        });
      });
    },

    //垃圾站搜索
    searchSite() {
      console.log(this.sitename);
      fetch(this.$URL + "/Facility/TrashCan/GetSiteAll?req=" + this.sitename, {
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
    handleSizeChange(size) {
      this.pagesize = size;
      console.log(this.pagesize);
    },
    handleCurrentChange(currentPage) {
      this.currentPage = currentPage;
      console.log(this.currentPage);
    },
    stateFormat(row) {
      if (row.condition == "B") return "异常";
      else if (row.condition == "D") return "损坏";
      else if (row.condition == "P") return "正常";
    },
    tagType(row) {
      if (row.type == "干垃圾") return "info";
      else if (row.type == "湿垃圾") return "warning";
      else if (row.type == "可回收物") return "primary";
      else if (row.type == "有害垃圾") return "danger";
    },
  },
  mounted() {
    console.log(1);
    console.log(localStorage.token);
    console.log(Base64.decode(localStorage.username));
    this.getData();
    this.getStation();
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
  