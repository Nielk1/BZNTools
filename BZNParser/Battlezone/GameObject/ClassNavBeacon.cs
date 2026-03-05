using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "beacon")]
    public class ClassNavBeaconFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassNavBeacon(preamble, classLabel);
            ClassNavBeacon.Hydrate(parent, reader, obj as ClassNavBeacon);
            return true;
        }
    }
    public class ClassNavBeacon : ClassGameObject
    {
        public ClassNavBeacon(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassNavBeacon? obj)
        {
            IBZNToken tok;

            tok = reader.ReadToken();
            if (!tok.Validate("name", BinaryFieldType.DATA_CHAR))
                throw new Exception("Failed to parse name/CHAR");
            string name = tok.GetString();

            tok = reader.ReadToken();
            if (!tok.Validate("navSlot", BinaryFieldType.DATA_LONG))
                throw new Exception("Failed to parse navSlot/LONG");
            //int navSlot = tok.GetInt32();

            if (reader.Version > 1104)
            {
                ClassGameObject.Hydrate(parent, reader, obj as ClassGameObject);
            }
        }
    }
}
