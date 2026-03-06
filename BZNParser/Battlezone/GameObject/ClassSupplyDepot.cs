using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "supplydepot")]
    [ObjectClass(BZNFormat.BattlezoneN64, "supplydepot")]
    public class ClassSupplyDepotFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassSupplyDepot(preamble, classLabel);
            ClassSupplyDepot.Hydrate(parent, reader, obj as ClassSupplyDepot);
            return true;
        }
    }
    public class ClassSupplyDepot : ClassBuilding
    {
        public ClassSupplyDepot(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassSupplyDepot? obj)
        {
            ClassBuilding.Hydrate(parent, reader, obj as ClassBuilding);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassSupplyDepot obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            ClassBuilding.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
