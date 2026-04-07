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
            {
                obj = new ClassAirCraft(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                ClassAirCraft.Hydrate(parent, reader, obj as ClassAirCraft);
                return true;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
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

        public ClassAirCraft(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) {
            deployTimer = 0;
            lastSteer = 0;
            lastThrot = 0;
            lastStrafe = 0;
            m_bLockMode = false;
            m_bLockModeDeployed = false;
        }
        public override void ClearMalformations()
        {
            Malformations.Clear();
            base.ClearMalformations();
        }

        // implement it later for non-BZN save types
        //private bool blockAutoFixMalformations = false;
        public override void DisableMalformationAutoFix()
        {
            //blockAutoFixMalformations = true;
            base.DisableMalformationAutoFix();
        }

        public override void EnableMalformationAutoFix()
        {
            //blockAutoFixMalformations = false;
            base.EnableMalformationAutoFix();
        }


        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassAirCraft? obj)
        {
            if (parent.SaveType != SaveType.BZN)
            {
                IBZNToken? tok;

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("state", BinaryFieldType.DATA_VOID))
                    throw new Exception("Failed to parse state/VOID");
                tok.ApplyVoidBytes(obj, x => x.state, 0, (v) => (VEHICLE_STATE)BitConverter.ToUInt32(v));

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("deployTimer", BinaryFieldType.DATA_FLOAT))
                    throw new Exception("Failed to parse deployTimer/FLOAT");
                tok.ApplySingle(obj, x => x.deployTimer);

                if (parent.SaveType == SaveType.LOCKSTEP)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("lastSteer", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse lastSteer/FLOAT");
                    tok.ApplySingle(obj, x => x.lastSteer);

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("lastSteer", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse lastSteer/FLOAT");
                    tok.ApplySingle(obj, x => x.lastThrot);

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("lastStrafe", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse lastStrafe/FLOAT");
                    tok.ApplySingle(obj, x => x.lastStrafe);
                }

                if (reader.Version >= 1138)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("lockMode", BinaryFieldType.DATA_BOOL))
                        throw new Exception("Failed to parse lockMode/BOOL");
                    tok.ApplyBoolean(obj, x => x.m_bLockMode); // lockMode

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("lockModeDeployed", BinaryFieldType.DATA_BOOL))
                        throw new Exception("Failed to parse lockModeDeployed/BOOL");
                    tok.ApplyBoolean(obj, x => x.m_bLockModeDeployed); // lockModeDeployed
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

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassAirCraft obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (parent.SaveType != SaveType.BZN)
            {
                writer.WriteVoidBytes("state", obj, x => x.state, (v) => BitConverter.GetBytes((UInt32)v));
                writer.WriteSingle("deployTimer", obj, x => x.deployTimer);

                if (parent.SaveType == SaveType.LOCKSTEP)
                {
                    writer.WriteSingle("lastSteer", obj, x => x.lastSteer);
                    writer.WriteSingle("lastThrot", obj, x => x.lastThrot);
                    writer.WriteSingle("lastStrafe", obj, x => x.lastStrafe);
                }

                if (writer.Version >= 1138)
                {
                    writer.WriteBoolean("lockMode", obj, x => x.m_bLockMode);
                    writer.WriteBoolean("lockModeDeployed", obj, x => x.m_bLockModeDeployed);
                }
            }

            ClassCraft.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
