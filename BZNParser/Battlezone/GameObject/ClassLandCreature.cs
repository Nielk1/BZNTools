using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "animal")]
    public class ClassLandCreatureFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassLandCreature(preamble, classLabel);
            ClassLandCreature.Hydrate(parent, reader, obj as ClassLandCreature);
            return true;
        }
    }
    public class ClassLandCreature : ClassCraft
    {
        public ClassLandCreature(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassLandCreature? obj)
        {
            ClassCraft.Hydrate(parent, reader, obj as ClassCraft);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassLandCreature obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            ClassCraft.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
