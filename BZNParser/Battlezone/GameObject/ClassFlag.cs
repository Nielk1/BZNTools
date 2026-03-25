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
        public Matrix startMat { get; set; }
        public UInt32 holder { get; set; }

        public ClassFlag(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }

        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassFlag? obj)
        {
            IBZNToken? tok;

            if (reader.Format == BZNFormat.Battlezone2)
            {
                reader.ReadMatrix("startMat", obj, x => x.startMat);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("holder", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse holder/LONG"); // type not confirmed
                tok.ApplyUInt32(obj, x => x.holder);
            }

            ClassPowerUp.Hydrate(parent, reader, obj as ClassPowerUp);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassFlag obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            if (writer.Format == BZNFormat.Battlezone2)
            {
                writer.WriteMatrix("startMat", obj, x => x.startMat);
                writer.WriteUInt32("holder", obj, x => x.holder);
            }
            ClassPowerUp.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
