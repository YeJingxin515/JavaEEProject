<template>
  <el-container class="outer" direction="vertical">
    <div class="form">
      <div class="login-form">
        <transition name="el-zoom-in-bottom">
          <div v-show="showLoginFormVisible">
            <el-container>
              <el-row justify="center" align="middle">
                <el-row
                  justify="center"
                  style="
                    width: 300px;
                    background: rgba(255, 255, 255, 0.8);
                    border-radius: 20px;
                    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.12),
                      0 0 6px rgba(0, 0, 0, 0.04);
                  "
                >
                  <el-row justify="center" style="height: 300px" align="bottom">
                    <el-form
                      :model="LoginForm"
                      status-icon
                      :rules="rules"
                      ref="LoginForm"
                      label-width="0px"
                      class="LoginForm"
                    >
                      <h1 style="font-size: 30px">登录</h1>
                      <el-form-item prop="username">
                        <el-input
                          type="username"
                          v-model="LoginForm.username"
                          autocomplete="on"
                          placeholder="用户名"
                        ></el-input>
                      </el-form-item>
                      <el-form-item prop="password">
                        <el-input
                          type="password"
                          v-model="LoginForm.password"
                          autocomplete="on"
                          placeholder="密码"
                        ></el-input>
                      </el-form-item>
                      <el-form-item>
                        <el-button
                          type="primary"
                          plain
                          @click="submitForm('LoginForm')"
                          >提交</el-button
                        >
                        <el-button plain @click="resetForm('LoginForm')"
                          >重置</el-button
                        >
                      </el-form-item>
                    </el-form>
                  </el-row>
                </el-row>
              </el-row>
            </el-container>
          </div>
        </transition>
      </div>

      <div class="login-form">
        <transition name="el-zoom-in-bottom">
          <div v-show="showRegisterFormVisible">
            <el-container>
              <el-row justify="center" align="middle">
                <el-row
                  justify="center"
                  style="
                    width: 300px;
                    background: rgba(255, 255, 255, 0.8);
                    border-radius: 20px;
                    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.12),
                      0 0 6px rgba(0, 0, 0, 0.04);
                  "
                >
                  <el-row justify="center" style="height: 400px" align="bottom">
                    <el-form
                      :model="RegisterForm"
                      status-icon
                      :rules="rules"
                      ref="RegisterForm"
                      label-width="0px"
                      class="RegisterForm"
                    >
                      <h1 style="font-size: 30px">注册</h1>
                      <el-form-item prop="userID">
                        <el-input
                          type="userID"
                          v-model="RegisterForm.userID"
                          autocomplete="on"
                          placeholder="ID"
                        ></el-input>
                      </el-form-item>
                      <el-form-item prop="password">
                        <el-input
                          type="password"
                          v-model="RegisterForm.password"
                          autocomplete="on"
                          placeholder="密码"
                        ></el-input>
                      </el-form-item>
                      <el-form-item>
                        <el-select
                          v-model="registerType"
                          placeholder="请选择注册身份"
                        >
                          <el-option
                            v-for="item in type_options"
                            :key="item.value"
                            :label="item.label"
                            :value="item.value"
                          >
                          </el-option>
                        </el-select>
                      </el-form-item>
                      <el-form-item>
                        <el-button
                          type="primary"
                          plain
                          @click="
                            submitRegisterForm('RegisterForm', registerType)
                          "
                          >提交</el-button
                        >
                        <el-button plain @click="resetForm('RegisterForm')"
                          >重置</el-button
                        >
                      </el-form-item>
                    </el-form>
                  </el-row>
                </el-row>
              </el-row>
            </el-container>
          </div>
        </transition>
      </div>

      <div class="btn">
        <el-button
          type="primary"
          plain
          round
          @click="
            showLoginFormVisible = !showLoginFormVisible;
            sloganVisible = !showLoginFormVisible;
            showRegisterFormVisible = false;
          "
          >登录</el-button
        >
        <el-button
          plain
          round
          @click="
            showRegisterFormVisible = !showRegisterFormVisible;
            sloganVisible = !showRegisterFormVisible;
            showLoginFormVisible = false;
          "
          >注册</el-button
        >
      </div>
    </div>
    <div style="position: absolute; z-index: 2; right: 7%; top: 12%">
      <img
        src="./../assets/logo_and_brand.png"
        alt="logo_and_brand"
        style="width: 400px; height: 70px"
      />
    </div>
    <transition name="el-fade-in">
      <div id="container" v-show="sloganVisible">
        <span id="text1"></span>
        <span id="text2"></span>
      </div>
    </transition>
    <svg id="filters">
      <defs>
        <filter id="threshold">
          <!-- Basically just a threshold effect - pixels with a high enough opacity are set to full opacity, and all other pixels are set to completely transparent. -->
          <feColorMatrix
            in="SourceGraphic"
            type="matrix"
            values="1 0 0 0 0
                      0 1 0 0 0
                      0 0 1 0 0
                      0 0 0 255 -140"
          />
        </filter>
      </defs>
    </svg>
    <div style="position: absolute; left: 0; bottom: 0; z-index: 1">
      <img
        src="./../assets/index_pic.png"
        style="width: 900px; height: 680px"
      />
    </div>
  </el-container>
