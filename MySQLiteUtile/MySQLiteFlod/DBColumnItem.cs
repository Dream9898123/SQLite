using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySQLiteUtile.MySQLiteFlod
{
    public class DBColumnItem
    {
        #region 字段&属性
        private string _columnName;
        public string ColumnName
        {
            get { return _columnName; }
            set { _columnName = value; }
        }

        private string _columnText;
        public string ColumnText
        {
            get { return _columnText; }
            set { _columnText = value; }
        }

        private object _columnValue;
        public object ColumnValue
        {
            get { return _columnValue; }
            set { _columnValue = value; }
        }

        private string _columnDataType;
        public string ColumnDataType
        {
            get { return _columnDataType; }
            set { _columnDataType = value; }
        }

        private bool _isNotNull;
        public bool IsNotNull
        {
            get { return _isNotNull; }
            set { _isNotNull = value; }
        }

        private bool _isPrimaryKey;
        public bool IsPrimaryKey
        {
            get { return _isPrimaryKey; }
            set { _isPrimaryKey = value; }
        }

        private bool _isAutoIncrement;
        public bool IsAutoIncrement
        {
            get { return _isAutoIncrement; }
            set { _isAutoIncrement = value; }
        }
        #endregion

        #region 构造函数
        public DBColumnItem()
        { }
        #endregion
    }
}
