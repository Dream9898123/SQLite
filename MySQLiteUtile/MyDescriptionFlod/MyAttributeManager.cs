using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDescription.MyDescriptionFlod
{
    public class MyAttributeManager
    {
        public List<string> GetMyClassObject()
        {
            MyClass myClass = new MyClass();
            return myClass.GetMyAttrbuteStr();
        }
    }
}
