using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    public class ClassRocketFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassRocket(preamble, classLabel);
            ClassRocket.Hydrate(parent, reader, obj as ClassRocket);
            return true;
        }
    }
    public class ClassRocket : ClassBullet
    {
        public ClassRocket(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassRocket? obj)
        {
            ClassBullet.Hydrate(parent, reader, obj as ClassBullet);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassRocket obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            ClassBullet.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
