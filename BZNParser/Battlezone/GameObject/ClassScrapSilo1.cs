using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "scrapsilo")]
    [ObjectClass(BZNFormat.BattlezoneN64, "scrapsilo")]
    public class ClassScrapSilo1Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassScrapSilo1(preamble, classLabel);
            ClassScrapSilo1.Hydrate(parent, reader, obj as ClassScrapSilo1);
            return true;
        }
    }
    public class ClassScrapSilo1 : ClassGameObject
    {
        public UInt32 undefptr { get; set; }

        public ClassScrapSilo1(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassScrapSilo1? obj)
        {
            if (reader.Format == BZNFormat.BattlezoneN64 || reader.Version > 1020)
            {
                IBZNToken tok = reader.ReadToken();
                if (!tok.Validate("undefptr", BinaryFieldType.DATA_PTR)) throw new Exception("Failed to parse undefptr/LONG");
                if (obj != null) obj.undefptr = tok.GetUInt32H(); // dropoff
            }

            ClassGameObject.Hydrate(parent, reader, obj as ClassGameObject);
        }
    }
}
