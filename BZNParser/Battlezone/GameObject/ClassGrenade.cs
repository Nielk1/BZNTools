using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    public class ClassGrenadeFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassGrenade(preamble, classLabel);
            ClassGrenade.Hydrate(parent, reader, obj as ClassGrenade);
            return true;
        }
    }
    public class ClassGrenade : ClassRocket
    {
        public ClassGrenade(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassGrenade? obj)
        {
            ClassRocket.Hydrate(parent, reader, obj as ClassRocket);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassGrenade obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            ClassRocket.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
