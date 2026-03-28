using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "magnet")]
    [ObjectClass(BZNFormat.BattlezoneN64, "magnet")]
    [ObjectClass(BZNFormat.Battlezone2, "magnet")]
    public class ClassMagnetMineFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassMagnetMine(preamble, classLabel);
            ClassMagnetMine.Hydrate(parent, reader, obj as ClassMagnetMine);
            return true;
        }
    }
    public class ClassMagnetMine : ClassMine
    {
        public ClassMagnetMine(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassMagnetMine? obj)
        {
            //IBZNToken tok;

            //if (reader.Format == BZNFormat.Battlezone2)
            //{
            //    tok = reader.ReadToken();
            //    if (!tok.Validate("undeffloat", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse undeffloat/FLOAT");
            //    //saveClass = tok.GetSingle();
            //}

            ClassMine.Hydrate(parent, reader, obj as ClassMine);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassMagnetMine obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            ClassMine.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
