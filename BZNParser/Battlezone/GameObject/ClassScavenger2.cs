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
        public UInt32 curScrap { get; set; }
        public UInt32 maxScrap { get; set; }
        public bool buildActive { get; set; }
        public UInt32 bornTime { get; set; }
        public UInt32 lifeTime { get; set; }
        public UInt32 buildTime { get; set; }
        public Matrix buildMatrix { get; set; }
        public UInt32 pickupScrap { get; set; }
        public UInt32 scrapTimer { get; set; }

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
                    if (obj != null) obj.curScrap = tok.GetUInt32();

                    tok = reader.ReadToken();
                    if (!tok.Validate("maxScrap", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse maxScrap/LONG");
                    if (obj != null) obj.maxScrap = tok.GetUInt32();
                }

                tok = reader.ReadToken();
                if (!tok.Validate("buildActive", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse buildActive/BOOL");
                if (obj != null) obj.buildActive = tok.GetBoolean();

                if (reader.Version < 1107)
                {
                    // severe type missmatch must be resolved
                    tok = reader.ReadToken();
                    if (!tok.Validate("bornTime", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse bornTime/LONG");
                    if (obj != null) obj.bornTime = tok.GetUInt32();

                    tok = reader.ReadToken();
                    if (!tok.Validate("lifeTime", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse lifeTime/LONG");
                    if (obj != null) obj.lifeTime = tok.GetUInt32();
                }
                else
                {
                    // severe type missmatch must be resolved
                    tok = reader.ReadToken();
                    if (!tok.Validate("buildTime", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse buildTime/LONG");
                    if (obj != null) obj.buildTime = tok.GetUInt32();
                }

                if (reader.Version >= 1109)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("buildMatrix", BinaryFieldType.DATA_MAT3D)) throw new Exception("Failed to parse buildMatrix/MAT3D"); // type unconfirmed
                    if (obj != null)
                    {
                        obj.buildMatrix = tok.GetMatrix();
                        tok.CheckMalformationsMatrix(obj.buildMatrix.Malformations, reader.FloatFormat);
                    }
                }

                if (reader.Version >= 1148)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("pickupScrap", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse pickupScrap/LONG");
                    if (obj != null) obj.pickupScrap = tok.GetUInt32();
                }

                if (reader.Version >= 1149)
                {
                    // severe type missmatch must be resolved
                    tok = reader.ReadToken();
                    if (!tok.Validate("scrapTimer", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse scrapTimer/LONG");
                    if (obj != null) obj.scrapTimer = tok.GetUInt32();
                }
            }

            ClassTrackedDeployable.Hydrate(parent, reader, obj as ClassTrackedDeployable);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassScavenger2 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            if (parent.SaveType != SaveType.BZN)
            {
                if (writer.Version >= 1109)
                {
                    writer.WriteUnsignedValues("curScrap", obj.curScrap);
                    writer.WriteUnsignedValues("maxScrap", obj.maxScrap);
                }
                writer.WriteBooleans("buildActive", preserveMalformations ? obj.Malformations : null, obj.buildActive);
                if (writer.Version < 1107)
                {
                    // severe type missmatch must be resolved
                    writer.WriteUnsignedValues("bornTime", (UInt32)obj.bornTime);
                    writer.WriteUnsignedValues("lifeTime", (UInt32)obj.lifeTime);
                }
                else
                {
                    // severe type missmatch must be resolved
                    writer.WriteUnsignedValues("buildTime", (UInt32)obj.buildTime);
                }
                if (writer.Version >= 1109)
                {
                    writer.WriteMat3Ds("buildMatrix", preserveMalformations, obj.buildMatrix); // type unconfirmed
                }
                if (writer.Version >= 1148)
                {
                    writer.WriteUnsignedValues("pickupScrap", obj.pickupScrap);
                }
                if (writer.Version >= 1149)
                {
                    // severe type missmatch must be resolved
                    writer.WriteUnsignedValues("scrapTimer", (UInt32)obj.scrapTimer);
                }
            }

            ClassTrackedDeployable.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
