<template>
  <!--增加垃圾桶的-->
  <el-dialog title="新增垃圾车" v-model="dialogFormVisible">
    <!--:close-on-click-modal 问你点外面可不可以关-->
    <!--:show-close 问你需不需要显示x按钮-->
    <el-form>
      <el-form-item label="垃圾车编号" :label-width="formLabelWidth">
        <el-input v-model="Truck.id" placeholder="请输入垃圾车编号"></el-input>
      </el-form-item>
      <el-form-item label="垃圾车状态" :label-width="formLabelWidth">
        <el-select v-model="Truck.condition" placeholder="请选择垃圾车状态">
          <el-option
            v-for="item in states"
            :key="item.value"
            :label="item.label"
            :value="item.value"
          ></el-option>
        </el-select>
      </el-form-item>
      <el-form-item label="运输员编号" :label-width="formLabelWidth">
        <el-select v-model="Truck.carrierID" placeholder="请选择运输员编号">
          <el-option
            v-for="(item, index) in carrierData"
            :key="index"
            :label="item.id"
            :value="item.id"
          ></el-option>
        </el-select>
      </el-form-item>
    </el-form>
    <template #footer>
      <span class="dialog-footer">
        <el-button type="primary" @click="addTruck">确 定</el-button>
      </span>
    </template>
  </el-dialog>

  <!--查看单个垃圾桶属性的界面,本来打算采用这个界面,但是不方便单个垃圾车的操作,所以还是显示在表格中-->
  <!--el-dialog
          title="垃圾车属性查看"
          v-model="dialogVisible"
      >
        <el-form>
          <el-form-item label="垃圾车编号" :label-width="formLabelWidth">
            <span>{{idData.id}}</span>
          </el-form-item>
          <el-form-item label="垃圾车状态" :label-width="formLabelWidth">
            <span>{{idData.condition}}</span>
          </el-form-item>
          <el-form-item label="运输员编号" :label-width="formLabelWidth">
            <span>{{idData.carrierID}}</span>
          </el-form-item>
        </el-form>
      </el-dialog-->

  <!--添加与搜索功能-->
  <el-form :inline="true">
    <el-button @click="getFree" style="margin: 20px" type="primary" plain
      >空闲垃圾车</el-button
    >
    <el-button @click="getData" style="margin: 20px" type="primary" plain
      >所有垃圾车</el-button
    >
    <el-form-item label="新增垃圾车">
      <el-button
        icon="el-icon-plus"
        style="margin-right: 40px; margin-bottom: 20px"
        type="primary"
        plain
        circle
        @click="dialogFormVisible = 1"
      ></el-button>
    </el-form-item>
    <el-form-item label="垃圾车编号">
      <el-select v-model="ID" placeholder="请选择垃圾车编号" @change="searchID">
        <el-option
          v-for="(item, index) in truckData"
          :key="index"
          :label="item.id"
          :value="item.id"
        ></el-option>
      </el-select>
    </el-form-item>
  </el-form>

  <!--主表格部分-->
  <h1>垃圾车管理</h1>
  <el-table
    :data="
      tableData.slice((currentPage - 1) * pagesize, currentPage * pagesize)
    "
    style="width: 100%"
    border
    v-loading="loading"
  >
    <el-table-column type="expand">
      <template #default="props">
        <el-form label-position="left" inline class="demo-table-expand">
          <el-form-item label="垃圾车编号">
            <span>{{ props.row.id }}</span>
          </el-form-item>
          <el-form-item label="垃圾车状态">
            <el-select v-model="Truck.condition" placeholder="请选择垃圾车状态">
              <el-option
                v-for="item in states"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              ></el-option>
            </el-select>
          </el-form-item>
          <el-form-item label="运输员编号">
            <!--el-select v-model="Truck.carrierID" placeholder="请选择运输员编号">
                  <el-option v-for="(item,index) in carrierData" :key="index" :label="item.id"
                            :value="item.id"></el-option>
                </el-select=-->
            <span>{{ props.row.carrierID }}</span>
          </el-form-item>
          <el-form-item>
            <el-button type="primary" @click="submitForm(props.row.id)"
              >提交</el-button
            >
          </el-form-item>
        </el-form>
      </template>
    </el-table-column>
    <el-table-column label="垃圾车编号" prop="id"> </el-table-column>
    <el-table-column label="垃圾车状态" prop="condition">
      <template #default="scope">
        <el-tag
          disable-transitions
          :type="
            scope.row.condition === 'D'
              ? 'danger'
              : scope.row.condition === 'F'
              ? 'success'
              : ''
          "
          effect="dark"
        >
          {{
            scope.row.condition === "D"
              ? "损坏"
              : scope.row.condition === "F"
              ? "正常"
              : ""
          }}
        </el-tag>
      </template>
    </el-table-column>
    <el-table-column label="运输员编号" prop="carrierID"> </el-table-column>
    <el-table-column fixed="right" label="操作">
      <template #default="scope">
        <el-button
          @click="
            withdraw(scope.row.id, scope.row.condition, scope.row.carrierID)
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
    layout="total,size,prev,pager,next,jumper"
    :total="tableData.length"
  >
  </el-pagination>
