using BZNParser.Tokenizer;
using System.Reflection.PortableExecutable;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "constructionrig")]
    public class ClassConstructionRig2Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassConstructionRig2(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                ClassConstructionRig2.Hydrate(parent, reader, obj as ClassConstructionRig2);
                return true;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassConstructionRig2 : ClassDeployable
    {
        public Matrix dropMat { get; set; }
        public SizedString dropClass { get; set; }

        public bool buildQueued { get; set; }
        public bool buildActive { get; set; }
        public float buildTime { get; set; }
        public UInt32 upgradeHandle { get; set; }
        public UInt32 buildGroup { get; set; }

        public ClassConstructionRig2(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
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


        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassConstructionRig2? obj)
        {
            IBZNToken? tok;

            if (reader.Version >= 1114)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("buildQueued", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse buildQueued/BOOL");
                tok.ApplyBoolean(obj, x => x.buildQueued);
            }

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("buildActive", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse buildActive/BOOL");
            tok.ApplyBoolean(obj, x => x.buildActive);

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("buildTime", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse buildTime/FLOAT");
            tok.ApplySingle(obj, x => x.buildTime);

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
                if (tok == null || !tok.Validate("upgradeHandle", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse upgradeHandle/LONG");
                tok.ApplyUInt32(obj, x => x.upgradeHandle);
            }

            if (parent.SaveType != SaveType.BZN)
            {
                if (reader.Version >= 1120)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("buildGroup", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse buildGroup/LONG");
                    tok.ApplyUInt32(obj, x => x.buildGroup);
                }

                //if (!mbIsHoverRig)
                //{
                //Load AminControl
                //}

                //(a2->vftable->out_bool)(a2, this + 2378, 1, "Alive");
                //(a2->vftable->out_float)(a2, this + 2472, 4, "Dying_Timer");
                //(a2->vftable->out_bool)(a2, this + 2379, 1, "Explosion");

                if (reader.Version < 1107)
                {
                    //float bornTime;
                    //float lifeTime;
                }
            }

            ClassDeployable.Hydrate(parent, reader, obj as ClassDeployable);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassConstructionRig2 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
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

            ClassDeployable.Dehydrate(obj as ClassDeployable, parent, writer, binary, save);
        }
    }
}
