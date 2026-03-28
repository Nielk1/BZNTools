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
                // this code path could serve to be better integrated into the below code

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("omegaTurret", BinaryFieldType.DATA_FLOAT))
                    throw new Exception("Failed to parse omegaTurret/FLOAT");
                tok.ApplySingle(obj, x => x.omegaTurret);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("timeDeploy", BinaryFieldType.DATA_FLOAT))
                    throw new Exception("Failed to parse timeDeploy/FLOAT");
                tok.ApplySingle(obj, x => x.timeDeploy);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("timeUndeploy", BinaryFieldType.DATA_FLOAT))
                    throw new Exception("Failed to parse timeUndeploy/FLOAT");
                tok.ApplySingle(obj, x => x.timeUndeploy);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("change_state", BinaryFieldType.DATA_LONG))
                    throw new Exception("Failed to parse change_state/LONG");
                tok.ApplyUInt32(obj, x => x.change_state);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("delayTimer", BinaryFieldType.DATA_FLOAT))
                    throw new Exception("Failed to parse delayTimer/FLOAT");
                tok.ApplySingle(obj, x => x.delayTimer);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("turretAligned", BinaryFieldType.DATA_BOOL))
                    throw new Exception("Failed to parse turretAligned/BOOL");
                tok.ApplyBoolean(obj, x => x.turretAligned);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("prevYaw", BinaryFieldType.DATA_FLOAT))
                    throw new Exception("Failed to parse prevYaw/FLOAT");
                tok.ApplySingle(obj, x => x.prevYaw);

                throw new NotImplementedException("Turret Control loading loop needed here");
            }
            else
            {
                bool m_Use13Aim = false; // we're assuming Use13Aim is impossible before 1109

                reader.Bookmark.Mark();
                // Use13Aim might be true if we're >= 1109, so be prepared to walk back and try again
                // if it is Use13Aim, we expect a bool first, if it's not we expect a float
                try
                {
                    tok = reader.ReadToken();
                    if (tok != null && tok.Validate("turretAligned", BinaryFieldType.DATA_BOOL))
                    {
                        // we see the a data field that only occurs when Use13Aim is true, so let's assume that's the case, set it true, and roll back the reader

                        if (reader.Version < 1109)
                        {
                            // we read a turretAligned but we're too old a version for that to be a thing
                            throw new Exception("Use13Aim turret save data found in BZN Version < 1109, impossible, parse error expected");
                        }

                        m_Use13Aim = true;
                    }
                }
                finally
                {
                    reader.Bookmark.RevertToBookmark();
                }

                if (obj != null) obj.m_Use13Aim = m_Use13Aim;

                if (!m_Use13Aim)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("omegaTurret", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse omegaTurret/FLOAT");
                    tok.ApplySingle(obj, x => x.omegaTurret);

                    // obsolete
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("alphaTurret", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse alphaTurret/FLOAT");
                    tok.ApplySingle(obj, x => x.alphaTurret);

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("timeDeploy", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse timeDeploy/FLOAT");
                    tok.ApplySingle(obj, x => x.timeDeploy);

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("timeUndeploy", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse timeUndeploy/FLOAT");
                    tok.ApplySingle(obj, x => x.timeUndeploy);

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("state", BinaryFieldType.DATA_VOID))
                        throw new Exception("Failed to parse state/VOID");
                    tok.ApplyVoidBytes(obj, x => x.state, 0, (v) => (VEHICLE_STATE)BitConverter.ToUInt32(v));

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("delayTimer", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse delayTimer/FLOAT");
                    tok.ApplySingle(obj, x => x.delayTimer);
                }

                if (m_Use13Aim)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("turretAligned", BinaryFieldType.DATA_BOOL))
                        throw new Exception("Failed to parse turretAligned/BOOL");
                    tok.ApplyBoolean(obj, x => x.turretAligned);
                }
                else
                {
                    if (reader.Version == 1100)
                    {
                        // obsolete
                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("wantTurret", BinaryFieldType.DATA_BOOL))
                            throw new Exception("Failed to parse wantTurret/BOOL");
                        //if (obj != null)
                        //{
                        //    obj.Malformations.AddMisinterpretation("wantTurret", "turretAligned");
                        //    obj.wantTurret = tok.GetBoolean(); // wantTurret
                        //}

                        tok.ApplyBoolean(obj, x => x.wantTurret);
                        // TODO restore "Misinterpretation" to Malformation engine
                    }
                    else
                    {
                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("turretAligned", BinaryFieldType.DATA_BOOL))
                            throw new Exception("Failed to parse turretAligned/BOOL");
                        tok.ApplyBoolean(obj, x => x.turretAligned);
                    }
                }

                if (!m_Use13Aim)
                {
                    if (parent.SaveType != SaveType.BZN && reader.Version >= 1140)
                    {
                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("prevYaw", BinaryFieldType.DATA_FLOAT))
                            throw new Exception("Failed to parse prevYaw/FLOAT");
                        tok.ApplySingle(obj, x => x.prevYaw);

                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("change_state", BinaryFieldType.DATA_LONG))
                            throw new Exception("Failed to parse change_state/LONG");
                        tok.ApplyUInt32(obj, x => x.change_state);
                    }
                }

                if (reader.Version < 1109)
                {
                    ClassHoverCraft.Hydrate(parent, reader, obj as ClassHoverCraft);
                    return;
                }

                if (parent.SaveType != SaveType.BZN)
                {
                    if (m_Use13Aim)
                    {
                        throw new NotImplementedException("Turret Control loading loop needed here");
                    }
                }
            }

            ClassDeployable.Hydrate(parent, reader, obj as ClassDeployable);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassTurretTank2 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (parent.SaveType == SaveType.LOCKSTEP || parent.SaveType == SaveType.JOIN)
            {
                writer.WriteSingle("omegaTurret", obj, x => x.omegaTurret);
                writer.WriteSingle("timeDeploy", obj, x => x.timeDeploy);
                writer.WriteSingle("timeUndeploy", obj, x => x.timeUndeploy);
                writer.WriteUInt32("change_state", obj, x => x.change_state);
                writer.WriteSingle("delayTimer", obj, x => x.delayTimer);
                writer.WriteBoolean("turretAligned", obj, x => x.turretAligned);
                writer.WriteSingle("prevYaw", obj, x => x.prevYaw);

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

                if (!obj.m_Use13Aim)
                {
                    writer.WriteSingle("omegaTurret", obj, x => x.omegaTurret);
                    writer.WriteSingle("alphaTurret", obj, x => x.alphaTurret); // obsolete
                    writer.WriteSingle("timeDeploy", obj, x => x.timeDeploy);
                    writer.WriteSingle("timeUndeploy", obj, x => x.timeUndeploy);
                    writer.WriteVoidBytes("state", obj, x => x.state, (v) => BitConverter.GetBytes((UInt32)v));
                    writer.WriteSingle("delayTimer", obj, x => x.delayTimer);
                }

                if (obj.m_Use13Aim)
                {
                    writer.WriteBoolean("turretAligned", obj, x => x.turretAligned);
                }
                else
                {
                    if (writer.Version == 1100)
                    {
                        //var mal = obj.Malformations.GetMalformations(Malformation.MISINTERPRET, "wantTurret");
                        //if (mal.Length > 0)
                        //{
                        //    writer.WriteBoolean("wantTurret", obj, x => x.turretAligned);
                        //}
                        //else
                        {
                            writer.WriteBoolean("wantTurret", obj, x => x.wantTurret);
                            // TODO restore "Misinterpretation" to Malformation engine
                        }
                    }
                    else
                    {
                        writer.WriteBoolean("turretAligned", obj, x => x.turretAligned);
                    }
                }

                if (obj.m_Use13Aim)
                {
                    if (parent.SaveType != SaveType.BZN && writer.Version >= 1140)
                    {
                        if (!obj.m_Use13Aim)
                        {
                            writer.WriteSingle("prevYaw", obj, x => x.prevYaw);
                            writer.WriteUInt32("change_state", obj, x => x.change_state);
                        }
                    }
                }

                if (writer.Version < 1109)
                {
                    ClassHoverCraft.Dehydrate(obj, parent, writer, binary, save);
                    return;
                }

                if (parent.SaveType != SaveType.BZN)
                {
                    if (obj.m_Use13Aim)
                    {
                        throw new NotImplementedException("Turret Control loading loop needed here");
                    }
                }
            }

            ClassDeployable.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
