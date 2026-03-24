using BZNParser.Tokenizer;
using System.Reflection.PortableExecutable;

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
        protected UInt32 change_state { get; set; }
        protected float delayTimer { get; set; }
        protected bool turretAligned { get; set; }
        protected bool wantTurret { get; set; } // obsolete, minterpreted as turretAligned
        protected float prevYaw { get; set; }

        public float alphaTurret { get; set; } // obsolete, only read in LOCKSTEP/JOINSAVE, ignored otherwise

        public ClassTurretTank2(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassTurretTank2? obj)
        {
            IBZNToken? tok;

            if (parent.SaveType == SaveType.LOCKSTEP || parent.SaveType == SaveType.JOIN)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("omegaTurret", BinaryFieldType.DATA_FLOAT))
                    throw new Exception("Failed to parse omegaTurret/FLOAT");
                if (obj != null) obj.omegaTurret = tok.GetSingle(); // omegaTurret

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("timeDeploy", BinaryFieldType.DATA_FLOAT))
                    throw new Exception("Failed to parse timeDeploy/FLOAT");
                if (obj != null) obj.timeDeploy = tok.GetSingle(); // timeDeploy

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("timeUndeploy", BinaryFieldType.DATA_FLOAT))
                    throw new Exception("Failed to parse timeUndeploy/FLOAT");
                if (obj != null) obj.timeUndeploy = tok.GetSingle(); // timeUndeploy

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("change_state", BinaryFieldType.DATA_LONG))
                    throw new Exception("Failed to parse change_state/LONG");
                if (obj != null) obj.change_state = tok.GetUInt32(); // change_state


                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("delayTimer", BinaryFieldType.DATA_FLOAT))
                    throw new Exception("Failed to parse delayTimer/FLOAT");
                if (obj != null) obj.delayTimer = tok.GetSingle(); // delayTimer

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("turretAligned", BinaryFieldType.DATA_BOOL))
                    throw new Exception("Failed to parse turretAligned/BOOL");
                tok.ApplyBoolean(obj, x => x.turretAligned); // turretAligned

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("prevYaw", BinaryFieldType.DATA_FLOAT))
                    throw new Exception("Failed to parse prevYaw/FLOAT");
                if (obj != null) obj.prevYaw = tok.GetSingle(); // prevYaw

                throw new NotImplementedException("Turret Control loading loop needed here");
            }
            else
            {
                bool m_Use13Aim = false; // we're assuming Use13Aim is impossible before 1109

                reader.Bookmark.Mark();
                // Use13Aim might be true if we're >= 1109, so be prepared to walk back and try again
                // if it is Use13Aim, we expect a bool first, if it's not we expect a float

                tok = reader.ReadToken();
                if (tok.Validate("turretAligned", BinaryFieldType.DATA_BOOL))
                {
                    reader.Bookmark.RevertToBookmark();

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
                    reader.Bookmark.RevertToBookmark();

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("omegaTurret", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse omegaTurret/FLOAT");
                    if (obj != null) obj.omegaTurret = tok.GetSingle(); // omegaTurret

                    // obsolete
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("alphaTurret", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse alphaTurret/FLOAT");
                    if (obj != null) obj.alphaTurret = tok.GetSingle(); // alphaTurret

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("timeDeploy", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse timeDeploy/FLOAT");
                    if (obj != null) obj.timeDeploy = tok.GetSingle(); // timeDeploy

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("timeUndeploy", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse timeUndeploy/FLOAT");
                    if (obj != null) obj.timeUndeploy = tok.GetSingle(); // timeUndeploy

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("state", BinaryFieldType.DATA_VOID))
                        throw new Exception("Failed to parse state/VOID");
                    if (obj != null) obj.state = (VEHICLE_STATE)tok.GetUInt32HR(); // state

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("delayTimer", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse delayTimer/FLOAT");
                    if (obj != null) obj.delayTimer = tok.GetSingle(); // delayTimer

                    if (reader.Version == 1100)
                    {
                        // obsolete
                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("wantTurret", BinaryFieldType.DATA_BOOL))
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
                        if (tok == null || !tok.Validate("turretAligned", BinaryFieldType.DATA_BOOL))
                            throw new Exception("Failed to parse turretAligned/BOOL");
                        tok.ApplyBoolean(obj, x => x.turretAligned); // turretAligned
                    }

                    if (parent.SaveType != SaveType.BZN && reader.Version >= 1140)
                    {
                        if (!m_Use13Aim)
                        {
                            tok = reader.ReadToken();
                            if (tok == null || !tok.Validate("prevYaw", BinaryFieldType.DATA_FLOAT))
                                throw new Exception("Failed to parse prevYaw/FLOAT");
                            if (obj != null) obj.prevYaw = tok.GetSingle(); // prevYaw

                            tok = reader.ReadToken();
                            if (tok == null || !tok.Validate("change_state", BinaryFieldType.DATA_LONG))
                                throw new Exception("Failed to parse change_state/LONG");
                            if (obj != null) obj.change_state = tok.GetUInt32(); // change_state
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
                    if (tok == null || !tok.Validate("turretAligned", BinaryFieldType.DATA_BOOL))
                        throw new Exception("Failed to parse turretAligned/BOOL");
                    tok.ApplyBoolean(obj, x => x.turretAligned); // turretAligned
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

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassTurretTank2 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            if (parent.SaveType == SaveType.LOCKSTEP || parent.SaveType == SaveType.JOIN)
            {
                writer.WriteFloats("omegaTurret", preserveMalformations ? obj.Malformations : null, obj.omegaTurret);
                writer.WriteFloats("timeDeploy", preserveMalformations ? obj.Malformations : null, obj.timeDeploy);
                writer.WriteFloats("timeUndeploy", preserveMalformations ? obj.Malformations : null, obj.timeUndeploy);
                writer.WriteUnsignedValues("change_state", obj.change_state);
                writer.WriteFloats("delayTimer", preserveMalformations ? obj.Malformations : null, obj.delayTimer);
                writer.WriteBoolean("turretAligned", obj, x => x.turretAligned);
                writer.WriteFloats("prevYaw", preserveMalformations ? obj.Malformations : null, obj.prevYaw);

                throw new NotImplementedException("Turret Control loading loop needed here");
            }
            else
            {
                if (obj.m_Use13Aim)
                {
                    if (writer.Version < 1109)
                    {
                        // we read a turretAligned but we're too old a version for that to be a thing
                        throw new Exception("Use13Aim turret save data found in BZN Version < 1109, impossible, parse error expected");
                    }
                }
                else
                {
                    writer.WriteFloats("omegaTurret", preserveMalformations ? obj.Malformations : null, obj.omegaTurret);
                    writer.WriteFloats("alphaTurret", preserveMalformations ? obj.Malformations : null, obj.alphaTurret); // obsolete
                    writer.WriteFloats("timeDeploy", preserveMalformations ? obj.Malformations : null, obj.timeDeploy);
                    writer.WriteFloats("timeUndeploy", preserveMalformations ? obj.Malformations : null, obj.timeUndeploy);
                    writer.WriteVoidBytes("state", (UInt32)obj.state); // uses root's state instead of change_state, until later
                    writer.WriteFloats("delayTimer", preserveMalformations ? obj.Malformations : null, obj.delayTimer);

                    if (writer.Version == 1100)
                    {
//                        var mal = obj.Malformations.GetMalformations(Malformation.MISINTERPRET, "wantTurret");
//                        if (mal.Length > 0)
//                        {
//                            writer.WriteBoolean("wantTurret", obj, x => x.turretAligned);
//                        }
//                        else
                        {
                            writer.WriteBoolean("wantTurret", obj, x => x.wantTurret);
                        }
                    }
                    else
                    {
                        writer.WriteBoolean("turretAligned", obj, x => x.turretAligned);
                    }

                    if (parent.SaveType != SaveType.BZN && writer.Version >= 1140)
                    {
                        if (!obj.m_Use13Aim)
                        {
                            writer.WriteFloats("prevYaw", preserveMalformations ? obj.Malformations : null, obj.prevYaw);
                            writer.WriteUnsignedValues("change_state", obj.change_state);
                        }
                    }

                    if (writer.Version < 1109)
                    {
                        ClassHoverCraft.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
                        return;
                    }
                }

                if (obj.m_Use13Aim)
                {
                    writer.WriteBoolean("turretAligned", obj, x => x.turretAligned);
                }

                if (parent.SaveType != SaveType.BZN)
                {
                    if (obj.m_Use13Aim)
                    {
                        throw new NotImplementedException("Turret Control loading loop needed here");
                    }
                }
            }

            ClassDeployable.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
