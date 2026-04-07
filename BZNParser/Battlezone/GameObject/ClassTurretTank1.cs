using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "turrettank")]
    [ObjectClass(BZNFormat.BattlezoneN64, "turrettank")]
    public class ClassTurretTank1Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassTurretTank1(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                ClassTurretTank1.Hydrate(parent, reader, obj as ClassTurretTank1);
                return true;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassTurretTank1 : ClassHoverCraft
    {
        protected float omegaTurret { get; set; } // obsolete
        protected float alphaTurret { get; set; } // obsolete
        protected float timeDeploy { get; set; } // obsolete
        protected float timeUndeploy { get; set; } // obsolete
        protected float delayTimer { get; set; }
        protected bool wantTurret { get; set; } // obsolete
        public ClassTurretTank1(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
            omegaTurret = 0;
            alphaTurret = 0;
            timeDeploy = 0;
            timeUndeploy = 0;
            delayTimer = 0;
            wantTurret = false;
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


        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassTurretTank1? obj)
        {
            IBZNToken? tok;

            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                if (reader.Format == BZNFormat.BattlezoneN64 || reader.Version > 1000)
                {
                    if (reader.Format == BZNFormat.BattlezoneN64 || reader.Version != 1042)
                    {
                        // obsolete

                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("undeffloat", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse undeffloat/FLOAT");
                        tok.ApplySingle(obj, x => x.omegaTurret);

                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("undeffloat", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse undeffloat/FLOAT");
                        tok.ApplySingle(obj, x => x.alphaTurret);

                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("undeffloat", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse undeffloat/FLOAT");
                        tok.ApplySingle(obj, x => x.timeDeploy);

                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("undeffloat", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse undeffloat/FLOAT");
                        tok.ApplySingle(obj, x => x.timeUndeploy);
                    }

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("undefraw", BinaryFieldType.DATA_VOID)) throw new Exception("Failed to parse undefraw/VOID");
                    tok.ApplyVoidBytes(obj, x => x.state, 0, (v) => (VEHICLE_STATE)BitConverter.ToUInt32(v));

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("undeffloat", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse undeffloat/FLOAT");
                    tok.ApplySingle(obj, x => x.delayTimer);

                    if (reader.Format == BZNFormat.BattlezoneN64 || reader.Version != 1042)
                    {
                        // obsolete

                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("undefbool", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse undefbool/BOOL");
                        tok.ApplyBoolean(obj, x => x.wantTurret); // wantTurret
                    }
                }
            }

            ClassHoverCraft.Hydrate(parent, reader, obj as ClassHoverCraft);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassTurretTank1 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (writer.Format == BZNFormat.Battlezone || writer.Format == BZNFormat.BattlezoneN64)
            {
                if (writer.Format == BZNFormat.BattlezoneN64 || writer.Version > 1000)
                {
                    if (writer.Format == BZNFormat.BattlezoneN64 || writer.Version != 1042)
                    {
                        // obsolete
                        writer.WriteSingle("undeffloat", obj, x => x.omegaTurret); // omegaTurret
                        writer.WriteSingle("undeffloat", obj, x => x.alphaTurret); // alphaTurret
                        writer.WriteSingle("undeffloat", obj, x => x.timeDeploy); // timeDeploy
                        writer.WriteSingle("undeffloat", obj, x => x.timeUndeploy); // timeUndeploy
                    }
                    writer.WriteVoidBytes("undefraw", obj, x => x.state, (v) => BitConverter.GetBytes((UInt32)v));
                    writer.WriteSingle("undeffloat", obj, x => x.delayTimer); // delayTimer
                    if (writer.Format == BZNFormat.BattlezoneN64 || writer.Version != 1042)
                    {
                        // obsolete
                        writer.WriteBoolean("undefbool", obj, x => x.wantTurret); // wantTurret
                    }
                }
            }
            ClassHoverCraft.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
