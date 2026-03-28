using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "powerplant")]
    [ObjectClass(BZNFormat.BattlezoneN64, "powerplant")]
    [ObjectClass(BZNFormat.Battlezone2, "powerplant")]
    [ObjectClass(BZNFormat.Battlezone2, "powerlung")]
    public class ClassPowerPlantFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassPowerPlant(preamble, classLabel);
            ClassPowerPlant.Hydrate(parent, reader, obj as ClassPowerPlant);
            return true;
        }
    }
    public class ClassPowerPlant : ClassBuilding
    {
        public ClassPowerPlant(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassPowerPlant? obj)
        {
            ClassBuilding.Hydrate(parent, reader, obj as ClassBuilding);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassPowerPlant obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            ClassBuilding.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
