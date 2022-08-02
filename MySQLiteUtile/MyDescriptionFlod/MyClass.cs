using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MyDescription.MyDescriptionFlod
{
    public class MyClass
    {
        private int _id;
        private string _name;
        private float _age;

        [My(MyDescriptionIsUse = true,MyDescriptionType = "int") ]
        public int Id { get => _id; set => _id = value; }
        [My(MyDescriptionIsUse = true,MyDescriptionType = "string")]
        public string Name { get => _name; set => _name = value; }
        [My(MyDescriptionIsUse = false,MyDescriptionType = "float")]
        public float Age { get => _age; set => _age = value; }

        public virtual List<string> GetMyAttrbuteStr()
        {
            List<string> myListAttrbute = new List<string>();
            PropertyInfo[] propertyInfos = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach(PropertyInfo propertyInfo in propertyInfos)
            {
                MyAttribute myAttribute = propertyInfo.GetCustomAttributes(false).OfType<MyAttribute>().FirstOrDefault();
                string oneDesc = "使用？" + myAttribute.MyDescriptionIsUse + "，类型：" + myAttribute.MyDescriptionType;
                myListAttrbute.Add(oneDesc);
            }
            return myListAttrbute;
        }
    }
}
