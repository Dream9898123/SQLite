using Bingo.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace MySQLiteUtile.MySQLiteFlod
{
    public abstract class DBBaseManager
    {
        /// <summary>
        /// 用于操作数据库
        /// </summary>
        protected SQLiteHelper _dbHelper;
        protected SQLiteHelper _historyDbHelper;

        public abstract string DbPostfix
        {
            get;
            set;
        }
        public abstract string DbDirectory
        {
            get;
            set;
        }
        public abstract string DbName
        {
            get;
            set;
        }
        public virtual string DbPassword
        {
            get;
            set;
        }
        public virtual string DbPath
        {
            get { return Path.Combine(DbDirectory, this.DbName + "." + DbPostfix); }
        }

        public virtual void DbCreate(IEnumerable<DBBaseFactory> entities = null)
        {
            //if (File.Exists(DbPath))
            //    this.DbOpen(this.DbPath);
            //else if (SQLiteHelper.TestConnection(DbPath, DbPassword))
            //{//创建数据库
            //    this.DbOpen(this.DbPath);
            //    this.DbCreateTables(entities);
            //}
            if (!File.Exists(DbPath))
                SQLiteHelper.TestConnection(DbPath, DbPassword);//创建空数据库
            if (File.Exists(DbPath))
            {
                this.DbOpen(this.DbPath);
                this.DbCreateTables(entities);
            }
        }
        public virtual void DbOpen(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || !File.Exists(fileName))
                return;
            if (fileName == this.DbPath)
            {
                if (this._historyDbHelper != null && !this._historyDbHelper.IsDisposed)
                    this._historyDbHelper.Dispose();
                this._historyDbHelper = null;

                if (this._dbHelper != null && !this._dbHelper.IsDisposed)
                    this._dbHelper.Dispose();
                var conn = new SQLiteConnection(SQLiteHelper.GetConnectionString(this.DbPath, this.DbPassword));
                var cmd = new SQLiteCommand(conn);
                conn.Open();
                this._dbHelper = new SQLiteHelper(cmd);
            }
            else
            {
                if (this._historyDbHelper != null && !this._historyDbHelper.IsDisposed)
                    this._historyDbHelper.Dispose();
                var conn = new SQLiteConnection(SQLiteHelper.GetConnectionString(this.DbPath, this.DbPassword));
                var cmd = new SQLiteCommand(conn);
                conn.Open();
                this._historyDbHelper = new SQLiteHelper(cmd);
            }
        }
        public virtual void DbCreateTables(IEnumerable<IDbEntity> entities = null)
        {
            entities.ForAll(t =>
            {
                //this._dbHelper.DropTable(t.GetType().Name);
                if (!this._dbHelper.ExistTableName(t.GetType().Name))
                    this._dbHelper.CreateTable(t.GetType().Name, t.GetDBColumnItems());
                else
                {//若列发生变化，需要处理下wxb20220315添加
                    this._dbHelper.AlterTable(t.GetType().Name, t.GetDBColumnItems());
                }
            });
        }

        public virtual void DbInsertOrUpdate(IDbEntity entity)
        {
            if (this._dbHelper == null || entity == null)
                return;
            this._dbHelper.InsertOrReplace(entity.GetType().Name, entity.GetDBColumnItems(true));
        }
        public virtual void DbInsertOrUpdate(IEnumerable<IDbEntity> entities)
        {
            if (this._dbHelper == null || entities == null || !entities.Any())
                return;
            try
            {
                this._dbHelper.BeginTransaction();
                entities.ForAll(entity => this.DbInsertOrUpdate(entity));
                this._dbHelper.Commit();
            }
            catch (Exception ex)
            {
                this._dbHelper.Rollback();
                //LogUtil.Log(Fpi.Log.Config.MessageType.ExceptionMessage, ex.Message + "\r\n异常堆栈:\t" + ex.StackTrace);
            }
        }
        public virtual void DbDelete(IDbEntity entity)
        {
            if (this._dbHelper == null || entity == null)
                return;
            //var whereColumnItem = entity.GetDBColumnItems().FirstOrDefault(t => t.IsPrimaryKey);
            var whereColumnItem = entity.GetDBColumnItems(true, t => t.IsPrimaryKey).FirstOrDefault();
            if (whereColumnItem != null)
            {
                //if (entity.ColumnPropertyInfoList.ContainsKey(whereColumnItem.ColumnName))
                //    whereColumnItem.ColumnValue = entity.ColumnPropertyInfoList[whereColumnItem.ColumnName].GetValue(entity, null);
                this._dbHelper.Delete(entity.GetType().Name, whereColumnItem);
            }
        }
        public virtual void DbDelete(IEnumerable<IDbEntity> entities)
        {
            if (this._dbHelper == null || entities == null || !entities.Any())
                return;
            try
            {
                this._dbHelper.BeginTransaction();
                entities.ForAll(entity => this.DbDelete(entity));
                this._dbHelper.Commit();
            }
            catch (Exception ex)
            {
                this._dbHelper.Rollback();
                //LogUtil.Log(Fpi.Log.Config.MessageType.ExceptionMessage, ex.Message + "\r\n异常堆栈:\t" + ex.StackTrace);
            }
        }

        //TODO由于查询条件复杂，未定义统一查询接口
        public static List<T> DataTable2Records<T>(DataTable dt) where T : DBBaseFactory
        {
            if (dt == null) return null;
            List<T> items = new List<T>();
            dt.Rows.OfType<DataRow>().ForAll(p =>
            {
                var newItem = ReflectionHelper.CreateInstance(typeof(T)) as T;
                if (newItem != null)
                {
                    newItem.SetValueFromDataRow(p);
                    items.Add(newItem);
                }
            });
            return items;
        }
    }
}
