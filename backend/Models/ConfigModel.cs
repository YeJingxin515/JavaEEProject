using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace DBPractice.Models
{
    public static class Config
    {
        public static int EMPTY = -3;//空值
        public static int TEST = -2;//测试返回值
        public static readonly int SUCCESS = 1;//成功
        public static readonly int FAIL = -1;//失败
        //private static readonly string HOST = "220.179.227.205";//"localhost";
        private static readonly string HOST = "localhost";//"localhost";
        private static readonly string USERNAME = "C##PDCR";//"system";//
        private static readonly string PASSWORD = "LTY20001212";//"Db123456";//
        private static readonly string SERVICENAME = "ORCL";//"DUSBIN";//
        private static readonly string PORT = "6001";
        public static readonly string CONN = $"Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={HOST})(PORT={PORT})))" +
                                             $"(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME={SERVICENAME})));" +
                                             $"User Id={USERNAME};" + $"Password={PASSWORD};";

        public static readonly string IMAGEPATH = "../../image/";
    }

    public class DB: IDisposable
    {
        private OracleConnection Conn;

        public DB()
        {
            Conn = new OracleConnection {ConnectionString = Config.CONN};           
            Conn.Open();
            //Console.WriteLine("open!");
        }
        public void Close()
        {
            Conn.Close();
            //Console.WriteLine("close111!");
        }
        void IDisposable.Dispose()
        {
            Conn.Close();
            Conn.Dispose();
            //Console.WriteLine("close333!");            
        }
        ~DB()
        {
            Conn.Close();
            //Console.WriteLine("close222!");
        }

        public OracleCommand CreateCommand()
        {
            try
            {
                return Conn.CreateCommand();
            }
            catch (Exception ex)
            {
                Console.Write("创建OracleCommand时出错");
                Console.WriteLine(ex.Message);
            }
            return null;          
        }
        public OracleTransaction OraTrans()
        {
            return Conn.BeginTransaction();
        }
    }
    
    public static class DBConn
    {
        public static OracleConnection OpenConn()
        {
            var conn = new OracleConnection {ConnectionString = Config.CONN};
            conn.Open();
            return conn;
        }

        public static void CloseConn(OracleConnection conn)
        {
            if (conn == null) { return; }
            else
            {
                conn.Close();
            }
        }
        //使用16位md5对密码进行加密
        public static string MD5Encrypt16(string password)
        {
            var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            var t2 = BitConverter.ToString(md5.ComputeHash(Encoding.Default.GetBytes(password)), 4, 8);
            t2 = t2.Replace("-", "");
            var md = MD5.Create();
            return t2;
        }
    }
}