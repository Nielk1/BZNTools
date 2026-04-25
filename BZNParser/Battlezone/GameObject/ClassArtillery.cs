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
            {
                obj = new ClassArtillery(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                return ClassArtillery.Hydrate(parent, reader, obj as ClassArtillery).Success;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassArtillery : ClassTurretTank2
    {
        public float heightDeploy { get; set; }
        //public float deployTimer { get; set; }
        //public float prevYaw { get; set; }

        public ClassArtillery(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
            heightDeploy = 0;
        }

        public override void ClearMalformations()
        {
            Malformations.Clear();
            base.ClearMalformations();
        }

        public override void DisableMalformationAutoFix()
        {
            base.DisableMalformationAutoFix();
        }

        public override void EnableMalformationAutoFix()
        {
            base.EnableMalformationAutoFix();
        }

        public static ParseResult Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassArtillery? obj)
        {
            if (reader.Version < 1110)
            {
                IBZNToken? tok;

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("state", BinaryFieldType.DATA_VOID))
                    return ParseResult.Fail("Failed to parse state/VOID");
                tok.ApplyVoidBytes(obj, x => x.state, 0, (v) => (VEHICLE_STATE)BitConverter.ToUInt32(v));

                if (parent.SaveType != SaveType.BZN)
                {
                    // ignored
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("omegaTurret", BinaryFieldType.DATA_FLOAT))
                        return ParseResult.Fail("Failed to parse omegaTurret/FLOAT");
                    tok.ApplySingle(obj, x => x.omegaTurret, format: reader.FloatFormat);

                    // ignored
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("heightDeploy", BinaryFieldType.DATA_FLOAT))
                        return ParseResult.Fail("Failed to parse heightDeploy/FLOAT");
                    tok.ApplySingle(obj, x => x.heightDeploy, format: reader.FloatFormat);

                    // ignored
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("timeDeploy", BinaryFieldType.DATA_FLOAT))
                        return ParseResult.Fail("Failed to parse timeDeploy/FLOAT");
                    tok.ApplySingle(obj, x => x.timeDeploy, format: reader.FloatFormat);

                    // ignored
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("timeUndeploy", BinaryFieldType.DATA_FLOAT))
                        return ParseResult.Fail("Failed to parse timeUndeploy/FLOAT");
                    tok.ApplySingle(obj, x => x.timeUndeploy, format: reader.FloatFormat);

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("deployTimer", BinaryFieldType.DATA_FLOAT))
                        return ParseResult.Fail("Failed to parse deployTimer/FLOAT");
                    tok.ApplySingle(obj, x => x.deployTimer, format: reader.FloatFormat);

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("prevYaw", BinaryFieldType.DATA_FLOAT))
                        return ParseResult.Fail("Failed to parse prevYaw/FLOAT");
                    tok.ApplySingle(obj, x => x.prevYaw, format: reader.FloatFormat);
                }

                return ClassHoverCraft.Hydrate(parent, reader, obj as ClassHoverCraft);
            }
            else
            {
                if (parent.SaveType != SaveType.BZN)
                {
                    IBZNToken? tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("prevYaw", BinaryFieldType.DATA_FLOAT))
                        return ParseResult.Fail("Failed to parse prevYaw/FLOAT");
                    tok.ApplySingle(obj, x => x.prevYaw, format: reader.FloatFormat);
                }

                return ClassTurretTank2.Hydrate(parent, reader, obj as ClassTurretTank2);
            }
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassArtillery obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (writer.Version < 1110)
            {
                writer.WriteVoidBytes("state", obj, x => x.state, (v) => BitConverter.GetBytes((UInt32)v));

                if (parent.SaveType != SaveType.BZN)
                {
                    writer.WriteSingle("omegaTurret", obj, x => x.omegaTurret);
                    writer.WriteSingle("heightDeploy", obj, x => x.heightDeploy);
                    writer.WriteSingle("timeDeploy", obj, x => x.timeDeploy);
                    writer.WriteSingle("timeUndeploy", obj, x => x.timeUndeploy);
                    writer.WriteSingle("deployTimer", obj, x => x.deployTimer);
                    writer.WriteSingle("prevYaw", obj, x => x.prevYaw);
                }

                ClassHoverCraft.Dehydrate(obj, parent, writer, binary, save);
            }
            else
            {
                if (parent.SaveType != SaveType.BZN)
                {
                    writer.WriteSingle("prevYaw", obj, x => x.prevYaw);
                }

                ClassTurretTank2.Dehydrate(obj, parent, writer, binary, save);
            }
        }
    }
}
