using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "assaulthover")]
    public class ClassAssaultHoverFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassAssaultHover(preamble, classLabel);
            ClassAssaultHover.Hydrate(parent, reader, obj as ClassAssaultHover);
            return true;
        }
    }
    public class ClassAssaultHover : ClassHoverCraft
    {
        public ClassAssaultHover(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }

        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassAssaultHover? obj)
        {
            if (parent.SaveType != SaveType.BZN)
            {
                // turret control
            }

            ClassHoverCraft.Hydrate(parent, reader, obj as ClassHoverCraft);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassAssaultHover obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (parent.SaveType != SaveType.BZN)
            {
                // turret control
            }

            ClassHoverCraft.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
