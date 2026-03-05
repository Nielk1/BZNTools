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
                obj = new ClassFlareMine(preamble, classLabel);
            ClassFlareMine.Hydrate(parent, reader, obj as ClassFlareMine);
            return true;
        }
    }
    public class ClassFlareMine : ClassMine
    {
        public ClassFlareMine(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassFlareMine? obj)
        {
            IBZNToken tok;

            if (reader.Format == BZNFormat.Battlezone2)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("undeffloat", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse undeffloat/FLOAT");
                //saveClass = tok.GetSingle();
                //shotTimer

                //tok = reader.ReadToken();
                //if (!tok.Validate("undeffloat", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse undeffloat/FLOAT");
                //saveClass = tok.GetSingle();
            }

            ClassMine.Hydrate(parent, reader, obj as ClassMine);
        }
    }
}
