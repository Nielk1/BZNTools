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
                obj = new ClassAPC2(preamble, classLabel);
            ClassAPC2.Hydrate(parent, reader, obj as ClassAPC2);
            return true;
        }
    }
    public class ClassAPC2 : ClassHoverCraft
    {
        const int APC_MAX_SOLDIERS = 16;

        public int InternalSoldierCount { get; set; }
        public float nextSoldierDelay { get; set; }
        public float nextSoldierAngle { get; set; }
        public float nextReturnToAPC { get; set; }
        public int ExternalSoldierCount { get; set; }
        public UInt32[]? ExternalSoldiers { get; set; }
        public bool DeployOnLanding { get; set; }
        public Int32 undeployTimeout { get; set; }

        public ClassAPC2(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassAPC2? obj)
        {
            IBZNToken tok;

            tok = reader.ReadToken();
            if (!tok.Validate("IsoldierCount", BinaryFieldType.DATA_LONG))
                throw new Exception("Failed to parse IsoldierCount/LONG");
            if (obj != null) obj.InternalSoldierCount = tok.GetInt32();

            tok = reader.ReadToken();
            if (!tok.Validate("EsoldierCount", BinaryFieldType.DATA_LONG))
                throw new Exception("Failed to parse EsoldierCount/LONG");
            int ExternalSoldierCount = tok.GetInt32();
            if (obj != null) obj.ExternalSoldierCount = ExternalSoldierCount;

            if (ExternalSoldierCount > 0)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("SoldierHandles", BinaryFieldType.DATA_PTR))
                    throw new Exception("Failed to parse SoldierHandles/PTR");
                //tok.GetUInt32H();
                if (obj != null)
                {
                    int count = tok.GetCount();
                    if (count > APC_MAX_SOLDIERS)
                        obj.Malformations.AddOvercount("ExternalSoldiers");
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
                if (!tok.Validate("nextSoldierDelay", BinaryFieldType.DATA_FLOAT))
                    throw new Exception("Failed to parse nextSoldierDelay/FLOAT");
                if (obj != null) obj.nextSoldierDelay = tok.GetSingle(); // nextSoldierDelay

                tok = reader.ReadToken();
                if (!tok.Validate("nextSoldierAngle", BinaryFieldType.DATA_FLOAT))
                    throw new Exception("Failed to parse nextSoldierAngle/FLOAT");
                if (obj != null) obj.nextSoldierAngle = tok.GetSingle(); // nextSoldierAngle

                tok = reader.ReadToken();
                if (!tok.Validate("nextReturnTimer", BinaryFieldType.DATA_FLOAT))
                    throw new Exception("Failed to parse nextReturnTimer/FLOAT");
                if (obj != null) obj.nextReturnToAPC = tok.GetSingle(); // nextReturnTimer

                tok = reader.ReadToken();
                if (!tok.Validate("DeployOnLanding", BinaryFieldType.DATA_BOOL))
                    throw new Exception("Failed to parse DeployOnLanding/BOOL");
                tok.ReadBoolean(obj, x => x.DeployOnLanding); // DeployOnLanding

                tok = reader.ReadToken();
                if (!tok.Validate("undeployTimeout", BinaryFieldType.DATA_LONG))
                    throw new Exception("Failed to parse undeployTimeout/LONG");
                if (obj != null) obj.undeployTimeout = tok.GetInt32(); // undeployTimeout
            }
            
            tok = reader.ReadToken();
            if (!tok.Validate("state", BinaryFieldType.DATA_VOID)) throw new Exception("Failed to parse state/VOID");
            if (obj != null) obj.state = (VEHICLE_STATE)tok.GetUInt32HR(); // state

            ClassHoverCraft.Hydrate(parent, reader, obj as ClassHoverCraft);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassAPC2 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            writer.WriteSignedValues("IsoldierCount", obj.InternalSoldierCount);
            writer.WriteSignedValues("EsoldierCount", obj.ExternalSoldierCount);

            if (obj.ExternalSoldierCount > 0)
            {
                writer.WritePtrs("SoldierHandles", obj.ExternalSoldiers);
            }

            if (parent.SaveType != SaveType.BZN)
            {
                writer.WriteFloats("nextSoldierDelay", preserveMalformations ? obj.Malformations : null, obj.nextSoldierDelay);
                writer.WriteFloats("nextSoldierAngle", preserveMalformations ? obj.Malformations : null, obj.nextSoldierAngle);
                writer.WriteFloats("nextReturnTimer", preserveMalformations ? obj.Malformations : null, obj.nextReturnToAPC);
                writer.WriteBoolean("DeployOnLanding", obj, x => x.DeployOnLanding);
                writer.WriteSignedValues("undeployTimeout", obj.undeployTimeout);
            }
            
            writer.WriteVoidBytes("state", (UInt32)obj.state);

            ClassHoverCraft.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
