using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "artillery")]
    public class ClassArtilleryFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassArtillery(preamble, classLabel);
            ClassArtillery.Hydrate(parent, reader, obj as ClassArtillery);
            return true;
        }
    }
    public class ClassArtillery : ClassTurretTank2
    {
        public float heightDeploy { get; set; }
        public float deployTimer { get; set; }
        public float prevYaw { get; set; }

        public ClassArtillery(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassArtillery? obj)
        {
            if (reader.Version < 1110)
            {
                IBZNToken tok;

                tok = reader.ReadToken();
                if (!tok.Validate("state", BinaryFieldType.DATA_VOID)) throw new Exception("Failed to parse state/VOID");
                if (obj != null) obj.state = (VEHICLE_STATE)tok.GetUInt32HR();

                if (parent.SaveType != SaveType.BZN)
                {
                    // ignored
                    tok = reader.ReadToken();
                    if (!tok.Validate("omegaTurret", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse omegaTurret/FLOAT");
                    if (obj != null) obj.omegaTurret = tok.GetSingle(); // omegaTurret

                    // ignored
                    tok = reader.ReadToken();
                    if (!tok.Validate("heightDeploy", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse heightDeploy/FLOAT");
                    if (obj != null) obj.heightDeploy = tok.GetSingle(); // heightDeploy

                    // ignored
                    tok = reader.ReadToken();
                    if (!tok.Validate("timeDeploy", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse timeDeploy/FLOAT");
                    if (obj != null) obj.timeDeploy = tok.GetSingle(); // timeDeploy

                    // ignored
                    tok = reader.ReadToken();
                    if (!tok.Validate("timeUndeploy", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse timeUndeploy/FLOAT");
                    if (obj != null) obj.timeUndeploy = tok.GetSingle(); // timeUndeploy

                    tok = reader.ReadToken();
                    if (!tok.Validate("deployTimer", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse deployTimer/FLOAT");
                    if (obj != null) obj.deployTimer = tok.GetSingle();

                    tok = reader.ReadToken();
                    if (!tok.Validate("prevYaw", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse prevYaw/FLOAT");
                    if (obj != null) obj.prevYaw = tok.GetSingle(); // prevYaw
                }

                ClassHoverCraft.Hydrate(parent, reader, obj as ClassHoverCraft);
            }
            else
            {
                if (parent.SaveType != SaveType.BZN)
                {
                    IBZNToken tok = reader.ReadToken();
                    if (!tok.Validate("prevYaw", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse prevYaw/FLOAT");
                    if (obj != null) obj.prevYaw = tok.GetSingle(); // prevYaw
                }

                ClassTurretTank2.Hydrate(parent, reader, obj as ClassTurretTank2);
            }
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassArtillery obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            if (writer.Version < 1110)
            {
                writer.WriteVoidBytes("state", (UInt32)obj.state);

                if (parent.SaveType != SaveType.BZN)
                {
                    writer.WriteFloats("omegaTurret", preserveMalformations ? obj.Malformations : null, obj.omegaTurret); // omegaTurret
                    writer.WriteFloats("heightDeploy", preserveMalformations ? obj.Malformations : null, obj.heightDeploy); // heightDeploy
                    writer.WriteFloats("timeDeploy", preserveMalformations ? obj.Malformations : null, obj.timeDeploy); // timeDeploy
                    writer.WriteFloats("timeUndeploy", preserveMalformations ? obj.Malformations : null, obj.timeUndeploy); // timeUndeploy
                    writer.WriteFloats("deployTimer", preserveMalformations ? obj.Malformations : null, obj.deployTimer);
                    writer.WriteFloats("prevYaw", preserveMalformations ? obj.Malformations : null, obj.prevYaw); // prevYaw
                }

                ClassHoverCraft.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
            }
            else
            {
                if (parent.SaveType != SaveType.BZN)
                {
                    writer.WriteFloats("prevYaw", preserveMalformations ? obj.Malformations : null, obj.prevYaw); // prevYaw
                }

                ClassTurretTank2.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
            }
        }
    }
}
