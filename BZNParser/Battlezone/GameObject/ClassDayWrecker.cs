using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "daywrecker")]
    [ObjectClass(BZNFormat.BattlezoneN64, "daywrecker")]
    [ObjectClass(BZNFormat.Battlezone2, "daywrecker")]
    public class ClassDayWreckerFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassDayWrecker(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                return ClassDayWrecker.Hydrate(parent, reader, obj as ClassDayWrecker).Success;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassDayWrecker : ClassPowerUp
    {
        public ClassDayWrecker(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static ParseResult Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassDayWrecker? obj)
        {
            return ClassPowerUp.Hydrate(parent, reader, obj as ClassPowerUp);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassDayWrecker obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            ClassPowerUp.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
