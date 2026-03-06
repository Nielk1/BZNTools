using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "shieldtower")]
    public class ClassShieldTower2Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassShieldTower2(preamble, classLabel);
            ClassShieldTower2.Hydrate(parent, reader, obj as ClassShieldTower2);
            return true;
        }
    }
    public class ClassShieldTower2 : ClassPoweredBuilding
    {
        public ClassShieldTower2(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassShieldTower2? obj)
        {
            ClassPoweredBuilding.Hydrate(parent, reader, obj as ClassPoweredBuilding);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassShieldTower2 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            ClassPoweredBuilding.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
