using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace MySQLiteUtile.MySQLiteFlod
{
    public class DBColumnAttribute : DescriptionAttribute
    {
        public DBColumnAttribute()
            : base()
        { }
        public DBColumnAttribute(string description)
            : base(description)
        { }

        private string _columnDataType;
        /// <summary>
        /// 数据类型：INTEGER，TEXT，BLOB，REAL，NUMERIC，DATETIME
        /// INTEGER-->对应:long类型
        /// REAL-->对应:double类型
        /// TEXT-->对应:string类型
        /// BLOB-->对应:byte[]类型
        /// DATETIME-->对应:DateTime类型
        /// </summary>
        public string ColumnDataType
        {
            get { return _columnDataType; }
            set { _columnDataType = value; }
        }

        private bool _isNotNull;
        /// <summary>
        /// 是否Not Null
        /// </summary>
        public bool IsNotNull
        {
            get { return _isNotNull; }
            set { _isNotNull = value; }
        }

        private bool _isPrimaryKey;
        /// <summary>
        /// 是否为主键
        /// </summary>
        public bool IsPrimaryKey
        {
            get { return _isPrimaryKey; }
            set { _isPrimaryKey = value; }
        }

        private bool _isAutoIncrement;
        /// <summary>
        /// 是否为自增
        /// </summary>
        public bool IsAutoIncrement
        {
            get { return _isAutoIncrement; }
            set { _isAutoIncrement = value; }
        }

    }
}
