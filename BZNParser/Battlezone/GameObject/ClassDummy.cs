using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "terrain")]
    public class ClassDummyFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassDummy(preamble, classLabel);
            ClassDummy.Hydrate(parent, reader, obj as ClassDummy);
            return true;
        }
    }
    public class ClassDummy : ClassGameObject
    {
        //public string name { get; set; }
        public ClassDummy(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }

        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassDummy? obj)
        {
            if (reader.Version == 1047)
            {
                ClassGameObject.Hydrate(parent, reader, obj as ClassGameObject); // this might be due to a changed base class on "spawnpnt"
                return;
            }

            IBZNToken? tok;

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("name", BinaryFieldType.DATA_CHAR))
                throw new Exception("Failed to parse name/CHAR");
            tok.ApplyChars(obj, x => x.name);

            // Terrain doesn't call base data load
            //base.Build(reader, obj);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassDummy obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (writer.Version == 1047)
            {
                ClassGameObject.Dehydrate(obj, parent, writer, binary, save); // this might be due to a changed base class on "spawnpnt"
                return;
            }

            writer.WriteChars("name", obj, x => x.name);
        }
    }
}
