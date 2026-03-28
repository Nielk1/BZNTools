using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "jammer")]
    public class ClassJammerTowerFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassJammerTower(preamble, classLabel);
            ClassJammerTower.Hydrate(parent, reader, obj as ClassJammerTower);
            return true;
        }
    }
    public class ClassJammerTower : ClassPoweredBuilding
    {
        public ClassJammerTower(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassJammerTower? obj)
        {
            ClassPoweredBuilding.Hydrate(parent, reader, obj as ClassPoweredBuilding);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassJammerTower obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            ClassPoweredBuilding.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
