using Bingo.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

namespace MySQLiteUtile.MySQLiteFlod
{
    public class SQLiteHelper
    {
        #region 字段
        private SQLiteCommand _command = null;
        #endregion

        #region 属性
        ///// <summary>
        ///// 默认数据库名称
        ///// </summary>
        //public static string DefaultDBName
        //{
        //    //get { return VarConfigUtility.GetValueFromVarConfig(VarConfigUtility.DataBase, string.Empty); }
        //    //set { VarConfigUtility.SetValueToVarConfig(VarConfigUtility.DataBase, value ?? string.Empty); }
        //    get { return "WindMeasureData"; }
        //}
        ///// <summary>
        ///// 默认数据库密码
        ///// </summary>
        //public static string DefaultDBPassword
        //{
        //    //get { return VarConfigUtility.GetValueFromVarConfig(VarConfigUtility.DBPassword, string.Empty); }
        //    //set { VarConfigUtility.SetValueToVarConfig(VarConfigUtility.DBPassword, value ?? string.Empty); }
        //    get { return string.Empty; }
        //}
        public bool IsDisposed
        {
            get { return this._command == null || this._command.Connection == null; }
        }
        #endregion

        #region 构造函数
        public SQLiteHelper(SQLiteCommand command)
        {
            _command = command;
        }
        #endregion

