using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "repairkit")]
    [ObjectClass(BZNFormat.BattlezoneN64, "repairkit")]
    public class ClassHealthPowerup1Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassHealthPowerup1(preamble, classLabel);
            ClassHealthPowerup1.Hydrate(parent, reader, obj as ClassHealthPowerup1);
            return true;
        }
    }
    public class ClassHealthPowerup1 : ClassPowerUp
    {
        public ClassHealthPowerup1(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassHealthPowerup1? obj)
        {
            ClassPowerUp.Hydrate(parent, reader, obj as ClassPowerUp);
        }
    }
}
