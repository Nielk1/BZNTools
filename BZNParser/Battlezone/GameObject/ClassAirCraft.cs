using BZNParser.Tokenizer;
using System.Reflection.PortableExecutable;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "aircraft")]
    public class ClassAirCraftFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassAirCraft(preamble, classLabel);
            ClassAirCraft.Hydrate(parent, reader, obj as ClassAirCraft);
            return true;
        }
    }
    public class ClassAirCraft : ClassCraft
    {
        protected float deployTimer { get; set; }
        protected float lastSteer { get; set; }
        protected float lastThrot { get; set; }
        protected float lastStrafe { get; set; }
        protected bool m_bLockMode { get; set; }
        protected bool m_bLockModeDeployed { get; set; }

        public ClassAirCraft(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassAirCraft? obj)
        {
            if (parent.SaveType != SaveType.BZN)
            {
                IBZNToken tok;

                tok = reader.ReadToken();
                if (!tok.Validate("state", BinaryFieldType.DATA_VOID))
                    throw new Exception("Failed to parse state/VOID");
                if (obj != null) obj.state = (VEHICLE_STATE)tok.GetUInt32HR(); // state

                tok = reader.ReadToken();
                if (!tok.Validate("deployTimer", BinaryFieldType.DATA_FLOAT))
                    throw new Exception("Failed to parse deployTimer/FLOAT");
                if (obj != null) obj.deployTimer = tok.GetSingle();

                if (parent.SaveType == SaveType.LOCKSTEP)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("lastSteer", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse lastSteer/FLOAT");
                    if (obj != null) obj.lastSteer = tok.GetSingle();

                    tok = reader.ReadToken();
                    if (!tok.Validate("lastSteer", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse lastSteer/FLOAT");
                    if (obj != null) obj.lastThrot = tok.GetSingle();

                    tok = reader.ReadToken();
                    if (!tok.Validate("lastStrafe", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse lastStrafe/FLOAT");
                    if (obj != null) obj.lastStrafe = tok.GetSingle();
                }

                if (reader.Version >= 1138)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("lockMode", BinaryFieldType.DATA_BOOL))
                        throw new Exception("Failed to parse lockMode/BOOL");
                    if (obj != null) obj.m_bLockMode = tok.GetBoolean(); // lockMode

                    tok = reader.ReadToken();
                    if (!tok.Validate("lockModeDeployed", BinaryFieldType.DATA_BOOL))
                        throw new Exception("Failed to parse lockModeDeployed/BOOL");
                    if (obj != null) obj.m_bLockModeDeployed = tok.GetBoolean(); // lockModeDeployed
                }
                else
                {
                    if (obj != null)
                    {
                        obj.m_bLockMode = false;
                        obj.m_bLockModeDeployed = false;
                    }
                }
            }

            ClassCraft.Hydrate(parent, reader, obj as ClassCraft);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassAirCraft obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            if (parent.SaveType != SaveType.BZN)
            {
                writer.WriteVoidBytes("state", (UInt32)obj.state);
                writer.WriteFloats("deployTimer", preserveMalformations ? obj.Malformations : null, obj.deployTimer);

                if (parent.SaveType == SaveType.LOCKSTEP)
                {
                    writer.WriteFloats("lastSteer", preserveMalformations ? obj.Malformations : null, obj.lastSteer);
                    writer.WriteFloats("lastThrot", preserveMalformations ? obj.Malformations : null, obj.lastThrot);
                    writer.WriteFloats("lastStrafe", preserveMalformations ? obj.Malformations : null, obj.lastSteer);
                }

                if (writer.Version >= 1138)
                {
                    writer.WriteBooleans("lockMode", preserveMalformations ? obj.Malformations : null, obj.m_bLockMode);
                    writer.WriteBooleans("lockModeDeployed", preserveMalformations ? obj.Malformations : null, obj.m_bLockModeDeployed);
                }
            }

            ClassCraft.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
