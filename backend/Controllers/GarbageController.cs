using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using DBPractice.Models;
using Microsoft.AspNetCore.Authorization;

namespace DBPractice.Controllers
{
    /// <summary>
    /// 垃圾有关的api接口
    /// </summary>
    [ApiController, Route("[controller]/[action]")]
    public class GarbageController : Controller
    {
        private static string GenerateGarbageID()
        {
            string gar_id = null;
            var dt = DateTime.Now;
            gar_id = $"{dt:yyMMddHHmmss}";
            return gar_id;
        }

        /*
         * 垃圾分为可回收垃圾、干垃圾、湿垃圾、有害垃圾，可回收垃圾须装入一个袋中并贴上一个条形码
         * 利用可回收垃圾的条形码可以读出投放人的id
         * 只有可回收垃圾需要更新垃圾处理的结果信息，此更新由stationStaff完成
         */


        /// <summary>
        /// 添加垃圾(传入垃圾类型即可)
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Authorize(Roles = "GarbageMan")]
        [HttpGet]
        public AddResponse Add(string req)
        {
            using (var conn = new DB())
            {
                var resp = new AddResponse();
                if (HttpContext.User.Identity == null) return resp;
                var gar_id = HttpContext.User.Identity.Name + GenerateGarbageID();
                try
                {
                    //INSERT INTO "C##PDCR"."GARBAGE" ("GAR_ID", "GAR_TYPE", "USER_ID", "CREATE_TIME", "TRANS_ID") VALUES ('1234567', '干垃圾', '1952108', TO_DATE('2021-07-19 14:25:55', 'SYYYY-MM-DD HH24:MI:SS'), '11111113');


                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO garbage " +
                                      $"VALUES(:gar_id,:type,:id," +
                                      $"TO_DATE('{DateTime.Now:yyyy-MM-dd HH:mm:ss}','yyyy-MM-dd hh24-mi-ss'),null)";
                    //添加参数
                    cmd.Parameters.Add(":gar_id", gar_id);
                    cmd.Parameters.Add(":type", req);
                    cmd.Parameters.Add(":id", HttpContext.User.Identity.Name);
                    //执行
                    cmd.ExecuteNonQuery();
                    resp.status = Config.SUCCESS;
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
        /// 投递垃圾
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Watcher")]
        [HttpPost]
        public Response Throw([FromBody] ThrowRequest req)
        {
            using (var conn = new DB())
            {
                var resp = new Response();
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO THROW " +
                                      $"VALUES(:gar_id,:bid,TO_DATE('{DateTime.Now:yyyy-MM-dd HH:mm:ss}','yyyy-MM-dd hh24-mi-ss'))";
                    //添加参数
                    cmd.Parameters.Add(":gar_id", req.gid);
                    cmd.Parameters.Add(":bid", req.bid);
                    //执行
                    cmd.ExecuteNonQuery();
                    resp.status = Config.SUCCESS;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    resp.status = Config.FAIL;
                }
                
                return resp;
            }
        }
        /*
        /// <summary>
        /// 垃圾状态的更新，由StationStaff来提供垃圾处理结果信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        [HttpPost] //[ Authorize(Roles = "StationStaff")]
        [Authorize(Roles = "StationStaff")]
        public UpdateResponse Update(string id, int result)
        {
            var resp = new UpdateResponse();
            if (result != 5 && result != 6)
            {
                resp.status = Config.FAIL;
                resp.updateMessage = "更改错误";
            }

            var conn = new DB();
            try
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE garbage " + $"SET gar_result =:result," +
                                  $"finish_time = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}' "
                                  + $"WHERE gar_id =:gar_id";
                //添加参数
                cmd.Parameters.Add(":gar_id", id);
                cmd.Parameters.Add(":result", result);            
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
        */
        /// <summary>
        /// 删除对应编号的垃圾
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost] //
        [Authorize(Roles = "Administrator")]
        public DeleteResponse Delete(string req)
        {
            using (var conn = new DB())
            {
                var resp = new DeleteResponse();
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "DELETE FROM garbage " +
                                      $"WHERE gar_id =:gar_id ";
                    //添加参数
                    cmd.Parameters.Add(":gar_id", req);
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
        /// 为了防止数据库垃圾记录过多，管理人员可定期清理早于某个时间的记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public DeleteResponse DeleteOldRecord(DateTime req)
        {
            using (var conn = new DB())
            {
                var resp = new DeleteResponse();
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "DELETE FROM garbage " +
                                      $"WHERE CREATE_TIME<=TO_DATE('{req:yyyy-MM-dd HH:mm:ss}','yyyy-mm-dd hh24:mi:ss') ";
                    cmd.ExecuteNonQuery();
                    resp.status = Config.SUCCESS;
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
        /// 一个垃圾运输的生命周期 状态说明：0是已经申请，1是已入桶，2是运输中，3是到达处理站
        /// </summary>
        public struct GarLife
        {
            public string gar_id { get; set; }
            public string type { get; set; }
            public string dustbin_id { get; set; }
            public string plant_name { get; set; }
            public string user_id { get; set; }
            public string truck_id { get; set; }
            public DateTime latest_time { get; set; }
            public int status { get; set; } //0是已经申请，1是已入桶，2是运输中，3是到达处理站
        }

        /// <summary>
        /// 以垃圾编号获取某一个垃圾投递的记录
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public GarLife Get(string req)
        {
            using (var conn = new DB())
            {
                var resp = new GarLife { status = Config.FAIL };
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText =
                        "SELECT " +
                        "USER_ID,GARBAGE.GAR_ID,GARBAGE.GAR_TYPE,CREATE_TIME," + //垃圾创建状态截止 3
                        "THROW.DUSTBIN_ID,THROW_TIME," + //垃圾投放状态截止 5
                        "GARBAGE.TRANS_ID,TRUCK_ID,START_TIME,END_TIME,TRANSPORT.PLANT_NAME " + //垃圾运输状态截止 8,9
                        "FROM garbage " +
                        "LEFT JOIN THROW ON garbage.GAR_ID=THROW.GAR_ID " +
                        "LEFT JOIN TRANSPORT ON TRANSPORT.TRANS_ID=GARBAGE.TRANS_ID " +
                        $"WHERE GARBAGE.GAR_ID=:gar_id"; //按照垃圾编号查找
                                                         //添加参数
                    cmd.Parameters.Add(":gar_id", req);
                    //执行
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        resp.user_id = reader.GetString(0);
                        resp.gar_id = reader.GetString(1);
                        resp.type = reader.GetString(2);
                        resp.latest_time = reader.GetDateTime(3);
                        resp.status = 0;
                        if (reader.IsDBNull(5) == false)
                        {
                            resp.dustbin_id = reader.GetString(4);
                            resp.latest_time = reader.GetDateTime(5);
                            resp.status = 1;
                        }

                        if (reader.IsDBNull(8) == false)
                        {
                            resp.truck_id = reader.GetString(7);
                            resp.latest_time = reader.GetDateTime(8);
                            resp.status = 2;
                        }

                        if (reader.IsDBNull(9) == false)
                        {
                            resp.latest_time = reader.GetDateTime(9);
                            resp.plant_name = reader.GetString(10);
                            resp.status = 3;
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
        /// 以投放人的id获取垃圾投放记录列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Administrator,GarbageMan")]
        public List<GarLife> GetAll(string req)
        {
            using (var conn = new DB())
            {
                var respList = new List<GarLife>();
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT " +
                                      "USER_ID,GARBAGE.GAR_ID,GARBAGE.GAR_TYPE,CREATE_TIME," + //垃圾创建状态截止 3
                                      "THROW.DUSTBIN_ID,THROW_TIME," + //垃圾投放状态截止 5
                                      "GARBAGE.TRANS_ID,TRUCK_ID,START_TIME,END_TIME,TRANSPORT.PLANT_NAME " + //垃圾运输状态截止 8,9
                                      "FROM garbage " +
                                      "LEFT JOIN THROW ON garbage.GAR_ID=THROW.GAR_ID " +
                                      "LEFT JOIN TRANSPORT ON TRANSPORT.TRANS_ID=GARBAGE.TRANS_ID " +
                                      $"WHERE GARBAGE.USER_ID=:id " +
                                      $"ORDER BY CREATE_TIME DESC"; //按照投递人编号查找
                                                                    //添加参数
                    cmd.Parameters.Add(":id", req);
                    //执行
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var resp = new GarLife
                        {
                            user_id = reader.GetString(0),
                            gar_id = reader.GetString(1),
                            type = reader.GetString(2),
                            latest_time = reader.GetDateTime(3),
                            status = 0
                        };
                        if (reader.IsDBNull(5) == false)
                        {
                            resp.dustbin_id = reader.GetString(4);
                            resp.latest_time = reader.GetDateTime(5);
                            resp.status = 1;
                        }

                        if (reader.IsDBNull(8) == false)
                        {
                            resp.truck_id = reader.GetString(7);
                            resp.latest_time = reader.GetDateTime(8);
                            resp.status = 2;
                        }

                        if (reader.IsDBNull(9) == false)
                        {
                            resp.latest_time = reader.GetDateTime(9);
                            resp.plant_name = reader.GetString(10);
                            resp.status = 3;
                        }

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
    }

    /// <summary>
    /// 垃圾投递管理
    /// </summary>
    [ApiController, Route("[controller]/[action]")]
    public class ThrowController : Controller
    {
        /*
         * INSERT INTO "C##PDCR"."THROW" ("GAR_ID", "DUSTBIN_ID", "THROW_TIME") VALUES ('09999999', 'D001', TO_DATE('2021-07-16 19:18:47', 'SYYYY-MM-DD HH24:MI:SS'));
         */
        /// <summary>
        /// 投递垃圾
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Watcher")]
        [HttpPost]
        public AddResponse Add([FromBody] ThrowRequest req)
        {
            using (var conn = new DB())
            {
                var resp = new AddResponse();
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO THROW " +
                                      $"VALUES(:gar_id,:bid,TO_DATE('{DateTime.Now:yyyy-MM-dd hh:mm:ss}','yyyy-mm-dd hh24:mi:ss'))";
                    //添加参数
                    cmd.Parameters.Add(":gar_id", req.gid);
                    cmd.Parameters.Add(":bid", req.bid);
                    //执行
                    cmd.ExecuteNonQuery();
                    resp.status = Config.SUCCESS;
                    resp.addMessage = "投递成功";
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
        /// 撤回投递记录，投递记录不能改，只能删.
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Authorize(Roles = "Watcher,Administrator")]
        [HttpGet]
        public DeleteResponse Delete(string req)
        {
            using (var conn = new DB())
            {
                var resp = new DeleteResponse();
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = $"DELETE FROM THROW WHERE GAR_ID=:gar_id";
                    //添加参数
                    cmd.Parameters.Add(":gar_id", req);
                    //执行
                    cmd.ExecuteNonQuery();
                    resp.status = Config.SUCCESS;
                    resp.deleteMessage = "撤回成功";
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
        /// 投递记录
        /// </summary>
        public struct ThrowRecord
        {
            public string user_id { get; set; }
            public string gar_id { get; set; }
            public string gar_type { get; set; }
            public string dustbin_id { get; set; }
            public DateTime throw_time { get; set; }
        };

        /// <summary>
        /// 根据垃圾站编号查找投递记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Administrator,Watcher")]
        public List<ThrowRecord> GetThrowRecord(string req)
        {
            using (var conn = new DB())
            {
                var resp = new List<ThrowRecord>();
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT " +
                                      "USER_ID,GAR_ID,GAR_TYPE," +
                                      "DUSTBIN_ID,THROW_TIME " +
                                      "FROM GARBAGE NATURAL JOIN THROW NATURAL JOIN DUSTBIN " +
                                      $"WHERE SITE_NAME=:site_name"; //按照垃圾站编号查找
                                                                     //添加参数
                    cmd.Parameters.Add(":site_name", req);
                    //执行
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var temp = new ThrowRecord()
                        {
                            user_id = reader.GetString(0),
                            gar_id = reader.GetString(1),
                            gar_type = reader.GetString(2),
                            dustbin_id = reader.GetString(3),
                            throw_time = reader.GetDateTime(4)
                        };
                        resp.Add(temp);
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

    ///<summary>
    ///有关所有垃圾运输的api
    ///</summary>
    [ApiController, Route("[controller]/[action]")]
    public class TransportController : Controller
    {
        struct Start
        {
            public string trans_id { get; set; }
            public string truck_id { get; set; } //垃圾车编号
            public string dustbin_id { get; set; }
        };

        private static List<Start> now = new(); //正在运送的还没写进数据库的。

        private Transport _newT = new Transport();

        /*
         * 这个是由运输员开始的，然后直接入now
         */
        /// <summary>
        /// 垃圾开始运输的行为
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Carrier")]
        public Response TransportStart([FromBody] TransportStart req)
        {
            using (var conn = new DB())
            {
                //INSERT INTO "C##PDCR"."TRANSPORT" ("TRUCK_ID", "START_TIME", "END_TIME", "PLANT_NAME", "DUSTBIN_ID", "TRANS_ID", "CARRIER_ID") VALUES ('T01', TO_DATE('2021-07-18 19:20:20', 'SYYYY-MM-DD HH24:MI:SS'), TO_DATE('2021-07-18 19:20:28', 'SYYYY-MM-DD HH24:MI:SS'), 'P01', 'D001', '11111111', '3952108');
                var resp = new Response { status = Config.FAIL };
                try
                {
                    if (HttpContext.User.Identity != null)
                    {
                        var trans_id = HttpContext.User.Identity.Name + $"{DateTime.Now:yyMMddHHmmss}"; //trans_id生成方式

                        var cmd = conn.CreateCommand();
                        cmd.CommandText = "INSERT INTO TRANSPORT " +
                                          " (TRUCK_ID, START_TIME, END_TIME, PLANT_NAME, DUSTBIN_ID, TRANS_ID, CARRIER_ID) " +
                                          $"VALUES(:truck_id,TO_DATE('{DateTime.Now:yyyy-MM-dd HH:mm:ss}','SYYYY-MM-DD HH24:MI:SS')," +
                                          $"null,null,:dustbin_id,:trans_id,:carrier_id)";
                        //添加参数
                        cmd.Parameters.Add(":truck_id", req.truck_id);
                        cmd.Parameters.Add(":dustbin_id", req.dustbin_id);
                        cmd.Parameters.Add(":trans_id", trans_id);
                        cmd.Parameters.Add(":carrier_id", HttpContext.User.Identity.Name);
                        //执行
                        cmd.ExecuteNonQuery();
                    }

                    resp.status = Config.SUCCESS;
                }
                catch (Exception ex)
                {
                    resp.status = Config.FAIL;
                    Console.WriteLine(ex.Message);
                }

                resp.status = Config.SUCCESS;               
                return resp;
            }
        }

        /// <summary>
        /// 垃圾结束运输到达垃圾处理站
        /// 从缓存中取出相应的开始运输记录并结合写入数据库
        /// trigger触发更新垃圾状态为到达垃圾处理站
        /// 只能由垃圾处理站人员访问本接口
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "StationStaff")]
        public Response TransportEnd([FromBody] TransportEnd req)
        {
            using (var conn = new DB())
            {
                var resp = new Response { status = Config.SUCCESS };
                var cmd = conn.CreateCommand();
                var endTime = DateTime.Now;
                try
                {
                    cmd.CommandText = "UPDATE TRANSPORT SET " +
                                      $"END_TIME=TO_DATE('{endTime:yyyy-MM-dd HH:mm:ss}','yyyy-mm-dd hh24:mi:ss')," +
                                      $"PLANT_NAME=:plant_name" +
                                      $" WHERE END_TIME is null AND TRUCK_ID=:truck_id";
                    //添加参数
                    cmd.Parameters.Add(":plant_name", req.plant_name);
                    cmd.Parameters.Add(":truck_id", req.truck_id);
                    //执行
                    Console.WriteLine(cmd.CommandText);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    //m_OraTrans.Rollback();
                    resp.status = Config.FAIL;
                }
                
                return resp;
            }
        }

        /// <summary>
        /// 运输结束
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "StationStaff")]
        public Response TransportEnd(string req)
        {
            using (var conn = new DB())
            {
                var resp = new Response { status = Config.SUCCESS };
                var cmd = conn.CreateCommand();
                var m_OraTrans = conn.OraTrans();
                DateTime endTime = DateTime.Now;
                try
                {
                    if (HttpContext.User.Identity != null)
                    {
                        cmd.CommandText = "UPDATE TRANSPORT SET " +
                                          $"END_TIME=TO_DATE('{endTime:yyyy-MM-dd HH:mm:ss}','yyyy-mm-dd hh24:mi:ss')," +
                                          $"PLANT_NAME= (SELECT PLANT_NAME FROM STAFF WHERE STAFF_ID=:staff_id)" +
                                          $"WHERE TRANS_ID=:trans_id";
                        //添加参数
                        cmd.Parameters.Add(":staff_id", HttpContext.User.Identity.Name);
                        cmd.Parameters.Add(":trans_id", req);
                        //执行
                        Console.WriteLine(cmd.CommandText);
                        //增加一条交互记录
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "INSERT INTO INTERACT " +
                                          $"VALUES(:staff_id," +
                                          $"TO_DATE('{endTime:yyyy-MM-dd HH:mm:ss}','yyyy-mm-dd hh24:mi:ss')," +
                                          $"'F',:trans_id)";
                        cmd.ExecuteNonQuery();
                        m_OraTrans.Commit();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    //m_OraTrans.Rollback();
                    resp.status = Config.FAIL;
                }
                
                return resp;
            }
        }

        // /// <summary>
        // /// 以垃圾编号获取对应垃圾运输记录
        // /// </summary>
        // /// <returns></returns>
        // [HttpGet]
        // [Authorize(Roles = "Administrator")]
        // public Transport Get(string req)
        // {
        // var resp = new Transport();
        //     try
        //
        // {
        //     var conn=new DB();
        //     var cmd = conn.CreateCommand();
        //     cmd.CommandText = "SELECT* " +
        //                       "FROM transport " +
        //                       $"WHERE gar_id='{req}'";
        //     var reader = cmd.ExecuteReader();
        //     while (reader.Read())
        //     {
        //         resp.gar_id = reader["gar_id"].ToString();
        //         resp.plant_name = reader["plant_name"].ToString();
        //         resp.dustbin_id = reader["dustbin_id"].ToString();
        //         resp.truck_id = reader["truck_id"].ToString();
        //         resp.start_time = Convert.ToDateTime(reader["start_time"].ToString());
        //         resp.end_time = Convert.ToDateTime(reader["end_time"].ToString());
        //     }
        //
        //     DBConn.CloseConn(conn);
        // }
        // catch (Exception ex) {
        // Console.WriteLine(ex.Message);
        // }
        // return resp;

        /// <summary>
        /// 检查员查看当前垃圾站的运输记录，输入：垃圾站编号
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Administrator,Watcher")]
        public List<Transport> WatcherGet(string req)
        {
            using (var conn = new DB())
            {
                var resp = new List<Transport>();
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT TRANS_ID,DUSTBIN_ID,CARRIER_ID,TRUCK_ID,START_TIME,END_TIME,PLANT_NAME " +
                                      "FROM TRANSPORT NATURAL JOIN DUSTBIN " +
                                      $"WHERE SITE_NAME=:site_name " +
                                      "ORDER BY START_TIME DESC";
                    //添加参数                
                    cmd.Parameters.Add(":site_name", req);
                    //执行
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var temp = new Transport
                        {
                            trans_id = reader.GetString(0),
                            dustbin_id = reader.GetString(1),
                            carrier_id = reader.GetString(2),
                            truck_id = reader.GetString(3),
                            start_time = reader.GetDateTime(4),
                            end_time = reader.GetDateTime(5),
                            plant_name = reader.GetString(6)
                        };
                        resp.Add(temp);
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
        /// 运输员查看自己运输记录，输入：运输员id
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Administrator,Carrier")]
        public List<Transport> CarrierGet(string req)
        {
            using (var conn = new DB())
            {
                var resp = new List<Transport>();
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT TRANS_ID,DUSTBIN_ID,CARRIER_ID,TRUCK_ID,START_TIME,END_TIME,PLANT_NAME " +
                                      "FROM TRANSPORT " +
                                      $"WHERE CARRIER_ID=:carrier_id " +
                                      "ORDER BY START_TIME DESC";
                    //添加参数                
                    cmd.Parameters.Add(":carrier_id", req);
                    //执行
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var temp = new Transport
                        {
                            trans_id = reader.GetString(0),
                            dustbin_id = reader.GetString(1),
                            carrier_id = reader.GetString(2),
                            truck_id = reader.GetString(3),
                            start_time = reader.GetDateTime(4),
                            end_time = reader.IsDBNull(5) ? reader.GetDateTime(4) : reader.GetDateTime(5),
                            plant_name = reader.IsDBNull(6) ? "" : reader.GetString(6)
                        };
                        resp.Add(temp);
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
        /// 按照垃圾处理站查找运输记录，输入处理站编号
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Administrator,StationStaff")]
        public List<Transport> StaffGet(string req)
        {
            using (var conn = new DB())
            {
                var resp = new List<Transport>();
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT TRANS_ID,DUSTBIN_ID,CARRIER_ID,TRUCK_ID,START_TIME,END_TIME,PLANT_NAME " +
                                      "FROM TRANSPORT " +
                                      $"WHERE PLANT_NAME=:plant_name OR PLANT_NAME is null " +
                                      "ORDER BY START_TIME DESC";
                    //添加参数                
                    cmd.Parameters.Add(":plant_name", req);
                    //执行
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var temp = new Transport
                        {
                            trans_id = reader.GetString(0),
                            dustbin_id = reader.GetString(1),
                            carrier_id = reader.GetString(2),
                            truck_id = reader.GetString(3),
                            start_time = reader.GetDateTime(4),
                            end_time = reader.IsDBNull(5) ? reader.GetDateTime(4) : reader.GetDateTime(5),
                            plant_name = reader.IsDBNull(6) ? "" : reader.GetString(6)
                        };
                        resp.Add(temp);
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
        /// 获取总的运输记录，不需要参数
        /// </summary>        
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public List<Transport> GetAll()
        {
            using (var conn = new DB())
            {
                var resp = new List<Transport>();
                try
                {

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT TRANS_ID,DUSTBIN_ID,CARRIER_ID,TRUCK_ID,START_TIME,END_TIME,PLANT_NAME " +
                                      "FROM TRANSPORT " +
                                      "ORDER BY START_TIME DESC";
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var temp = new Transport
                        {
                            trans_id = reader.GetString(0),
                            dustbin_id = reader.GetString(1),
                            carrier_id = reader.GetString(2),
                            truck_id = reader.GetString(3),
                            start_time = reader.GetDateTime(4),
                            end_time = reader.IsDBNull(5) ? reader.GetDateTime(4) : reader.GetDateTime(5),
                            plant_name = reader.IsDBNull(6) ? "" : reader.GetString(6)
                        };
                        resp.Add(temp);
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
    /// 有关违规记录处理的API
    /// </summary>
    [ApiController, Route("[controller]/[action]")]
    public class ViolateRecordController : Controller
    {
        /// <summary>
        /// 增加违规记录，同时对应修改用户积分credit
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost] //Authorize(Roles = "Administrator")]
        [Authorize(Roles = "Watcher")]
        public AddResponse Add([FromBody] ViolateRecord req)
        {
            using (var conn = new DB())
            {
                var resp = new AddResponse();
                var cmd = conn.CreateCommand();
                var dt = DateTime.Now;
                try
                {
                    //INSERT INTO "C##PDCR"."VIOLATE_RECORD" ("WATCHER_ID", "REASON", "PUNISHMENT", "VIOLATE_TIME", "GAR_ID") VALUES ('2952108', '123', '1', TO_DATE('2021-07-20 11:35:11', 'SYYYY-MM-DD HH24:MI:SS'), '11111111');
                    cmd.CommandText = "INSERT INTO violate_record " +
                                      $"VALUES(:watcher_id,:reason,:punishment," +
                                      $"TO_DATE('{dt:yyyy-MM-dd hh:mm:ss}','yyyy-mm-dd hh24:mi:ss'),:gar_id)";
                    //添加参数
                    cmd.Parameters.Add(":watcher_id", req.watcher_id);
                    cmd.Parameters.Add(":reason", req.reason);
                    cmd.Parameters.Add(":punishment", req.punishment);
                    cmd.Parameters.Add(":gar_id", req.gar_id);
                    //执行
                    cmd.ExecuteNonQuery();
                    resp.status = Config.SUCCESS;
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
        /// 修改违规记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost] //Authorize(Roles = "Administrator")]
        [Authorize(Roles = "Administrator,Watcher")]
        public UpdateResponse Update([FromBody] ViolateRecord req)
        {
            using (var conn = new DB())
            {
                var resp = new UpdateResponse();
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "UPDATE violate_record " +
                                      $"SET reason=:reason,punishment=:punishment " +
                                      $"WHERE gar_id=:gar_id";
                    //添加参数              
                    cmd.Parameters.Add(":reason", req.reason);
                    cmd.Parameters.Add(":punishment", req.punishment);
                    cmd.Parameters.Add(":gar_id", req.gar_id);
                    //执行
                    var k = cmd.ExecuteNonQuery();
                    if (k == 1)
                    {
                        resp.status = Config.SUCCESS;
                        resp.updateMessage = "更新成功";
                    }
                    else
                    {
                        resp.status = Config.FAIL;
                        resp.updateMessage = "更新失败，未找到指定记录";
                    }
                }
                catch (Exception ex)
                {
                    resp.status = Config.FAIL;
                    resp.updateMessage = ex.Message;
                }

                conn.Close();
                return resp;
            }
        }

        /// <summary>
        /// 删除违规记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet] //Authorize(Roles = "Administrator")]
        [Authorize(Roles = "Watcher,Administrator")]
        public DeleteResponse Delete(string req)
        {
            using (var conn = new DB())
            {
                var resp = new DeleteResponse();
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "DELETE FROM VIOLATE_RECORD " +
                                      $"WHERE gar_id=:gar_id";
                    //添加参数               
                    cmd.Parameters.Add(":gar_id", req);
                    //执行
                    var k = cmd.ExecuteNonQuery();
                    if (k == 1)
                    {
                        resp.status = Config.SUCCESS;
                        resp.deleteMessage = "删除成功";
                    }
                    else
                    {
                        resp.status = Config.FAIL;
                        resp.deleteMessage = "删除失败，未找到指定记录";
                    }
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
        /// 得到对应投放人编号的违规记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "GarbageMan,Administrator")]
        public List<ViolateRecord> Get(string req)
        {
            using (var conn = new DB())
            {
                var respList = new List<ViolateRecord>();
                try
                {

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT gar_id,watcher_id,reason,punishment,violate_time " +
                                      "FROM violate_record natural join GARBAGE " +
                                      $"WHERE user_id=:user_id " +
                                      "ORDER BY violate_time DESC";
                    //添加参数               
                    cmd.Parameters.Add(":user_id", req);
                    //执行
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var resp = new ViolateRecord
                        {
                            user_id = "",
                            gar_id = reader.GetString(0),
                            watcher_id = reader.GetString(1),
                            reason = reader.GetString(2),
                            punishment = reader.GetInt32(3),
                            violate_time = reader.GetDateTime(4)
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
        /// 以检查员的ID获取违规记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Watcher,Administrator")]
        public List<ViolateRecord> GetAll(string req)
        {
            using (var conn = new DB())
            {
                var respList = new List<ViolateRecord>();
                try
                {

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT gar_id,user_id,reason,punishment,violate_time " +
                                      "FROM violate_record natural join GARBAGE " +
                                      $"WHERE watcher_id=:watcher_id " +
                                      "ORDER BY violate_time DESC";
                    //添加参数               
                    cmd.Parameters.Add(":watcher_id", req);
                    //执行
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var resp = new ViolateRecord
                        {
                            watcher_id = "",
                            gar_id = reader.GetString(0),
                            user_id = reader.GetString(1),
                            reason = reader.GetString(2),
                            punishment = reader.GetInt32(3),
                            violate_time = reader.GetDateTime(4)
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
    }

    /// <summary>
    /// 有关Carrier和StationStaff交互记录的API
    /// </summary>
    [ApiController, Route("[controller]/[action]")]
    public class InteractController : Controller
    {
        /// <summary>
        /// 增加交互记录（interact_time不需要传入）
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost, Authorize(Roles = "StationStaff")]
        public AddResponse Add([FromBody] Interact req)
        {
            using (var conn = new DB())
            {
                var resp = new AddResponse();
                try
                {
                    var cmd = conn.CreateCommand();
                    if (HttpContext.User.Identity == null)
                        throw (new Exception("HttpContext.User.Identity is null"));
                    cmd.CommandText = "INSERT INTO interact " +
                                      $"VALUES(:staff_id,TO_DATE('{DateTime.Now:yyyy-MM-dd HH:mm:ss}','SYYYY-MM-DD HH24:MI:SS')," +
                                      $":interact_result,:trans_id)";
                    //添加参数               
                    cmd.Parameters.Add(":staff_id", HttpContext.User.Identity.Name);
                    cmd.Parameters.Add(":interact_result", req.interact_result);
                    cmd.Parameters.Add(":trans_id", req.trans_id);
                    //执行
                    cmd.ExecuteNonQuery();
                    resp.status = Config.SUCCESS;
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
        /// 修改交互记录(只需传入主码trans_id即可)
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet, Authorize(Roles = "Administrator,StationStaff")]
        public UpdateResponse Update(string req)
        {
            using (var conn = new DB())
            {
                var resp = new UpdateResponse();
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "UPDATE interact " +
                                      $"SET interact_result='S' " +
                                      $"WHERE trans_id=:trans_id";
                    //添加参数                              
                    cmd.Parameters.Add(":trans_id", req);
                    //执行
                    var k = cmd.ExecuteNonQuery();
                    if (k == 1)
                    {
                        resp.status = Config.SUCCESS;
                        resp.updateMessage = "更新成功";
                    }
                    else
                    {
                        resp.status = Config.FAIL;
                        resp.updateMessage = "更新失败，未找到指定记录";
                    }
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
        /// 得到对应处理人员编号的交互记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Administrator,StationStaff")]
        public List<Interact> Get(string req)
        {
            using (var conn = new DB())
            {
                var respList = new List<Interact>();
                try
                {

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT* " +
                                      "FROM interact " +
                                      $"WHERE staff_id='{req}'" +
                                      $"ORDER BY interact_time DESC";
                    //添加参数               
                    cmd.Parameters.Add(":staff_id", req);
                    //执行
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var resp = new Interact
                        {
                            trans_id = reader["trans_id"].ToString(),
                            staff_id = reader["staff_id"].ToString(),
                            interact_time = reader.GetDateTime(1),
                            interact_result = Convert.ToChar(reader["interact_result"].ToString() ?? string.Empty)
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
        /// 得到对应处理人员还未进行处理的交互记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Administrator,StationStaff")]
        public List<Interact> GetUnfinished(string req)
        {
            using (var conn = new DB())
            {
                var respList = new List<Interact>();
                try
                {

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT * FROM interact " +
                                      $"WHERE staff_id =:staff_id " +
                                      $"AND interact_result = 'F'";
                    //添加参数               
                    cmd.Parameters.Add(":staff_id", req);
                    //执行
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var resp = new Interact
                        {
                            trans_id = reader["trans_id"].ToString(),
                            staff_id = reader["staff_id"].ToString(),
                            interact_time = reader.GetDateTime(1),
                            interact_result = Convert.ToChar(reader["interact_result"].ToString() ?? string.Empty)
                        };
                        respList.Add(resp);
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                conn.Close();
                return respList;
            }
        }

        /// <summary>
        /// 管理员获取所有的交接记录
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Administrator,StationStaff")]
        public List<Interact> GetAll()
        {
            using (var conn = new DB())
            {
                var respList = new List<Interact>();
                try
                {

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT* " +
                                      "FROM interact " +
                                      $"ORDER BY interact_time DESC";
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var resp = new Interact
                        {
                            trans_id = reader["trans_id"].ToString(),
                            staff_id = reader["staff_id"].ToString(),
                            interact_time = reader.GetDateTime(1),
                            interact_result = Convert.ToChar(reader["interact_result"].ToString() ?? string.Empty)
                        };
                        respList.Add(resp);
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                conn.Close();
                return respList;
            }
        }
    }
}