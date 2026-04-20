using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "sav")]
    [ObjectClass(BZNFormat.BattlezoneN64, "sav")]
    [ObjectClass(BZNFormat.Battlezone2, "sav")]
    public class ClassSAVFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassSAV(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                return ClassSAV.Hydrate(parent, reader, obj as ClassSAV).Success;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassSAV : ClassHoverCraft
    {
        public ClassSAV(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static ParseResult Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassSAV? obj)
        {
            return ClassHoverCraft.Hydrate(parent, reader, obj as ClassHoverCraft);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassSAV obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            ClassHoverCraft.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