</template>
    
    <script>
import { Base64 } from "js-base64";

export default {
  data() {
    return {
      formLabelWidth: "120px",
      Truck: {
        id: "",
        condition: "",
        carrierID: "",
      },
      showDetail: false,
      dialogFormVisible: false,
      //dialogVisible:false,

      ID: "",
      tableData: [], //展示的数据
      truckData: [], //真正的所有垃圾车数据
      idData: [], //某个垃圾车的数据
      idFree: [], //空余垃圾车的编号
      loading: true,
      states: [
        {
          //垃圾桶状态的选项集合
          value: "F",
          label: "正常",
        },
        {
          value: "D",
          label: "损坏",
        },
      ],
      carrierData: [], //运输员的选项集合
      currentPage: 1,
      pagesize: 5,
    };
  },
  methods: {
    //获取所有垃圾桶信息
    getData() {
      const url = this.$URL + "/Facility/Truck/GetAll";
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
          this.truckData = data;
          this.loading = false;
        });
      });
    },

    //删除某个垃圾车
    withdraw(id, condition, carrierID) {
      const req = {
        id: id,
        condition: condition,
        carrierID: carrierID,
      };
      fetch(this.$URL + "/Facility/Truck/Delete", {
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

    //获取所有运输员信息
    getCarrier() {
      fetch(this.$URL + "/User/GetInformation/AllCarrier", {
        method: "GET",
        headers: {
          Authorization:
            "Bearer " + Base64.decode(localStorage.getItem("token")),
        },
      }).then((response) => {
        let result = response.json();
        result.then((result) => {
          console.log(result);
          this.carrierData = result;
        });
      });
    },

    //更新信息
    submitForm(id) {
      console.log(id);
      const req = {
        id: id,
        condition: this.Truck.condition,
        carrierID: this.Truck.carrierID,
      };
      fetch(this.$URL + "/Facility/Truck/Update", {
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
          this.Truck.condition = "";
          this.Truck.carrierID = "";
        });
      });
    },

    //添加垃圾车
    addTruck() {
      const req = {
        id: this.Truck.id,
        condition: this.Truck.condition,
        carrierID: this.Truck.carrierID,
      };
      fetch(this.$URL + "/Facility/Truck/Add", {
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
          this.Truck.id = "";
          this.Truck.condition = "";
          this.Truck.carrierID = "";
        });
      });
    },

    //得到单独的垃圾车
    searchID() {
      //this.dialogVisible=1;
      console.log(this.ID);
      fetch(this.$URL + "/Facility/Truck/Get?req=" + this.ID, {
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
          for (let i = 0; i < this.truckData.length; i++) {
            //遍历所有垃圾车
            if (this.truckData[i].id === this.idData.id) {
              //垃圾车的id与指定id比较,相同则加入显示列表tableData
              this.tableData.push(this.truckData[i]);
            }
          }
        });
      });
    },

    //得到空余垃圾桶
    getFree() {
      fetch(this.$URL + "/Facility/Truck/GetFree", {
        method: "GET",
        headers: {
          Authorization:
            "Bearer " + Base64.decode(localStorage.getItem("token")),
        },
      }).then((response) => {
        let result = response.json();
        result.then((result) => {
          console.log(result);
          this.idFree = result;
          this.tableData = [];
          for (let i = 0; i < this.truckData.length; i++) {
            //遍历所有垃圾车
            console.log(this.truckData[i].id);
            if (this.idFree.indexOf(this.truckData[i].id) >= 0) {
              //垃圾车的id与空闲垃圾车的id比较,相同则加入显示列表tableData
              this.tableData.push(this.truckData[i]);
            }
          }
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
    this.getCarrier();
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