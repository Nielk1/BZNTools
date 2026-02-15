using BZNParser.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "craft")]
    [ObjectClass(BZNFormat.Battlezone2, "boid")]
    public class ClassCraftFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassCraft(preamble, classLabel);
            ClassCraft.Hydrate(parent, reader, obj as ClassCraft);
            return true;
        }
    }
    public class ClassCraft : ClassGameObject
    {
        public enum VEHICLE_STATE { UNDEPLOYED, DEPLOYING, DEPLOYED, UNDEPLOYING };
        public VEHICLE_STATE state { get; set; } = VEHICLE_STATE.UNDEPLOYED;


        public Int32 abandoned { get; set; }



        public bool m_Use13Aim { get; set; } = false;

        public ClassCraft(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassCraft? obj)
        {
            IBZNToken tok;

            if (reader.Format == BZNFormat.Battlezone && reader.Version < 1019)
            {
                // obsolete
                if (reader.Version > 1001)
                {
                    tok = reader.ReadToken(); // energy0current
                    tok = reader.ReadToken(); // energy0maximum
                    tok = reader.ReadToken(); // energy1current
                    tok = reader.ReadToken(); // energy1maximum
                    tok = reader.ReadToken(); // energy2current
                    tok = reader.ReadToken(); // energy2maximum

                    tok = reader.ReadToken(); // bumpers

                    //if(!tok.Validate(null, BinaryFieldType.DATA_VEC3D))
                    //    throw new Exception("Failed to parse ???/VEC3D");
                    // there are 6 vectors here, but we don't know what they are for and are probably able to be forgotten
                }
                else
                {
                    tok = reader.ReadToken(); // bumpers or armor, 24 0x00s raw
                    tok = reader.ReadToken(); // bumpers, 6 VEC3
                }
            }

            //if (reader.Format == BZNFormat.BattlezoneN64 || ((reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.Battlezone2) && reader.Version > 1022))
            //if (reader.Format == BZNFormat.BattlezoneN64 || ((reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.Battlezone2) && reader.Version >= 1037))
            if (reader.Format == BZNFormat.BattlezoneN64
            || (reader.Format == BZNFormat.Battlezone && reader.Version > 1027)
            || (reader.Format == BZNFormat.Battlezone2))// && reader.Version >= 1034))
            {
                tok = reader.ReadToken();
                if (!tok.Validate("abandoned", BinaryFieldType.DATA_LONG))
                    throw new Exception("Failed to parse abandoned/LONG");

                if (obj != null) obj.abandoned = tok.GetInt32();
            }
            /*if (reader.Format == BZNFormat.Battlezone && reader.Version <= 1022 && reader.Version != 1001)
            {
                // does this ever even happen?
                tok = reader.ReadToken();//setAltitude [1] =
                                         //1
                tok = reader.ReadToken();//accelDragStop [1] =
                                         //3.5
                tok = reader.ReadToken();//accelDragFull [1] =
                                         //1
                tok = reader.ReadToken();//alphaTrack [1] =
                                         //20
                tok = reader.ReadToken();//alphaDamp [1] =
                                         //5
                tok = reader.ReadToken();//pitchPitch [1] =
                                         //0.25
                tok = reader.ReadToken();//pitchThrust [1] =
                                         //0.1
                tok = reader.ReadToken();//rollStrafe [1] =
                                         //0.1
                tok = reader.ReadToken();//rollSteer [1] =
                                         //0.1
                tok = reader.ReadToken();//velocForward [1] =
                                         //20
                tok = reader.ReadToken();//velocReverse [1] =
                                         //15
                tok = reader.ReadToken();//velocStrafe [1] =
                                         //20
                tok = reader.ReadToken();//accelThrust [1] =
                                         //20
                tok = reader.ReadToken();//accelBrake [1] =
                                         //75
                tok = reader.ReadToken();//omegaSpin [1] =
                                         //4
                tok = reader.ReadToken();//omegaTurn [1] =
                                         //1.5
                tok = reader.ReadToken();//alphaSteer [1] =
                                         //5
                tok = reader.ReadToken();//accelJump [1] =
                                         //20
                tok = reader.ReadToken();//thrustRatio [1] =
                                         //1
                tok = reader.ReadToken();//throttle [1] =
                                         //0
                tok = reader.ReadToken();//airBorne [1] =
                                         //5.96046e-008
            }*/

            //if (reader.Format == BZNFormat.Battlezone && (reader.Version >= 1032 && reader.Version <= 1033))
            //if (reader.Format == BZNFormat.Battlezone && (reader.Version >= 1030 && reader.Version <= 1033))
            /*if (reader.Format == BZNFormat.Battlezone && (reader.Version >= 1030 && reader.Version <= 1033))
            {
                tok = reader.ReadToken();
                if (!tok.Validate("????", BinaryFieldType.DATA_LONG))
                    throw new Exception("Failed to parse ????/LONG");
                uint unknown = (uint)tok.GetUInt32H();
            }*/

            // guesses: omit version 2016, 2011
            if (reader.Format == BZNFormat.Battlezone && reader.Version >= 2000)
            {
                if (reader.Version < 2002)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("cloakTransitionTime", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse cloakTransitionTime/FLOAT");
                    float cloakTransitionTime = (uint)tok.GetSingle();
                }
                
                tok = reader.ReadToken();
                if (!tok.Validate("cloakState", BinaryFieldType.DATA_VOID))
                    throw new Exception("Failed to parse cloakState/VOID");
                UInt32 cloakState = (uint)tok.GetUInt32H();

                tok = reader.ReadToken();
                if (!tok.Validate("cloakTransBeginTime", BinaryFieldType.DATA_FLOAT))
                    throw new Exception("Failed to parse cloakTransBeginTime/FLOAT");
                float cloakTransBeginTime = (uint)tok.GetSingle();

                tok = reader.ReadToken();
                if (!tok.Validate("cloakTransEndTime", BinaryFieldType.DATA_FLOAT))
                    throw new Exception("Failed to parse cloakTransEndTime/FLOAT");
                float cloakTransEndTime = (uint)tok.GetSingle();
            }

            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version >= 1143)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("curAmmo", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse curAmmo/FLOAT");
                    if (obj != null) obj.curAmmo = (int)tok.GetSingle();

                    tok = reader.ReadToken();
                    if (!tok.Validate("maxAmmo", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse maxAmmo/FLOAT");
                    if (obj != null) obj.maxAmmo = (int)tok.GetSingle();

                    tok = reader.ReadToken();
                    if (!tok.Validate("addAmmo", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse addAmmo/FLOAT");
                    float addAmmo = (uint)tok.GetSingle();

                    if (reader.InBinary)
                    {
                        tok = reader.ReadToken();
                        if (!tok.Validate(null, BinaryFieldType.DATA_CHAR))
                            throw new Exception("Failed to parse ?/CHAR");
                        byte curPilotLength = tok.GetUInt8();

                        if (curPilotLength > 0)
                        {
                            tok = reader.ReadToken();
                            if (!tok.Validate("curPilot", BinaryFieldType.DATA_CHAR))
                                throw new Exception("Failed to parse curPilot/CHAR");
                            if (obj != null) obj.curPilot = tok.GetString();
                        }
                    }
                    else
                    {
                        tok = reader.ReadToken();
                        if (reader.Version == 1145 || reader.Version == 1147 || reader.Version == 1148 || reader.Version == 1149 || reader.Version == 1151 || reader.Version == 1154)
                        {
                            if (!tok.Validate("config", BinaryFieldType.DATA_CHAR))
                                throw new Exception("Failed to parse curPilot/CHAR");
                        }
                        else
                        {
                            if (!tok.Validate("curPilot", BinaryFieldType.DATA_CHAR))
                                throw new Exception("Failed to parse curPilot/CHAR");
                        }
                        if (obj != null) obj.curPilot = tok.GetString();
                    }

                    if (reader.Version == 1195)
                    {
                        tok = reader.ReadToken();
                        if (!tok.Validate("m_ejectRatio", BinaryFieldType.DATA_FLOAT))
                            throw new Exception("Failed to parse m_ejectRatio/FLOAT");
                        //m_ejectRatio = tok.GetSingle();
                    }
                    else if (reader.Version >= 1196)
                    {
                        tok = reader.ReadToken();
                        if (!tok.Validate("ejectRatio", BinaryFieldType.DATA_FLOAT))
                            throw new Exception("Failed to parse ejectRatio/FLOAT");
                        //m_ejectRatio = tok.GetSingle();
                    }
                }
            }

            ClassGameObject.Hydrate(parent, reader, obj as ClassGameObject);
        }
    }
}
