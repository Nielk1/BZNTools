using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "flag")]
    public class ClassFlagFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassFlag(preamble, classLabel);
            ClassFlag.Hydrate(parent, reader, obj as ClassFlag);
            return true;
        }
    }
    public class ClassFlag : ClassPowerUp
    {
        public ClassFlag(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }

        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassFlag? obj)
        {
            IBZNToken tok;

            if (reader.Format == BZNFormat.Battlezone2)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("startMat", BinaryFieldType.DATA_MAT3D)) throw new Exception("Failed to parse startMat/MAT3D"); // type not confirmed
                //startMat = tok.GetMatrix();

                tok = reader.ReadToken();
                if (!tok.Validate("holder", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse holder/LONG"); // type not confirmed
                //state = tok.GetUInt32();
            }

            ClassPowerUp.Hydrate(parent, reader, obj as ClassPowerUp);
        }
    }
}
