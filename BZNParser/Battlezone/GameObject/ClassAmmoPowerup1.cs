using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{

    [ObjectClass(BZNFormat.Battlezone, "ammopack")]
    [ObjectClass(BZNFormat.BattlezoneN64, "ammopack")]
    public class ClassAmmoPowerup1Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassAmmoPowerup1(preamble, classLabel);
            ClassAmmoPowerup1.Hydrate(parent, reader, obj as ClassAmmoPowerup1);
            return true;
        }
    }
    public class ClassAmmoPowerup1 : ClassPowerUp
    {
        public ClassAmmoPowerup1(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassAmmoPowerup1? obj)
        {
            ClassPowerUp.Hydrate(parent, reader, obj as ClassPowerUp);
        }
    }
}
