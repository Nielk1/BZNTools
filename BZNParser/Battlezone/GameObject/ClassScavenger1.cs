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
            {
                obj = new ClassScavenger1(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                return ClassScavenger1.Hydrate(parent, reader, obj as ClassScavenger1).Success;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassScavenger1 : ClassHoverCraft
    {
        public UInt32 scrapHeld { get; set; }

        public ClassScavenger1(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
            scrapHeld = 0;
        }

        public override void ClearMalformations()
        {
            Malformations.Clear();
            base.ClearMalformations();
        }

        public override void DisableMalformationAutoFix()
        {
            base.DisableMalformationAutoFix();
        }

        public override void EnableMalformationAutoFix()
        {
            base.EnableMalformationAutoFix();
        }


        public static ParseResult Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassScavenger1? obj)
        {
            if (reader.Format == BZNFormat.Battlezone)
            {
                //if (reader.Version > 1034)
                //if (reader.Version > 1037)
                if ((reader.Version >= 1039 && reader.Version < 2000) || reader.Version > 2004)
                {
                    IBZNToken? tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("scrapHeld", BinaryFieldType.DATA_LONG))
                        return ParseResult.Fail("Failed to parse scrapHeld/LONG");
                    tok.ApplyUInt32(obj, x => x.scrapHeld);
                }
            }

            return ClassHoverCraft.Hydrate(parent, reader, obj as ClassHoverCraft);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassScavenger1 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (writer.Format == BZNFormat.Battlezone)
            {
                //if (writer.Version > 1034)
                //if (writer.Version > 1037)
                if ((writer.Version >= 1039 && writer.Version < 2000) || writer.Version > 2004)
                {
                    writer.WriteUInt32("scrapHeld", obj, x => x.scrapHeld);
                }
            }
            ClassHoverCraft.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
