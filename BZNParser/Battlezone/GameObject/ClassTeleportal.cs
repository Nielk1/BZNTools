using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "teleportal")]
    public class ClassTeleportalFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassTeleportal(preamble, classLabel);
            ClassTeleportal.Hydrate(parent, reader, obj as ClassTeleportal);
            return true;
        }
    }
    public class ClassTeleportal : ClassPoweredBuilding
    {
        public ClassTeleportal(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassTeleportal? obj)
        {
            ClassPoweredBuilding.Hydrate(parent, reader, obj as ClassPoweredBuilding);
        }
    }
}
