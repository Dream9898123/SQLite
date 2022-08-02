using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace MyDescription.MyDescriptionFlod
{
    public class MyAttribute : DescriptionAttribute
    {
        public MyAttribute()
        {

        }

        private string _myDescriptionType;
        private bool _myDescriptionIsUse;

        /// <summary>
        /// 我的描述类型
        /// </summary>
        public string MyDescriptionType { get => _myDescriptionType; set => _myDescriptionType = value; }
        /// <summary>
        /// 我的描述使用状态
        /// </summary>
        public bool MyDescriptionIsUse { get => _myDescriptionIsUse; set => _myDescriptionIsUse = value; }
    }
}
