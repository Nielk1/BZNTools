using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "repairkit")]
    [ObjectClass(BZNFormat.BattlezoneN64, "repairkit")]
    public class ClassHealthPowerup1Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassHealthPowerup1(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                return ClassHealthPowerup1.Hydrate(parent, reader, obj as ClassHealthPowerup1).Success;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassHealthPowerup1 : ClassPowerUp
    {
        public ClassHealthPowerup1(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static ParseResult Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassHealthPowerup1? obj)
        {
            return ClassPowerUp.Hydrate(parent, reader, obj as ClassPowerUp);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassHealthPowerup1 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            ClassPowerUp.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
