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
        public ClassMine(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }

        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassMine? obj)
        {
            if (reader.Format == BZNFormat.Battlezone)
            {
                if (reader.Version >= 1038 && parent.SaveType != SaveType.BZN)
                {
                    IBZNToken tok = reader.ReadToken();
                    if (!tok.Validate("lifeTimer", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse lifeTimer/FLOAT");
                    //lifeTimer = tok.GetSingle();
                }
            }

            if (reader.Format == BZNFormat.Battlezone2)
            {
                IBZNToken tok = reader.ReadToken();
                if (!tok.Validate("undeffloat", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse undeffloat/FLOAT");
                //saveClass = tok.GetSingle();
                //lifeTimer

                //if (reader.Version > 1123)
                //{
                //    IBZNToken tok;
                //
                //    long pos = reader.BaseStream.Position;
                //
                //    tok = reader.ReadToken();
                //    if (!tok.Validate("undeffloat", BinaryFieldType.DATA_FLOAT))
                //    {
                //        reader.BaseStream.Position = pos;
                //    }
                //    else
                //    {
                //        if (!tok.Validate("undeffloat", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse undeffloat/FLOAT");
                //        float undeffloat = (int)tok.GetSingle();
                //    }
                //}
                //if (reader.Version >= 1180 && reader.Version != 1192) // unless it needs to be over 1192, do check
                //{
                //    IBZNToken tok;
                //
                //    tok = reader.ReadToken();
                //    if (!tok.Validate("undeffloat", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse undeffloat/FLOAT");
                //    float undeffloat = (int)tok.GetSingle();
                //}
            }

            ClassBuilding.Hydrate(parent, reader, obj as ClassBuilding);
        }
    }
}
