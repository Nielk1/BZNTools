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
        public ClassKingOfHill(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassKingOfHill? obj)
        {
            IBZNToken tok;

            tok = reader.ReadToken();
            if (!tok.Validate("scoreTimer", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse scoreTimer/FLOAT");
            float scoreTimer = tok.GetSingle();

            ClassBuilding.Hydrate(parent, reader, obj as ClassBuilding);
        }
    }
}
