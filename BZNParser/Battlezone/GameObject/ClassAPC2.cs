using BZNParser.Tokenizer;
using System.Reflection.PortableExecutable;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "apc")]
    public class ClassAPC2Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassAPC2(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                return ClassAPC2.Hydrate(parent, reader, obj as ClassAPC2).Success;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassAPC2 : ClassHoverCraft
    {
        const int APC_MAX_SOLDIERS = 16;

        public int InternalSoldierCount { get; set; }
        public float NextSoldierDelay { get; set; }
        public float NextSoldierAngle { get; set; }
        public float NextReturnToAPC { get; set; }
        public int ExternalSoldierCount { get; set; }
        public UInt32[]? ExternalSoldiers { get; set; }
        public bool DeployOnLanding { get; set; }
        public Int32 UndeployTimeout { get; set; }

        public ClassAPC2(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
            InternalSoldierCount = 0;
            NextSoldierDelay = 0;
            NextSoldierAngle = 0;
            NextReturnToAPC = 0;
            ExternalSoldierCount = 0;
            ExternalSoldiers = null;
            DeployOnLanding = false;
            UndeployTimeout = 0;
        }

        public override void ClearMalformations()
        {
            Malformations.Clear();
            base.ClearMalformations();
        }

        private bool blockAutoFixMalformations = false;
        public override void DisableMalformationAutoFix()
        {
            blockAutoFixMalformations = true;
            base.DisableMalformationAutoFix();
        }

        public override void EnableMalformationAutoFix()
        {
            blockAutoFixMalformations = false;
            base.EnableMalformationAutoFix();
        }

        public static ParseResult Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassAPC2? obj)
        {
            IBZNToken? tok;

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("IsoldierCount", BinaryFieldType.DATA_LONG))
                return ParseResult.Fail("Failed to parse IsoldierCount/LONG");
            tok.ApplyInt32(obj, x => x.InternalSoldierCount);

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("EsoldierCount", BinaryFieldType.DATA_LONG))
                return ParseResult.Fail("Failed to parse EsoldierCount/LONG");
            (int ExternalSoldierCount, _) = tok.ApplyInt32(obj, x => x.ExternalSoldierCount);

            if (ExternalSoldierCount > 0)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("SoldierHandles", BinaryFieldType.DATA_PTR))
                    return ParseResult.Fail("Failed to parse SoldierHandles/PTR");
                //tok.GetUInt32H();
                if (obj != null)
                {
                    int count = tok.GetCount();
                    //if (count > APC_MAX_SOLDIERS)
                    //    obj.Malformations.AddOvercount("ExternalSoldiers");
                    obj.ExternalSoldiers = new UInt32[Math.Max(APC_MAX_SOLDIERS, count)];
                    for(int i = 0; i < count; i++)
                    {
                        obj.ExternalSoldiers[i] = tok.GetUInt32(i);
                    }
                }
            }
            
            if (parent.SaveType != SaveType.BZN)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("nextSoldierDelay", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse nextSoldierDelay/FLOAT");
                tok.ApplySingle(obj, x => x.NextSoldierDelay, format: reader.FloatFormat);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("nextSoldierAngle", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse nextSoldierAngle/FLOAT");
                tok.ApplySingle(obj, x => x.NextSoldierAngle, format: reader.FloatFormat);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("nextReturnTimer", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse nextReturnTimer/FLOAT");
                tok.ApplySingle(obj, x => x.NextReturnToAPC, format: reader.FloatFormat);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("DeployOnLanding", BinaryFieldType.DATA_BOOL))
                    return ParseResult.Fail("Failed to parse DeployOnLanding/BOOL");
                tok.ApplyBoolean(obj, x => x.DeployOnLanding);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("undeployTimeout", BinaryFieldType.DATA_LONG))
                    return ParseResult.Fail("Failed to parse undeployTimeout/LONG");
                tok.ApplyInt32(obj, x => x.UndeployTimeout);
            }

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("state", BinaryFieldType.DATA_VOID))
                return ParseResult.Fail("Failed to parse state/VOID");
            //if (obj != null) obj.state = (VEHICLE_STATE)tok.GetUInt32HR(); // state
            tok.ApplyVoidBytes(obj, x => x.state, 0, (v) => (VEHICLE_STATE)BitConverter.ToUInt32(v));

            return ClassHoverCraft.Hydrate(parent, reader, obj as ClassHoverCraft);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassAPC2 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            writer.WriteInt32("IsoldierCount", obj, x => x.InternalSoldierCount);
            writer.WriteInt32("EsoldierCount", obj, x => x.ExternalSoldierCount);

            if (obj.ExternalSoldierCount > 0)
            {
                writer.WritePtrs("SoldierHandles", obj, x => x.ExternalSoldiers);
            }

            if (parent.SaveType != SaveType.BZN)
            {
                writer.WriteSingle("nextSoldierDelay", obj, x => x.NextSoldierDelay);
                writer.WriteSingle("nextSoldierAngle", obj, x => x.NextSoldierAngle);
                writer.WriteSingle("nextReturnTimer", obj, x => x.NextReturnToAPC);
                writer.WriteBoolean("DeployOnLanding", obj, x => x.DeployOnLanding);
                writer.WriteInt32("undeployTimeout", obj, x => x.UndeployTimeout);
            }

            writer.WriteVoidBytes("state", obj, x => x.state, (v) => BitConverter.GetBytes((UInt32)v));

            ClassHoverCraft.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
