using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "moneybag")]
    public class ClassMoneyPowerupFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassMoneyPowerup(preamble, classLabel);
            ClassMoneyPowerup.Hydrate(parent, reader, obj as ClassMoneyPowerup);
            return true;
        }
    }
    public class ClassMoneyPowerup : ClassPowerUp
    {
        public ClassMoneyPowerup(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassMoneyPowerup? obj)
        {
            ClassPowerUp.Hydrate(parent, reader, obj as ClassPowerUp);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassMoneyPowerup obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            ClassPowerUp.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
