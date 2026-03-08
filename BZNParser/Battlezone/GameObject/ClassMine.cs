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
                    IBZNToken tok = reader.ReadToken();
                    if (!tok.Validate("lifeTimer", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse lifeTimer/FLOAT");
                    if (obj != null) obj.lifeTimer = tok.GetSingle();
                }
            }

            if (reader.Format == BZNFormat.Battlezone2)
            {
                IBZNToken tok = reader.ReadToken();
                if (!tok.Validate("undeffloat", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse undeffloat/FLOAT");
                if (obj != null) obj.lifeTimer = tok.GetSingle(); // might be lifeTimer
            }

            ClassBuilding.Hydrate(parent, reader, obj as ClassBuilding);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassMine obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            if (writer.Format == BZNFormat.Battlezone)
            {
                if (writer.Version >= 1038 && parent.SaveType != SaveType.BZN)
                {
                    writer.WriteFloats("lifeTimer", obj.lifeTimer);
                }
            }

            if (writer.Format == BZNFormat.Battlezone2)
            {
                writer.WriteFloats("undeffloat", obj.lifeTimer);
            }

            ClassBuilding.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
