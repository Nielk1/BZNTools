using BZNParser.Reader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    public class ClassGameObject : Entity
    {
        public float illumination { get; set; }
        public Vector3D pos { get; set; }
        public Euler euler { get; set; }
        public UInt32 seqNo { get; set; }
        public string name { get; set; }
        public bool isObjective { get; set; }
        public bool isSelected { get; set; }
        public UInt32 isVisible { get; set; }
        public UInt32 seen { get; set; }
        public float healthRatio { get; set; }
        public UInt32 curHealth { get; set; }
        public UInt32 maxHealth { get; set; }
        public float ammoRatio { get; set; }
        public Int32 curAmmo { get; set; }
        public Int32 maxAmmo { get; set; }
        public UInt32 priority { get; set; }
        public UInt32 what { get; set; }
        //public UInt32 who { get; set; }
        public Int32 who { get; set; }
        public UInt32 where { get; set; }
        public UInt32 param { get; set; }
        public bool aiProcess { get; set; }
        public AiCmdInfo curCmd { get; set; }
        public AiCmdInfo nextCmd { get; set; }
        public bool isCargo { get; set; }
        public UInt32 independence { get; set; }
        public string curPilot { get; set; }
        public Int32 perceivedTeam { get; set; }

        public ClassGameObject(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
        }

        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassGameObject? obj)
        {
            IBZNToken tok;

            tok = reader.ReadToken();
            if (!tok.Validate("illumination", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse illumination/FLOAT");
            if (obj != null) obj.illumination = tok.GetSingle();

            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("pos", BinaryFieldType.DATA_VEC3D)) throw new Exception("Failed to parse pos/VEC3D");
                if (obj != null) obj.pos = tok.GetVector3D();
            }

            if (obj != null) obj.euler = reader.GetEuler(parent.SaveType);

            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("seqNo", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse seqNo/LONG");
                //seqNo = tok.GetUInt16();
                if (obj != null) obj.seqNo = tok.GetUInt32();
            }
            if (reader.Format == BZNFormat.Battlezone2)
            {
                //if (reader.Version >= 1123 && reader.Version < 1145)
                if (reader.Version < 1145)
                {
                    // 1123 1124 1128 1141 1142
                    tok = reader.ReadToken();
                    if (!tok.Validate("seqNo", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse seqNo/LONG");
                    //seqNo = tok.GetUInt16();
                    if (obj != null) obj.seqNo = tok.GetUInt32H();
                }
            }

            if (reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("name", BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse name/CHAR");
                if (obj != null) obj.name = tok.GetString();
            }
            if (reader.Format == BZNFormat.Battlezone)
            {
                // broke this section, need to fix it
                if (reader.Version > 1030)
                    if (reader.Version < 1145)
                    {
                        /*if (reader.InBinary)
                        {
                            tok = reader.ReadToken();
                            if (!tok.Validate(null, BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse ?/LONG");
                            UInt32 odfLength = tok.GetUInt32();
                        }*/

                        tok = reader.ReadToken();
                        if (!tok.Validate("name", BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse name/CHAR");
                        if (obj != null) obj.name = tok.GetString();
                    }
                    else
                    {
                        //tok = reader.ReadToken();
                        //if (!tok.Validate(null, BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse ?/CHAR");
                        //byte odfLength = tok.GetUInt8();

                        tok = reader.ReadToken();
                        if (!tok.Validate("name", BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse name/CHAR");
                        if (obj != null) obj.name = tok.GetString();
                    }
            }
            if (reader.Format == BZNFormat.Battlezone2)
            {
                string name = reader.ReadSizedString_BZ2_1145("name", 32);
                if (obj != null) obj.name = name;
            }

            // if save type != 0, msgString

            byte saveFlags = 0;
            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version >= 1145)
                {
                    saveFlags = reader.ReadBytePossibleRawPossibleSigned_BZ2("saveFlags");
                }

                if (reader.Version < 1145)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("isObjective", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse isObjective/BOOL");
                    if (obj != null) obj.isObjective = tok.GetBoolean();
                }
                else
                {
                    //isObjective = saveFlags & 0x01 != 0;
                }

                if (reader.Version < 1145)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("isSelected", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse isSelected/BOOL");
                    if (obj != null) obj.isSelected = tok.GetBoolean();
                }
                else
                {
                    //selected = saveFlags & 0x02 != 0;
                }

                if (reader.Version < 1145)
                {
                    tok = reader.ReadToken();
                    if (tok.Validate("isVisible", BinaryFieldType.DATA_LONG))
                    {
                        if (obj != null) obj.isVisible = (UInt16)tok.GetUInt32H();
                    }
                    else if (tok.Validate("isVisible", BinaryFieldType.DATA_SHORT))
                    {
                        // not sure if this should ever happen, the code doesn't handle it as a thing
                        if (obj != null) obj.isVisible = tok.GetUInt16H();
                    }
                    else
                    {
                        throw new Exception("Failed to parse isVisible/LONG/SHORT");
                    }
                }
                else
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("isVisible", BinaryFieldType.DATA_SHORT)) throw new Exception("Failed to parse isVisible/SHORT");
                    if (obj != null) obj.isVisible = tok.GetUInt16();
                }

                // savetype != 0 stuff

                if (reader.Version >= 1151)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("EffectsMask", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse EffectsMask/LONG");
                    UInt32 EffectsMask = tok.GetUInt32();
                }

                if (reader.Version == 1041 || reader.Version == 1047 || reader.Version == 1070)
                {
                    // bz2001.bzn // 1041
                    tok = reader.ReadToken();
                    if (!tok.Validate("seen", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse seen/LONG");
                    if (obj != null) obj.seen = tok.GetUInt32H();
                }
                else if (reader.Version < 1145)
                {
                    tok = reader.ReadToken();
                    if (tok.Validate("isSeen", BinaryFieldType.DATA_LONG))
                    {
                        if (obj != null) obj.seen = tok.GetUInt32H();
                    }
                    else if (tok.Validate("isSeen", BinaryFieldType.DATA_SHORT))
                    {
                        if (obj != null) obj.seen = tok.GetUInt16H();
                    }
                    else
                    {
                        throw new Exception("Failed to parse isSeen/LONG/SHORT");
                    }
                }
                else
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("isSeen", BinaryFieldType.DATA_SHORT)) throw new Exception("Failed to parse isSeen/SHORT");
                    if (obj != null) obj.seen = tok.GetUInt16(); // seen should be 16bit shouldn't it?
                }
                /*if (reader.Version > 1105)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("saveFlags", BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse saveFlags/CHAR");
                    //saveFlags = tok.GetUInt32(); // another RAW in ASCII
                    //saveFlags = tok.GetUInt8(); // another RAW in ASCII

                    tok = reader.ReadToken();
                    //if (!tok.Validate("isVisible", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse isVisible/LONG");
                    if (!tok.Validate("isVisible", BinaryFieldType.DATA_SHORT)) throw new Exception("Failed to parse isVisible/SHORT");
                    //isVisible = tok.GetUInt32();
                    isVisible = tok.GetUInt16();
                }
                else
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("saveFlags", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse saveFlags/BOOL");
                    //saveFlags = tok.GetUInt8(); // another RAW in ASCII

                    tok = reader.ReadToken();
                    //if (!tok.Validate("isVisible", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse isVisible/LONG");
                    if (!tok.Validate("isVisible", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse isVisible/BOOL");
                    //isVisible = tok.GetUInt32();
                    isVisible = tok.GetBoolean() ? 1u : 0u;
                }

                tok = reader.ReadToken();
                if (!tok.Validate("EffectsMask", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse EffectsMask/LONG");
                UInt32 EffectsMask = tok.GetUInt32();

                tok = reader.ReadToken();
                if (!tok.Validate("isSeen", BinaryFieldType.DATA_SHORT)) throw new Exception("Failed to parse isSeen/SHORT");
                seen = tok.GetUInt16();*/
            }

            //if (reader.Format == BZNFormat.Battlezone && reader.Version >= 2011)
            //if (reader.Format == BZNFormat.Battlezone && reader.Version >= 1049)
            if (reader.Format == BZNFormat.Battlezone)// || reader.Format == BZNFormat.BattlezoneN64)
            {
                // not sure if this is on the n64 build
                if ((reader.Version >= 1046 && reader.Version < 2000) || reader.Version >= 2010)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("isCritical", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse isCritical/BOOL");
                    //isCritical = tok.GetBoolean();
                }
            }

            if (reader.Format == BZNFormat.Battlezone && (reader.Version == 1001 || reader.Version == 1011 || reader.Version == 1012 || reader.Version == 1017)) // TODO get range for these
            {
                tok = reader.ReadToken();
                if (!tok.Validate("liveColor", BinaryFieldType.DATA_UNKNOWN)) throw new Exception("Failed to parse liveColor/UNKNOWN");

                tok = reader.ReadToken();
                if (!tok.Validate("deadColor", BinaryFieldType.DATA_UNKNOWN)) throw new Exception("Failed to parse deadColor/UNKNOWN");

                tok = reader.ReadToken();
                if (!tok.Validate("teamNumber", BinaryFieldType.DATA_UNKNOWN)) throw new Exception("Failed to parse teamNumber/UNKNOWN");

                tok = reader.ReadToken();
                if (!tok.Validate("teamSlot", BinaryFieldType.DATA_UNKNOWN)) throw new Exception("Failed to parse teamSlot/UNKNOWN");
            }

            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("isObjective", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse isObjective/BOOL");
                if (obj != null) obj.isObjective = tok.GetBoolean();

                // I seriously don't understand why this is a thing, it must be wrong, but this is where we get into BZ98R or 1.5 (unclear)
                // code says it should always be read in???
                //if (/*reader.Version != 2004 &&*/ reader.Version != 2003)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("isSelected", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse isSelected/BOOL");
                    if (obj != null) obj.isSelected = tok.GetBoolean();
                }

                tok = reader.ReadToken();
                if (!tok.Validate("isVisible", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse isVisible/LONG");
                if (obj != null) obj.isVisible = tok.GetUInt32H();

                tok = reader.ReadToken();
                if (!tok.Validate("seen", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse seen/LONG");
                if (reader.Format == BZNFormat.Battlezone)
                {
                    UInt32 seen;
                    try
                    {
                        seen = tok.GetUInt32H();
                        if ((seen & 0xFFFF0000u) != 0)
                        {
                            bool HasSignBit = (seen & 0x80000000) != 0;
                            bool HasOtherOverflowBits = (seen & 0x7FFF0000) != 0;
                            if (HasSignBit && !HasOtherOverflowBits)
                            {
                                if (HasSignBit && !HasOtherOverflowBits)
                                {
                                    // issue was caused by a bad sign bit forcing a mis-write of the data as a decimal number instead of hex
                                    // TODO note malformation
                                    seen &= 0x0000FFFF;
                                }
                                else
                                {
                                    // issue is undetermined other than it being decimal instead of hex
                                    // TODO note malformation
                                }
                            }
                            else if (!tok.IsBinary && tok.GetString().Any(c => !"1234567890".Contains(c)))
                            {
                                // assume this is a decimal number instead of hex
                                seen = tok.GetUInt32();
                                HasSignBit = (seen & 0x80000000) != 0;
                                HasOtherOverflowBits = (seen & 0x7FFF0000) != 0;
                                if (HasSignBit && !HasOtherOverflowBits)
                                {
                                    // issue was caused by a bad sign bit forcing a mis-write of the data as a decimal number instead of hex
                                    // TODO note malformation
                                    seen &= 0x0000FFFF;
                                }
                                else
                                {
                                    // issue is undetermined other than it being decimal instead of hex
                                    // TODO note malformation
                                }
                            }
                            else
                            {
                                // TODO note malformation
                            }
                        }
                    }
                    catch (System.OverflowException)
                    {
                        // if we overflowed we have to assume the value might be improperly stored as a decimal instead of a hexadecimal string
                        seen = tok.GetUInt32();

                        //bool HasMalformation = (seen & 0xFFFF0000u) != 0;
                        bool HasSignBit = (seen & 0x80000000) != 0;
                        bool HasOtherOverflowBits = (seen & 0x7FFF0000) != 0;
                        if (HasSignBit && ! HasOtherOverflowBits)
                        {
                            // issue was caused by a bad sign bit forcing a mis-write of the data as a decimal number instead of hex
                            // TODO note malformation
                            seen &= 0x0000FFFF;
                        }
                        else
                        {
                            // issue is undetermined other than it being decimal instead of hex
                            // TODO note malformation
                        }
                    }
                    if (obj != null) obj.seen = seen;
                }
            }

            if (reader.Format == BZNFormat.Battlezone2 && reader.Version != 1041 && reader.Version != 1047) // avoid bz2001.bzn via != 1041
            {
                tok = reader.ReadToken();
                Int32 groupNumber;
                if (tok.Validate("groupNumber", BinaryFieldType.DATA_LONG))
                {
                    groupNumber = tok.GetInt32();
                }
                else if (tok.Validate("groupNumber", BinaryFieldType.DATA_SHORT))
                {
                    groupNumber = tok.GetInt16();
                }
                else if (tok.Validate("groupNumber", BinaryFieldType.DATA_CHAR))
                {
                    groupNumber = tok.GetInt8();
                }
                else
                {
                    throw new Exception("Failed to parse groupNumber/LONG/SHORT/CHAR");
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
                    tok = reader.ReadToken(); // float (-HUGE_NUMBER) // playerCollide
                    tok = reader.ReadToken(); // float (-HUGE_NUMBER) // friendShot
                    tok = reader.ReadToken(); // float (-HUGE_NUMBER) // friendCollide
                    tok = reader.ReadToken(); // float (-HUGE_NUMBER) // enemyShot
                    tok = reader.ReadToken(); // float                // groundCollide
                }
            }
            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("healthRatio", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse healthRatio/FLOAT");
                if (obj != null) obj.healthRatio = tok.GetSingle();

                tok = reader.ReadToken();
                if (!tok.Validate("curHealth", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse curHealth/LONG");
                if (obj != null) obj.curHealth = tok.GetUInt32();

                tok = reader.ReadToken();
                if (!tok.Validate("maxHealth", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse maxHealth/LONG");
                if (obj != null) obj.maxHealth = tok.GetUInt32();
            }
            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version < 1143)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("healthRatio", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse healthRatio/FLOAT");
                    if (obj != null) obj.healthRatio = tok.GetSingle();
                }

                bool defaultHealth = reader.Version >= 1145 && ((saveFlags & 0x08) != 0);

                if (defaultHealth)
                {
                    // set MaxHealth, CurHealth, and AddHealth from ODF
                }
                else
                {

                    tok = reader.ReadToken();
                    if (!tok.Validate("curHealth", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse curHealth/FLOAT");
                    //curHealth = tok.GetUInt32();
                    //curHealth = tok.GetSingle();

                    // is this a single instead of a long?
                    tok = reader.ReadToken();
                    if (!tok.Validate("maxHealth", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse maxHealth/FLOAT");
                    //maxHealth = tok.GetUInt32();
                    if (obj != null) obj.maxHealth = (uint)tok.GetSingle();

                    if (reader.Version != 1041 && reader.Version != 1047)
                    {
                        tok = reader.ReadToken();
                        if (!tok.Validate("addHealth", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse addHealth/FLOAT");
                        //addHealth = tok.GetUInt32();
                    }
                }
            }

            if (reader.Format == BZNFormat.Battlezone)
            {
                if (reader.Version < 1015)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("heatRatio", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse heatRatio/FLOAT");
                    float heatRatio = tok.GetSingle();

                    tok = reader.ReadToken();
                    if (!tok.Validate("curHeat", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse curHeat/LONG");
                    Int32 curHeat = tok.GetInt32();

                    tok = reader.ReadToken();
                    if (!tok.Validate("maxHeat", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse maxHeat/LONG");
                    Int32 maxHeat = tok.GetInt32();
                }
            }

            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("ammoRatio", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse ammoRatio/FLOAT");
                if (obj != null) obj.ammoRatio = tok.GetSingle();

                tok = reader.ReadToken();
                if (!tok.Validate("curAmmo", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse curAmmo/LONG");
                if (obj != null) obj.curAmmo = tok.GetInt32();

                tok = reader.ReadToken();
                if (!tok.Validate("maxAmmo", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse maxAmmo/LONG");
                if (obj != null) obj.maxAmmo = tok.GetInt32();
            }
            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version < 1143)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("ammoRatio", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse ammoRatio/FLOAT");
                    if (obj != null) obj.ammoRatio = tok.GetSingle();

                    if (reader.Version >= 1070)
                    {
                        // these probably should be floats not longs
                        tok = reader.ReadToken();
                        if (!tok.Validate("curAmmo", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse curAmmo/FLOAT");
                        if (obj != null) obj.curAmmo = (Int32)tok.GetSingle();

                        tok = reader.ReadToken();
                        if (!tok.Validate("maxAmmo", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse maxAmmo/FLOAT");
                        if (obj != null) obj.maxAmmo = (Int32)tok.GetSingle();
                    }
                    else
                    {
                        tok = reader.ReadToken();
                        if (!tok.Validate("curAmmo", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse curAmmo/LONG");
                        if (obj != null) obj.curAmmo = tok.GetInt32();

                        tok = reader.ReadToken();
                        if (!tok.Validate("maxAmmo", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse maxAmmo/LONG");
                        if (obj != null) obj.maxAmmo = tok.GetInt32();
                    }

                    if (reader.Version >= 1070)
                    {
                        // probably should be a float
                        tok = reader.ReadToken();
                        if (!tok.Validate("addAmmo", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse addAmmo/FLOAT");
                        UInt32 addAmmo = (UInt32)tok.GetSingle();
                    }
                    else if (reader.Version != 1041 && reader.Version != 1047) // avoid bz2001.bzn != 1041
                    {
                        tok = reader.ReadToken();
                        if (!tok.Validate("addAmmo", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse addAmmo/LONG");
                        UInt32 addAmmo = tok.GetUInt32();
                    }
                }
            }
            // not sure at all that this IF handles binary properly
            if (!reader.InBinary && reader.Format == BZNFormat.Battlezone2)
            {
                // not sure when this reads if ever
                tok = reader.ReadToken();
                if (!tok.Validate("undefaicmd", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse undefaicmd/LONG");
            }

            // start read of AiCmdInfo
            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (parent.SaveType == 0)
                {
                    {
                        AiCmdInfo info = reader.GetAiCmdInfo();
                        if (obj != null)
                            obj.nextCmd = info; // TODO is this the correct storage for BZ2's
                    }
                    // end read of AiCmdInfo

                    tok = reader.ReadToken();
                    if (!tok.Validate("aiProcess", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse aiProcess/BOOL");
                    if (obj != null) obj.aiProcess = tok.GetBoolean();
                }
                else
                {
                    // savegame
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
                    if (reader.Format == BZNFormat.Battlezone && (reader.Version == 1001 || reader.Version == 1011 || reader.Version == 1012))
                    {
                        tok = reader.ReadToken();
                        if (!tok.Validate("undefptr", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse aiProcess/BOOL");
                        if (obj != null) obj.aiProcess = tok.GetUInt32H() != 0;
                    }
                    else
                    {
                        if (reader.Format == BZNFormat.BattlezoneN64 || (reader.Version != 1017 && reader.Version != 1018)) // TODO get range for these
                        {
                            tok = reader.ReadToken();
                            if (!tok.Validate("aiProcess", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse aiProcess/BOOL");
                            if (obj != null) obj.aiProcess = tok.GetBoolean();
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
                    if (!tok.Validate("isCargo", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse isCargo/BOOL");
                    if (obj != null) obj.isCargo = tok.GetBoolean();
                }
            }
            else if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version < 1145)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("isCargo", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse isCargo/BOOL");
                    if (obj != null) obj.isCargo = tok.GetBoolean();
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
                    if (!tok.Validate("independence", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse independence/LONG");
                    if (obj != null) obj.independence = tok.GetUInt32();
                }
            }
            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version < 1145)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("independence", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse independence");
                    //independence = BitConverter.ToUInt32(tok.GetRaw(0));
                    //independence = tok.GetUInt8();
                    if (obj != null) obj.independence = tok.GetUInt32(); // this is a bit odd, the game only uses 8 bits it appears but it uses a 32bit here
                }
                else if (parent.SaveType == 0)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("independence", BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse independence");
                    //independence = BitConverter.ToUInt32(tok.GetRaw(0));
                    if (obj != null) obj.independence = tok.GetRaw(0, 1)[0]; // game uses 1 byte by force here
                }
            }

            if (reader.Format == BZNFormat.BattlezoneN64) // unsure of this version check
            {
                tok = reader.ReadToken();
                UInt16 curPilotID = tok.GetUInt16();
                if (obj != null) obj.curPilot = parent?.Hints?.EnumerationPrjID?[curPilotID] ?? string.Format("bzn64prjid_{0,4:X4}", curPilotID);
            }
            if (reader.Format == BZNFormat.Battlezone && reader.Version > 1016)
            {
                if (reader.Version < 1030)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("hasPilot", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse hasPilot/BOOL");
                    bool hasPilot = tok.GetBoolean();
                    if (obj != null) obj.curPilot = hasPilot ? obj.isUser ? obj.PrjID[0] + "suser" : obj.PrjID[0] + "spilo" : string.Empty;
                }
                else
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("curPilot", BinaryFieldType.DATA_ID)) throw new Exception("Failed to parse curPilot/ID");
                    if (obj != null) obj.curPilot = tok.GetString();
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
                        //if (!tok.Validate("curPilot", BinaryFieldType.DATA_ID)) throw new Exception("Failed to parse curPilot/ID");
                        if (!tok.Validate("curPilot", BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse curPilot/CHAR");
                        if (obj != null) obj.curPilot = tok.GetString();
                    }
                }
            }

            if (reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("perceivedTeam", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse perceivedTeam/LONG");
                if (obj != null) obj.perceivedTeam = tok.GetInt32();
            }
            if (reader.Format == BZNFormat.Battlezone)
            {
                if (reader.Version > 1031)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("perceivedTeam", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse perceivedTeam/LONG");
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
                    if (!tok.Validate("perceivedTeam", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse perceivedTeam/LONG");
                    if (obj != null) obj.perceivedTeam = tok.GetInt32();
                }
                else
                {
                    if (obj != null) obj.perceivedTeam = -1;
                }
            }

            // section for SaveType != 0
        }
    }
}
