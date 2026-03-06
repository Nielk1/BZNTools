using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "scrapfield")]
    [ObjectClass(BZNFormat.BattlezoneN64, "scrapfield")]
    [ObjectClass(BZNFormat.Battlezone2, "scrapfield")]
    public class ClassScrapFieldFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassScrapField(preamble, classLabel);
            ClassScrapField.Hydrate(parent, reader, obj as ClassScrapField);
            return true;
        }
    }
    public class ClassScrapField : ClassBuilding
    {
        public ClassScrapField(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassScrapField? obj)
        {
            ClassBuilding.Hydrate(parent, reader, obj as ClassBuilding);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassScrapField obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            ClassBuilding.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
