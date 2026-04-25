using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "flare")]
    [ObjectClass(BZNFormat.BattlezoneN64, "flare")]
    [ObjectClass(BZNFormat.Battlezone2, "flare")]
    public class ClassFlareMineFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassFlareMine(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                return ClassFlareMine.Hydrate(parent, reader, obj as ClassFlareMine).Success;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassFlareMine : ClassMine
    {
        public float Undeffloat { get; set; }

        public ClassFlareMine(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
            Undeffloat = 0;
        }

        public override void ClearMalformations()
        {
            Malformations.Clear();
            base.ClearMalformations();
        }

        public override void DisableMalformationAutoFix()
        {
            base.DisableMalformationAutoFix();
        }

        public override void EnableMalformationAutoFix()
        {
            base.EnableMalformationAutoFix();
        }


        public static ParseResult Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassFlareMine? obj)
        {
            IBZNToken? tok;

            if (reader.Format == BZNFormat.Battlezone2)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("undeffloat", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse undeffloat/FLOAT");
                //if (obj != null) obj.Undeffloat = tok.GetSingle();
                //shotTimer?
                tok.ApplySingle(obj, x => x.Undeffloat, format: reader.FloatFormat);
            }

            return ClassMine.Hydrate(parent, reader, obj as ClassMine);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassFlareMine obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (writer.Format == BZNFormat.Battlezone2)
            {
                writer.WriteSingle("undeffloat", obj, x => x.Undeffloat); // type and name unconfirmed
            }
            ClassMine.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
