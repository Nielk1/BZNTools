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
                obj = new ClassKingOfHill(preamble, classLabel);
            ClassKingOfHill.Hydrate(parent, reader, obj as ClassKingOfHill);
            return true;
        }
    }
    public class ClassKingOfHill : ClassBuilding
    {
        public float scoreTimer { get; set; }

        public ClassKingOfHill(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassKingOfHill? obj)
        {
            IBZNToken? tok;

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("scoreTimer", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse scoreTimer/FLOAT");
            tok.ApplySingle(obj, x => x.scoreTimer);

            ClassBuilding.Hydrate(parent, reader, obj as ClassBuilding);
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
