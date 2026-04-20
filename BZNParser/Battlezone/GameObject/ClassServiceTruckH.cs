using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "serviceh")]
    public class ClassServiceTruckHFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassServiceTruckH(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                return ClassServiceTruckH.Hydrate(parent, reader, obj as ClassServiceTruckH).Success;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassServiceTruckH : ClassHoverCraft
    {
        public ClassServiceTruckH(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static ParseResult Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassServiceTruckH? obj)
        {
            return ClassHoverCraft.Hydrate(parent, reader, obj as ClassHoverCraft);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassServiceTruckH obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            ClassHoverCraft.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
