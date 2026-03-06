using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "servicepod")]
    [ObjectClass(BZNFormat.Battlezone2, "ammopack")]
    [ObjectClass(BZNFormat.Battlezone2, "repairkit")]
    public class ClassServicePowerupFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassServicePowerup(preamble, classLabel);
            ClassServicePowerup.Hydrate(parent, reader, obj as ClassServicePowerup);
            return true;
        }
    }
    public class ClassServicePowerup : ClassPowerUp
    {
        public ClassServicePowerup(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassServicePowerup? obj)
        {
            ClassPowerUp.Hydrate(parent, reader, obj as ClassPowerUp);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassServicePowerup obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            ClassPowerUp.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
