using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using System.Xml.Linq;

namespace BZNParser.Battlezone.GameObject
{
    public class ClassGameObject : Entity
    {
        public float illumination { get; set; }
        //public Vector3D pos { get; set; }
        public Euler euler { get; set; }
        //public UInt32 seqNo { get; set; }
        public SizedString name { get; set; }
        public byte? saveFlags { get; set; }
        public bool isObjective { get; set; }
        public bool isSelected { get; set; }

        public bool defaultShot { get; set; }
        public bool defaultHealth { get; set; }
        //public bool curCmdClear { get; set; }
        //public bool nextCmdClear { get; set; }


        public UInt32 isVisible { get; set; }
        public UInt16 isDamped { get; set; }
        public UInt32 EffectsMask { get; set; }
        public UInt32 seen { get; set; }
        public Int32 groupNumber { get; set; }
        public bool isCritical { get; set; }
        public float healthRatio { get; set; }
        public DualModeValue<Int32, float> curHealth { get; set; }
        public DualModeValue<Int32, float> maxHealth { get; set; }
        public float addHealth { get; set; }
        public float ammoRatio { get; set; }
        public DualModeValue<Int32, float> curAmmo { get; set; }
        public DualModeValue<Int32, float> maxAmmo { get; set; }
        public DualModeValue<Int32, float> addAmmo { get; set; }
        public UInt32 undefaicmd { get; set; }
        public UInt32 priority { get; set; }
        public UInt32 what { get; set; }
        //public UInt32 who { get; set; }
        public Int32 who { get; set; }
        public UInt32 where { get; set; }
        public UInt32 param { get; set; }
        public bool aiProcess { get; set; }
        public AiCmdInfo? curCmd { get; set; }
        public AiCmdInfo? nextCmd { get; set; }
        public bool isCargo { get; set; }
        public UInt32 independence { get; set; }
        public SizedString curPilot { get; set; }
        public Int32 perceivedTeam { get; set; }


        // legacy data, or maybe from saves?
        public float playerShot { get; set; }
        public float playerCollide { get; set; }
        public float friendShot { get; set; }
        public float friendCollide { get; set; }
        public float enemyShot { get; set; }
        public float groundCollide { get; set; }



        //legacy
        public UInt32 liveColor { get; set; }
        public UInt32 deadColor { get; set; }
        public UInt32 teamNumber { get; set; }
        public UInt32 teamSlot { get; set; }


        // legacy
        public UInt32 aiProcessPtr { get; set; }


        // legacy
        public float heatRatio { get; set; }
        public Int32 curHeat { get; set; }
        public Int32 maxHeat { get; set; }


        public ClassGameObject(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
            illumination = 0;
            //pos = new Vector3D();
            euler = new Euler();
            //seqNo = 0;
            name = new SizedString();
            saveFlags = null;
            isObjective = false;
            isSelected = false;
            defaultShot = false;
            defaultHealth = false;
            //curCmdClear = false;
            //nextCmdClear = false;
            isVisible = 0;
            isDamped = 0;
            EffectsMask = 0;
            seen = 0;
            groupNumber = 0;
            isCritical = false;
            healthRatio = 0;
            curHealth = new DualModeValue<Int32, float>(0);
            maxHealth = new DualModeValue<Int32, float>(0);
            addHealth = 0;
            ammoRatio = 0;
            curAmmo = new DualModeValue<Int32, float>(0);
            maxAmmo = new DualModeValue<Int32, float>(0);
            addAmmo = new DualModeValue<int, float>(0);
            undefaicmd = 0;
            priority = 0;
            what = 0;
            who = -1; // unsigned -1 for easier hex display of uninitialized value
            where = 0;
            param = 0;
            aiProcess = false;
            curCmd = null;
            nextCmd = null;
            isCargo = false;
            independence = 0;
            curPilot = new SizedString();
            perceivedTeam = -1; // default to -1 for easier hex display of uninitialized value
            
            playerShot = -1e30f; // -HUGE_NUMBER
            playerCollide = -1e30f; // -HUGE_NUMBER
            friendShot = -1e30f; // -HUGE_NUMBER
            friendCollide = -1e30f; // -HUGE_NUMBER
            enemyShot = -1e30f; // -HUGE_NUMBER
            groundCollide = -1e30f; // -HUGE_NUMBER
        }

        public override void ClearMalformations()
        {
            Malformations.Clear();
            saveFlags = null;
            euler.ClearMalformations();
            name.ClearMalformations();
            curPilot.ClearMalformations();
            base.ClearMalformations();
        }
        public override void DisableMalformationAutoFix()
        {
            euler.DisableMalformationAutoFix();
            name.DisableMalformationAutoFix();
            curPilot.DisableMalformationAutoFix();
            base.DisableMalformationAutoFix();
        }
        public override void EnableMalformationAutoFix()
        {
            euler.EnableMalformationAutoFix();
            name.EnableMalformationAutoFix();
            curPilot.EnableMalformationAutoFix();
            base.EnableMalformationAutoFix();
        }

        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassGameObject? obj)
        {
            IBZNToken? tok;

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("illumination", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse illumination/FLOAT");
            tok.ApplySingle(obj, x => x.illumination);

            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("pos", BinaryFieldType.DATA_VEC3D)) throw new Exception("Failed to parse pos/VEC3D");
                tok.ApplyVector3D(obj, x => x.pos2);
            }

