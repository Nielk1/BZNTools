using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "scavengerh")]
    public class ClassScavengerHFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassScavengerH(preamble, classLabel);
            ClassScavengerH.Hydrate(parent, reader, obj as ClassScavengerH);
            return true;
        }
    }
    public class ClassScavengerH : ClassDeployable
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

        public ClassScavengerH(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassScavengerH? obj)
        {
            if (parent.SaveType != SaveType.BZN)
            {
                IBZNToken? tok;

                if (reader.Version >= 1109)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("curScrap", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse curScrap/LONG");
                    tok.ApplyUInt32(obj, x => x.curScrap);

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("maxScrap", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse maxScrap/LONG");
                    tok.ApplyUInt32(obj, x => x.maxScrap);
                }

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("buildActive", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse buildActive/BOOL");
                tok.ApplyBoolean(obj, x => x.buildActive);

                if (reader.Version < 1107)
                {
                    // severe type missmatch must be resolved
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("bornTime", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse bornTime/LONG");
                    tok.ApplyUInt32(obj, x => x.bornTime);

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("lifeTime", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse lifeTime/LONG");
                    tok.ApplyUInt32(obj, x => x.lifeTime);
                }
                else
                {
                    // severe type missmatch must be resolved
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("buildTime", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse buildTime/LONG");
                    tok.ApplyUInt32(obj, x => x.buildTime);
                }

                if (reader.Version >= 1109)
                {
                    //tok = reader.ReadToken();
                    //if (tok == null || !tok.Validate("buildMatrix", BinaryFieldType.DATA_MAT3D)) throw new Exception("Failed to parse buildMatrix/MAT3D"); // type unconfirmed
                    //if (obj != null)
                    //{
                    //    obj.buildMatrix = tok.GetMatrix();
                    //    tok.CheckMalformationsMatrix(obj.buildMatrix.Malformations, reader.FloatFormat);
                    //}

                    reader.ReadMatrix("buildMatrix", obj, x => x.buildMatrix);
                }

                if (reader.Version >= 1148)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("pickupScrap", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse pickupScrap/LONG");
                    tok.ApplyUInt32(obj, x => x.pickupScrap);
                }

                if (reader.Version >= 1149)
                {
                    // severe type missmatch must be resolved
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("scrapTimer", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse scrapTimer/LONG");
                    tok.ApplyUInt32(obj, x => x.scrapTimer);
                }
            }

            ClassDeployable.Hydrate(parent, reader, obj as ClassDeployable);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassScavengerH obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (parent.SaveType != SaveType.BZN)
            {
                if (writer.Version >= 1109)
                {
                    writer.WriteUInt32("curScrap", obj, x => x.curScrap);
                    writer.WriteUInt32("maxScrap", obj, x => x.maxScrap);
                }
                writer.WriteBoolean("buildActive", obj, x => x.buildActive);
                if (writer.Version < 1107)
                {
                    // severe type missmatch must be resolved
                    writer.WriteUInt32("bornTime", obj, x => x.bornTime);
                    writer.WriteUInt32("lifeTime", obj, x => x.lifeTime);
                }
                else
                {
                    // severe type missmatch must be resolved
                    writer.WriteUInt32("buildTime", obj, x => x.buildTime);
                }
                if (writer.Version >= 1109)
                {
                    //writer.WriteMat3Ds("buildMatrix", obj.buildMatrix); // type unconfirmed
                    writer.WriteMatrix("buildMatrix", obj, x => x.buildMatrix);
                }
                if (writer.Version >= 1148)
                {
                    writer.WriteUInt32("pickupScrap", obj, x => x.pickupScrap);
                }
                if (writer.Version >= 1149)
                {
                    // severe type missmatch must be resolved
                    writer.WriteUInt32("scrapTimer", obj, x => x.scrapTimer);
                }
            }

            ClassDeployable.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
