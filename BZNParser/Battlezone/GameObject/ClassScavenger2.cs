using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "scavenger")]
    public class ClassScavenger2Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassScavenger2(preamble, classLabel);
            ClassScavenger2.Hydrate(parent, reader, obj as ClassScavenger2);
            return true;
        }
    }
    public class ClassScavenger2 : ClassTrackedDeployable
    {
        public ClassScavenger2(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassScavenger2? obj)
        {
            if (parent.SaveType != SaveType.BZN)
            {
                IBZNToken tok;

                if (reader.Version >= 1109)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("curScrap", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse curScrap/LONG");
                    UInt32 curScrap = tok.GetUInt32();

                    tok = reader.ReadToken();
                    if (!tok.Validate("maxScrap", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse maxScrap/LONG");
                    UInt32 maxScrap = tok.GetUInt32();
                }

                tok = reader.ReadToken();
                if (!tok.Validate("buildActive", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse buildActive/BOOL");
                bool buildActive = tok.GetBoolean();

                if (reader.Version < 1107)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("bornTime", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse bornTime/FLOAT");
                    float bornTime = tok.GetSingle();

                    tok = reader.ReadToken();
                    if (!tok.Validate("lifeTime", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse lifeTime/FLOAT");
                    float lifeTime = tok.GetSingle();
                }
                else
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("buildTime", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse buildTime/FLOAT");
                    float buildTime = tok.GetSingle();
                }

                if (reader.Version >= 1109)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("buildMatrix", BinaryFieldType.DATA_MAT3D)) throw new Exception("Failed to parse buildMatrix/MAT3D"); // type unconfirmed
                    Matrix dropMat = tok.GetMatrix();
                }

                if (reader.Version >= 1148)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("pickupScrap", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse pickupScrap/LONG");
                    UInt32 pickupScrap = tok.GetUInt32();
                }

                if (reader.Version >= 1149)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("scrapTimer", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse scrapTimer/FLOAT");
                    float scrapTimer = tok.GetSingle();
                }
            }

            ClassTrackedDeployable.Hydrate(parent, reader, obj as ClassTrackedDeployable);
        }
    }
}
