using MySQLiteUtile.MySQLiteFlod;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace MySQLiteUtile.MySQLiteTest
{
    public class MyTestManager : DBBaseManager
    {
        public MyTestManager()
        {
            this._dbBaseFactory = new MyTestFactory();
        }
        private static object syncObj = new object();
        private static MyTestManager instance = null;
        public static MyTestManager GetInstance()
        {
            lock (syncObj)
            {
                if (instance == null)
                    instance = new MyTestManager();
            }
            return instance;
        }


        public override string DbPostfix
        {
            get
            {
                return "db";
            }
            set
            {

            }
        }
        public override string DbDirectory
        {
            get
            {
                return Environment.CurrentDirectory;
            }
            set
            {

            }
        }
        public override string DbName
        {
            get
            {
                return "testDatabase";
            }
            set
            {

            }
        }

        public void Initialize()
        {
            if (!File.Exists(DbPath))
            {
                //创建官方的库
                List<MyTestFactory> listMyTestFactory = new List<MyTestFactory>();
                MyTestFactory MyTestFactory = new MyTestFactory();
                listMyTestFactory.Add(MyTestFactory);
                base.DbCreate(listMyTestFactory);
            }
            else
                base.DbOpen(DbPath);
        }
        
        public void InsetDataToMyTestFactoryTable(DataTable dataTable)
        {
            if (dataTable != null && this._dbHelper != null)
            {
                this._dbHelper.DropTable(_dbBaseFactory.GetType().Name);
                this._dbHelper.Insert(_dbBaseFactory.GetType().Name, dataTable);
            }
            else
            {
                throw new Exception("数据文件为空！");
            }
        }
        public DataTable QueryAllSource()
        {
            string sql = "select * from  " + _dbBaseFactory.GetType().Name;
            return this._dbHelper.Select(_dbBaseFactory.GetType().Name, sql);
        }
        public List<MyTestFactory> QuerySource()
        {
            string sql = "select * from  " + _dbBaseFactory.GetType().Name;
            DataTable dt =  this._dbHelper.Select(_dbBaseFactory.GetType().Name, sql);
            return this.DataTable2Records<MyTestFactory>(dt);
        }
    }
}
