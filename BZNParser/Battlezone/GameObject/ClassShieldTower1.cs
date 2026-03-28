using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "shieldtower")]
    [ObjectClass(BZNFormat.BattlezoneN64, "shieldtower")]
    public class ClassShieldTower1Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassShieldTower1(preamble, classLabel);
            ClassShieldTower1.Hydrate(parent, reader, obj as ClassShieldTower1);
            return true;
        }
    }
    public class ClassShieldTower1 : ClassBuilding
    {
        public ClassShieldTower1(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassShieldTower1? obj)
        {
            ClassBuilding.Hydrate(parent, reader, obj as ClassBuilding);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassShieldTower1 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            ClassBuilding.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
