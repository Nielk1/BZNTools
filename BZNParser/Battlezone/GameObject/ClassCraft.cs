using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.PortableExecutable;
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
            {
                obj = new ClassCraft(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                return ClassCraft.Hydrate(parent, reader, obj as ClassCraft).Success;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassCraft : ClassGameObject
    {
        public enum VEHICLE_STATE { UNDEPLOYED, DEPLOYING, DEPLOYED, UNDEPLOYING };
        public VEHICLE_STATE state { get; set; } = VEHICLE_STATE.UNDEPLOYED;


        public Int32 abandoned { get; set; }

        public float cloakTransitionTime { get; set; }
        public UInt32 cloakState { get; set; }
        public float cloakTransBeginTime { get; set; }
        public float cloakTransEndTime { get; set; }

        public float m_ejectRatio { get; set; }

        public bool m_Use13Aim { get; set; } = false;



        // legacy
        public UInt32 energy0current { get; set; }
        public UInt32 energy0maximum { get; set; }
        public UInt32 energy1current { get; set; }
        public UInt32 energy1maximum { get; set; }
        public UInt32 energy2current { get; set; }
        public UInt32 energy2maximum { get; set; }
        public Vector3D[]? bumpers { get; set; }




        public ClassCraft(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
            abandoned = 0;
            cloakTransitionTime = 0;
            cloakState = 0;
            cloakTransBeginTime = 0;
            cloakTransEndTime = 0;
            m_ejectRatio = 0;

            energy0current = 0;
            energy0maximum = 0;
            energy1current = 0;
            energy1maximum = 0;
            energy2current = 0;
            energy2maximum = 0;
            bumpers = new Vector3D[6];
            for (int i = 0; i < bumpers.Length; i++)
                bumpers[i] = new Vector3D();
        }

        public override void ClearMalformations()
        {
            Malformations.Clear();
            if (bumpers != null)
                foreach (var bumper in bumpers)
                    bumper?.ClearMalformations();
            base.ClearMalformations();
        }

        public override void DisableMalformationAutoFix()
        {
            if (bumpers != null)
                foreach (var bumper in bumpers)
                    bumper?.DisableMalformationAutoFix();
            base.DisableMalformationAutoFix();
        }

        public override void EnableMalformationAutoFix()
        {
            if (bumpers != null)
                foreach (var bumper in bumpers)
                    bumper?.EnableMalformationAutoFix();
            base.EnableMalformationAutoFix();
        }


        public static ParseResult Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassCraft? obj)
        {
            IBZNToken? tok;

            if (reader.Format == BZNFormat.Battlezone && reader.Version < 1019)
            {
                // obsolete
                if (reader.Version > 1001)
                {
                    tok = reader.ReadToken(); // energy0current
                    if (tok == null || !tok.Validate("energy0current", BinaryFieldType.DATA_LONG))
                        return ParseResult.Fail("Failed to parse energy0current/LONG");
                    tok.ApplyUInt32(obj, x => x.energy0current);

                    tok = reader.ReadToken(); // energy0maximum
                    if (tok == null || !tok.Validate("energy0maximum", BinaryFieldType.DATA_LONG))
                        return ParseResult.Fail("Failed to parse energy0maximum/LONG");
                    tok.ApplyUInt32(obj, x => x.energy0maximum);

                    tok = reader.ReadToken(); // energy1current
                    if (tok == null || !tok.Validate("energy1current", BinaryFieldType.DATA_LONG))
                        return ParseResult.Fail("Failed to parse energy1current/LONG");
                    tok.ApplyUInt32(obj, x => x.energy1current);

                    tok = reader.ReadToken(); // energy1maximum
                    if (tok == null || !tok.Validate("energy1maximum", BinaryFieldType.DATA_LONG))
                        return ParseResult.Fail("Failed to parse energy1maximum/LONG");
                    tok.ApplyUInt32(obj, x => x.energy1maximum);

                    tok = reader.ReadToken(); // energy2current
                    if (tok == null || !tok.Validate("energy2current", BinaryFieldType.DATA_LONG))
                        return ParseResult.Fail("Failed to parse energy2current/LONG");
                    tok.ApplyUInt32(obj, x => x.energy2current);

                    tok = reader.ReadToken(); // energy2maximum
                    if (tok == null || !tok.Validate("energy2maximum", BinaryFieldType.DATA_LONG))
                        return ParseResult.Fail("Failed to parse energy2maximum/LONG");
                    tok.ApplyUInt32(obj, x => x.energy2maximum);

                    tok = reader.ReadToken(); // bumpers
                    if (tok == null || !tok.Validate("bumpers", BinaryFieldType.DATA_VEC3D) || tok.GetCount() != 6)
                        return ParseResult.Fail("Failed to parse energy2maximum/LONG");

                    if (obj != null)
                    {
                        obj.bumpers = new Vector3D[6];
                        for (int i = 0; i < 6; i++)
                            obj.bumpers[i] = tok.GetVector3D(i);
                    }
                }
                else
                {
                    tok = reader.ReadToken(); // bumpers or armor, 24 0x00s raw
                    if (tok == null || !tok.Validate("armor", BinaryFieldType.DATA_VOID) || tok.GetCount() != 1)
                        return ParseResult.Fail("Failed to parse energy2maximum/LONG");

                    if (obj != null)
                    {
                        obj.energy0current = BitConverter.ToUInt32(tok.GetRaw(4 * 0, 4));
                        obj.energy0maximum = BitConverter.ToUInt32(tok.GetRaw(4 * 1, 4));
                        obj.energy1current = BitConverter.ToUInt32(tok.GetRaw(4 * 2, 4));
                        obj.energy1maximum = BitConverter.ToUInt32(tok.GetRaw(4 * 3, 4));
                        obj.energy2current = BitConverter.ToUInt32(tok.GetRaw(4 * 4, 4));
                        obj.energy2maximum = BitConverter.ToUInt32(tok.GetRaw(4 * 5, 4));
                    }
                    
                    tok = reader.ReadToken(); // bumpers
                    if (tok == null || !tok.Validate("bumpers", BinaryFieldType.DATA_VEC3D) || tok.GetCount() != 6)
                        return ParseResult.Fail("Failed to parse energy2maximum/LONG");
                    if (obj != null)
                    {
                        obj.bumpers = new Vector3D[6];
                        for (int i = 0; i < 6; i++)
                            obj.bumpers[i] = tok.GetVector3D(i);
                    }
                }
            }

            if (reader.Format == BZNFormat.BattlezoneN64
            || (reader.Format == BZNFormat.Battlezone && reader.Version > 1027)
            || (reader.Format == BZNFormat.Battlezone2))// && reader.Version >= 1034))
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("abandoned", BinaryFieldType.DATA_LONG))
                    return ParseResult.Fail("Failed to parse abandoned/LONG");
                if (tok.GetCount(BinaryFieldType.DATA_LONG) != 1)
                    return ParseResult.Fail("Failed to parse abandoned/LONG (wrong entry count)"); // vastly improves type auto-detect

                tok.ApplyInt32(obj, x => x.abandoned);
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
                    if (tok == null || !tok.Validate("cloakTransitionTime", BinaryFieldType.DATA_FLOAT))
                        return ParseResult.Fail("Failed to parse cloakTransitionTime/FLOAT");
                    tok.ApplySingle(obj, x => x.cloakTransitionTime, format: reader.FloatFormat);
                }

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("cloakState", BinaryFieldType.DATA_VOID))
                    return ParseResult.Fail("Failed to parse cloakState/VOID");
                tok.ApplyVoidBytes(obj, x => x.cloakState, 0, (v) => BitConverter.ToUInt32(v));

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("cloakTransBeginTime", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse cloakTransBeginTime/FLOAT");
                tok.ApplySingle(obj, x => x.cloakTransBeginTime, format: reader.FloatFormat);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("cloakTransEndTime", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse cloakTransEndTime/FLOAT");
                tok.ApplySingle(obj, x => x.cloakTransEndTime, format: reader.FloatFormat);

                if (reader.Version >= 2002 && obj != null)
                {
                    obj.cloakTransitionTime = obj.cloakTransEndTime - obj.cloakTransBeginTime;
                }
            }

            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version >= 1143)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("curAmmo", BinaryFieldType.DATA_FLOAT))
                        return ParseResult.Fail("Failed to parse curAmmo/FLOAT");
                    //if (obj != null) obj.curAmmo = new DualModeValue<int, float>(tok.GetSingle());
                    tok.ApplySingle(obj, x => x.curAmmo, format: reader.FloatFormat);

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("maxAmmo", BinaryFieldType.DATA_FLOAT))
                        return ParseResult.Fail("Failed to parse maxAmmo/FLOAT");
                    //if (obj != null) obj.maxAmmo = new DualModeValue<int, float>(tok.GetSingle());
                    tok.ApplySingle(obj, x => x.maxAmmo, format: reader.FloatFormat);

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("addAmmo", BinaryFieldType.DATA_FLOAT))
                        return ParseResult.Fail("Failed to parse addAmmo/FLOAT");
                    //if (obj != null) obj.addAmmo = new DualModeValue<int, float>(tok.GetSingle());
                    tok.ApplySingle(obj, x => x.addAmmo, format: reader.FloatFormat);

                    // TODO this entire CurPilot section might be able to use our existing SaveClass logic for both binary and ASCII
                    //if (reader.InBinary)
                    //{
                    //    tok = reader.ReadToken();
                    //    if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_CHAR))
                    //        throw new Exception("Failed to parse ?/CHAR");
                    //    byte curPilotLength = tok.GetUInt8();
                    //
                    //    if (curPilotLength > 0)
                    //    {
                    //        tok = reader.ReadToken();
                    //        if (tok == null || !tok.Validate("curPilot", BinaryFieldType.DATA_CHAR))
                    //            throw new Exception("Failed to parse curPilot/CHAR");
                    //        //if (obj != null) obj.curPilot = tok.GetString();
                    //        tok.ReadChars(obj, x => x.curPilot);
                    //    }
                    //}
                    //else
                    {
                        if (reader.Version == 1145 || reader.Version == 1147 || reader.Version == 1148 || reader.Version == 1149 || reader.Version == 1151 || reader.Version == 1154)
                        {
                            //tok = reader.ReadToken();
                            //if (!tok.Validate("config", BinaryFieldType.DATA_CHAR))
                            //    throw new Exception("Failed to parse curPilot/CHAR");
                            //if (obj != null) obj.curPilot = tok.GetString();

                            //string curPilot = reader.ReadGameObjectClass_BZ2(parent, "config", obj?.Malformations);
                            //if (obj != null) obj.curPilot = curPilot;
                            reader.ReadSizedString("config", obj, x => x.curPilot);
                        }
                        else
                        {
                            //tok = reader.ReadToken();
                            //if (!tok.Validate("curPilot", BinaryFieldType.DATA_CHAR))
                            //    throw new Exception("Failed to parse curPilot/CHAR");
                            //if (obj != null) obj.curPilot = tok.GetString();

                            //string curPilot = reader.ReadGameObjectClass_BZ2(parent, "curPilot", obj?.Malformations);
                            //if (obj != null) obj.curPilot = curPilot;
                            reader.ReadSizedString("curPilot", obj, x => x.curPilot, buffSize: 16);
                        }
                    }

                    if (reader.Version == 1195)
                    {
                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("m_ejectRatio", BinaryFieldType.DATA_FLOAT))
                            return ParseResult.Fail("Failed to parse m_ejectRatio/FLOAT");
                        //if (obj != null) obj.m_ejectRatio = tok.GetSingle();
                        tok.ApplySingle(obj, x => x.m_ejectRatio, format: reader.FloatFormat);
                    }
                    else if (reader.Version >= 1196)
                    {
                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("ejectRatio", BinaryFieldType.DATA_FLOAT))
                            return ParseResult.Fail("Failed to parse ejectRatio/FLOAT");
                        //if (obj != null) obj.m_ejectRatio = tok.GetSingle();
                        tok.ApplySingle(obj, x => x.m_ejectRatio, format: reader.FloatFormat);
                    }
                }
            }

            return ClassGameObject.Hydrate(parent, reader, obj as ClassGameObject);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassCraft obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (writer.Format == BZNFormat.Battlezone && writer.Version < 1019)
            {
                // obsolete
                if (writer.Version > 1001)
                {
                    writer.WriteUInt32("energy0current", obj, x => x.energy0current);
                    writer.WriteUInt32("energy0maximum", obj, x => x.energy0maximum);
                    writer.WriteUInt32("energy1current", obj, x => x.energy1current);
                    writer.WriteUInt32("energy1maximum", obj, x => x.energy1maximum);
                    writer.WriteUInt32("energy2current", obj, x => x.energy2current);
                    writer.WriteUInt32("energy2maximum", obj, x => x.energy2maximum);

                    writer.WriteVector3D("bumpers", obj, x => x.bumpers);
                }
                else
                {
                    writer.WriteVoidBytesRaw("armor",
                                BitConverter.GetBytes(obj.energy0current)
                        .Concat(BitConverter.GetBytes(obj.energy0maximum))
                        .Concat(BitConverter.GetBytes(obj.energy1current))
                        .Concat(BitConverter.GetBytes(obj.energy1maximum))
                        .Concat(BitConverter.GetBytes(obj.energy2current))
                        .Concat(BitConverter.GetBytes(obj.energy2maximum))
                        .ToArray()
                    );

                    writer.WriteVector3D("bumpers", obj, x => x.bumpers);
                }
            }

            if (writer.Format == BZNFormat.BattlezoneN64
            || (writer.Format == BZNFormat.Battlezone && writer.Version > 1027)
            || (writer.Format == BZNFormat.Battlezone2))// && reader.Version >= 1034))
            {
                writer.WriteInt32("abandoned", obj, x => x.abandoned);
            }

            // guesses: omit version 2016, 2011
            if (writer.Format == BZNFormat.Battlezone && writer.Version >= 2000)
            {
                if (writer.Version < 2002)
                {
                    writer.WriteSingle("cloakTransitionTime", obj, x => x.cloakTransitionTime);
                }

                writer.WriteVoidBytes("cloakState", obj, x => x.cloakState);
                writer.WriteSingle("cloakTransBeginTime", obj, x => x.cloakTransBeginTime);
                writer.WriteSingle("cloakTransEndTime", obj, x => x.cloakTransEndTime);
            }

            if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version >= 1143)
                {
                    writer.WriteSingle("curAmmo", obj, x => x.curAmmo);
                    writer.WriteSingle("maxAmmo", obj, x => x.maxAmmo);
                    writer.WriteSingle("addAmmo", obj, x => x.addAmmo);

                    if (writer.Version == 1145 || writer.Version == 1147 || writer.Version == 1148 || writer.Version == 1149 || writer.Version == 1151 || writer.Version == 1154)
                    {
                        writer.WriteSizedString("config", obj, x => x.curPilot);
                    }
                    else
                    {
                        writer.WriteSizedString("curPilot", obj, x => x.curPilot, buffSize: 16);
                    }

                    if (writer.Version == 1195)
                    {
                        writer.WriteSingle("m_ejectRatio", obj, x => x.m_ejectRatio);
                    }
                    else if (writer.Version >= 1196)
                    {
                        writer.WriteSingle("ejectRatio", obj, x => x.m_ejectRatio);
                    }
                }
            }

            ClassGameObject.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
