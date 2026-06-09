using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformDevFramework.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ModelBindControlAttribute : System.Attribute
    {
        private string ModelName = null;
        public ModelBindControlAttribute(string name)
        {
            ModelName = name;
        }

        public string GetModelName()
        {
            return ModelName;
        }
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class NoEditAttribute : System.Attribute
    {
        private string ModelName = null;
        public NoEditAttribute(string name)
        {
            ModelName = name;
        }

        public string GetModelName()
        {
            return ModelName;
        }
    }
}
