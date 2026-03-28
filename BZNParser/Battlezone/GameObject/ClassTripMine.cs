using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "tripmine")]
    public class ClassTripMineFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassTripMine(preamble, classLabel);
            ClassTripMine.Hydrate(parent, reader, obj as ClassTripMine);
            return true;
        }
    }
    public class ClassTripMine : ClassMine
    {
        public ClassTripMine(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassTripMine? obj)
        {
            ClassMine.Hydrate(parent, reader, obj as ClassMine);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassTripMine obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            ClassMine.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
