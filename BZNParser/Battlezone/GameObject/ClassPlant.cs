using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "plant")]
    public class ClassPlantFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassPlant(preamble, classLabel);
            ClassPlant.Hydrate(parent, reader, obj as ClassPlant);
            return true;
        }
    }
    public class ClassPlant : ClassBuilding
    {
        public ClassPlant(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassPlant? obj)
        {
            ClassBuilding.Hydrate(parent, reader, obj as ClassBuilding);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassPlant obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            ClassBuilding.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
