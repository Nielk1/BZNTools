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
        public UInt32 dropoff { get; set; }

        public ClassScrapSilo1(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassScrapSilo1? obj)
        {
            if (reader.Format == BZNFormat.BattlezoneN64 || reader.Version > 1020)
            {
                IBZNToken tok = reader.ReadToken();
                if (!tok.Validate("dropoff", BinaryFieldType.DATA_PTR)) throw new Exception("Failed to parse dropoff/LONG");
                if (obj != null) obj.dropoff = tok.GetUInt32H();
            }

            ClassGameObject.Hydrate(parent, reader, obj as ClassGameObject);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassScrapSilo1 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            if (writer.Format == BZNFormat.BattlezoneN64 || writer.Version > 1020)
            {
                writer.WritePtr("dropoff", obj.dropoff);
            }
            ClassGameObject.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
