using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicClassGenerator
{
    public class Parameter
    {
        public Parameter(Type type, string name)
        {
            Type = type;
            Name = name;
        }

        public Type Type { get; set; }
        public string Name { get; set; }
    }
}