            reader.ReadEuler("euler", obj, x => x.euler);

            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("seqNo", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse seqNo/LONG");
                tok.ApplyUInt32(obj, x => x.seqNo);
            }
            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version < 1145)
                {
                    // 1123 1124 1128 1141 1142
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("seqNo", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse seqNo/LONG");
                    tok.ApplyUInt32h(obj, x => x.seqNo);
                }
            }

            if (reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("name", BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse name/CHAR");
                tok.ApplyChars(obj, x => x.name);
            }
            if (reader.Format == BZNFormat.Battlezone)
            {
                // not present on 1030, presnet on 1032?
                if (reader.Version > 1030)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("name", BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse name/CHAR");
                    tok.ApplyChars(obj, x => x.name);
                }
            }
            if (reader.Format == BZNFormat.Battlezone2)
            {
                reader.ReadSizedString("name", obj, x => x.name);
            }

            byte? saveFlags = 0;
            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version >= 1145)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("saveFlags", BinaryFieldType.DATA_CHAR))
                        throw new Exception("Failed to parse saveFlags/CHAR");
                    if (reader.Version >= 1187)
                    {
                        //1187A and up tested
                        (saveFlags, _) = tok.ApplyUInt8(obj, x => x.saveFlags);
                    }
                    else
                    {
                        //1183A and under tested
                        (saveFlags, _) = tok.ApplyVoidBytesRaw(obj, x => x.saveFlags, 0, (v) => v[0]);
                    }
                    if (obj != null)
                    {
                        if ((saveFlags & 0x01) != 0) obj.isObjective = true;
                        if ((saveFlags & 0x02) != 0) obj.isSelected = true;
                        if ((saveFlags & 0x04) != 0) obj.defaultShot = true;
                        if ((saveFlags & 0x08) != 0) obj.defaultHealth = true;
                        if ((saveFlags & 0x10) != 0) obj.isCargo = true;
                        //if ((saveFlags & 0x20) != 0) obj.curCmdClear = true;
                        //if ((saveFlags & 0x40) != 0) obj.nextCmdClear = true;
                        //if ((saveFlags & 0x80) != 0) obj.Unused = true;
                    }
                }

                if (reader.Version < 1145)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("isObjective", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse isObjective/BOOL");
                    tok.ApplyBoolean(obj, x => x.isObjective);
                }
                else
                {
                    //isObjective = saveFlags & 0x01 != 0;
                }

                if (reader.Version < 1145)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("isSelected", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse isSelected/BOOL");
                    tok.ApplyBoolean(obj, x => x.isSelected);
                }
                else
                {
                    //selected = saveFlags & 0x02 != 0;
                }

                if (reader.Version < 1145)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("isVisible", BinaryFieldType.DATA_LONG))
                        throw new Exception("Failed to parse isVisible/LONG");
                    tok.ApplyUInt32h(obj, x => x.isVisible);
                }
                else
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("isVisible", BinaryFieldType.DATA_SHORT)) throw new Exception("Failed to parse isVisible/SHORT");
                    tok.ApplyUInt16(obj, x => x.isVisible);
                }

                if (reader.Version >= 1197)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("isDamped", BinaryFieldType.DATA_SHORT)) throw new Exception("Failed to parse isDamped/SHORT");
                    tok.ApplyUInt16(obj, x => x.isDamped);
                }
                else
                {
                    // does not exist before version 1197, as it used to be a single bit flag that might no even have been saved
                    // not sure this is actually in the save, seems like saveFlags is only 1 byte so the damping is a runtime level there
                    // looks like a far larger flags int is used in the savegame, but in the bzn we just get saveflags which is only a byte
                }

                // savetype != 0 stuff

                if (reader.Version >= 1151)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("EffectsMask", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse EffectsMask/LONG");
                    tok.ApplyUInt32(obj, x => x.EffectsMask);
                }

                if (reader.Version == 1041 || reader.Version == 1047 || reader.Version == 1070)
                {
                    // bz2001.bzn // 1041
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("seen", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse seen/LONG");
                    tok.ApplyUInt32h(obj, x => x.seen);
                }
                else if (reader.Version < 1145)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("isSeen", BinaryFieldType.DATA_LONG))
                        throw new Exception("Failed to parse isSeen/LONG");
                    tok.ApplyUInt32h(obj, x => x.seen);
                }
                else
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("isSeen", BinaryFieldType.DATA_SHORT)) throw new Exception("Failed to parse isSeen/SHORT");
                    if (reader.Version >= 1165)
                    {
                        tok.ApplyUInt16(obj, x => x.seen);
                    }
                    else
                    {
                        tok.ApplyUInt16h(obj, x => x.seen);
                    }
                }
            }

            //if (reader.Format == BZNFormat.Battlezone && reader.Version >= 2011)
            //if (reader.Format == BZNFormat.Battlezone && reader.Version >= 1049)
            if (reader.Format == BZNFormat.Battlezone)// || reader.Format == BZNFormat.BattlezoneN64)
            {
                // not sure if this is on the n64 build
                if ((reader.Version >= 1046 && reader.Version < 2000) || reader.Version >= 2010)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("isCritical", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse isCritical/BOOL");
                    tok.ApplyBoolean(obj, x => x.isCritical);
                }
            }

            if (reader.Format == BZNFormat.Battlezone && (reader.Version == 1001 || reader.Version == 1011 || reader.Version == 1012 || reader.Version == 1017)) // TODO get range for these
            {
                tok = reader.ReadToken(); // 250
                if (tok == null || !tok.Validate("liveColor", BinaryFieldType.DATA_UNKNOWN)) throw new Exception("Failed to parse liveColor/UNKNOWN");
                tok.ApplyUInt32(obj, x => x.liveColor);

                tok = reader.ReadToken(); // 251
                if (tok == null || !tok.Validate("deadColor", BinaryFieldType.DATA_UNKNOWN)) throw new Exception("Failed to parse deadColor/UNKNOWN");
                tok.ApplyUInt32(obj, x => x.deadColor);

                tok = reader.ReadToken(); // 1
                if (tok == null || !tok.Validate("teamNumber", BinaryFieldType.DATA_UNKNOWN)) throw new Exception("Failed to parse teamNumber/UNKNOWN");
                tok.ApplyUInt32(obj, x => x.teamNumber);

                tok = reader.ReadToken(); // 0
                if (tok == null || !tok.Validate("teamSlot", BinaryFieldType.DATA_UNKNOWN)) throw new Exception("Failed to parse teamSlot/UNKNOWN");
                tok.ApplyUInt32(obj, x => x.teamSlot);
            }

            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("isObjective", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse isObjective/BOOL");
                tok.ApplyBoolean(obj, x => x.isObjective);

                // I seriously don't understand why this is a thing, it must be wrong, but this is where we get into BZ98R or 1.5 (unclear)
                // code says it should always be read in???
                //if (/*reader.Version != 2004 &&*/ reader.Version != 2003)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("isSelected", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse isSelected/BOOL");
                    tok.ApplyBoolean(obj, x => x.isSelected);
                }

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("isVisible", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse isVisible/LONG");
                if (obj != null) obj.isVisible = tok.GetUInt32H();

                //if (reader.Format == BZNFormat.Battlezone)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("seen", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse seen/LONG");
                    tok.ApplyUInt32h(obj, x => x.seen);
                }
            }

            if (reader.Format == BZNFormat.Battlezone2 && reader.Version != 1041 && reader.Version != 1047) // avoid bz2001.bzn via != 1041
            {
                tok = reader.ReadToken();
                if (reader.Version <= 1128)
                {
                    if (tok == null || !tok.Validate("groupNumber", BinaryFieldType.DATA_LONG))
                        throw new Exception("Failed to parse groupNumber/?");
                }
                else
                {
                    if (tok == null || !tok.Validate("groupNumber", BinaryFieldType.DATA_CHAR))
                        throw new Exception("Failed to parse groupNumber/?");
                }
                tok.ApplyInt8(obj, x => x.groupNumber);
            }

            //if (parent.SaveType == 0)
            //{
            //    isVisible = 0UL;
            //    isSeen = 0UL;
            //    isPinged = 0UL;
            //    isObjective = 0UL;
            //}

            // bunch of SaveType != 0 stuff

            // BZ2 allyNumber
            // BZ2 if !IsDefaultShotData then 
            //     (a2->vftable->field_38)(a2, &this->field_52C, 4, "playerShot", v13);
            //     (a2->vftable->field_38)(a2, this->gap530, 4, "playerCollide");
            //     (a2->vftable->field_38)(a2, &this->gap530[4], 4, "friendShot");
            //     (a2->vftable->field_38)(a2, &this->gap530[8], 4, "friendCollide");
            //     (a2->vftable->field_38)(a2, &this->field_53C, 4, "enemyShot");
            //     (a2->vftable->field_38)(a2, &this->gap540[4], 4, "groundCollide");
            //     (a2->vftable->read_long)(a2, this->gap550, 4, "who_shot_me");
            //     v13 = "team_who_shot_me";

            if (reader.Format == BZNFormat.Battlezone)
            {
                //[10:03:38 PM] Kenneth Miller: I think I may have figured out what that stuff is, maybe
                //[10:03:50 PM] Kenneth Miller: They're timestamps
                //[10:04:04 PM] Kenneth Miller: playerShot, playerCollide, friendShot, friendCollide, enemyShot, groundCollide
                //[10:04:13 PM] Kenneth Miller: the default value is -HUGE_NUMBER (-1e30)
                //[10:04:26 PM] Kenneth Miller: And due to the nature of the game, groundCollide is the most likely to get set first
                //[10:05:02 PM] Kenneth Miller: Old versions of the mission format used to contain those values but later versions only include them in the savegame
                //[10:05:05 PM] Kenneth Miller: (not the mission)
                //[10:05:31 PM] Kenneth Miller: (version 1033 was where they were removed from the mission)
                if (reader.Version < 1033)
                {
                    tok = reader.ReadToken(); // float (-HUGE_NUMBER) // playerShot
                    if (tok == null || !tok.Validate("playerShot", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse playerShot/FLOAT");
                    tok.ApplySingle(obj, x => x.playerShot);

                    tok = reader.ReadToken(); // float (-HUGE_NUMBER) // playerCollide
                    if (tok == null || !tok.Validate("playerCollide", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse playerCollide/FLOAT");
                    tok.ApplySingle(obj, x => x.playerCollide);

                    tok = reader.ReadToken(); // float (-HUGE_NUMBER) // friendShot
                    if (tok == null || !tok.Validate("friendShot", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse friendShot/FLOAT");
                    tok.ApplySingle(obj, x => x.friendShot);

                    tok = reader.ReadToken(); // float (-HUGE_NUMBER) // friendCollide
                    if (tok == null || !tok.Validate("friendCollide", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse friendCollide/FLOAT");
                    tok.ApplySingle(obj, x => x.friendCollide);

                    tok = reader.ReadToken(); // float (-HUGE_NUMBER) // enemyShot
                    if (tok == null || !tok.Validate("enemyShot", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse enemyShot/FLOAT");
                    tok.ApplySingle(obj, x => x.enemyShot);

                    tok = reader.ReadToken(); // float                // groundCollide
                    if (tok == null || !tok.Validate("groundCollide", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse groundCollide/FLOAT");
                    tok.ApplySingle(obj, x => x.groundCollide);
                }
            }
            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("healthRatio", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse healthRatio/FLOAT");
                tok.ApplySingle(obj, x => x.healthRatio);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("curHealth", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse curHealth/LONG");
                tok.ApplyInt32(obj, x => x.curHealth);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("maxHealth", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse maxHealth/LONG");
                tok.ApplyInt32(obj, x => x.maxHealth);
            }
            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version < 1143)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("healthRatio", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse healthRatio/FLOAT");
                    tok.ApplySingle(obj, x => x.healthRatio);
                }

                bool defaultHealth = reader.Version >= 1145 && ((saveFlags & 0x08) != 0);

                if (defaultHealth)
                {
                    // set MaxHealth, CurHealth, and AddHealth from ODF
                }
                else
                {
                    // code path that needs more research

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("curHealth", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse curHealth/FLOAT");
                    tok.ApplySingle(obj, x => x.curHealth);

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("maxHealth", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse maxHealth/FLOAT");
                    tok.ApplySingle(obj, x => x.maxHealth);

                    if (reader.Version != 1041 && reader.Version != 1047)
                    {
                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("addHealth", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse addHealth/FLOAT");
                        tok.ApplySingle(obj, x => x.addHealth);
                    }
                }
            }

            if (reader.Format == BZNFormat.Battlezone)
            {
                if (reader.Version < 1015)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("heatRatio", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse heatRatio/FLOAT");
                    tok.ApplySingle(obj, x => x.heatRatio);

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("curHeat", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse curHeat/LONG");
                    tok.ApplyInt32(obj, x => x.curHeat);

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("maxHeat", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse maxHeat/LONG");
                    tok.ApplyInt32(obj, x => x.maxHeat);
                }
            }

            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("ammoRatio", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse ammoRatio/FLOAT");
                //if (obj != null) obj.ammoRatio = tok.GetSingle();
                tok.ApplySingle(obj, x => x.ammoRatio);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("curAmmo", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse curAmmo/LONG");
                //if (obj != null) obj.curAmmo = new DualModeValue<int, float>(tok.GetInt32());
                tok.ApplyInt32(obj, x => x.curAmmo);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("maxAmmo", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse maxAmmo/LONG");
                //if (obj != null) obj.maxAmmo = new DualModeValue<int, float>(tok.GetInt32());
                tok.ApplyInt32(obj, x => x.maxAmmo);
            }
            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version < 1143)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("ammoRatio", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse ammoRatio/FLOAT");
                    //if (obj != null) obj.ammoRatio = tok.GetSingle();
                    tok.ApplySingle(obj, x => x.ammoRatio);

                    if (reader.Version >= 1070)
                    {
                        // these probably should be floats not longs
                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("curAmmo", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse curAmmo/FLOAT");
                        //if (obj != null) obj.curAmmo = new DualModeValue<int, float>(tok.GetSingle());
                        tok.ApplySingle(obj, x => x.curAmmo);

                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("maxAmmo", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse maxAmmo/FLOAT");
                        //if (obj != null) obj.maxAmmo = new DualModeValue<int, float>(tok.GetSingle());
                        tok.ApplySingle(obj, x => x.maxAmmo);
                    }
                    else
                    {
                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("curAmmo", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse curAmmo/LONG");
                        //if (obj != null) obj.curAmmo = new DualModeValue<int, float>(tok.GetInt32());
                        tok.ApplyInt32(obj, x => x.curAmmo);

                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("maxAmmo", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse maxAmmo/LONG");
                        //if (obj != null) obj.maxAmmo = new DualModeValue<int, float>(tok.GetInt32());
                        tok.ApplyInt32(obj, x => x.maxAmmo);
                    }

                    if (reader.Version >= 1070)
                    {
                        // probably should be a float
                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("addAmmo", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse addAmmo/FLOAT");
                        //if (obj != null) obj.addAmmo = new DualModeValue<int, float>(tok.GetSingle());
                        tok.ApplySingle(obj, x => x.addAmmo);
                    }
                    else if (reader.Version != 1041 && reader.Version != 1047) // avoid bz2001.bzn != 1041
                    {
                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("addAmmo", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse addAmmo/LONG");
                        //if (obj != null) obj.addAmmo = new DualModeValue<int, float>(tok.GetInt32());
                        tok.ApplyInt32(obj, x => x.addAmmo);
                    }
                }
            }

            // start read of AiCmdInfo
            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (parent.SaveType == SaveType.BZN)
                {
                    // technicly this is the nextCmd unless the next command is CMD_NONE, then it's the curCmd, which is a bit jank but whatever
                    {
                        AiCmdInfo info = reader.GetAiCmdInfo("undefaicmd");
                        if (obj != null)
                            obj.curCmd = info;
                    }

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("aiProcess", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse aiProcess/BOOL");
                    tok.ApplyBoolean(obj, x => x.aiProcess);
                }
                else
                {
                    bool curCmdClear = (saveFlags & 0x20) != 0;
                    bool nextCmdClear = (saveFlags & 0x40) != 0;

                    // read curCmd unless saveflags says no CurCmd
                    if (!curCmdClear)
                    {
                        AiCmdInfo info = reader.GetAiCmdInfo("undefaicmd");
                        if (obj != null)
                            obj.curCmd = info; // TODO is this the correct storage for BZ2's
                    }
                    else
                    {
                        if (obj != null)
                            obj.curCmd = null;
                    }
                    // read nextCmd unless saveflags says no NextCmd
                    if (!nextCmdClear)
                    {
                        AiCmdInfo info = reader.GetAiCmdInfo("undefaicmd");
                        if (obj != null)
                            obj.nextCmd = info; // TODO is this the correct storage for BZ2's
                    }
                    else
                    {
                        if (obj != null)
                            obj.nextCmd = null;
                    }

                    // UNTESTED
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("undefptr", BinaryFieldType.DATA_UNKNOWN)) throw new Exception("Failed to parse undefptr/?");
                    tok.ApplyUInt32H8(obj, x => x.aiProcessPtr);
                }
            }
            else if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                if (parent.SaveType == SaveType.BZN)
                {
                    // start read of AiCmdInfo
                    if (reader.Format == BZNFormat.Battlezone && (reader.Version == 1001 || reader.Version == 1011 || reader.Version == 1012))
                    {
                        AiCmdInfo info = reader.GetAiCmdInfo();
                        if (obj != null)
                            obj.curCmd = info;
                    }

                    {
                        AiCmdInfo info = reader.GetAiCmdInfo();
                        if (obj != null)
                            obj.nextCmd = info;
                    }
                    // end read of AiCmdInfo

                    // aiProcess?
                    if (reader.Format == BZNFormat.Battlezone && reader.Version <= 1012)
                    {
                        // 1011A confirmed
                        // 1012A confirmed
                        // 1001A confirmed with 00000000
                        // might be worth making this a dual value with the bool
                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("undefptr", BinaryFieldType.DATA_UNKNOWN)) throw new Exception("Failed to parse undefptr/?");
                        tok.ApplyUInt32H8(obj, x => x.aiProcessPtr);
                    }
                    else
                    {
                        if (reader.Format == BZNFormat.BattlezoneN64 || (reader.Version != 1017 && reader.Version != 1018)) // TODO get range for these
                        {
                            tok = reader.ReadToken();
                            if (tok == null || !tok.Validate("aiProcess", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse aiProcess/BOOL");
                            tok.ApplyBoolean(obj, x => x.aiProcess);
                        }
                    }
                }
                //else
                //{
                // savegame
                //curCmd
                //nextCmd
                //aiProcess
                //}
            }

            if (reader.Format == BZNFormat.Battlezone
             || reader.Format == BZNFormat.BattlezoneN64)
            {
                if (reader.Format == BZNFormat.BattlezoneN64 || reader.Version > 1007)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("isCargo", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse isCargo/BOOL");
                    tok.ApplyBoolean(obj, x => x.isCargo);
                }
            }
            else if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version < 1145)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("isCargo", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse isCargo/BOOL");
                    tok.ApplyBoolean(obj, x => x.isCargo);
                }
                else
                {
                    if (obj != null) obj.isCargo = (saveFlags & 0x10) != 0;
                }
            }

            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                if (reader.Format == BZNFormat.BattlezoneN64 || reader.Version > 1016)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("independence", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse independence/LONG");
                    tok.ApplyUInt32(obj, x => x.independence);
                }
            }
            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version < 1145)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("independence", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse independence");
                    tok.ApplyUInt32(obj, x => x.independence);
                }
                else if (parent.SaveType == SaveType.BZN)
                {
                    if (reader.Version > 1183)
                    {
                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("independence", BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse independence");
                        tok.ApplyUInt8(obj, x => x.independence);
                    }
                    else
                    {
                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("independence", BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse independence");
                        tok.ApplyVoidBytesRaw(obj, x => x.independence, 0, (v) => v[0]);
                    }
                }
            }

            if (reader.Format == BZNFormat.BattlezoneN64) // unsure of this version check
            {
                tok = reader.ReadToken();
                if (tok == null)
                    throw new Exception("Failed to parse hasPilot/BOOL");
                tok.ApplyUInt16(obj, x => x.curPilot, 0, (v) => new SizedString(parent.Hints?.EnumerationPrjID?[v] ?? string.Format("bzn64prjid_{0,4:X4}", v)));
            }
            if (reader.Format == BZNFormat.Battlezone && reader.Version > 1016)
            {
                if (reader.Version < 1030)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("hasPilot", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse hasPilot/BOOL");
                    // test this works
                    tok.ApplyBoolean<ClassGameObject, SizedString>(obj, x => x.curPilot, 0, (hasPilot) => new SizedString(hasPilot ? obj!.isUser ? obj.PrjID.Value[0] + "suser" : obj.PrjID.Value[0] + "spilo" : string.Empty));
                }
                else
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("curPilot", BinaryFieldType.DATA_ID)) throw new Exception("Failed to parse curPilot/ID");
                    tok.ApplyID(obj, x => x.curPilot);
                }
            }
            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version < 1143)
                {
                    // "game object read"
                    if (reader.Version < 1145)
                    {
                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("curPilot", BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse curPilot/CHAR");
                        tok.ApplyChars(obj, x => x.curPilot);
                    }
                }
            }

            if (reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("perceivedTeam", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse perceivedTeam/LONG");
                if (obj != null) obj.perceivedTeam = tok.GetInt32();
            }
            if (reader.Format == BZNFormat.Battlezone)
            {
                if (reader.Version > 1031)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("perceivedTeam", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse perceivedTeam/LONG");
                    if (obj != null) obj.perceivedTeam = tok.GetInt32();
                }
                else
                {
                    if (obj != null) obj.perceivedTeam = -1;
                }
            }
            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version < 1145)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("perceivedTeam", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse perceivedTeam/LONG");
                    if (obj != null) obj.perceivedTeam = tok.GetInt32();
                }
                else
                {
                    if (obj != null) obj.perceivedTeam = -1;
                }
            }

            // section for SaveType != 0
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassGameObject obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            writer.WriteSingle("illumination", obj, x => x.illumination);

            if (writer.Format == BZNFormat.Battlezone || writer.Format == BZNFormat.BattlezoneN64)
            {
                writer.WriteVector3D("pos", obj, x => x.pos2);
            }

            writer.WriteEuler("euler", obj, x => x.euler);

            if (writer.Format == BZNFormat.Battlezone || writer.Format == BZNFormat.BattlezoneN64)
            {
                writer.WriteUInt32("seqNo", obj, x => x.seqNo);
            }
            if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version < 1145)
                {
                    writer.WriteUInt32h("seqNo", obj, x => x.seqNo);
                }
            }

            if (writer.Format == BZNFormat.BattlezoneN64)
            {
                writer.WriteChars("name", obj, x => x.name);
            }
            if (writer.Format == BZNFormat.Battlezone)
            {
                if (writer.Version > 1030)
                    if (writer.Version < 1145)
                    {
                        writer.WriteChars("name", obj, x => x.name);
                    }
                    else
                    {
                        writer.WriteChars("name", obj, x => x.name);
                    }
            }
            if (writer.Format == BZNFormat.Battlezone2)
            {
                writer.WriteSizedString("name", obj, x => x.name);
            }

            // if save type != 0, msgString

            if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version >= 1145)
                {
                    writer.WriteSaveFlags("saveFlags", obj, x => x.saveFlags, (v) =>
                    {
                        if (v.HasValue)
                            return v.Value;

                        byte saveFlags = 0;

                        if (obj.isObjective) saveFlags |= 0x01;
                        if (obj.isSelected) saveFlags |= 0x02;
                        if (obj.defaultShot) saveFlags |= 0x04;
                        if (obj.defaultHealth) saveFlags |= 0x08;
                        if (obj.isCargo) saveFlags |= 0x10;
                        if (parent.SaveType != SaveType.BZN)
                        {
                            // these only happen in non BZN saves.
                            if (obj.curCmd == null) saveFlags |= 0x20;
                            if (obj.nextCmd == null) saveFlags |= 0x40;
                        }

                        return saveFlags;
                    });
                }

                if (writer.Version < 1145)
                {
                    writer.WriteBoolean("isObjective", obj, x => x.isObjective);
                }
                else
                {
                    //isObjective = saveFlags & 0x01 != 0;
                }

                if (writer.Version < 1145)
                {
                    writer.WriteBoolean("isSelected", obj, x => x.isSelected);
                }
                else
                {
                    //selected = saveFlags & 0x02 != 0;
                }

                if (writer.Version < 1145)
                {
                    writer.WriteUInt32h("isVisible", obj, x => x.isVisible);
                }
                else
                {
                    writer.WriteUInt16("isVisible", obj, x => x.isVisible);
                }

                if (writer.Version >= 1197)
                {
                    writer.WriteUInt16("isDamped", obj, x => x.isDamped);
                }
                else
                {
                    //if (obj != null) obj.isDamped = saveFlags & 0x2000 != 0 ? 0xffff : 0; // old depricated value
                }

                // savetype != 0 stuff

                if (writer.Version >= 1151)
                {
                    writer.WriteUInt32("EffectsMask", obj, x => x.EffectsMask);
                }

                if (writer.Version == 1041 || writer.Version == 1047 || writer.Version == 1070)
                {
                    // bz2001.bzn // 1041
                    writer.WriteUInt32h("seen", obj, x => x.seen);
                }
                else if (writer.Version < 1145)
                {
                    writer.WriteUInt32h("isSeen", obj, x => x.seen);
                }
                else
                {
                    // 1165 1180, 1183, 1192, 1197
                    if (writer.Version >= 1165)
                    {
                        writer.WriteUInt16("isSeen", obj, x => x.seen);
                    }
                    else
                    {
                        writer.WriteUInt16h("isSeen", obj, x => x.seen);
                    }
                }
            }

            //if (reader.Format == BZNFormat.Battlezone && reader.Version >= 2011)
            //if (reader.Format == BZNFormat.Battlezone && reader.Version >= 1049)
            if (writer.Format == BZNFormat.Battlezone)// || reader.Format == BZNFormat.BattlezoneN64)
            {
                // not sure if this is on the n64 build
                if ((writer.Version >= 1046 && writer.Version < 2000) || writer.Version >= 2010)
                {
                    writer.WriteBoolean("isCritical", obj, x => x.isCritical);
                }
            }

            if (writer.Format == BZNFormat.Battlezone && (writer.Version == 1001 || writer.Version == 1011 || writer.Version == 1012 || writer.Version == 1017)) // TODO get range for these
            {
                writer.WriteUInt32("liveColor", obj, x => x.liveColor);
                writer.WriteUInt32("deadColor", obj, x => x.deadColor);
                writer.WriteUInt32("teamNumber", obj, x => x.teamNumber);
                writer.WriteUInt32("teamSlot", obj, x => x.teamSlot);
            }

            if (writer.Format == BZNFormat.Battlezone || writer.Format == BZNFormat.BattlezoneN64)
            {
                writer.WriteBoolean("isObjective", obj, x => x.isObjective);

                // I seriously don't understand why this is a thing, it must be wrong, but this is where we get into BZ98R or 1.5 (unclear)
                // code says it should always be read in???
                //if (/*reader.Version != 2004 &&*/ reader.Version != 2003)
                {
                    writer.WriteBoolean("isSelected", obj, x => x.isSelected);
                }

                writer.WriteUInt32h("isVisible", obj, x => x.isVisible);

                //if (writer.Format == BZNFormat.Battlezone)
                {
                    writer.WriteUInt32h("seen", obj, x => x.seen);
                    
                    // problem: if the sign bit is set, it will print as as an unsigned decimal instead of hexadecial
                }
            }

            if (writer.Format == BZNFormat.Battlezone2 && writer.Version != 1041 && writer.Version != 1047) // avoid bz2001.bzn via != 1041
            {
                //if (writer.InBinary)
                //{
                //    if (writer.Version <= 1128)
                //    {
                //        writer.WriteLongFlags(null, (UInt32)obj.groupNumber);
                //    }
                //    else
                //    {
                //        writer.WriteCompressedNumberFromBinary((UInt32)obj.groupNumber);
                //    }
                //}
                //else
                if (writer.Version <= 1128)
                {
                    writer.WriteInt32("groupNumber", obj, x => x.groupNumber);
                }
                else
                {
                    writer.WriteInt8("groupNumber", obj, x => x.groupNumber);
                }
            }

            //if (parent.SaveType == 0)
            //{
            //    isVisible = 0UL;
            //    isSeen = 0UL;
            //    isPinged = 0UL;
            //    isObjective = 0UL;
            //}

            // bunch of SaveType != 0 stuff

            // BZ2 allyNumber
            // BZ2 if !IsDefaultShotData then 
            //     (a2->vftable->field_38)(a2, &this->field_52C, 4, "playerShot", v13);
            //     (a2->vftable->field_38)(a2, this->gap530, 4, "playerCollide");
            //     (a2->vftable->field_38)(a2, &this->gap530[4], 4, "friendShot");
            //     (a2->vftable->field_38)(a2, &this->gap530[8], 4, "friendCollide");
            //     (a2->vftable->field_38)(a2, &this->field_53C, 4, "enemyShot");
            //     (a2->vftable->field_38)(a2, &this->gap540[4], 4, "groundCollide");
            //     (a2->vftable->read_long)(a2, this->gap550, 4, "who_shot_me");
            //     v13 = "team_who_shot_me";

            if (writer.Format == BZNFormat.Battlezone)
            {
                //[10:03:38 PM] Kenneth Miller: I think I may have figured out what that stuff is, maybe
                //[10:03:50 PM] Kenneth Miller: They're timestamps
                //[10:04:04 PM] Kenneth Miller: playerShot, playerCollide, friendShot, friendCollide, enemyShot, groundCollide
                //[10:04:13 PM] Kenneth Miller: the default value is -HUGE_NUMBER (-1e30)
                //[10:04:26 PM] Kenneth Miller: And due to the nature of the game, groundCollide is the most likely to get set first
                //[10:05:02 PM] Kenneth Miller: Old versions of the mission format used to contain those values but later versions only include them in the savegame
                //[10:05:05 PM] Kenneth Miller: (not the mission)
                //[10:05:31 PM] Kenneth Miller: (version 1033 was where they were removed from the mission)
                if (writer.Version < 1033)
                {
                    writer.WriteSingle("playerShot", obj, x => x.playerShot);
                    writer.WriteSingle("playerCollide", obj, x => x.playerCollide);
                    writer.WriteSingle("friendShot", obj, x => x.friendShot);
                    writer.WriteSingle("friendCollide", obj, x => x.friendCollide);
                    writer.WriteSingle("enemyShot", obj, x => x.enemyShot);
                    writer.WriteSingle("groundCollide", obj, x => x.groundCollide);
                }
            }
            if (writer.Format == BZNFormat.Battlezone || writer.Format == BZNFormat.BattlezoneN64)
            {
                writer.WriteSingle("healthRatio", obj, x => x.healthRatio);
                writer.WriteInt32("curHealth", obj, x => x.curHealth);
                writer.WriteInt32("maxHealth", obj, x => x.maxHealth);
            }
            if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version < 1143)
                {
                    writer.WriteSingle("healthRatio", obj, x => x.healthRatio);
                }

                bool defaultHealth = writer.Version >= 1145 && ((obj.saveFlags & 0x08) != 0);

                if (defaultHealth)
                {
                    // set MaxHealth, CurHealth, and AddHealth from ODF
                }
                else
                {
                    writer.WriteSingle("curHealth", obj, x => x.curHealth);
                    writer.WriteSingle("maxHealth", obj, x => x.maxHealth);

                    if (writer.Version != 1041 && writer.Version != 1047)
                    {
                        writer.WriteSingle("addHealth", obj, x => x.addHealth);
                    }
                }
            }

            if (writer.Format == BZNFormat.Battlezone)
            {
                if (writer.Version < 1015)
                {
                    writer.WriteSingle("heatRatio", obj, x => x.heatRatio);
                    writer.WriteInt32("curHeat", obj, x => x.curHeat);
                    writer.WriteInt32("maxHeat", obj, x => x.maxHeat);
                }
            }

            if (writer.Format == BZNFormat.Battlezone || writer.Format == BZNFormat.BattlezoneN64)
            {
                writer.WriteSingle("ammoRatio", obj, x => x.ammoRatio);
                writer.WriteInt32("curAmmo", obj, x => x.curAmmo);
                writer.WriteInt32("maxAmmo", obj, x => x.maxAmmo);
            }
            if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version < 1143)
                {
                    writer.WriteSingle("ammoRatio", obj, x => x.ammoRatio);

                    if (writer.Version >= 1070)
                    {
                        writer.WriteSingle("curAmmo", obj, x => x.curAmmo);
                        writer.WriteSingle("maxAmmo", obj, x => x.maxAmmo);
                    }
                    else
                    {
                        writer.WriteInt32("curAmmo", obj, x => x.curAmmo);
                        writer.WriteInt32("maxAmmo", obj, x => x.maxAmmo);
                    }

                    if (writer.Version >= 1070)
                    {
                        // probably should be a float
                        writer.WriteSingle("addAmmo", obj, x => x.addAmmo);
                    }
                    else if (writer.Version != 1041 && writer.Version != 1047) // avoid bz2001.bzn != 1041
                    {
                        writer.WriteInt32("addAmmo", obj, x => x.addAmmo);
                    }
                }
            }
            // start read of AiCmdInfo
            if (writer.Format == BZNFormat.Battlezone2)
            {
                if (parent.SaveType == SaveType.BZN)
                {
                    if (obj.nextCmd == null || obj.nextCmd.what == 0) // == CMD_NONE
                    {
                        if (obj.curCmd == null)
                        {
                            writer.WriteAiCmdInfo(new AiCmdInfo() { what = 0 }, "undefaicmd");
                        }
                        else
                        {
                            writer.WriteAiCmdInfo(obj.curCmd, "undefaicmd");
                        }
                    }
                    else
                    {
                        writer.WriteAiCmdInfo(obj.nextCmd, "undefaicmd");
                    }
                    writer.WriteBoolean("aiProcess", obj, x => x.aiProcess);
                }
                else
                {
                    if (obj.curCmd != null)
                        writer.WriteAiCmdInfo(obj.curCmd, "undefaicmd");
                    if (obj.nextCmd != null)
                        writer.WriteAiCmdInfo(obj.nextCmd, "undefaicmd");

                    // might be worth making this a dual value with the bool
                    writer.WritePtr("undefptr", obj, x => x.aiProcessPtr);
                }
            }
            else if (writer.Format == BZNFormat.Battlezone || writer.Format == BZNFormat.BattlezoneN64)
            {
                if (parent.SaveType == SaveType.BZN)
                {
                    if (writer.Format == BZNFormat.Battlezone && (writer.Version == 1001 || writer.Version == 1011 || writer.Version == 1012))
                    {
                        writer.WriteAiCmdInfo(obj.curCmd);
                    }

                    {
                        writer.WriteAiCmdInfo(obj.nextCmd);
                    }

                    // aiProcess?
                    if (writer.Format == BZNFormat.Battlezone && writer.Version <= 1012)
                    {
                        // 1011A confirmed
                        // 1012A confirmed
                        // 1001A confirmed with 00000000
                        // might be worth making this a dual value with the bool
                        writer.WritePtr("undefptr", obj, x => x.aiProcessPtr);
                    }
                    else if (writer.Format == BZNFormat.Battlezone && (writer.Version == 1001))
                    {
                        writer.WriteBoolean("undefptr", obj, x => x.aiProcess);
                    }
                    else
                    {
                        if (writer.Format == BZNFormat.BattlezoneN64 || (writer.Version != 1017 && writer.Version != 1018))
                        {
                            writer.WriteBoolean("aiProcess", obj, x => x.aiProcess);
                        }
                    }
                }
                //else
                //{
                // savegame
                //curCmd
                //nextCmd
                //aiProcess
                //}
            }

            if (writer.Format == BZNFormat.Battlezone
             || writer.Format == BZNFormat.BattlezoneN64)
            {
                if (writer.Format == BZNFormat.BattlezoneN64 || writer.Version > 1007)
                {
                    writer.WriteBoolean("isCargo", obj, x => x.isCargo);
                }
            }
            else if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version < 1145)
                {
                    writer.WriteBoolean("isCargo", obj, x => x.isCargo);
                }
                else
                {
                    //if (obj != null) obj.isCargo = (saveFlags & 0x10) != 0;
                }
            }

            if (writer.Format == BZNFormat.Battlezone || writer.Format == BZNFormat.BattlezoneN64)
            {
                if (writer.Format == BZNFormat.BattlezoneN64 || writer.Version > 1016)
                {
                    writer.WriteUInt32("independence", obj, x => x.independence);
                }
            }
            if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version < 1145)
                {
                    writer.WriteUInt32("independence", obj, x => x.independence);
                }
                else if (parent.SaveType == SaveType.BZN)
                {
                    if (writer.Version > 1183)
                    {
                        writer.WriteUInt8("independence", obj, x => x.independence);
                    }
                    else
                    {
                        writer.WriteUInt8Raw("independence", obj, x => x.independence);
                    }
                }
            }

            if (writer.Format == BZNFormat.BattlezoneN64) // unsure of this version check
            {
                writer.WriteUInt16("curPilot", obj, x => x.curPilot, (v) =>
                {
                    if (v.Value.StartsWith("bzn64prjid_"))
                    {
                        string possibleLabel = v.Value.Substring("bzn64prjid_".Length);
                        if (ushort.TryParse(possibleLabel, System.Globalization.NumberStyles.HexNumber, null, out ushort possibleItemID))
                            return possibleItemID;
                    }
                    else
                    {
                        var lookup = parent.Hints?.EnumerationPrjID;
                        if (lookup != null)
                        {
                            UInt16? key = lookup.Where(dr => dr.Value == v.Value.ToLowerInvariant()).Select(dr => dr.Key).FirstOrDefault();
                            if (key.HasValue)
                            {
                                return key.Value;
                            }
                        }
                    }
                    throw new Exception("Failed to parse curPilot/ID");
                });
            }
            if (writer.Format == BZNFormat.Battlezone && writer.Version > 1016)
            {
                if (writer.Version < 1030)
                {
                    writer.WriteBoolean("hasPilot", obj, x => x.curPilot, (curPilot => !string.IsNullOrEmpty(curPilot.Value)));
                }
                else
                {
                    writer.WriteID("curPilot", obj, x => x.curPilot);
                }
            }
            if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version < 1143)
                {
                    // "game object read"
                    if (writer.Version < 1145)
                    {
                        writer.WriteChars("curPilot", obj, x => x.curPilot);
                    }
                }
            }

            if (writer.Format == BZNFormat.BattlezoneN64)
            {
                writer.WriteInt32("perceivedTeam", obj, x => x.perceivedTeam);
            }
            if (writer.Format == BZNFormat.Battlezone)
            {
                if (writer.Version > 1031)
                {
                    writer.WriteInt32("perceivedTeam", obj, x => x.perceivedTeam);
                }
                else
                {
                    //if (obj != null) obj.perceivedTeam = -1;
                }
            }
            if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version < 1145)
                {
                    writer.WriteInt32("perceivedTeam", obj, x => x.perceivedTeam);
                }
                else
                {
                    //if (obj != null) obj.perceivedTeam = -1;
                }
            }

            // section for SaveType != 0
        }
    }
}
