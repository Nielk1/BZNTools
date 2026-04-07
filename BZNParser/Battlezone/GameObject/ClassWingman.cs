using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "wingman")]
    [ObjectClass(BZNFormat.BattlezoneN64, "wingman")]
    [ObjectClass(BZNFormat.Battlezone2, "wingman")]
    public class ClassWingmanFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassWingman(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                ClassWingman.Hydrate(parent, reader, obj as ClassWingman);
                return true;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassWingman : ClassHoverCraft
    {
        public ClassWingman(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassWingman? obj)
        {
            ClassHoverCraft.Hydrate(parent, reader, obj as ClassHoverCraft);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public void Dehydrate(ClassWingman classWingman, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            ClassHoverCraft.Dehydrate(classWingman as ClassHoverCraft, parent, writer, binary, save);
        }
    }
}
