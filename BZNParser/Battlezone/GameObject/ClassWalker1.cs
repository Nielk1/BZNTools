using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "walker")]
    [ObjectClass(BZNFormat.BattlezoneN64, "walker")]
    public class ClassWalker1Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassWalker1(preamble, classLabel);
            ClassWalker1.Hydrate(parent, reader, obj as ClassWalker1);
            return true;
        }
    }
    public class ClassWalker1 : ClassCraft
    {
        public ClassWalker1(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassWalker1? obj)
        {
            if (reader.Version > 1001 && reader.Version < 1026)
            {
                // junk hovercraft params
                reader.ReadToken();
                reader.ReadToken();
                reader.ReadToken();
                reader.ReadToken();
                reader.ReadToken();
                reader.ReadToken();
                reader.ReadToken();
                reader.ReadToken();
                reader.ReadToken();
                reader.ReadToken();
                reader.ReadToken();
                reader.ReadToken();
                reader.ReadToken();
                reader.ReadToken();
                reader.ReadToken();
                reader.ReadToken();
                reader.ReadToken();
                reader.ReadToken();
                reader.ReadToken();
                reader.ReadToken();
                reader.ReadToken();
            }

            ClassCraft.Hydrate(parent, reader, obj as ClassCraft);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassWalker1 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            if (writer.Version > 1001 && writer.Version < 1026)
            {
                throw new NotImplementedException("Can't write walker for versions 1002-1025 due to unknown hovercraft params");
            }
            ClassCraft.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
