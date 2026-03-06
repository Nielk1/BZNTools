using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "spawnpnt")]
    public class ClassSpawnBuoy2Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassSpawnBuoy2(preamble, classLabel);
            ClassSpawnBuoy2.Hydrate(parent, reader, obj as ClassSpawnBuoy2);
            return true;
        }
    }
    public class ClassSpawnBuoy2 : ClassDummy
    {
        public ClassSpawnBuoy2(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassSpawnBuoy2? obj)
        {
            ClassDummy.Hydrate(parent, reader, obj as ClassDummy);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassSpawnBuoy2 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            ClassDummy.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