</template>
<script>
import { Base64 } from "js-base64";

export default {
  data() {
    let validateUsername = (rule, value, callback) => {
      if (value === "") {
        callback(new Error("请输入用户名"));
      } else {
        callback();
      }
    };
    let validatePass = (rule, value, callback) => {
      if (value === "") {
        callback(new Error("请输入密码"));
      } else {
        callback();
      }
    };
    let validateUserID = (rule, value, callback) => {
      if (value === "") {
        callback(new Error("请输入要注册的学号/工号"));
      } else {
        callback();
      }
    };
    return {
      LoginForm: {
        username: "",
        password: "",
      },
      RegisterForm: {
        userID: "",
        password: "",
        type: "",
      },
      rules: {
        username: [{ validator: validateUsername, trigger: "blur" }],
        password: [{ validator: validatePass, trigger: "blur" }],
        userID: [{ validator: validateUserID, trigger: "blur" }],
      },
      LoginStatus: 0,
      RegisterStatus: 0,
      showLoginFormVisible: false,
      showRegisterFormVisible: false,
      sloganVisible: true,
      registerType: "",

      type_options: [
        { label: "投放人", value: "garbageman" },
        { label: "值班员", value: "watcher" },
        { label: "运输员", value: "carrier" },
        { label: "处理员", value: "stationstaff" },
      ],
    };
  },
  methods: {
    submitForm(formName) {
      this.$refs[formName].validate((valid) => {
        if (valid) {
          //console.log(this.$store.state.URL)
          const req = {
            userID: this.LoginForm.username,
            password: this.LoginForm.password,
          };
          fetch(this.$URL + "/User/Login", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(req),
          }).then((response) => {
            let result = response.json();
            result.then((res) => {
              if (res.status === 1) {
                this.LoginStatus = 1;
                this.$alert("登陆成功。", "提示信息", {
                  confirmButtonText: "确定",
                });
                localStorage.setItem(
                  "username",
                  Base64.encode(this.LoginForm.username)
                );
                localStorage.setItem(
                  "password",
                  Base64.encode(this.LoginForm.password)
                );
                localStorage.setItem("token", Base64.encode(res.token));
                switch (res.role) {
                  case "Administrator":
                    this.$router.push("/Admin");
                    break;
                  case "GarbageMan":
                    this.$router.push("/GarbageMan");
                    break;
                  case "Watcher":
                    this.$router.push("/Watcher");
                    break;
                  case "Carrier":
                    this.$router.push("/Carrier");
                    break;
                  case "StationStaff":
                    this.$router.push("/StationStaff");
                    break;
                }
              } else {
                this.LoginStatus = -1;
                this.$alert("登陆失败，请检查您的用户名和密码。", "提示信息", {
                  confirmButtonText: "确定",
                });
              }
              console.log(
                res.token,
                this.LoginForm.username,
                this.LoginForm.password
              );
            });
          });
        } else {
          console.log("error submit!!");
          return false;
        }
      });
    },
    submitRegisterForm(formName, TypeName) {
      this.$refs[formName].validate((valid) => {
        if (valid) {
          //console.log(this.$store.state.URL)
          const reqG = {
              garbageman: {
                id: this.RegisterForm.userID,
                name: "请修改姓名",
                password: this.RegisterForm.password,
                phonenumber: "1xxyyyyzzzz",
                credit: 0,
                address: "同济大学",
              },
            },
            reqW = {
              watcher: {
                id: this.RegisterForm.userID,
                name: "请修改姓名",
                password: this.RegisterForm.password,
                phonenumber: "1xxyyyyzzzz",
                address: "同济大学",
              },
            },
            reqC = {
              transportpersonnel: {
                id: this.RegisterForm.userID,
                name: "请修改姓名",
                password: this.RegisterForm.password,
                phonenumber: "1xxyyyyzzzz",
              },
            },
            reqS = {
              stationstaff: {
                id: this.RegisterForm.userID,
                name: "请修改姓名",
                password: this.RegisterForm.password,
                phonenumber: "1xxyyyyzzzz",
                plantname: "P01",
              },
            };
          switch (TypeName) {
            case "garbageman":
              fetch(this.$URL + "/User/Register/GarbageMan", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(reqG),
              }).then((response) => {
                let result = response.json();
                result.then((res) => {
                  if (res.status === 1) {
                    this.RegisterStatus = 1;
                    console.log("register 1");
                    this.$alert("账号注册成功，请登录。", "提示信息", {
                      confirmButtonText: "确定",
                      callback: (action) => {
                        this.$message({
                          type: "success",
                          message: `${"已跳转至登录界面，请您登陆。"}`,
                        });
                      },
                    });
                    this.resetForm(formName);
                    this.showLoginFormVisible = true;
                    this.showRegisterFormVisible = false;
                  } else {
                    this.RegisterStatus = -1;
                  }
                  console.log(
                    res.status,
                    this.RegisterForm.userID,
                    this.RegisterForm.password
                  );
                });
              });
              break;
            case "watcher":
              fetch(this.$URL + "/User/Register/Watcher", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(reqW),
              }).then((response) => {
                let result = response.json();
                result.then((res) => {
                  if (res.status === 1) {
                    this.RegisterStatus = 1;
                    console.log("register 2");
                    this.$alert("账号注册成功，请登录。", "提示信息", {
                      confirmButtonText: "确定",
                      callback: (action) => {
                        this.$message({
                          type: "success",
                          message: `${"已跳转至登录界面，请您登陆。"}`,
                        });
                      },
                    });
                    this.resetForm(formName);
                    this.showLoginFormVisible = true;
                    this.showRegisterFormVisible = false;
                  } else {
                    this.RegisterStatus = -1;
                  }
                  console.log(
                    res.status,
                    this.RegisterForm.userID,
                    this.RegisterForm.password
                  );
                });
              });
              break;
            case "carrier":
              fetch(this.$URL + "/User/Register/Carrier", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(reqC),
              }).then((response) => {
                let result = response.json();
                result.then((res) => {
                  if (res.status === 1) {
                    this.RegisterStatus = 1;
                    console.log("register 3");
                    this.$alert("账号注册成功，请登录。", "提示信息", {
                      confirmButtonText: "确定",
                      callback: (action) => {
                        this.$message({
                          type: "success",
                          message: `${"已跳转至登录界面，请您登陆。"}`,
                        });
                      },
                    });
                    this.resetForm(formName);
                    this.showLoginFormVisible = true;
                    this.showRegisterFormVisible = false;
                  } else {
                    this.RegisterStatus = -1;
                  }
                  console.log(
                    res.status,
                    this.RegisterForm.userID,
                    this.RegisterForm.password
                  );
                });
              });
              break;
            case "stationstaff":
              fetch(this.$URL + "/User/Register/StationStaff", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(reqS),
              }).then((response) => {
                let result = response.json();
                result.then((res) => {
                  if (res.status === 1) {
                    this.RegisterStatus = 1;
                    console.log("register 4");
                    this.$alert("账号注册成功，请登录。", "提示信息", {
                      confirmButtonText: "确定",
                      callback: (action) => {
                        this.$message({
                          type: "success",
                          message: `${"已跳转至登录界面，请您登陆。"}`,
                        });
                      },
                    });
                    this.resetForm(formName);
                    this.showLoginFormVisible = true;
                    this.showRegisterFormVisible = false;
                  } else {
                    this.RegisterStatus = -1;
                  }
                  console.log(
                    res.status,
                    this.RegisterForm.userID,
                    this.RegisterForm.password
                  );
                });
              });
              break;
          }
        } else {
          console.log("error submit!!");
          return false;
        }
      });
    },
    resetForm(formName) {
      this.$refs[formName].resetFields();
    },
  },
  mounted() {
    console.log(this.LoginStatus);
    console.log(this.RegisterStatus);
    this.LoginStatus = 0;
    this.RegisterStatus = 0;
    localStorage.clear();

    const elts = {
      text1: document.getElementById("text1"),
      text2: document.getElementById("text2"),
    };

    const texts = ["For", "our", "planet,", "for", "a", "better", "future."];

    // Controls the speed of morphing.
    const morphTime = 1;
    const cooldownTime = 0.25;

    let textIndex = texts.length - 1;
    let time = new Date();
    let morph = 0;
    let cooldown = cooldownTime;

    elts.text1.textContent = texts[textIndex % texts.length];
    elts.text2.textContent = texts[(textIndex + 1) % texts.length];

    function doMorph() {
      morph -= cooldown;
      cooldown = 0;

      let fraction = morph / morphTime;

      if (fraction > 1) {
        cooldown = cooldownTime;
        fraction = 1;
      }
      setMorph(fraction);
    }

    // A lot of the magic happens here, this is what applies the blur filter to the text.
    function setMorph(fraction) {
      // fraction = Math.cos(fraction * Math.PI) / -2 + .5;

      elts.text2.style.filter = `blur(${Math.min(8 / fraction - 8, 100)}px)`;
      elts.text2.style.opacity = `${Math.pow(fraction, 0.4) * 100}%`;

      fraction = 1 - fraction;
      elts.text1.style.filter = `blur(${Math.min(8 / fraction - 8, 100)}px)`;
      elts.text1.style.opacity = `${Math.pow(fraction, 0.4) * 100}%`;

      elts.text1.textContent = texts[textIndex % texts.length];
      elts.text2.textContent = texts[(textIndex + 1) % texts.length];
    }

    function doCooldown() {
      morph = 0;

      elts.text2.style.filter = "";
      elts.text2.style.opacity = "100%";

      elts.text1.style.filter = "";
      elts.text1.style.opacity = "0%";
    }

    // Animation loop, which is called every frame.
    function animate() {
      requestAnimationFrame(animate);
      let newTime = new Date();
      let shouldIncrementIndex = cooldown > 0;
      let dt = (newTime - time) / 1000;
      time = newTime;

      cooldown -= dt;

      if (cooldown <= 0) {
        if (shouldIncrementIndex) {
          textIndex++;
        }
        doMorph();
      } else {
        doCooldown();
      }
    }

    // Start the animation.
    animate();
  },
};
</script>
<style>
body,
html {
  margin: 0px;
  padding: 0px;
  height: 100%;
  width: 100%;
}

