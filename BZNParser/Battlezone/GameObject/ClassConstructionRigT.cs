using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "constructionrigT")]
    public class ClassConstructionRigTFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassConstructionRigT(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                return ClassConstructionRigT.Hydrate(parent, reader, obj as ClassConstructionRigT).Success;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassConstructionRigT : ClassTrackedDeployable
    {
        public Matrix dropMat { get; set; }
        public SizedString dropClass { get; set; }

        public bool buildQueued { get; set; }
        public bool buildActive { get; set; }
        public float buildTime { get; set; }
        public UInt32 upgradeHandle { get; set; }
        public UInt32 buildGroup { get; set; }

        public ClassConstructionRigT(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
            dropMat = new Matrix();
            dropClass = new SizedString();
            buildQueued = false;
            buildActive = false;
            buildTime = 0;
            upgradeHandle = 0;
            buildGroup = 0;
        }

        public override void ClearMalformations()
        {
            Malformations.Clear();
            dropMat.ClearMalformations();
            dropClass.ClearMalformations();
            base.ClearMalformations();
        }

        public override void DisableMalformationAutoFix()
        {
            dropMat.DisableMalformationAutoFix();
            dropClass.DisableMalformationAutoFix();
            base.DisableMalformationAutoFix();
        }

        public override void EnableMalformationAutoFix()
        {
            dropMat.EnableMalformationAutoFix();
            dropClass.EnableMalformationAutoFix();
            base.EnableMalformationAutoFix();
        }


        public static ParseResult Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassConstructionRigT? obj)
        {
            // constructionrigT was introduced in BZ2 save version 1192 (git 1d5e41a3d, 2018-07-05 per GBD). Fail
            // fast on older BZN files so it is not considered by the "try everything" MultiClass candidate loop.
            if (reader.Version < 1192)
                return ParseResult.Fail("constructionrigT requires BZN save version >= 1192 (file is " + reader.Version + ")");

            IBZNToken? tok;

            if (reader.Version >= 1114)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("buildQueued", BinaryFieldType.DATA_BOOL))
                    return ParseResult.Fail("Failed to parse buildQueued/BOOL");
                tok.ApplyBoolean(obj, x => x.buildQueued);
            }

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("buildActive", BinaryFieldType.DATA_BOOL))
                return ParseResult.Fail("Failed to parse buildActive/BOOL");
            tok.ApplyBoolean(obj, x => x.buildActive);

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("buildTime", BinaryFieldType.DATA_FLOAT))
                return ParseResult.Fail("Failed to parse buildTime/FLOAT");
            tok.ApplySingle(obj, x => x.buildTime, format: reader.FloatFormat);

            reader.ReadMatrix("buildMatrix", obj, x => x.dropMat);

            if (reader.Version == 1149 || reader.Version == 1151)
            {
                reader.ReadSizedString("config", obj, x => x.dropClass);
            }
            else
            {
                reader.ReadSizedString("buildClass", obj, x => x.dropClass);
            }

            if (reader.Version >= 1150)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("upgradeHandle", BinaryFieldType.DATA_LONG))
                    return ParseResult.Fail("Failed to parse upgradeHandle/LONG");
                tok.ApplyUInt32(obj, x => x.upgradeHandle);
            }

            if (parent.SaveType != SaveType.BZN)
            {
                if (reader.Version >= 1120)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("buildGroup", BinaryFieldType.DATA_LONG))
                        return ParseResult.Fail("Failed to parse buildGroup/LONG");
                    tok.ApplyUInt32(obj, x => x.buildGroup);
                }
            }

            return ClassTrackedDeployable.Hydrate(parent, reader, obj as ClassTrackedDeployable);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassConstructionRigT obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (writer.Version >= 1114)
            {
                writer.WriteBoolean("buildQueued", obj, x => x.buildQueued);
            }

            writer.WriteBoolean("buildActive", obj, x => x.buildActive);
            writer.WriteSingle("buildTime", obj, x => x.buildTime);
            writer.WriteMatrix("buildMatrix", obj, x => x.dropMat);

            if (writer.Version == 1149 || writer.Version == 1151)
            {
                writer.WriteSizedString("config", obj, x => x.dropClass);
            }
            else
            {
                writer.WriteSizedString("buildClass", obj, x => x.dropClass);
            }

            if (writer.Version >= 1150)
            {
                writer.WriteUInt32("upgradeHandle", obj, x => x.upgradeHandle);
            }

            if (parent.SaveType != SaveType.BZN)
            {
                if (writer.Version >= 1120)
                {
                    writer.WriteUInt32("buildGroup", obj, x => x.buildGroup);
                }
            }

            ClassTrackedDeployable.Dehydrate(obj as ClassTrackedDeployable, parent, writer, binary, save);
        }
    }
}