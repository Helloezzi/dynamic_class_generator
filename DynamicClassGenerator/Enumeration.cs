using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicClassGenerator
{
    public class Enumeration
    {
        public enum CODE_TYPE
        {
            CLASS = 0,
            STRUCT,
            ENUM,
            INTERFACE,
            PARTIAL,

        }

        public enum VARIABLE_TYPE
        {
            Byte = 0,
            Char,
            Boolean,
            UInt16,
            UInt32,
            Int16,
            Int32,
            Double,
            String,
            Decimal
        }

        public enum MEMBER_TYPE
        {
            Field = 0,
            Property
        }
    }
}
