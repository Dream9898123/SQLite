using MySQLiteUtile.MySQLiteFlod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySQLiteUtile.MySQLiteTest
{
    public class MyTestFactory : DBBaseFactory
    {
        private long id;
        private string name;
        private double age;

        [DBColumn(ColumnDataType = "INTEGER", IsAutoIncrement = true, IsPrimaryKey = true)]
        public long Id { get => id; set => id = value; }
        [DBColumn(ColumnDataType = "TEXT")]
        public string Name { get => name; set => name = value; }
        [DBColumn(ColumnDataType = "REAL")]
        public double Age { get => age; set => age = value; }
    }
}
