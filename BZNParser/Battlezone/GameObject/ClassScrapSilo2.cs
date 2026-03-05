using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "silo")]
    public class ClassScrapSilo2Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassScrapSilo2(preamble, classLabel);
            ClassScrapSilo2.Hydrate(parent, reader, obj as ClassScrapSilo2);
            return true;
        }
    }
    public class ClassScrapSilo2 : ClassBuilding
    {
        public UInt32 undefptr { get; set; }

        public ClassScrapSilo2(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassScrapSilo2? obj)
        {
            IBZNToken tok;

            tok = reader.ReadToken();
            if (!tok.Validate("scrapTimer", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse scrapTimer/FLOAT");
            //saveClass = tok.GetSingle();

            ClassBuilding.Hydrate(parent, reader, obj as ClassBuilding);
        }
    }
}
