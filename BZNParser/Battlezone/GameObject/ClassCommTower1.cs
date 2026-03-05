using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "commtower")]
    [ObjectClass(BZNFormat.BattlezoneN64, "commtower")]
    public class ClassCommTower1Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassCommTower1(preamble, classLabel);
            ClassCommTower1.Hydrate(parent, reader, obj as ClassCommTower1);
            return true;
        }
    }
    public class ClassCommTower1 : ClassBuilding
    {
        public ClassCommTower1(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassCommTower1? obj)
        {
            ClassBuilding.Hydrate(parent, reader, obj as ClassBuilding);
        }
    }
}
