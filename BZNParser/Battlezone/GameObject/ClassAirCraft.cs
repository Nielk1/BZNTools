using BZNParser.Tokenizer;

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
                if (obj != null) obj.state = (VEHICLE_STATE)tok.GetUInt32(); // state

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
                    if (!tok.Validate("lastSteer", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse lastSteer/FLOAT");
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
    }
}
