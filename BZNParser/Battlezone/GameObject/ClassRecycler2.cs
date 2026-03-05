using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "recycler")]
    public class ClassRecycler2Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassRecycler2(preamble, classLabel);
            ClassRecycler2.Hydrate(parent, reader, obj as ClassRecycler2);
            return true;
        }
    }
    public class ClassRecycler2 : ClassFactory2
    {
        public UInt32 undefptr { get; set; }

        public ClassRecycler2(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassRecycler2? obj)
        {
            IBZNToken tok;

            tok = reader.ReadToken();
            if (!tok.Validate("scrapTimer", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse scrapTimer/FLOAT");
            //scrapTimer = tok.GetSingle();

            ClassFactory2.Hydrate(parent, reader, obj as ClassFactory2);
        }
    }
}
