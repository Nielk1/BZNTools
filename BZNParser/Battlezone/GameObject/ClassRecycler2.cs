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
            {
                obj = new ClassRecycler2(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                ClassRecycler2.Hydrate(parent, reader, obj as ClassRecycler2);
                return true;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassRecycler2 : ClassFactory2
    {
        public UInt32 undefptr { get; set; } // ?
        public float scrapTimer { get; set; }

        public ClassRecycler2(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
            undefptr = 0;
            scrapTimer = 0;
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


        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassRecycler2? obj)
        {
            IBZNToken? tok;

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("scrapTimer", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse scrapTimer/FLOAT");
            tok.ApplySingle(obj, x => x.scrapTimer);

            ClassFactory2.Hydrate(parent, reader, obj as ClassFactory2);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassRecycler2 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            writer.WriteSingle("scrapTimer", obj, x => x.scrapTimer);

            ClassFactory2.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