        #region 公有方法
        public void Dispose()
        {
            try
            {
                if (_command != null)
                {
                    if (_command.Connection != null)
                    {
                        _command.Connection.Close();
                        _command.Connection.Dispose();
                        _command.Connection = null;
                    }
                    _command.Dispose();
                    _command = null;
                }
            }
            catch { }
        }
        public static string GetConnectionString(string dbPath, string dbPassword = null)
        {
            return string.Format("data source={0};password={1}", dbPath ?? string.Empty, dbPassword ?? string.Empty);
        }
        public static bool TestConnection(string dbPath, string dbPassword = null, bool isCreateNewDB = true)
        {
            if (string.IsNullOrEmpty(dbPath) || (!isCreateNewDB && !File.Exists(dbPath))) return false;
            if (!Directory.Exists(Path.GetDirectoryName(dbPath)))//若不存在文件夹，则会conn.Open()时，会报错；
                Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
            var connetionString = GetConnectionString(dbPath, dbPassword);
            using (SQLiteConnection conn = new SQLiteConnection(connetionString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    try
                    {
                        cmd.Connection = conn;
                        conn.Open();//其实，若连接能打开成功，则已能说明能连接上数据库了，以下可以不用执行；
                        cmd.CommandText = "SELECT count(*) FROM sqlite_master;";
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            return true;
        }
        public static bool ChangePassword(string path, string newPassword, string oldPassword = null)
        {
            try
            {
                using (SQLiteConnection con = new SQLiteConnection(GetConnectionString(path, oldPassword)))
                {
                    con.Open();
                    con.ChangePassword(newPassword);
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        #region 事务
        public void BeginTransaction()
        {
            if (_command == null) return;
            _command.CommandText = "begin transaction;";//或者"begin;"
            _command.ExecuteNonQuery();
        }
        public void Commit()
        {
            if (_command == null) return;
            _command.CommandText = "commit;";//或者"commit transaction;"
            _command.ExecuteNonQuery();
        }
        public void Rollback()
        {
            if (_command == null) return;
            _command.CommandText = "rollback";
            _command.ExecuteNonQuery();
        }
        public void Vacuum()
        {//整理数据库达到紧凑之用，比如把删除的彻底删除掉等等，注意：transaction事务中不能执行
            if (_command == null) return;
            _command.CommandText = "VACUUM";
            _command.ExecuteNonQuery();
        }
        #endregion

        #region Table
        public void CreateTable(string tableName, IEnumerable<DBColumnItem> columnItems)
        {
            if (string.IsNullOrWhiteSpace(tableName) || columnItems == null || !columnItems.Any() || _command == null)
                return;
            StringBuilder sb = new StringBuilder();
            sb.Append("create table if not exists `");
            sb.Append(tableName);
            sb.AppendLine("`(");

            bool firstRecord = true;
            foreach (var col in columnItems)
            {
                if (string.IsNullOrWhiteSpace(col.ColumnName))
                    continue;

                if (firstRecord)
                    firstRecord = false;
                else
                    sb.AppendLine(",");

                sb.Append(col.ColumnName);
                sb.Append(" ");
                sb.Append(col.ColumnDataType);
                sb.Append(" ");
                if (col.IsNotNull)
                    sb.Append(" NOT NULL");
                if (col.IsPrimaryKey)
                    sb.Append(" PRIMARY KEY");
                if (col.IsAutoIncrement)
                    sb.Append(" AUTOINCREMENT");
            }

            sb.AppendLine(");");

            this._command.CommandText = sb.ToString();
            this._command.ExecuteNonQuery();
        }
        public void DropTable(string tableName)
        {//[DROP TABLE 表名]
            if (_command == null) return;
            this._command.CommandText = string.Format("drop table if exists `{0}`", tableName);
            this._command.ExecuteNonQuery();
        }
        public void AlterTable(string tableName, IEnumerable<DBColumnItem> columnItems)
        {// 更新表的列，保留原先表，即只增不减
            //string sql = string.Format("select * from [{0}] where 1=2", tableName);
            //this._command.CommandText = sql;
            //DataTable dataTable = null;
            //using (IDataReader reader = this._command.ExecuteReader(CommandBehavior.KeyInfo | CommandBehavior.SchemaOnly))
            //    dataTable = reader.GetSchemaTable();
            var dataTable = this.SelectTableColumns(tableName);
            if (dataTable == null) return;
            var columnNames = dataTable.AsEnumerable().Select(t => t.Field<string>("ColumnName")).ToList();
            StringBuilder sb = new StringBuilder();
            foreach (var col in columnItems)
            {
                if (!columnNames.Contains(col.ColumnName))
                {//注意：每次只能加一列，SQL: Alter Table 表名 ADD COLUMN 列名 数据类型 [NOT NULL] [约束] 其中[]表示可选  
                    sb.Clear();
                    sb.Append(string.Format(@"alter table [{0}] add ", tableName));
                    sb.Append(col.ColumnName);
                    sb.Append(" ");
                    sb.Append(col.ColumnDataType);
                    sb.Append(" ");
                    if (col.IsNotNull)
                        sb.Append(" NOT NULL");
                    if (col.IsPrimaryKey)
                        sb.Append(" PRIMARY KEY");
                    if (col.IsAutoIncrement)
                        sb.Append(" AUTOINCREMENT");
                    this._command.CommandText = sb.ToString();
                    this._command.ExecuteNonQuery();
                }
            }
        }

        public bool ExistTableName(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                return false;
            string sql = string.Format("select count(1) from sqlite_master where type ='table' and name ='{0}'", tableName);
            int count = ExecuteScalar<int>(sql);
            return count > 0;
        }
        public DataTable SelectTableNames(string wheresql = "")
        {//wheresql = "name like '{0}%'"模糊查询
            if (string.IsNullOrEmpty(wheresql))
                this._command.CommandText = "select name from sqlite_master where type ='table' order by name";
            else
                this._command.CommandText = string.Format("select name from sqlite_master where type ='table' and {0} order by name", wheresql);
            SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(_command);
            DataTable dataTable = new DataTable("TableNames");
            dataAdapter.Fill(dataTable);
            return dataTable;
        }
        public DataTable SelectTableColumns(string tableName)
        {
            string sql = string.Format("select * from [{0}] where 1=2", tableName);
            this._command.CommandText = sql;
            SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(_command);
            DataTable dataTable = null;
            using (IDataReader reader = this._command.ExecuteReader(CommandBehavior.KeyInfo | CommandBehavior.SchemaOnly))
                dataTable = reader.GetSchemaTable();
            return dataTable;
        }
        #endregion

        #region Record
        public void InsertOrReplace(string tableName, IEnumerable<DBColumnItem> columnItems)
        {
            if (string.IsNullOrWhiteSpace(tableName) || columnItems == null || tableName.Count() <= 0 || _command == null)
                return;
            StringBuilder sbCol = new System.Text.StringBuilder();
            StringBuilder sbVal = new System.Text.StringBuilder();

            columnItems.Where(t => !t.IsAutoIncrement).ForAll(t =>
            {
                if (sbCol.Length == 0)
                {
                    sbCol.Append("insert or replace into ");
                    sbCol.Append(tableName);
                    sbCol.Append("(");
                }
                else
                {
                    sbCol.Append(",");
                }

                sbCol.Append("`");
                sbCol.Append(t.ColumnName);
                sbCol.Append("`");

                if (sbVal.Length == 0)
                    sbVal.Append(" values(");
                else
                    sbVal.Append(", ");

                sbVal.Append("@v");
                sbVal.Append(t.ColumnName);
            });
            sbCol.Append(") ");
            sbVal.Append(");");

            this._command.CommandText = sbCol.ToString() + sbVal.ToString();

            this._command.Parameters.Clear();
            columnItems.Where(t => !t.IsAutoIncrement).ForAll(t => this._command.Parameters.AddWithValue("@v" + t.ColumnName, t.ColumnValue));

            this._command.ExecuteNonQuery();
        }
        public void Insert(string tableName, IEnumerable<DBColumnItem> columnItems)
        {
            if (string.IsNullOrWhiteSpace(tableName) || columnItems == null || tableName.Count() <= 0 || _command == null)
                return;
            StringBuilder sbCol = new System.Text.StringBuilder();
            StringBuilder sbVal = new System.Text.StringBuilder();

            columnItems.Where(t => !t.IsAutoIncrement).ForAll(t =>
            {
                if (sbCol.Length == 0)
                {
                    sbCol.Append("insert into ");
                    sbCol.Append(tableName);
                    sbCol.Append("(");
                }
                else
                {
                    sbCol.Append(",");
                }

                sbCol.Append("`");
                sbCol.Append(t.ColumnName);
                sbCol.Append("`");

                if (sbVal.Length == 0)
                    sbVal.Append(" values(");
                else
                    sbVal.Append(", ");

                sbVal.Append("@v");
                sbVal.Append(t.ColumnName);
            });
            sbCol.Append(") ");
            sbVal.Append(");");

            this._command.CommandText = sbCol.ToString() + sbVal.ToString();

            this._command.Parameters.Clear();
            columnItems.Where(t => !t.IsAutoIncrement).ForAll(t => this._command.Parameters.AddWithValue("@v" + t.ColumnName, t.ColumnValue));

            this._command.ExecuteNonQuery();
        }
        public void Insert(string tableName, DataRow dataRow)
        {
            this.Insert(tableName, dataRow == null ? null : new DataRow[] { dataRow });
        }
        public void Insert(string tableName, DataTable dataTable)
        {
            this.Insert(tableName, dataTable == null ? null : dataTable.Rows.OfType<DataRow>());
        }
        public void Insert(string tableName, IEnumerable<DataRow> dataRows)
        {//very good
            if (string.IsNullOrWhiteSpace(tableName) || dataRows == null || dataRows.Count() <= 0 || _command == null)
                return;
            StringBuilder sbCommand = new System.Text.StringBuilder();
            StringBuilder sbCol = new System.Text.StringBuilder();
            StringBuilder sbVal = new System.Text.StringBuilder();

            int rowIndex = 0;
            dataRows.ForAll(q =>
            {
                sbVal.Clear();
                sbCol.Clear();
                sbCol.Append("insert into '").Append(tableName).Append("'(");
                sbVal.Append(" values(");
                int columnIndex = 0;
                q.Table.Columns.OfType<DataColumn>().ForAll(t =>
                {
                    if (!t.AutoIncrement)
                    {
                        sbCol.Append(string.Format("'{0}',", t.ColumnName));
                        sbVal.Append(string.Format("@v{0}{1},", rowIndex, t.ColumnName));
                        this._command.Parameters.AddWithValue(string.Format("@v{0}{1}", rowIndex, t.ColumnName), q.ItemArray[columnIndex]);
                    }
                    columnIndex++;
                });
                sbCommand.Append(sbCol.ToString().TrimEnd(",".ToArray()) + ")" + sbVal.ToString().TrimEnd(",".ToArray()) + ");");
                rowIndex++;
            });
            this._command.CommandText = sbCommand.ToString();
            this._command.ExecuteNonQuery();
        }
        public void Update(string tableName, IEnumerable<DBColumnItem> columnItems, DBColumnItem whereColumnItem)
        {
            this.Update(tableName, columnItems, whereColumnItem == null ? null : new DBColumnItem[] { whereColumnItem });
        }
        public void Update(string tableName, IEnumerable<DBColumnItem> columnItems, IEnumerable<DBColumnItem> whereColumnItems)
        {
            if (string.IsNullOrWhiteSpace(tableName) || columnItems == null || tableName.Count() <= 0 || _command == null)
                return;

            StringBuilder sbData = new System.Text.StringBuilder();

            sbData.Append("update `");
            sbData.Append(tableName);
            sbData.Append("` set ");

            bool firstRecord = true;
            columnItems.Where(t => !t.IsAutoIncrement).ForAll(t =>
            {
                if (firstRecord)
                    firstRecord = false;
                else
                    sbData.Append(",");

                sbData.Append("`");
                sbData.Append(t.ColumnName);
                sbData.Append("` = ");

                sbData.Append("@v");
                sbData.Append(t.ColumnName);
            });

            firstRecord = true;
            whereColumnItems.ForAll(t =>
            {
                if (firstRecord)
                {
                    sbData.Append(" where ");
                    firstRecord = false;
                }
                else
                {
                    sbData.Append(" and ");
                }

                sbData.Append("`");
                sbData.Append(t.ColumnName);
                sbData.Append("` = ");

                sbData.Append("@c");
                sbData.Append(t.ColumnName);
            });
            sbData.Append(";");

            this._command.CommandText = sbData.ToString();

            this._command.Parameters.Clear();
            columnItems.Where(t => !t.IsAutoIncrement).ForAll(t => this._command.Parameters.AddWithValue("@v" + t.ColumnName, t.ColumnValue));
            whereColumnItems.ForAll(t => this._command.Parameters.AddWithValue("@c" + t.ColumnName, t.ColumnValue));

            this._command.ExecuteNonQuery();
        }
        public void Delete(string tableName, DBColumnItem whereColumnItem)
        {
            this.Delete(tableName, whereColumnItem == null ? null : new DBColumnItem[] { whereColumnItem });
        }
        public void Delete(string tableName, IEnumerable<DBColumnItem> whereColumnItems = null)
        {
            if (string.IsNullOrWhiteSpace(tableName) || _command == null)
                return;
            StringBuilder sbCommand = new System.Text.StringBuilder();

            sbCommand.Append("delete from `");
            sbCommand.Append(tableName);
            sbCommand.Append("` ");

            bool firstRecord = true;
            whereColumnItems.ForAll(t =>
            {
                if (firstRecord)
                {
                    sbCommand.Append(" where ");
                    firstRecord = false;
                }
                else
                {
                    sbCommand.Append(" and ");//or
                }

                sbCommand.Append("`");
                sbCommand.Append(t.ColumnName);
                sbCommand.Append("` = ");

                sbCommand.Append("@c");
                sbCommand.Append(t.ColumnName);
            });
            sbCommand.Append(";");

            this._command.CommandText = sbCommand.ToString();

            this._command.Parameters.Clear();
            whereColumnItems.ForAll(t => this._command.Parameters.AddWithValue("@c" + t.ColumnName, t.ColumnValue));

            this._command.ExecuteNonQuery();
        }
        #endregion

        #region Query
        public DataTable Select(string tableName, string sql, List<SQLiteParameter> parameters = null, DataTable dataTable = null)
        {
            if (string.IsNullOrEmpty(sql) || _command == null)
                return null;
            this._command.CommandText = sql;
            if (parameters != null && parameters.Count > 0)
                foreach (var item in parameters)
                    this._command.Parameters.Add(item);
            SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(_command);
            if (dataTable == null)
                dataTable = new DataTable(tableName);
            dataAdapter.Fill(dataTable);
            return dataTable;
        }
        public void ExecuteNonQuery(string sql, List<SQLiteParameter> parameters = null)
        {
            if (string.IsNullOrEmpty(sql) || _command == null)
                return;
            this._command.CommandText = sql;
            if (parameters != null && parameters.Count > 0)
                foreach (var item in parameters)
                    this._command.Parameters.Add(item);
            this._command.ExecuteNonQuery();
        }
        public object ExecuteScalar(string sql, List<SQLiteParameter> parameters = null)
        {
            if (string.IsNullOrEmpty(sql) || _command == null)
                return null;
            this._command.CommandText = sql;
            if (parameters != null && parameters.Count > 0)
                foreach (var item in parameters)
                    this._command.Parameters.Add(item);
            return this._command.ExecuteScalar();
        }
        public T ExecuteScalar<T>(string sql, List<SQLiteParameter> parameters = null)
        {
            var retValue = this.ExecuteScalar(sql, parameters);
            return (T)Convert.ChangeType(retValue, typeof(T));
        }
        public long LastInsertRowId()
        {//在Insert之后,使用SELECT last_insert_rowid()可直接得到最后一次插入的记录的id，如果之前没有进行任何Insert的操作,则返回0;
            if (_command == null) return -1;
            return this.ExecuteScalar<long>("select last_insert_rowid();");
        }
        public static object SqliteVersion(string connetionString)
        {//返回SQLite的版本
            using (SQLiteConnection conn = new SQLiteConnection(connetionString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    try
                    {
                        cmd.Connection = conn;
                        conn.Open();
                        cmd.CommandText = "select sqlite_version(*);";
                        return cmd.ExecuteScalar();
                    }
                    catch (Exception ex)
                    { }
                }
            }
            return null;
        }
        #endregion

        #region Attach Database
        public void AttachDatabase(string dbPath, string aliasName, string dbPassword = null)
        {
            if (_command == null || string.IsNullOrEmpty(dbPath) || !File.Exists(dbPath) || string.IsNullOrEmpty(aliasName)) return;
            _command.CommandText = string.Format("attach database '{0}' as {1} key '{2}';", dbPath, aliasName, dbPassword ?? string.Empty);
            _command.ExecuteNonQuery();
        }
        public void DetachDatabase(string aliasName)
        {
            if (_command == null || string.IsNullOrEmpty(aliasName)) return;
            _command.CommandText = string.Format("detach database '{0}';", aliasName);
            _command.ExecuteNonQuery();
        }
        #endregion
        #endregion

        #region 私有方法
        private void Test()
        {

        }
        #endregion
    }
}