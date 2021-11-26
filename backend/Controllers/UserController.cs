using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using DBPractice.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Oracle.ManagedDataAccess.Client;
using System.Drawing;

// ReSharper disable InterpolatedStringExpressionIsNotIFormattable
namespace DBPractice.Controllers
{
    /// <summary>
    /// 登录
    /// 返回JWT token
    /// </summary>
    [ApiController]
    [Route("User/[controller]")]
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="configuration"></param>
        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// image 类
        /// </summary>
        public class image
        {
            public string prefix { get; set; }
            public string content { get; set; }
        }


        private enum Roles {GarbageMan,Watcher,Carrier,StationStaff,Administrator}

        /// <summary>
        /// 登录用户id以第一位区分角色
        /// 1:GarbageMan;
        /// 2:Watcher;
        /// 3:Carrier;
        /// 4:StationStaff;
        /// 5:Administrator
        /// </summary>
        [AllowAnonymous]
        [HttpPost]
        public LoginResponse Login([FromBody] LoginRequest req)
        {
            using (var conn = new DB())
            {
                var resp = new LoginResponse { token = "", role = "", status = Config.FAIL, loginMessage = "登录失败，用户名或密码错误" };
                if (string.IsNullOrEmpty(req.userID) || string.IsNullOrEmpty(req.password))
                {
                    resp.status = Config.EMPTY;
                    return resp;
                }

                string[] commands =
                {
                $"SELECT user_password FROM alluser WHERE user_id=:id",
                $"SELECT watcher_password FROM watcher WHERE watcher_id=:id",
                $"SELECT carrier_password FROM carrier WHERE carrier_id=:id",
                $"SELECT staff_password FROM staff WHERE staff_id=:id",
                $"SELECT adm_password FROM administrator WHERE adm_id  =:id"
            };
                var role = (Roles)(req.userID[0] - '1');
                if ((int)role < 0 || (int)role > 4)
                    return resp;                
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = commands[(int)role];
                    //添加参数               
                    cmd.Parameters.Add(":id", req.userID);
                    //执行
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        if (DBConn.MD5Encrypt16(req.password) == reader.GetString(0))
                        {
                            resp.token = GenerateJWT(req.userID, role.ToString());
                            resp.status = Config.SUCCESS;
                            resp.role = role.ToString();
                            resp.loginMessage = "登录成功";
                        }
                    }
                    reader.Close();                    

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }                
                return resp;
            }
        }

        /// <summary>
        /// 图片上传API
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Upload")]
        [Authorize(Roles = "GarbageMan,Watcher,Carrier,StationStaff,Administrator")]
        public void Upload([FromBody] image req)
        {
            var path = Config.IMAGEPATH;
            if (HttpContext.User.Identity == null) return;
            var strpath = path + HttpContext.User.Identity.Name + '.' + req.prefix;
            Base64ToImg(req.content.Split(',')[1]).Save(strpath);
        }

        private static Bitmap Base64ToImg(string base64Code)
        {
            var stream = new MemoryStream(Convert.FromBase64String(base64Code));
            return new Bitmap(stream);
        }

 

        private string GenerateJWT(string username, string role)
        {
            const string algorithm = SecurityAlgorithms.HmacSha256;
            var claims = new[]
            {
                //new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.Name, username),
            };
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));
            var signingCredentials = new SigningCredentials(secretKey, algorithm);
            var token = new JwtSecurityToken(
                _configuration["JWT:Issuer"], //Issuer
                _configuration["JWT:Audience"], //Audience
                claims, //Claims,
                DateTime.Now, //notBefore
                DateTime.Now.AddDays(1), //expires
                signingCredentials
            );
            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return jwtToken;
        }
    }

    /// <summary>
    /// 注册
    /// </summary>
    [ApiController]
    [Route("User/[controller]")]
    public class RegisterController : Controller
    {
        /// <summary>
        /// 垃圾投递者的注册
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("GarbageMan")]
        public RegisterResponse Register([FromBody] GMRegisterRequest req)
        {
            var resp = new RegisterResponse();
            if (string.IsNullOrEmpty(req.garbageman.id) ||
                string.IsNullOrEmpty(req.garbageman.name) ||
                string.IsNullOrEmpty(req.garbageman.address) ||
                string.IsNullOrEmpty(req.garbageman.password) ||
                string.IsNullOrEmpty(req.garbageman.phonenumber)
            )
            {
                resp.status = Config.EMPTY;
                return resp;
            }

            var crypt = DBConn.MD5Encrypt16(req.garbageman.password); //加密密码
            if (string.IsNullOrEmpty(req.garbageman.id) || req.garbageman.id[0] != '1')
            {
                resp.status = Config.FAIL;
                resp.registerMessage = "投放人ID格式不正确，须以1开头";
                return resp;
            }

            using (var conn = new DB())
            {
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO alluser " +
                                     $"VALUES(:id,:name,:password,:phonenumber," +
                                     $":address,'0')";//初始积分默认为0
                                                      //添加参数
                    cmd.Parameters.Add(":id", req.garbageman.id);
                    cmd.Parameters.Add(":name", req.garbageman.name);
                    cmd.Parameters.Add(":password", crypt);
                    cmd.Parameters.Add(":phonenumber", req.garbageman.phonenumber);
                    cmd.Parameters.Add(":address", req.garbageman.address);
                    //执行
                    cmd.ExecuteNonQuery();
                    resp.status = Config.SUCCESS;
                    resp.registerMessage = "注册成功";
                }
                catch (Exception ex)
                {
                    resp.status = Config.FAIL;
                    resp.registerMessage = ex.Message; //返回异常提示（如数据库连接失败，ID重复等）
                }
                //conn.Close();
                return resp;
            }
        }

        /// <summary>
        /// 垃圾站检查员注册，注意鉴权
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("Watcher")]
        public RegisterResponse Register([FromBody] WCRegisterRequest req)
        {
            var resp = new RegisterResponse();
            if (string.IsNullOrEmpty(req.watcher.id) ||
                string.IsNullOrEmpty(req.watcher.name) ||
                string.IsNullOrEmpty(req.watcher.address) ||
                string.IsNullOrEmpty(req.watcher.password) ||
                string.IsNullOrEmpty(req.watcher.phonenumber)
            )
            {
                resp.status = Config.EMPTY;
                return resp;
            }

            string crypt = DBConn.MD5Encrypt16(req.watcher.password); //加密密码
            if (req.watcher.id[0] != '2')
            {
                resp.status = Config.FAIL;
                resp.registerMessage = "看管人ID格式不正确，须以2开头";
                return resp;
            }

            using (var conn = new DB())
            {
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO watcher " +
                                     $"VALUES(:id,:name,:phonenumber,:password," +
                                     $":address)";
                    //添加参数
                    cmd.Parameters.Add(":id", req.watcher.id);
                    cmd.Parameters.Add(":name", req.watcher.name);
                    cmd.Parameters.Add(":phonenumber", req.watcher.phonenumber);
                    cmd.Parameters.Add(":password", crypt);
                    cmd.Parameters.Add(":address", req.watcher.address);
                    //执行
                    cmd.ExecuteNonQuery();
                    resp.status = Config.SUCCESS;
                    resp.registerMessage = "注册成功";
                }
                catch (Exception ex)
                {
                    resp.status = Config.FAIL;
                    resp.registerMessage = ex.Message; //返回异常提示（如数据库连接失败，ID重复等）
                }              
                return resp;
            }
        }

        /// <summary>
        /// 运输员注册，注意鉴权
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("Carrier")]
        public RegisterResponse Register([FromBody] CRRegisterRequest req)
        {
            var resp = new RegisterResponse();
            if (string.IsNullOrEmpty(req.transportpersonnel.id) ||
                string.IsNullOrEmpty(req.transportpersonnel.name) ||
                string.IsNullOrEmpty(req.transportpersonnel.password) ||
                string.IsNullOrEmpty(req.transportpersonnel.phonenumber)
            )
            {
                resp.status = Config.EMPTY;
                return resp;
            }

            var crypt = DBConn.MD5Encrypt16(req.transportpersonnel.password); //加密密码
            if (req.transportpersonnel.id[0] != '3')
            {
                resp.status = Config.FAIL;
                resp.registerMessage = "运输人ID格式不正确，须以3开头";
                return resp;
            }

            using (var conn = new DB())
            {
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO carrier " +
                                     $"VALUES(:id,:name," +
                                     $":phonenumber,:password)";
                    //添加参数
                    cmd.Parameters.Add(":id", req.transportpersonnel.id);
                    cmd.Parameters.Add(":name", req.transportpersonnel.name);
                    cmd.Parameters.Add(":phonenumber", req.transportpersonnel.phonenumber);
                    cmd.Parameters.Add(":password", crypt);
                    //执行
                    cmd.ExecuteNonQuery();
                    resp.status = Config.SUCCESS;
                    resp.registerMessage = "注册成功";
                }
                catch (Exception ex)
                {
                    resp.status = Config.FAIL;
                    resp.registerMessage = ex.Message; //返回异常提示（如数据库连接失败，ID重复等）
                }               
                return resp;
            }
        }

        /// <summary>
        /// 垃圾处理站员工录入
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("StationStaff")]
        public RegisterResponse Register([FromBody] SSRegisterRequest req)
        {
            var resp = new RegisterResponse();
            if (string.IsNullOrEmpty(req.stationstaff.id) ||
                string.IsNullOrEmpty(req.stationstaff.name) ||
                string.IsNullOrEmpty(req.stationstaff.password) ||
                string.IsNullOrEmpty(req.stationstaff.phonenumber) ||
                string.IsNullOrEmpty(req.stationstaff.plantname)
            )
            {
                resp.status = Config.EMPTY;
                return resp;
            }

            var crypt = DBConn.MD5Encrypt16(req.stationstaff.password); //加密密码
            if (req.stationstaff.id[0] != '4')
            {
                resp.status = Config.FAIL;
                resp.registerMessage = "处理人ID格式不正确，须以4开头";
                return resp;
            }

            using (var conn = new DB())
            {
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO staff " +
                                     $"VALUES(:id,:name," +
                                     $":phonenumber,:plantname,:password)";
                    //添加参数
                    cmd.Parameters.Add(":id", req.stationstaff.id);
                    cmd.Parameters.Add(":name", req.stationstaff.name);
                    cmd.Parameters.Add(":phonenumber", req.stationstaff.phonenumber);
                    cmd.Parameters.Add(":plantname", req.stationstaff.plantname);
                    cmd.Parameters.Add(":password", crypt);
                    //执行
                    cmd.ExecuteNonQuery();
                    resp.status = Config.SUCCESS;
                    resp.registerMessage = "注册成功";
                }
                catch (Exception ex)
                {
                    resp.status = Config.FAIL;
                    resp.registerMessage = ex.Message; //返回异常提示（如数据库连接失败，ID重复等）
                }              
                return resp;
            }
        }
