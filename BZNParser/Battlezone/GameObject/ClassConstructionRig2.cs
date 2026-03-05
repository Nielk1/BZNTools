using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "constructionrig")]
    public class ClassConstructionRig2Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassConstructionRig2(preamble, classLabel);
            ClassConstructionRig2.Hydrate(parent, reader, obj as ClassConstructionRig2);
            return true;
        }
    }
    public class ClassConstructionRig2 : ClassDeployable
    {
        public Matrix dropMat { get; set; }
        public string dropClass { get; set; }

        public ClassConstructionRig2(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassConstructionRig2? obj)
        {
            IBZNToken tok;

            if (reader.Version >= 1114)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("buildQueued", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse buildQueued/BOOL");
                //saveClass = tok.GetBool();
            }

            tok = reader.ReadToken();
            if (!tok.Validate("buildActive", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse buildActive/BOOL");
            //saveClass = tok.GetBool();

            tok = reader.ReadToken();
            if (!tok.Validate("buildTime", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse buildTime/FLOAT");
            //saveClass = tok.GetSingle();

            tok = reader.ReadToken();
            if (!tok.Validate("buildMatrix", BinaryFieldType.DATA_MAT3D)) throw new Exception("Failed to parse buildMatrix/MAT3D"); // type unconfirmed
            if (obj != null) obj.dropMat = tok.GetMatrix();

            //tok = reader.ReadToken();
            //if (!tok.Validate("buildClass", BinaryFieldType.DATA_ID)) throw new Exception("Failed to parse buildClass/ID");
            //dropClass = tok.GetString();
            if (reader.Version == 1149 || reader.Version == 1151)
            //if (reader.Version < 1155) // Doesn't seem to be true here
            {
                if (obj != null) obj.dropClass = reader.ReadGameObjectClass_BZ2(parent, "config");
            }
            else
            {
                if (obj != null) obj.dropClass = reader.ReadGameObjectClass_BZ2(parent, "buildClass");
            }

            if (reader.Version >= 1150)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("upgradeHandle", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse upgradeHandle/LONG");
            }

            if (parent.SaveType != SaveType.BZN)
            { 
                if (reader.Version >= 1120)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("buildGroup", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse buildGroup/LONG");
                    //buildGroup = tok.GetUInt32H();
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
    }
}
