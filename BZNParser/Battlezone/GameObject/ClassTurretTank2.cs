using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "turrettank")]
    public class ClassTurretTank2Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassTurretTank2(preamble, classLabel);
            ClassTurretTank2.Hydrate(parent, reader, obj as ClassTurretTank2);
            return true;
        }
    }
    public class ClassTurretTank2 : ClassDeployable
    {
        protected float omegaTurret { get; set; }
        protected float timeDeploy { get; set; }
        protected float timeUndeploy { get; set; }
        protected int change_state { get; set; }
        protected float delayTimer { get; set; }
        protected bool turretAligned { get; set; }
        protected bool wantTurret { get; set; } // obsolete, minterpreted as turretAligned
        protected float prevYaw { get; set; }
        public ClassTurretTank2(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassTurretTank2? obj)
        {
            IBZNToken tok;

            if (parent.SaveType == SaveType.LOCKSTEP || parent.SaveType == SaveType.JOIN)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("omegaTurret", BinaryFieldType.DATA_FLOAT))
                    throw new Exception("Failed to parse omegaTurret/FLOAT");
                if (obj != null) obj.omegaTurret = tok.GetSingle(); // omegaTurret

                tok = reader.ReadToken();
                if (!tok.Validate("timeDeploy", BinaryFieldType.DATA_FLOAT))
                    throw new Exception("Failed to parse timeDeploy/FLOAT");
                if (obj != null) obj.timeDeploy = tok.GetSingle(); // timeDeploy

                tok = reader.ReadToken();
                if (!tok.Validate("timeUndeploy", BinaryFieldType.DATA_FLOAT))
                    throw new Exception("Failed to parse timeUndeploy/FLOAT");
                if (obj != null) obj.timeUndeploy = tok.GetSingle(); // timeUndeploy

                tok = reader.ReadToken();
                if (!tok.Validate("change_state", BinaryFieldType.DATA_LONG))
                    throw new Exception("Failed to parse change_state/LONG");
                int change_state = tok.GetInt32(); // change_state


                tok = reader.ReadToken();
                if (!tok.Validate("delayTimer", BinaryFieldType.DATA_FLOAT))
                    throw new Exception("Failed to parse delayTimer/FLOAT");
                if (obj != null) obj.delayTimer = tok.GetSingle(); // delayTimer

                tok = reader.ReadToken();
                if (!tok.Validate("turretAligned", BinaryFieldType.DATA_BOOL))
                    throw new Exception("Failed to parse turretAligned/BOOL");
                if (obj != null) obj.turretAligned = tok.GetBoolean(); // turretAligned

                tok = reader.ReadToken();
                if (!tok.Validate("prevYaw", BinaryFieldType.DATA_FLOAT))
                    throw new Exception("Failed to parse prevYaw/FLOAT");
                float prevYaw = tok.GetSingle(); // prevYaw

                throw new NotImplementedException("Turret Control loading loop needed here");
            }
            else
            {
                bool m_Use13Aim = false; // we're assuming Use13Aim is impossible before 1109

                reader.Bookmark.Push();
                // Use13Aim might be true if we're >= 1109, so be prepared to walk back and try again
                // if it is Use13Aim, we expect a bool first, if it's not we expect a float

                tok = reader.ReadToken();
                if (tok.Validate("turretAligned", BinaryFieldType.DATA_BOOL))
                {
                    reader.Bookmark.Pop();

                    if (reader.Version < 1109)
                    {
                        // we read a turretAligned but we're too old a version for that to be a thing
                        throw new Exception("Use13Aim turret save data found in BZN Version < 1109, impossible, parse error expected");
                    }

                    //turretAligned = tok.GetBoolean();
                    m_Use13Aim = true;
                }
                else
                {
                    // walk back and try again
                    reader.Bookmark.Pop();

                    tok = reader.ReadToken();
                    if (!tok.Validate("omegaTurret", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse omegaTurret/FLOAT");
                    if (obj != null) obj.omegaTurret = tok.GetSingle(); // omegaTurret

                    // obsolete
                    tok = reader.ReadToken();
                    if (!tok.Validate("alphaTurret", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse alphaTurret/FLOAT");
                    float alphaTurret = tok.GetSingle(); // alphaTurret

                    tok = reader.ReadToken();
                    if (!tok.Validate("timeDeploy", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse timeDeploy/FLOAT");
                    if (obj != null) obj.timeDeploy = tok.GetSingle(); // timeDeploy

                    tok = reader.ReadToken();
                    if (!tok.Validate("timeUndeploy", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse timeUndeploy/FLOAT");
                    if (obj != null) obj.timeUndeploy = tok.GetSingle(); // timeUndeploy

                    tok = reader.ReadToken();
                    if (!tok.Validate("state", BinaryFieldType.DATA_VOID))
                        throw new Exception("Failed to parse state/VOID");
                    if (obj != null) obj.state = (VEHICLE_STATE)tok.GetUInt32(); // state

                    tok = reader.ReadToken();
                    if (!tok.Validate("delayTimer", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse delayTimer/FLOAT");
                    if (obj != null) obj.delayTimer = tok.GetSingle(); // delayTimer

                    if (reader.Version == 1100)
                    {
                        // obsolete
                        tok = reader.ReadToken();
                        if (!tok.Validate("wantTurret", BinaryFieldType.DATA_BOOL))
                            throw new Exception("Failed to parse wantTurret/BOOL");
                        if (obj != null)
                        {
                            obj.Malformations.AddMisinterpretation("wantTurret", "turretAligned");
                            obj.wantTurret = tok.GetBoolean(); // wantTurret
                        }
                    }
                    else
                    {
                        tok = reader.ReadToken();
                        if (!tok.Validate("turretAligned", BinaryFieldType.DATA_BOOL))
                            throw new Exception("Failed to parse turretAligned/BOOL");
                        if (obj != null) obj.turretAligned = tok.GetBoolean(); // turretAligned
                    }

                    if (parent.SaveType != SaveType.BZN && reader.Version >= 1140)
                    {
                        if (!m_Use13Aim)
                        {
                            tok = reader.ReadToken();
                            if (!tok.Validate("prevYaw", BinaryFieldType.DATA_FLOAT))
                                throw new Exception("Failed to parse prevYaw/FLOAT");
                            if (obj != null) obj.prevYaw = tok.GetSingle(); // prevYaw

                            tok = reader.ReadToken();
                            if (!tok.Validate("change_state", BinaryFieldType.DATA_LONG))
                                throw new Exception("Failed to parse change_state/LONG");
                            if (obj != null) obj.change_state = tok.GetInt32(); // change_state
                        }
                    }

                    if (reader.Version < 1109)
                    {
                        if (obj != null) obj.m_Use13Aim = m_Use13Aim;
                        ClassHoverCraft.Hydrate(parent, reader, obj as ClassHoverCraft);
                        return;
                    }
                }

                if (obj != null) obj.m_Use13Aim = m_Use13Aim;

                if (m_Use13Aim)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("turretAligned", BinaryFieldType.DATA_BOOL))
                        throw new Exception("Failed to parse turretAligned/BOOL");
                    if (obj != null) obj.turretAligned = tok.GetBoolean(); // turretAligned
                }

                if (parent.SaveType != SaveType.BZN)
                {
                    if (m_Use13Aim)
                    {
                        throw new NotImplementedException("Turret Control loading loop needed here");
                    }
                }
            }

            // parent.SaveType != SaveType.BZN

            ClassDeployable.Hydrate(parent, reader, obj as ClassDeployable);
        }
    }
}
