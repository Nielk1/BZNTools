using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "scavenger")]
    [ObjectClass(BZNFormat.BattlezoneN64, "scavenger")]
    public class ClassScavenger1Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassScavenger1(preamble, classLabel);
            ClassScavenger1.Hydrate(parent, reader, obj as ClassScavenger1);
            return true;
        }
    }
    public class ClassScavenger1 : ClassHoverCraft
    {
        public UInt32 scrapHeld { get; set; }

        public ClassScavenger1(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassScavenger1? obj)
        {
            if (reader.Format == BZNFormat.Battlezone)
            {
                //if (reader.Version > 1034)
                //if (reader.Version > 1037)
                if ((reader.Version >= 1039 && reader.Version < 2000) || reader.Version > 2004)
                {
                    IBZNToken? tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("scrapHeld", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse scrapHeld/LONG");
                    if (obj != null) obj.scrapHeld = tok.GetUInt32();
                }
            }

            ClassHoverCraft.Hydrate(parent, reader, obj as ClassHoverCraft);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassScavenger1 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            if (writer.Format == BZNFormat.Battlezone)
            {
                //if (writer.Version > 1034)
                //if (writer.Version > 1037)
                if ((writer.Version >= 1039 && writer.Version < 2000) || writer.Version > 2004)
                {
                    writer.WriteUnsignedValues("scrapHeld", obj.scrapHeld);
                }
            }
            ClassHoverCraft.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