.outer {
  height: 100%;
  width: 100%;
  background-color: #89c2ed;
}

.form {
  height: 100%;
  width: 300px;
  position: absolute;
  right: 10%;
  z-index: 2;
}

.login-form {
  position: relative;
  right: 0;
  top: 30%;
}

.btn {
  display: flex;
  flex-direction: row;
  position: absolute;
  top: 82%;
  left: 69px;
}

@import url("https://fonts.googleapis.com/css?family=Raleway:900&display=swap");

#container {
  position: absolute;
  top: 40%;
  right: 20%;
  z-index: 2;
  width: 400px;
  height: 50px;
  filter: url(#threshold) blur(0.6px);
}

#text1,
#text2 {
  position: absolute;
  width: 100%;
  display: inline-block;
  font-family: "Raleway", sans-serif;
  font-size: 80pt;
  /* text-align: center; */
  user-select: none;
  /* color: white; */
}

.form {
  height: 100%;
  width: 300px;
  position: absolute;
  right: 10%;
  z-index: 2;
}

.login-form {
  position: relative;
  right: 0;
  top: 30%;
}

.btn {
  display: flex;
  flex-direction: row;
  position: absolute;
  top: 82%;
  left: 69px;
}

@import url("https://fonts.googleapis.com/css?family=Raleway:900&display=swap");

#container {
  position: absolute;
  top: 40%;
  right: 20%;
  z-index: 2;
  width: 400px;
  height: 50px;
  filter: url(#threshold) blur(0.6px);
}

#text1,
#text2 {
  position: absolute;
  width: 100%;
  display: inline-block;
  font-family: "Raleway", sans-serif;
  font-size: 80pt;
  /* text-align: center; */
  user-select: none;
  /* color: white; */
}
</style>