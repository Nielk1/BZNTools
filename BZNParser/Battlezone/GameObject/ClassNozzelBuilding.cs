using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "cnozzle")]
    public class ClassNozzelBuildingFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassNozzelBuilding(preamble, classLabel);
            ClassNozzelBuilding.Hydrate(parent, reader, obj as ClassNozzelBuilding);
            return true;
        }
    }
    public class ClassNozzelBuilding: ClassBuilding
    {
        public ClassNozzelBuilding(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassNozzelBuilding? obj)
        {
            ClassBuilding.Hydrate(parent, reader, obj as ClassBuilding);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassNozzelBuilding obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            ClassBuilding.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
