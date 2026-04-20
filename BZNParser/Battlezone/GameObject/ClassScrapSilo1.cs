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
            {
                obj = new ClassScrapSilo1(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                return ClassScrapSilo1.Hydrate(parent, reader, obj as ClassScrapSilo1).Success;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassScrapSilo1 : ClassGameObject
    {
        public UInt32 undefptr { get; set; }

        public ClassScrapSilo1(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
            undefptr = 0;
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


        public static ParseResult Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassScrapSilo1? obj)
        {
            if (reader.Format == BZNFormat.BattlezoneN64 || reader.Version > 1020)
            {
                IBZNToken? tok = reader.ReadToken();
                //if (!tok.Validate("dropoff", BinaryFieldType.DATA_PTR)) throw new Exception("Failed to parse dropoff/LONG");
                if (tok == null || !tok.Validate("undefptr", BinaryFieldType.DATA_PTR))
                    return ParseResult.Fail("Failed to parse undefptr/LONG");
                //if (obj != null) obj.undefptr = tok.GetUInt32H();
                tok.ApplyUInt32H8(obj, x => x.undefptr);
            }

            return ClassGameObject.Hydrate(parent, reader, obj as ClassGameObject);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassScrapSilo1 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (writer.Format == BZNFormat.BattlezoneN64 || writer.Version > 1020)
            {
                //writer.WriteBZ1_Ptr("undefptr", obj.undefptr);
                writer.WritePtr("undefptr", obj, x => x.undefptr);
            }
            ClassGameObject.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
