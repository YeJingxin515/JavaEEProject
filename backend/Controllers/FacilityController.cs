using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using DBPractice.Models;
using Microsoft.AspNetCore.Authorization;
using Oracle.ManagedDataAccess.Client;

namespace DBPractice.Controllers
{
    ///<summary>
    /// 垃圾桶相关的api接口
    /// 请注意垃圾桶的属性 condition是char类型的
    /// </summary>>
    [ApiController, Route("Facility/[controller]/[action]")]
    public class TrashCanController : Controller
    {
        /// <summary>　　
        /// 垃圾桶的添加
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public AddResponse Add([FromBody] TrashCan req)
        { 
            using (var conn = new DB())
            {
                var resp = new AddResponse();
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO dustbin " +
                                            $"VALUES(:id,:type,:condition,:siteName)";
                    //添加参数
                    cmd.Parameters.Add(":id", req.id);
                    cmd.Parameters.Add(":type", req.type);
                    cmd.Parameters.Add(":condition", req.condition);
                    cmd.Parameters.Add(":siteName", req.siteName);
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
        /// 垃圾桶的删除
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Authorize(Roles="Administrator")]
        [HttpPost]
        public DeleteResponse Delete([FromBody] TrashCan req)
        {
            using (var conn = new DB())
            {
                var resp = new DeleteResponse();
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "DELETE FROM dustbin " +
                                            $"WHERE dustbin_id=:id";
                    //添加参数
                    cmd.Parameters.Add(":id", req.id);
                    //执行
                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        resp.status = Config.FAIL;
                        resp.deleteMessage = "未找到指定ID的垃圾桶";
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
        /// 垃圾桶的状态更新
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Authorize(Roles="Administrator,Watcher")]
        [HttpPost]
        public UpdateResponse Update([FromBody] TrashCan req)
        {
            using (var conn = new DB())
            {
                var resp = new UpdateResponse();
                try
                {
                    var cmd = conn.CreateCommand();
                    if (!string.IsNullOrEmpty(req.siteName))
                    {
                        cmd.CommandText = "UPDATE dustbin " +
                                            $"SET site_name=:siteName" +
                                            $" WHERE dustbin_id=:id";
                        //添加参数
                        cmd.Parameters.Add(":siteName", req.siteName);
                        cmd.Parameters.Add(":id", req.id);
                        //执行
                        if (cmd.ExecuteNonQuery() == 0)
                        {
                            resp.status = Config.FAIL;
                            resp.updateMessage = "未找到指定ID的垃圾桶";

                            return resp;
                        }
                    }
                    if (req.condition != ' ')
                    {
                        cmd.CommandText = "UPDATE dustbin " +
                                            $"SET condition=:condition" +
                                            $" WHERE dustbin_id=:id";
                        //清空
                        cmd.Parameters.Clear();
                        //重新添加
                        cmd.Parameters.Add(":condition", req.condition);
                        cmd.Parameters.Add(":id", req.id);
                        //执行
                        if (cmd.ExecuteNonQuery() == 0)
                        {
                            resp.status = Config.FAIL;
                            resp.updateMessage = "未找到指定ID的垃圾桶";

                            return resp;
                        }
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
        /// 获取某一个垃圾桶的属性
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Authorize(Roles="Administrator,Watcher")]
        [HttpGet]
        public TrashCan Get(string req)
        {
            using (var conn = new DB())
            {
                var resp = new TrashCan();
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT* " +
                                      "FROM dustbin " +
                                     $"WHERE dustbin_id=:id";
                    //添加参数
                    cmd.Parameters.Add(":id", req);
                    //执行
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        resp.id = reader["dustbin_id"].ToString();
                        resp.type = reader["dustbin_type"].ToString();
                        resp.condition = Convert.ToChar(reader["condition"].ToString() ?? throw new InvalidOperationException());
                        resp.siteName = reader["site_name"].ToString();
                    }
                    reader.Close();
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }              
                return resp;
            }
        }
        /// <summary>
        /// 获取指定垃圾站所有垃圾桶一个列表
        /// </summary>
        /// <returns></returns>       
        [HttpGet]
        [Authorize(Roles = "Administrator,Watcher")]
        public List<TrashCan> GetSiteAll(string req)
        {
            using (var conn = new DB())
            {
                var respList = new List<TrashCan>();
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT* " +
                                      "FROM dustbin " +
                                     $"WHERE site_name=:site_name";
                    //添加参数
                    cmd.Parameters.Add(":site_name", req);
                    //执行
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var resp = new TrashCan
                        {
                            id = reader["dustbin_id"].ToString(),
                            type = reader["dustbin_type"].ToString(),
                            condition = Convert.ToChar(reader["condition"].ToString() ?? throw new InvalidOperationException()),
                            siteName = reader["site_name"].ToString()
                        };
                        respList.Add(resp);
                    }
                    reader.Close();
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }               
                return respList;
            }
        }
        /// <summary>
        /// 获取所有垃圾桶一个列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Administrator,Watcher,Carrier")]
        public List<TrashCan> GetAll()
        {
            using (var conn = new DB())
            {
                var respList = new List<TrashCan>();               
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT* " +
                                      "FROM dustbin ";
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var resp = new TrashCan
                        {
                            id = reader["dustbin_id"].ToString(),
                            type = reader["dustbin_type"].ToString(),
                            condition = Convert.ToChar(reader["condition"].ToString() ??
                                                       throw new InvalidOperationException()),
                            siteName = reader["site_name"].ToString()
                        };
                        respList.Add(resp);
                    }
                    reader.Close();
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }                
                return respList;
            }
        }
        /// <summary>
        /// 获取垃圾桶中的垃圾数量，输入：垃圾桶编号
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Administrator,Watcher")]
        public int GetCount(string req)
        {
            //SELECT count(*) AS c FROM GARBAGE NATURAL JOIN THROW WHERE throw.DUSTBIN_ID ='D002' AND garbage.TRANS_ID IS NULL  
            using (var conn = new DB())
            {
                int count = 0;
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT count(*) AS c " +
                                      "FROM GARBAGE NATURAL JOIN THROW " +
                                     $"WHERE throw.DUSTBIN_ID =:id AND garbage.TRANS_ID IS NULL";
                    //添加参数
                    cmd.Parameters.Add(":id", req);
                    //执行
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        count = reader.GetInt32(0);
                    }
                    reader.Close();
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }               
                return count;
            }
        }
    }
    /// <summary>
    /// 垃圾站
    /// </summary>
    [ApiController, Route("Facility/[controller]/[action]")]
    public class BinSiteController : Controller
    {
        /// <summary>
        /// 垃圾站的添加
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public AddResponse Add([FromBody] BinSite req)
        {
            using (var conn = new DB())
            {
                var resp = new AddResponse();
                try
                {

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO binsite " +
                                      $"VALUES(:name,:location)";
                    //添加参数
                    cmd.Parameters.Add(":name", req.name);
                    cmd.Parameters.Add(":location", req.location);
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
        /// 以垃圾站编号删除垃圾站
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public DeleteResponse Delete(string req)
        {
            using (var conn = new DB())
            {
                var resp = new DeleteResponse();
                try
                {

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "DELETE FROM binsite " +
                                      $"WHERE site_name=:name";
                    //添加参数
                    cmd.Parameters.Add(":name", req);
                    //执行
                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        resp.status = Config.FAIL;
                        resp.deleteMessage = "未找到指定ID";

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
        /// 垃圾站的属性更新
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public UpdateResponse Update([FromBody] BinSite req)
        {
            using (var conn = new DB())
            {
                var resp = new UpdateResponse();
                try
                {

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "UPDATE binsite " + $"SET bin_location=:location" +
                                     $" WHERE site_name=:name";
                    //添加参数
                    cmd.Parameters.Add(":location", req.location);
                    cmd.Parameters.Add(":name", req.name);
                    //执行
                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        resp.status = Config.FAIL;
                        resp.updateMessage = "未找到指定ID";

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
        /// 获取一个垃圾站的信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public BinSite Get(string req)
        {
            using (var conn = new DB())
            {
                var resp = new BinSite();
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT* " +
                                      "FROM binsite " +
                                      $"WHERE site_name=:site_name";
                    //添加参数
                    cmd.Parameters.Add(":site_name", req);
                    //执行
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        resp.name = reader["site_name"].ToString();
                        resp.location = reader["bin_location"].ToString();
                    }
                    reader.Close();
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }              
                return resp;
            }
        }
        /// <summary>
        /// 获取所有垃圾站的一个列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<BinSite> GetAll()
        {
            using (var conn = new DB())
            {
                var respList = new List<BinSite>();
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT* " +
                                      "FROM binsite";
                    OracleDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var resp = new BinSite
                        {
                            name = reader["site_name"].ToString(),
                            location = reader["bin_location"].ToString()
                        };
                        respList.Add(resp);
                    }
                    reader.Close();
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }                
                return respList;
            }
        }
    }

    /// <summary>
    /// 垃圾车
    /// </summary>
    [ApiController, Route("Facility/[controller]/[action]")]
    public class TruckController : Controller
    {
        /// <summary>
        /// 垃圾车的添加
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public AddResponse Add([FromBody] Truck req)
        {
            using (var conn = new DB())
            {
                var resp = new AddResponse();
                try
                {

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO truck " +
                                      $"VALUES(:id,:condition,:carrierID)";
                    //添加参数
                    cmd.Parameters.Add(":id", req.id);
                    cmd.Parameters.Add(":condition", req.condition);
                    cmd.Parameters.Add(":carrierID", req.carrierID);
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
        /// 垃圾车的删除
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public DeleteResponse Delete([FromBody] Truck req)
        {
            using (var conn = new DB())
            {
                var resp = new DeleteResponse();
                try
                {

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "DELETE FROM truck " +
                                      $"WHERE truck_id =:id";
                    //添加参数
                    cmd.Parameters.Add(":id", req.id);
                    //执行
                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        resp.status = Config.FAIL;
                        resp.deleteMessage = "未找到指定ID";

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
        /// 垃圾车的状态更新
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrator,Carrier")]
        public UpdateResponse Update([FromBody] Truck req)
        {
            using (var conn = new DB())
            {
                var resp = new UpdateResponse();
                try
                {

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "UPDATE truck " + $"SET condition =:condition" +
                                      $" WHERE truck_id =:id";
                    //添加参数
                    cmd.Parameters.Add(":condition", req.condition);
                    cmd.Parameters.Add(":id", req.id);
                    //执行
                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        resp.status = Config.FAIL;
                        resp.updateMessage = "未找到指定ID";
                    }
                    else
                    {
                        resp.status = Config.SUCCESS;
                        resp.updateMessage = "更改成功";
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
        /// 获取某一个垃圾车的属性
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Administrator,Carrier")]
        public Truck Get(string req)
        {
            using (var conn = new DB())
            {
                var resp = new Truck();
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT* " +
                                      "FROM truck " +
                                      $"WHERE truck_id=:id";
                    //添加参数
                    cmd.Parameters.Add(":id", req);
                    //执行
                    OracleDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        resp.id = reader["truck_id"].ToString();
                        resp.condition =
                            Convert.ToChar(reader["condition"].ToString() ?? throw new InvalidOperationException());
                        resp.carrierID = reader["carrier_id"].ToString();
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
        /// 获取所有垃圾车的一个列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Administrator,Carrier")]
        public List<Truck> GetAll()
        {
            using (var conn = new DB())
            {
                var respList = new List<Truck>();
                try
                {

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT* " +
                                      "FROM truck";
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var resp = new Truck
                        {
                            id = reader["truck_id"].ToString(),
                            condition = Convert.ToChar(reader["condition"].ToString() ??
                                                       throw new InvalidOperationException()),
                            carrierID = reader["carrier_id"].ToString()
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
        /// 获取当前空闲的车辆，返回一个可用的ID列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Administrator,Carrier")]
        public List<String> GetFree()
        {
            using (var conn = new DB())
            {
                var resp = new List<String>();
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "select TRUCK_ID from truck where CARRIER_ID is null and CONDITION!='B'";
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var temp = reader.GetString(0);
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
        /// 离开车辆，车辆状态设置为null
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Carrier")]
        public void OffWork()
        {
            using (var conn = new DB())
            {
                var resp = new DeleteResponse();
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "UPDATE truck " + $"SET carrier_id = null " +
                                      $"WHERE carrier_id = :id";//?
                                                                //添加参数
                    cmd.Parameters.Add(":id", HttpContext.User.Identity.Name);
                    //执行
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
              
                return;
            }
        }

        /// <summary>
        /// 切换车辆，原来的变成null
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Carrier")]
        public UpdateResponse SetTruck(string req)
        {
            using (var conn = new DB())
            {
                var resp = new UpdateResponse();
                var m_OraTrans = conn.OraTrans();
                try
                {

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "UPDATE TRUCK " + "SET CARRIER_ID = null " +
                                      $"WHERE CARRIER_ID = :id";
                    //添加参数
                    cmd.Parameters.Add(":id", HttpContext.User.Identity.Name);
                    //执行
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "UPDATE TRUCK " + $"SET CARRIER_ID = :id " +
                                      $"WHERE TRUCK_ID = :truck_id";
                    //添加参数
                    cmd.Parameters.Add(":truck_id", req); //?
                                                          //执行
                    cmd.ExecuteNonQuery();
                    m_OraTrans.Commit();
                    resp.updateMessage = "更新成功";
                    resp.status = Config.SUCCESS;
                }
                catch (Exception ex)
                {
                    m_OraTrans.Rollback();
                    Console.WriteLine(ex.Message);
                    resp.updateMessage = ex.Message;
                    resp.status = Config.FAIL;
                }               
                return resp;
            }
        }

        /// <summary>
        /// 获取卡车信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Carrier")]
        public string getTruck()
        {
            using (var conn = new DB())
            {
                var resp = "";
                try
                {
                    var cmd = conn.CreateCommand();
                    if (HttpContext.User.Identity == null)
                        throw (new Exception("HttpContext.User.Identity is null"));
                    cmd.CommandText = $"select TRUCK_ID from truck where CARRIER_ID=:id";
                    //添加参数
                    cmd.Parameters.Add(":id", HttpContext.User.Identity.Name);
                    //执行
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        resp = reader.GetString(0);
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
    /// 垃圾处理站
    /// </summary>
    [ApiController, Route("Facility/[controller]/[action]")]
    public class PlantController : Controller
    {
        /// <summary>
        /// 垃圾处理站的添加
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public AddResponse Add([FromBody] Plant req)
        {
            using (var conn = new DB())
            {
                var resp = new AddResponse();
                try
                {

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO plant " +
                                            $"VALUES(:name,:address)";
                    //添加参数
                    cmd.Parameters.Add(":name", req.name);
                    cmd.Parameters.Add(":address", req.address);
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
        /// 垃圾处理站的删除
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public DeleteResponse Delete([FromBody] Plant req)
        {
            using (var conn = new DB())
            {
                var resp = new DeleteResponse();
                try
                {

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "DELETE FROM plant " +
                                            $"WHERE plant_name =:name";
                    //添加参数
                    cmd.Parameters.Add(":name", req.name);
                    //执行
                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        resp.status = Config.FAIL;
                        resp.deleteMessage = "未找到指定ID";

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
        /// 垃圾处理站的更新
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public UpdateResponse Update([FromBody] Plant req)
        {
            using (var conn = new DB())
            {
                var resp = new UpdateResponse();
                try
                {

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "UPDATE plant " + $"SET address =:address " +
                                      $"WHERE plant_name =:name";
                    //添加参数
                    cmd.Parameters.Add(":address", req.address);
                    cmd.Parameters.Add(":name", req.name);
                    //执行
                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        resp.status = Config.FAIL;
                        resp.updateMessage = "未找到指定ID";

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
        /// 获取某一个垃圾处理站的信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Administrator,StationStaff")]
        public Plant Get(string req)
        {
            using (var conn = new DB())
            {
                var resp = new Plant();
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT* " +
                                      "FROM plant " +
                                     $"WHERE plant_name=:name";
                    //添加参数
                    cmd.Parameters.Add(":name", req);
                    //执行
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        resp.name = reader["plant_name"].ToString();
                        resp.address = reader["address"].ToString();
                    }
                    reader.Close();
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }               
                return resp;
            }
        }
        /// <summary>
        /// 获取所有垃圾处理站的信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public List<Plant> GetAll()
        {
            using (var conn = new DB())
            {
                var respList = new List<Plant>();
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT* " +
                                      "FROM plant";
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var resp = new Plant
                        {
                            name = reader["plant_name"].ToString(),
                            address = reader["address"].ToString()
                        };
                        respList.Add(resp);
                    }
                    reader.Close();
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }                
                return respList;
            }
        }
    }
}