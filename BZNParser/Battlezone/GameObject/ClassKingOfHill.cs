using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "kingofhill")]
    public class ClassKingOfHillFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassKingOfHill(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                return ClassKingOfHill.Hydrate(parent, reader, obj as ClassKingOfHill).Success;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassKingOfHill : ClassBuilding
    {
        public float scoreTimer { get; set; }

        public ClassKingOfHill(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
            scoreTimer = 0;
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


        public static ParseResult Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassKingOfHill? obj)
        {
            IBZNToken? tok;

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("scoreTimer", BinaryFieldType.DATA_FLOAT))
                return ParseResult.Fail("Failed to parse scoreTimer/FLOAT");
            tok.ApplySingle(obj, x => x.scoreTimer, format: reader.FloatFormat);

            return ClassBuilding.Hydrate(parent, reader, obj as ClassBuilding);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassKingOfHill obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            writer.WriteSingle("scoreTimer", obj, x => x.scoreTimer);
            ClassBuilding.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
