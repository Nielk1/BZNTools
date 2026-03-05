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
                obj = new ClassTurretTank1(preamble, classLabel);
            ClassTurretTank1.Hydrate(parent, reader, obj as ClassTurretTank1);
            return true;
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
        public ClassTurretTank1(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassTurretTank1? obj)
        {
            IBZNToken tok;

            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                if (reader.Format == BZNFormat.BattlezoneN64 || reader.Version > 1000)
                {
                    if (reader.Format == BZNFormat.BattlezoneN64 || reader.Version != 1042)
                    {
                        // obsolete

                        tok = reader.ReadToken();
                        if (!tok.Validate("undeffloat", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse undeffloat/FLOAT");
                        if (obj != null) obj.omegaTurret = tok.GetSingle(); // omegaTurret

                        tok = reader.ReadToken();
                        if (!tok.Validate("undeffloat", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse undeffloat/FLOAT");
                        if (obj != null) obj.alphaTurret = tok.GetSingle(); // alphaTurret

                        tok = reader.ReadToken();
                        if (!tok.Validate("undeffloat", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse undeffloat/FLOAT");
                        if (obj != null) obj.timeDeploy = tok.GetSingle(); // timeDeploy

                        tok = reader.ReadToken();
                        if (!tok.Validate("undeffloat", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse undeffloat/FLOAT");
                        if (obj != null) obj.timeUndeploy = tok.GetSingle(); // timeUndeploy
                    }

                    tok = reader.ReadToken();
                    if (!tok.Validate("undefraw", BinaryFieldType.DATA_VOID)) throw new Exception("Failed to parse undefraw/VOID");
                    if (obj != null) obj.state = (VEHICLE_STATE)tok.GetUInt32(); // state

                    tok = reader.ReadToken();
                    if (!tok.Validate("undeffloat", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse undeffloat/FLOAT");
                    if (obj != null) obj.delayTimer = tok.GetSingle(); // delayTimer

                    if (reader.Format == BZNFormat.BattlezoneN64 || reader.Version != 1042)
                    {
                        // obsolete

                        tok = reader.ReadToken();
                        if (!tok.Validate("undefbool", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse undefbool/BOOL");
                        if (obj != null) obj.wantTurret = tok.GetBoolean(); // wantTurret
                    }
                }
            }

            ClassHoverCraft.Hydrate(parent, reader, obj as ClassHoverCraft);
        }
    }
}
