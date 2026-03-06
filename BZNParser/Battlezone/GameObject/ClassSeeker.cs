using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "seeker")]
    public class ClassSeekerFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassSeeker(preamble, classLabel);
            ClassSeeker.Hydrate(parent, reader, obj as ClassSeeker);
            return true;
        }
    }
    public class ClassSeeker : ClassMine
    {
        public ClassSeeker(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassSeeker? obj)
        {
            ClassMine.Hydrate(parent, reader, obj as ClassMine);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassSeeker obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            ClassMine.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
