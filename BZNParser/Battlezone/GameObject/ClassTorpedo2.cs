using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "torpedo")]
    public class ClassTorpedo2Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassTorpedo2(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                ClassTorpedo2.Hydrate(parent, reader, obj as ClassTorpedo2);
                return true;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassTorpedo2 : ClassGameObject
    {
        public float lifeTimer { get; set; }

        public ClassTorpedo2(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
            lifeTimer = 0;
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


        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassTorpedo2? obj)
        {
            IBZNToken? tok;

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("lifeTimer", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse lifeTimer/FLOAT");
            tok.ApplySingle(obj, x => x.lifeTimer);

            ClassGameObject.Hydrate(parent, reader, obj as ClassGameObject);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassTorpedo2 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            writer.WriteSingle("lifeTimer", obj, x => x.lifeTimer);
            ClassGameObject.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
