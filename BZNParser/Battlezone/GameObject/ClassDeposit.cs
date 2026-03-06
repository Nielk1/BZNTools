using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "deposit")]
    public class ClassDepositFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassDeposit(preamble, classLabel);
            ClassDeposit.Hydrate(parent, reader, obj as ClassDeposit);
            return true;
        }
    }
    public class ClassDeposit : ClassBuilding
    {
        public ClassDeposit(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassDeposit? obj)
        {
            IBZNToken tok;

            //if (reader.Format == BZNFormat.Battlezone2 && reader.Version > 1123)
            //{
            //    // unsure of the type of this
            //    tok = reader.ReadToken();
            //    if (!tok.Validate("saveClass", BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse saveClass/CHAR");
            //    //saveClass = tok.GetString();
            //}

            ClassBuilding.Hydrate(parent, reader, obj as ClassBuilding);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassDeposit obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            //if (writer.Format == BZNFormat.Battlezone2 && writer.Version > 1123)
            //{
            //    // unsure of the type of this
            //    writer.WriteToken(new BZNTokenString("saveClass", BinaryFieldType.DATA_CHAR, obj.saveClass));
            //}

            ClassBuilding.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
