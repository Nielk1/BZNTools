using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "spraybomb")]
    [ObjectClass(BZNFormat.BattlezoneN64, "spraybomb")]
    [ObjectClass(BZNFormat.Battlezone2, "spraybomb")]
    public class ClassSprayBuildingFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassSprayBuilding(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                ClassSprayBuilding.Hydrate(parent, reader, obj as ClassSprayBuilding);
                return true;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassSprayBuilding : ClassBuilding
    {
        public ClassSprayBuilding(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassSprayBuilding? obj)
        {
            ClassBuilding.Hydrate(parent, reader, obj as ClassBuilding);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassSprayBuilding obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            ClassBuilding.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