/*
        /// <summary>
        /// 管理人员信息注册
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("Administrator")]
        public RegisterResponse Register([FromBody] ADRegisterRequest req)
        {
            var crypt = DBConn.MD5Encrypt16(req.administrator.password);//加密密码
            var resp = new RegisterResponse();
            if (req.administrator.id[0] != '5')
            {
                resp.status = Config.FAIL;
                resp.registerMessage = "管理员ID格式不正确，须以5开头";
                return resp;
            }
                        var conn=new DB();
            try
            {
                
                var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO administrator " +
                                 $"VALUES('{req.administrator.id}','{req.administrator.name}'," +
                                 $"'{req.administrator.phonenumber}','{crypt}')";
                cmd.ExecuteNonQuery();
                resp.status = Config.SUCCESS;
                resp.registerMessage = "注册成功";
            }
            catch (Exception ex)
            {
                resp.status = Config.FAIL;
                resp.registerMessage = ex.Message;//返回异常提示（如数据库连接失败，ID重复等）
            }
            
            return resp;
        }*/
    }

    /// <summary>
    /// 用户属性更新(用户必须再次输入密码后才能更新相关信息)
    /// </summary>
    [ApiController]
    [Route("User/[controller]")]
    public class UpdateController : Controller
    {
        /// <summary>
        /// 垃圾投递人状态更新
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("GarbageMan")]
        [Authorize(Roles = "Administrator,GarbageMan")]
        public UpdateResponse Update([FromBody] GarbageMan req)
        {
            using (var conn = new DB())
            {
                var resp = new UpdateResponse { status = Config.FAIL };
                try
                {
                    var cmd = conn.CreateCommand();
                    //判断传入请求是否有空值，若有则不进行修改               
                    var connectString = "UPDATE alluser " + $"SET " +
                                        (!string.IsNullOrEmpty(req.name) ? $"user_name=:name," : "") +
                                        (!string.IsNullOrEmpty(req.password) ? $"user_password=:password," : "") +
                                        (!string.IsNullOrEmpty(req.phonenumber) ? $"phone_num=:phonenumber," : "") +
                                        (!string.IsNullOrEmpty(req.address) ? $"address=:address," : "") +
                                        $"user_id=:id" +
                                        $" WHERE user_id=:id";
                    cmd.CommandText = connectString;
                    //添加参数               
                    if (!string.IsNullOrEmpty(req.name)) cmd.Parameters.Add(":name", req.name);
                    if (!string.IsNullOrEmpty(req.password)) cmd.Parameters.Add(":password", DBConn.MD5Encrypt16(req.password));
                    if (!string.IsNullOrEmpty(req.phonenumber)) cmd.Parameters.Add(":phonenumber", req.phonenumber);
                    if (!string.IsNullOrEmpty(req.address)) cmd.Parameters.Add(":address", req.address);
                    cmd.Parameters.Add(":id", req.id);
                    //执行               
                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        resp.status = Config.FAIL;
                        resp.updateMessage = "未找到符合条件的指定行";

                        return resp;
                    }

                    resp.status = Config.SUCCESS;
                    resp.updateMessage = "更改成功";
                }
                catch (Exception ex)
                {
                    resp.status = Config.FAIL;
                    resp.updateMessage = ex.Message;
                }                
                return resp;
            }
        }

        /// <summary>
        /// 垃圾站监察员状态更新
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("Watcher")]
        [Authorize(Roles = "Administrator,Watcher")]
        public UpdateResponse Update([FromBody] Watcher req)
        {
            using (var conn = new DB())
            {
                var resp = new UpdateResponse { status = Config.FAIL };
                try
                {
                    var cmd = conn.CreateCommand();
                    //判断传入请求是否有null值，若有则不进行修改
                    var connectString = "UPDATE WATCHER " + $"SET " +
                                        (!string.IsNullOrEmpty(req.name) ? $"WATCHER_NAME=:name," : "") +
                                        (!string.IsNullOrEmpty(req.password) ? $"WATCHER_PASSWORD=:password," : "") +
                                        (!string.IsNullOrEmpty(req.phonenumber) ? $"PHONE_NUM=:phonenumber," : "") +
                                        (!string.IsNullOrEmpty(req.address) ? $"ADDRESS=:address," : "") +
                                        $"watcher_id =:id" +
                                        $" WHERE WATCHER_ID=:id";
                    cmd.CommandText = connectString;
                    //添加参数               
                    if (!string.IsNullOrEmpty(req.name)) cmd.Parameters.Add(":name", req.name);
                    if (!string.IsNullOrEmpty(req.password)) cmd.Parameters.Add(":password", DBConn.MD5Encrypt16(req.password));
                    if (!string.IsNullOrEmpty(req.phonenumber)) cmd.Parameters.Add(":phonenumber", req.phonenumber);
                    if (!string.IsNullOrEmpty(req.address)) cmd.Parameters.Add(":address", req.address);
                    cmd.Parameters.Add(":id", req.id);
                    //执行                               
                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        resp.status = Config.FAIL;
                        resp.updateMessage = "未找到符合条件的指定行";

                        return resp;
                    }

                    resp.status = Config.SUCCESS;
                    resp.updateMessage = "更改成功";
                }
                catch (Exception ex)
                {
                    resp.status = Config.FAIL;
                    resp.updateMessage = ex.Message;
                }                
                return resp;
            }
        }

        /// <summary>
        /// 运输员的状态更新
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("Carrier")]
        [Authorize(Roles = "Administrator,Carrier")]
        public UpdateResponse Update([FromBody] Carrier req)
        {
            using (var conn = new DB())
            {
                var resp = new UpdateResponse { status = Config.FAIL };
                try
                {
                    var cmd = conn.CreateCommand();
                    //判断传入请求是否有null值，若有则不进行修改
                    var connectString = "UPDATE CARRIER " + $"SET " +
                                        (!string.IsNullOrEmpty(req.name) ? $"CARRIER_NAME=:name," : "") +
                                        (!string.IsNullOrEmpty(req.password) ? $"CARRIER_PASSWORD=:password," : "") +
                                        (!string.IsNullOrEmpty(req.phonenumber) ? $"PHONE_NUM=:phonenumber," : "") +
                                        $"CARRIER_ID =:id" +
                                        $" WHERE CARRIER_ID=:id";
                    cmd.CommandText = connectString;
                    //添加参数
                    if (!string.IsNullOrEmpty(req.name)) cmd.Parameters.Add(":name", req.name);
                    if (!string.IsNullOrEmpty(req.password)) cmd.Parameters.Add(":password", DBConn.MD5Encrypt16(req.password));
                    if (!string.IsNullOrEmpty(req.phonenumber)) cmd.Parameters.Add(":phonenumber", req.phonenumber);
                    cmd.Parameters.Add(":id", req.id);
                    //执行    
                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        resp.status = Config.FAIL;
                        resp.updateMessage = "未找到符合条件的指定行";

                        return resp;
                    }

                    resp.status = Config.SUCCESS;
                    resp.updateMessage = "更改成功";
                }
                catch (Exception ex)
                {
                    resp.status = Config.FAIL;
                    resp.updateMessage = ex.Message;
                }               
                return resp;
            }
        }

        /// <summary>
        /// 垃圾处理站员工的更新
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("StationStaff")]
        [Authorize(Roles = "Administrator,StationStaff")]
        public UpdateResponse Update([FromBody] StationStaff req)
        {
            using (var conn = new DB())
            {
                var resp = new UpdateResponse { status = Config.FAIL };
                try
                {
                    var cmd = conn.CreateCommand();
                    //判断传入请求是否有null值，若有则不进行修改
                    var connectString = "UPDATE STAFF " + $"SET " +
                                        (!string.IsNullOrEmpty(req.name) ? $"STAFF_NAME=:name," : "") +
                                        (!string.IsNullOrEmpty(req.password) ? $"STAFF_PASSWORD=:password," : "") +
                                        (!string.IsNullOrEmpty(req.phonenumber) ? $"PHONE_NUM=:phonenumber," : "") +
                                        $"staff_id=:id" +
                                        $" WHERE STAFF_ID=:id";
                    //添加参数               
                    if (!string.IsNullOrEmpty(req.name)) cmd.Parameters.Add(":name", req.name);
                    if (!string.IsNullOrEmpty(req.password)) cmd.Parameters.Add(":password", DBConn.MD5Encrypt16(req.password));
                    if (!string.IsNullOrEmpty(req.phonenumber)) cmd.Parameters.Add(":phonenumber", req.phonenumber);
                    cmd.Parameters.Add(":id", req.id);
                    //执行    
                    cmd.CommandText = connectString;
                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        resp.status = Config.FAIL;
                        resp.updateMessage = "未找到符合条件的指定行";

                        return resp;
                    }

                    resp.status = Config.SUCCESS;
                    resp.updateMessage = "更改成功";
                }
                catch (Exception ex)
                {
                    resp.status = Config.FAIL;
                    resp.updateMessage = ex.Message;
                }                
                return resp;
            }
        }

        /// <summary>
        /// 管理人员的更新
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("Administrator")]
        [Authorize(Roles = "Administrator")]
        public UpdateResponse Update([FromBody] Administrator req)
        {
            using (var conn = new DB())
            {
                var resp = new UpdateResponse { status = Config.FAIL };
                try
                {
                    var cmd = conn.CreateCommand();
                    //判断传入请求是否有null值，若有则不进行修改
                    cmd.CommandText = "UPDATE administrator SET " +
                                      (!string.IsNullOrEmpty(req.name) ? $"ADM_NAME=:name," : "") +
                                      (!string.IsNullOrEmpty(req.phonenumber) ? $"PHONE_NUM=:phonenumber," : "") +
                                      (!string.IsNullOrEmpty(req.password) ? $"ADM_PASSWORD=:password," : "") +
                                      $"adm_id=:id" +
                                      $" WHERE adm_id=:id";
                    //添加参数               
                    if (!string.IsNullOrEmpty(req.name)) cmd.Parameters.Add(":name", req.name);
                    if (!string.IsNullOrEmpty(req.phonenumber)) cmd.Parameters.Add(":phonenumber", req.phonenumber);
                    if (!string.IsNullOrEmpty(req.password)) cmd.Parameters.Add(":password", DBConn.MD5Encrypt16(req.password));
                    cmd.Parameters.Add(":id", req.id);
                    //执行    
                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        resp.status = Config.FAIL;
                        resp.updateMessage = "未找到符合条件的指定行";

                        return resp;
                    }

                    resp.status = Config.SUCCESS;
                    resp.updateMessage = "更改成功";
                }
                catch (Exception ex)
                {
                    resp.status = Config.FAIL;
                    resp.updateMessage = ex.Message;
                }
                return resp;
            }
        }
    }

    /// <summary>
    /// 用户注销
    /// </summary>
    [ApiController]
    [Route("User/[controller]")]
    public class DeleteController : Controller
    {
        /// <summary>
        /// 注销普通用户
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public DeleteResponse DeleteGarbageMan(string req = "")
        {
            var resp = new DeleteResponse {status = Config.FAIL};
            if (req == "")
            {
                resp.status = Config.EMPTY;
                resp.deleteMessage = "输入为空";
                return resp;
            }

            if (req[0] != '1' && req[0] != '2' && req[0] != '3' && req[0] != '4' && req[0] != '5')
            {
                resp.status = Config.FAIL;
                resp.deleteMessage = "用户类型错误";
                return resp;
            }

            using (var conn = new DB())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = req[0] switch
                {
                    '1' => "DELETE FROM alluser " + $"WHERE user_id=:id",
                    '2' => "DELETE FROM watcher " + $"WHERE watcher_id=:id",
                    '3' => "DELETE FROM carrier " + $"WHERE carrier_id=:id",
                    '4' => "DELETE FROM staff " + $"WHERE staff_id=:id",
                    _ => cmd.CommandText
                };
                //添加参数
                cmd.Parameters.Add(":id", req);
                try
                {
                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        resp.status = Config.FAIL;
                        resp.deleteMessage = "未找到符合条件的指定行";

                        return resp;
                    }

                    resp.status = Config.SUCCESS;
                    resp.deleteMessage = "删除成功";
                }
                catch (Exception ex)
                {
                    resp.status = Config.FAIL;
                    resp.deleteMessage = ex.Message;
                }                
                return resp;
            }
        }
        /*
        /// <summary>
        /// 注销监察员
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("Watcher")]
        [Authorize(Roles = "Administrator")]
        public DeleteResponse DeleteWatcher([FromBody] DeleteRequest req)
        {
            var resp = new DeleteResponse { status = Config.FAIL };
                        var conn=new DB();
            try
            {
                
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT* FROM watcher " +
                                 $"WHERE watcher_id='{req.id}'";
                OracleDataReader reader = cmd.ExecuteReader();
                reader.Read();
                if (DBConn.MD5Encrypt16(req.password) != reader["watcher_password"].ToString())//判断密码是否正确
                {
                    resp.status = Config.FAIL;
                    resp.deleteMessage = "密码错误";
                    
                    return resp;
                }
                cmd.CommandText = "DELETE FROM watcher " +
                                 $"WHERE watcher_id='{req.id}'";
                if (cmd.ExecuteNonQuery() == 0)
                {
                    resp.status = Config.FAIL;
                    resp.deleteMessage = "未找到符合条件的指定行";
                    
                    return resp;
                }
                resp.status = Config.SUCCESS;
                resp.deleteMessage = "删除成功";
            }
            catch (Exception ex)
            {
                resp.status = Config.FAIL;
                resp.deleteMessage = ex.Message;
            }
            
            return resp;
        }
        /// <summary>
        /// 注销运输人员
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("Carrier")]
        [Authorize(Roles = "Administrator")]
        public DeleteResponse DeleteCarrier([FromBody] DeleteRequest req)
        {
            var resp = new DeleteResponse { status = Config.FAIL };
                        var conn=new DB();
            try
            {
                
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT* FROM carrier " +
                                 $"WHERE carrier_id='{req.id}'";
                var reader = cmd.ExecuteReader();
                reader.Read();
                if (DBConn.MD5Encrypt16(req.password) != reader["carrier_password"].ToString())//判断密码是否正确
                {
                    resp.status = Config.FAIL;
                    resp.deleteMessage = "密码错误";
                    
                    return resp;
                }
                cmd.CommandText = "DELETE FROM carrier " +
                                 $"WHERE carrier_id='{req.id}'";
                if (cmd.ExecuteNonQuery() == 0)
                {
                    resp.status = Config.FAIL;
                    resp.deleteMessage = "未找到符合条件的指定行";
                    
                    return resp;
                }
                resp.status = Config.SUCCESS;
                resp.deleteMessage = "删除成功";
            }
            catch (Exception ex)
            {
                resp.status = Config.FAIL;
                resp.deleteMessage = ex.Message;
            }
            
            return resp;
        }
        /// <summary>
        /// 注销垃圾处理站员工
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("StationStaff")]
        [Authorize(Roles = "Administrator")]
        public DeleteResponse DeleteStationStaff([FromBody] DeleteRequest req)
        {
            var resp = new DeleteResponse { status = Config.FAIL };
                        var conn=new DB();
            try
            {
                
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT* FROM staff " +
                                 $"WHERE staff_id='{req.id}'";
                var reader = cmd.ExecuteReader();
                reader.Read();
                if (DBConn.MD5Encrypt16(req.password) != reader["staff_password"].ToString())//判断密码是否正确
                {
                    resp.status = Config.FAIL;
                    resp.deleteMessage = "密码错误";
                    
                    return resp;
                }
                cmd.CommandText = "DELETE FROM staff " +
                                 $"WHERE staff_id='{req.id}'";
                if (cmd.ExecuteNonQuery() == 0)
                {
                    resp.status = Config.FAIL;
                    resp.deleteMessage = "未找到符合条件的指定行";
                    
                    return resp;
                }
                resp.status = Config.SUCCESS;
                resp.deleteMessage = "删除成功";
            }
            catch (Exception ex)
            {
                resp.status = Config.FAIL;
                resp.deleteMessage = ex.Message;
            }
            
            return resp;
        }
        /// <summary>
        /// 注销管理员
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("Administrator")]
        public DeleteResponse DeleteAdministrator([FromBody] DeleteRequest req)
        {
            var resp = new DeleteResponse { status = Config.FAIL };
                        var conn=new DB();
            try
            {
                
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT* FROM administrator " +
                                 $"WHERE adm_id='{req.id}'";
                var reader = cmd.ExecuteReader();
                reader.Read();
                if (DBConn.MD5Encrypt16(req.password) != reader["adm_password"].ToString())//判断密码是否正确
                {
                    resp.status = Config.FAIL;
                    resp.deleteMessage = "密码错误";
                    
                    return resp;
                }
                cmd.CommandText = "DELETE FROM administrator " +
                                 $"WHERE adm_id='{req.id}'";
                if (cmd.ExecuteNonQuery() == 0)
                {
                    resp.status = Config.FAIL;
                    resp.deleteMessage = "未找到符合条件的指定行";
                    
                    return resp;
                }
                resp.status = Config.SUCCESS;
                resp.deleteMessage = "删除成功";
            }
            catch (Exception ex)
            {
                resp.status = Config.FAIL;
                resp.deleteMessage = ex.Message;
            }
            
            return resp;
        }*/
    }

    /// <summary>
    /// 用户信息的获取
    /// </summary>
    [ApiController]
    [Route("User/[controller]")]
    public class GetInformationController : Controller
    {
        /// <summary>
        /// 垃圾投递人员的信息查看
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("GarbageMan")]
        [Authorize(Roles = "Administrator,GarbageMan")]
        public GarbageMan GetGarbageMan(string req)
        {
            using (var conn = new DB())
            {
                var resp = new GarbageMan();
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT* " +
                                      "FROM alluser " +
                                     $"WHERE user_id=:id";
                    //添加参数
                    cmd.Parameters.Add(":id", req);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        resp.id = reader["user_id"].ToString();
                        resp.name = reader["user_name"].ToString();
                        resp.phonenumber = reader["phone_num"].ToString();
                        resp.address = reader["address"].ToString();
                        resp.credit = Convert.ToInt32(reader["cre_points"]);
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }               
                return resp;
            }
        }

        /// <summary>
        /// 所有垃圾投递人员的信息查看
        /// </summary>
        /// <returns></returns>
        [HttpGet("AllGarbageMan")]
        public List<GarbageMan> GetAllGarbageMan()
        {
            using (var conn = new DB())
            {
                var respList = new List<GarbageMan>();
                try
                {

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT* " +
                                      "FROM alluser";
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var resp = new GarbageMan
                        {
                            id = reader["user_id"].ToString(),
                            name = reader["user_name"].ToString(),
                            phonenumber = reader["phone_num"].ToString(),
                            address = reader["address"].ToString(),
                            credit = Convert.ToInt32(reader["cre_points"])
                        };
                        respList.Add(resp);
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }                
                return respList;
            }
        }

        /// <summary>
        /// 监察员信息的查看
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("Watcher")]
        [Authorize(Roles = "Administrator,Watcher")]
        public Watcher GetWatcher(string req)
        {
            using (var conn = new DB())
            {
                var resp = new Watcher();
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT* " +
                                      "FROM watcher " +
                                     $"WHERE watcher_id=:id";
                    //添加参数
                    cmd.Parameters.Add(":id", req);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        resp.id = reader["watcher_id"].ToString();
                        resp.name = reader["watcher_name"].ToString();
                        resp.phonenumber = reader["phone_num"].ToString();
                        resp.address = reader["address"].ToString();
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }                
                return resp;
            }
        }

        /// <summary>
        /// 所有监察员的信息查看
        /// </summary>
        /// <returns></returns>
        [HttpGet("AllWatcher")]
        public List<Watcher> GetAllWatcher()
        {
            using (var conn = new DB())
            {
                var respList = new List<Watcher>();
                try
                {

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT* " +
                                      "FROM watcher";
                    OracleDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var resp = new Watcher
                        {
                            id = reader["watcher_id"].ToString(),
                            name = reader["watcher_name"].ToString(),
                            phonenumber = reader["phone_num"].ToString(),
                            address = reader["address"].ToString()
                        };
                        respList.Add(resp);
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }               
                return respList;
            }
        }

        /// <summary>
        /// 运输人员信息的查看 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("Carrier")]
        [Authorize(Roles = "Administrator,Carrier")]
        public Carrier GetCarrier(string req)
        {
            using (var conn = new DB())
            {
                var resp = new Carrier();
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT* " +
                                      "FROM carrier " +
                                     $"WHERE carrier_id=:id";
                    //添加参数
                    cmd.Parameters.Add(":id", req);
                    OracleDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        resp.id = reader["carrier_id"].ToString();
                        resp.name = reader["carrier_name"].ToString();
                        resp.phonenumber = reader["phone_num"].ToString();
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }                
                return resp;
            }
        }

        /// <summary>
        /// 所有运输人员的信息查看
        /// </summary>
        /// <returns></returns>
        [HttpGet("AllCarrier")]
        public List<Carrier> GetAllCarrier()
        {
            using (var conn = new DB())
            {
                var respList = new List<Carrier>();
                try
                {

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT* " +
                                      "FROM carrier";
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var resp = new Carrier
                        {
                            id = reader["carrier_id"].ToString(),
                            name = reader["carrier_name"].ToString(),
                            phonenumber = reader["phone_num"].ToString()
                        };
                        respList.Add(resp);
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }                
                return respList;
            }
        }

        /// <summary>
        /// 垃圾处理站员工信息的查看
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("StationStaff")]
        [Authorize(Roles = "Administrator,StationStaff")]
        public StationStaff GetStationStaff(string req)
        {
            using (var conn = new DB())
            {
                var resp = new StationStaff();
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT* " +
                                      "FROM staff " +
                                     $"WHERE staff_id=:id";
                    //添加参数
                    cmd.Parameters.Add(":id", req);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        resp.id = reader["staff_id"].ToString();
                        resp.name = reader["staff_name"].ToString();
                        resp.phonenumber = reader["phone_num"].ToString();
                        resp.plantname = reader["plant_name"].ToString();
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }              
                return resp;
            }
        }

        /// <summary>
        /// 所有垃圾处理站员工信息的查看
        /// </summary>
        /// <returns></returns>
        [HttpGet("AllStationStaff")]
        public List<StationStaff> GetAllStationStaff()
        {
            using (var conn = new DB())
            {
                var respList = new List<StationStaff>();
                try
                {

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT* " +
                                      "FROM staff";
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var resp = new StationStaff
                        {
                            id = reader["staff_id"].ToString(),
                            name = reader["staff_name"].ToString(),
                            phonenumber = reader["phone_num"].ToString(),
                            plantname = reader["plant_name"].ToString()
                        };
                        respList.Add(resp);
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return respList;
            }
        }

        /// <summary>
        /// 垃圾管理员工信息的查看 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("Administrator")]
        [Authorize(Roles = "Administrator")]
        public Administrator GetAdministrator(string req)
        {
            using (var conn = new DB())
            {
                var resp = new Administrator();
                try
                {

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT* " +
                                      "FROM administrator " +
                                     $"WHERE adm_id=:id";
                    //添加参数
                    cmd.Parameters.Add(":id", req);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        resp.id = reader["adm_id"].ToString();
                        resp.name = reader["adm_name"].ToString();
                        resp.phonenumber = reader["phone_num"].ToString();
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }               
                return resp;
            }
        }
    }

    /// <summary>
    /// 对watcher的工作进性记录
    /// </summary>
    [ApiController]
    [Route("User/[controller]/[action]")]
    public class DutyArrangeController : Controller
    {
        /// <summary>
        /// 增加工作安排(watcher登入时自动填入)
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Watcher")]
        public AddResponse Add(DutyArrange req)
        {
            using (var conn = new DB())
            {
                var resp = new AddResponse();
                try
                {

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO dutyarrange " +
                                     $"VALUES(:id,:site_name," +
                                     $"TO_DATE('{req.start_time:yyyy-MM-dd HH:mm:ss}','yyyy-mm-dd hh24:mi:ss'),null)";
                    //添加参数
                    cmd.Parameters.Add(":id", HttpContext.User.Identity.Name);
                    cmd.Parameters.Add(":site_name", req.site_name);
                    //执行
                    cmd.ExecuteNonQuery();
                    resp.status = Config.SUCCESS;
                    resp.addMessage = "添加成功";
                }
                catch (Exception ex)
                {
                    resp.status = Config.FAIL;
                    resp.addMessage = ex.Message;
                }                
                return resp;
            }
        }

        /// <summary>
        /// 增加工作安排(watcher登出时自动填入,只需传入一个时间参数)
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Watcher")]
        public UpdateResponse Update(DateTime req)
        {
            using (var conn = new DB())
            {
                var resp = new UpdateResponse();
                try
                {

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "UPDATE dutyarrange " +
                                     $"SET end_time=TO_DATE('{DateTime.Now:yyyy-MM-dd HH:mm:ss}','SYYYY-MM-DD HH24:MI:SS') " +
                                     $"WHERE watcher_id=:id " +
                                     $"AND start_time=TO_DATE('{req:yyyy-MM-dd HH:mm:ss}','SYYYY-MM-DD HH24:MI:SS')";
                    //添加参数
                    cmd.Parameters.Add(":id", HttpContext.User.Identity.Name);
                    //执行
                    cmd.ExecuteNonQuery();
                    resp.status = Config.SUCCESS;
                    resp.updateMessage = "更改成功";
                }
                catch (Exception ex)
                {
                    resp.status = Config.FAIL;
                    resp.updateMessage = ex.Message;
                }                
                return resp;
            }
        }

        /// <summary>
        /// 删除工作安排
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public DeleteResponse Delete([FromBody] DutyArrange req)
        {
            using (var conn = new DB())
            {
                var resp = new DeleteResponse();
                try
                {

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "DELETE FROM dutyarrange " +
                                     $"WHERE watcher_id=:id AND site_name=:site_name " +
                                     $"AND start_time=TO_DATE('{req.start_time:yyyy-MM-dd HH:mm:ss}','SYYYY-MM-DD HH24:MI:SS')";
                    //添加参数
                    cmd.Parameters.Add(":id", req.watcher_id);
                    cmd.Parameters.Add(":site_name", req.site_name);
                    //执行
                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        resp.status = Config.FAIL;
                        resp.deleteMessage = "未找到符合条件的指定行";

                        return resp;
                    }

                    resp.status = Config.SUCCESS;
                    resp.deleteMessage = "删除成功";
                }
                catch (Exception ex)
                {
                    resp.status = Config.FAIL;
                    resp.deleteMessage = ex.Message;
                }               
                return resp;
            }
        }

        /// <summary>
        /// 查找一个watcher的系列工作安排，输入为watcher_id，此接口可被watcher访问(只能查看已有结束时间的工作安排)
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>     
        [HttpGet]
        [Authorize(Roles = "Administrator,Watcher")]
        public List<DutyArrange> Get(string req)
        {
            using (var conn = new DB())
            {
                var resp = new List<DutyArrange>();
                try
                {

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT* FROM dutyarrange " +
                                     $"WHERE watcher_id=:id AND end_time IS NOT NULL " +
                                     $"ORDER BY start_time DESC";
                    //添加参数
                    cmd.Parameters.Add(":id", req);
                    //执行
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var da = new DutyArrange
                        {
                            watcher_id = reader["watcher_id"].ToString(),
                            site_name = reader["site_name"].ToString(),
                            start_time = reader.GetDateTime(2),
                            end_time = reader.GetDateTime(3)
                        };
                        resp.Add(da);
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }               
                return resp;
            }
        }

        /// <summary>
        /// 获取所有的工作安排(仅能获取所有已有结束时间的工作安排)
        /// </summary>
        /// <returns></returns>     
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public List<DutyArrange> GetAll()
        {
            using (var conn = new DB())
            {
                var resp = new List<DutyArrange>();
                try
                {

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT WATCHER_ID,SITE_NAME,START_TIME,END_TIME " +
                                      "FROM dutyarrange " +
                                      "WHERE END_TIME IS NOT NULL " +
                                      "ORDER BY start_time DESC";
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var da = new DutyArrange
                        {
                            watcher_id = reader.GetString(0),
                            site_name = reader.GetString(1),
                            start_time = reader.GetDateTime(2),
                            end_time = reader.GetDateTime(3)
                        };
                        resp.Add(da);
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return resp;
            }
        }
    }
}