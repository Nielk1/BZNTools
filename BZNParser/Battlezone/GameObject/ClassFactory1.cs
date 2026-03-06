using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "factory")]
    [ObjectClass(BZNFormat.BattlezoneN64, "factory")]
    public class ClassFactory1Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassFactory1(preamble, classLabel);
            ClassFactory1.Hydrate(parent, reader, obj as ClassFactory1);
            return true;
        }
    }
    public class ClassFactory1 : ClassProducer
    {
        public ClassFactory1(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassFactory1? obj)
        {
            ClassProducer.Hydrate(parent, reader, obj as ClassProducer);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassFactory1 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            ClassProducer.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
