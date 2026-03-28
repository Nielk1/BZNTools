using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    // Done BZCC

    public class ClassMineFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassMine(preamble, classLabel);
            ClassMine.Hydrate(parent, reader, obj as ClassMine);
            return true;
        }
    }
    public class ClassMine : ClassBuilding
    {
        public float lifeTimer { get; set; }

        public ClassMine(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }

        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassMine? obj)
        {
            if (reader.Format == BZNFormat.Battlezone)
            {
                if (reader.Version >= 1038 && parent.SaveType != SaveType.BZN)
                {
                    IBZNToken? tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("lifeTimer", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse lifeTimer/FLOAT");
                    tok.ApplySingle(obj, x => x.lifeTimer);
                }
            }

            if (reader.Format == BZNFormat.Battlezone2)
            {
                IBZNToken? tok = reader.ReadToken();
                if (tok == null || !tok.Validate("undeffloat", BinaryFieldType.DATA_FLOAT))
                    throw new Exception("Failed to parse undeffloat/FLOAT");
                tok.ApplySingle(obj, x => x.lifeTimer);
            }

            ClassBuilding.Hydrate(parent, reader, obj as ClassBuilding);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassMine obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (writer.Format == BZNFormat.Battlezone)
            {
                if (writer.Version >= 1038 && parent.SaveType != SaveType.BZN)
                {
                    writer.WriteSingle("lifeTimer", obj, x => x.lifeTimer);
                }
            }

            if (writer.Format == BZNFormat.Battlezone2)
            {
                writer.WriteSingle("undeffloat", obj, x => x.lifeTimer);
            }

            ClassBuilding.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
