using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BZNParser.Battlezone
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ObjectClassAttribute : Attribute
    {
        public BZNFormat Format { get; set; }
        public string ClassName { get; set; }
        public int MinimumVersion { get; set; } // should be used to rank items in MultiClass, but technicly a BZN is valid containing items that postdate its version so long as it is loaded on a game version the class predates

        public ObjectClassAttribute(BZNFormat format, string className, int minimumVersion = 0)
        {
            Format = format;
            ClassName = className;
            MinimumVersion = minimumVersion;
        }
    }
}
