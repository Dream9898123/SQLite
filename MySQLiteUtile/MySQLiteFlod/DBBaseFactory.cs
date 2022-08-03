using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MySQLiteUtile.MySQLiteFlod
{
    public class DBBaseFactory
    {
        public Dictionary<string, PropertyInfo> ColumnPropertyInfoList { get; private set; }

        public virtual IEnumerable<DBColumnItem> GetDBColumnItems(bool needUpdateValue = false, Func<DBColumnItem, bool> predicate = null)
        {
            //创建数据表结构或保存数据到数据库
            #region MyRegion
            var myColumnItemList = new List<DBColumnItem>();
            var myColumnPropertyInfoList = new Dictionary<string, PropertyInfo>();
            PropertyInfo[] propertyInfos = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                DBColumnAttribute desciptionAttribute = propertyInfo.GetCustomAttributes(false).OfType<DBColumnAttribute>().FirstOrDefault();
                if (desciptionAttribute != null)
                {
                    if (!myColumnPropertyInfoList.ContainsKey(propertyInfo.Name))
                        myColumnPropertyInfoList.Add(propertyInfo.Name, propertyInfo);
                    DBColumnItem item = new DBColumnItem();
                    item.ColumnName = propertyInfo.Name;
                    item.ColumnText = desciptionAttribute.Description;
                    item.ColumnDataType = desciptionAttribute.ColumnDataType;
                    item.IsPrimaryKey = desciptionAttribute.IsPrimaryKey;
                    item.IsAutoIncrement = desciptionAttribute.IsAutoIncrement;
                    item.IsNotNull = desciptionAttribute.IsNotNull;
                    if (needUpdateValue)//更新数值
                        item.ColumnValue = propertyInfo.GetValue(this, null);
                    myColumnItemList.Add(item);
                }
            }
            #endregion
            this.ColumnPropertyInfoList = myColumnPropertyInfoList;
            return myColumnItemList;
        }
        public virtual void SetValueFromDataRow(DataRow dataRow)
        {
            //查询，数据行转化为对象
            if (dataRow != null && dataRow.Table != null)
            {
                var myColumnPropertyInfoList = this.ColumnPropertyInfoList;
                if (myColumnPropertyInfoList == null || myColumnPropertyInfoList.Count <= 0)
                    this.GetDBColumnItems();
                foreach (var dataColumn in dataRow.Table.Columns.OfType<DataColumn>())
                {
                    if (myColumnPropertyInfoList.ContainsKey(dataColumn.ColumnName))
                    {
                        //通过反射转化对象
                        myColumnPropertyInfoList[dataColumn.ColumnName].SetValue(this, dataRow[dataColumn.ColumnName] == DBNull.Value ? null : dataRow[dataColumn.ColumnName], null);
                    }
                }
            }
        }
    }
}
