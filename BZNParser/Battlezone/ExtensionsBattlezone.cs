using BZNParser.Reader;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BZNParser.Battlezone
{
    static class ExtensionsBattlezone
    {
        private static Encoding win1252;
        static ExtensionsBattlezone()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            win1252 = Encoding.GetEncoding(1252);
        }

        public static uint ReadBZ1_PtrDepricated(this BZNStreamReader reader, string name)
        {
            IBZNToken tok;

            if (reader.InBinary)
            {
                // untested
                tok = reader.ReadToken();
                if (!tok.Validate(name, BinaryFieldType.DATA_VOID))
                    throw new Exception($"Failed to parse {name ?? "???"}/PTR");
                return tok.GetUInt32H();
            }
            else
            {
                // untested
                tok = reader.ReadToken();
                if (!tok.Validate(name, BinaryFieldType.DATA_VOID))
                    throw new Exception($"Failed to parse {name ?? "???"}/PTR");
                //return tok.GetUInt32H();
                return tok.GetUInt32Raw(); // might be only version 1001 of BZ1
            }
        }
        public static uint ReadBZ1_Ptr(this BZNStreamReader reader, string name)
        {
            IBZNToken tok;
            
            tok = reader.ReadToken();
            if (!tok.Validate(name, BinaryFieldType.DATA_PTR))
                throw new Exception($"Failed to parse {name ?? "???"}/PTR");
            return tok.GetUInt32H();
        }

        public static uint ReadCompressedNumberFromBinary(this BZNStreamReader reader)
        {
            IBZNToken tok;

            tok = reader.ReadToken();
            if (tok.Validate(null, BinaryFieldType.DATA_LONG))
            {
                return tok.GetUInt32();
            }
            else if (tok.Validate(null, BinaryFieldType.DATA_SHORT))
            {
                return tok.GetUInt16();
            }
            else if (tok.Validate(null, BinaryFieldType.DATA_CHAR))
            {
                return tok.GetUInt8();
            }
            else
            {
                throw new Exception("Failed to parse LONG/SHORT/CHAR");
            }
        }
        public static string? ReadBZ2InputString(this BZNStreamReader reader, string name)
        {
            IBZNToken tok;
            if (reader.InBinary)
            {
                tok = reader.ReadToken();
                if (!tok.Validate(null, BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse ?/CHAR");
                uint length = tok.GetUInt8();

                if (length > 0)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate(name, BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse name/CHAR");
                    return tok.GetString();
                }
                return null;
            }

            tok = reader.ReadToken();
            if (!tok.Validate(name, BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse name/CHAR");
            return tok.GetString();
        }
        public static string? ReadBZ2StringInSized(this BZNStreamReader reader, string name, int bufferSize)
        {
            IBZNToken tok;
            if (reader.InBinary)
            {
                uint length = reader.ReadCompressedNumberFromBinary();

                if (length > 0)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate(name, BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse name/CHAR");
                    return tok.GetString();
                }
                return null;
            }

            tok = reader.ReadToken();
            if (!tok.Validate(name, BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse name/CHAR");
            return tok.GetString();
        }
        
        public static string? ReadGameObjectClass_BZ2(this BZNStreamReader reader, BZNFileBattlezone parent, string name, [System.Runtime.CompilerServices.CallerFilePath] string callerFile = "")
        {
            if (reader.Version < 1145)
            {
                return reader.ReadSizedString_BZ2_1145(name, 16);
            }
            else
            {
                if (parent.SaveType == SaveType.LOCKSTEP)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    return reader.ReadBZ2InputString(name);
                }
            }
        }

        /// <summary>
        /// Read a byte from the BZN which in ASCII mode might be stored as signed byte or raw bytes in the ASCII stream
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static byte ReadBytePossibleRawPossibleSigned_BZ2(this BZNStreamReader reader, string name)
        {
            IBZNToken tok = reader.ReadToken();
            if (!tok.Validate(name, BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse saveFlags/CHAR");
            if (reader.Version >= 1187)
            {
                return tok.GetUInt8();
            }
            else
            {
                return tok.GetRaw(0, 1)[0];
            }
        }

        /// <summary>
        /// Read a string with size, handles versions < 1145 as using bufferSize and >= 1145 as unbounded
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="name"></param>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="Exception"></exception>
        public static string? ReadSizedString_BZ2_1145(this BZNStreamReader reader, string name, int bufferSize)
        {
            if (reader.Format != BZNFormat.Battlezone2)
                throw new NotImplementedException();

            IBZNToken tok;

            if (reader.Version <= 1128) // <= 1128 <= 1124 <= 1112 <= 1108 <= 1105?  < 1103
            {
                tok = reader.ReadToken();
                if (!tok.Validate(name, BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse name/CHAR");
                return tok.GetString();
            }
            else if (reader.Version < 1145)
            {
                // bufferSize applies in this branch
                // in
                return reader.ReadBZ2StringInSized(name, bufferSize);
            }
            else
            {
                // inputstring
                return reader.ReadBZ2InputString(name);
            }
        }
        public static AiCmdInfo GetAiCmdInfo(this BZNStreamReader reader)
        {
            AiCmdInfo retVal = new AiCmdInfo();

            IBZNToken tok = reader.ReadToken();
            if (!tok.Validate("priority", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse priority/LONG");
            retVal.priority = tok.GetUInt32();

            tok = reader.ReadToken();
            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
                if (!tok.Validate("what", BinaryFieldType.DATA_VOID)) throw new Exception("Failed to parse what/VOID");
            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version < 1145)
                {
                    if (!tok.Validate("what", BinaryFieldType.DATA_VOID)) throw new Exception("Failed to parse what/VOID");
                }
                else
                {
                    if (!tok.Validate("what", BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse what/CHAR");
                }
                if (reader.InBinary)
                {
                    retVal.what = tok.GetUInt8();
                }
                else
                {
                    retVal.what = tok.GetUInt32H();
                }
            }

            tok = reader.ReadToken();
            if (!tok.Validate("who", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse who/LONG");
            retVal.who = tok.GetInt32();

            //if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (reader.Format == BZNFormat.Battlezone && (reader.Version == 1001 || reader.Version == 1011 || reader.Version == 1012))
                {
                    if (!tok.Validate("undefptr", BinaryFieldType.DATA_PTR)) throw new Exception("Failed to parse undefptr/PTR");
                }
                else
                {
                    if (!tok.Validate("where", BinaryFieldType.DATA_PTR)) throw new Exception("Failed to parse where/PTR");
                }
                retVal.where = tok.GetUInt32H();

                tok = reader.ReadToken();
                //if (reader.Format == BZNFormat.Battlezone && reader.Version >= 2016)
                if (reader.Format == BZNFormat.Battlezone && reader.Version >= 2012)
                {
                    if (!tok.Validate("param", BinaryFieldType.DATA_ID)) throw new Exception("Failed to parse param/ID");
                    string tmp = tok.GetString();
                    if (tmp == string.Empty)
                    {
                        retVal.param = 0;
                    }
                    else
                    {
                        //param = tok.GetUInt32();
                        byte[] rawBytes = tok.GetRaw(0, -1);
                        if (rawBytes.Length > 8)
                        {
                            // bugged path!
                            // Probably not converting these properly
                            retVal.Malformations.Add(Malformation.INCORRECT, "param", rawBytes);

                            string utf8Str = Encoding.UTF8.GetString(rawBytes);
                            byte[] newRawBytes = win1252.GetBytes(utf8Str);
                            rawBytes = newRawBytes;
                        }
                        byte[] raw2 = new byte[8];
                        Array.Copy(rawBytes, 0, raw2, 0, Math.Min(8, rawBytes.Length));
                        retVal.param = BitConverter.ToUInt64(raw2, 0);
                    }
                }
                else
                {
                    if (!tok.Validate("param", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse param/LONG");
                    retVal.param = tok.GetUInt32();
                }
            }

            return retVal;
        }

        public static Euler GetEuler(this BZNStreamReader reader, SaveType saveType)
        {
            if (reader.Format != BZNFormat.Battlezone2 || saveType == SaveType.BZN) // Battlezone 2 has side paths
            {
                if (reader.InBinary)
                {
                    IBZNToken tok = reader.ReadToken();
                    if (!tok.Validate(null, BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse euler's FLOAT");
                    float euler_mass = tok.GetSingle();

                    tok = reader.ReadToken();
                    if (!tok.Validate(null, BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse euler's FLOAT");
                    float euler_mass_inv = tok.GetSingle();

                    tok = reader.ReadToken();
                    if (!tok.Validate(null, BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse euler's FLOAT");
                    float euler_v_mag = tok.GetSingle();

                    tok = reader.ReadToken();
                    if (!tok.Validate(null, BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse euler's FLOAT");
                    float euler_v_mag_inv = tok.GetSingle();

                    tok = reader.ReadToken();
                    if (!tok.Validate(null, BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse euler's FLOAT");
                    float euler_I = tok.GetSingle();

                    tok = reader.ReadToken();
                    if (!tok.Validate(null, BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse euler's FLOAT");
                    float euler_k_i = tok.GetSingle();

                    tok = reader.ReadToken();
                    if (!tok.Validate(null, BinaryFieldType.DATA_VEC3D)) throw new Exception("Failed to parse euler's VEC3D");
                    Vector3D euler_v = tok.GetVector3D();

                    tok = reader.ReadToken();
                    if (!tok.Validate(null, BinaryFieldType.DATA_VEC3D)) throw new Exception("Failed to parse euler's VEC3D");
                    Vector3D euler_omega = tok.GetVector3D();

                    tok = reader.ReadToken();
                    if (!tok.Validate(null, BinaryFieldType.DATA_VEC3D)) throw new Exception("Failed to parse euler's VEC3D");
                    Vector3D euler_Accel = tok.GetVector3D();

                    Euler euler = new Euler()
                    {
                        mass = euler_mass,
                        mass_inv = euler_mass_inv,
                        v_mag = euler_v_mag,
                        v_mag_inv = euler_v_mag_inv,
                        I = euler_I,
                        I_inv = euler_k_i,
                        v = euler_v,
                        omega = euler_omega,
                        Accel = euler_Accel
                    };

                    return euler;
                }
                else
                {
                    IBZNToken tok = reader.ReadToken();
                    if (!tok.Validate("euler")) throw new Exception("Failed to parse euler");
                    Euler euler = tok.GetEuler();

                    return euler;
                }
            }
            else if (reader.Format == BZNFormat.Battlezone2 && reader.Version < 1145)
            {
                // byte buffer as void*
                throw new NotImplementedException("Version <1145 Euler Save");
            }
            else
            {
                if (reader.InBinary)
                {
                    IBZNToken tok = reader.ReadToken();
                    if (!tok.Validate("mass", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse euler's FLOAT");
                    float euler_mass = tok.GetSingle();

                    //float euler_mass_inv = tok.GetSingle();
                    //float euler_v_mag = tok.GetSingle();
                    //float euler_v_mag_inv = tok.GetSingle();

                    tok = reader.ReadToken();
                    if (!tok.Validate("I", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse euler's FLOAT");
                    float euler_I = tok.GetSingle();

                    //float euler_k_i = tok.GetSingle();

                    tok = reader.ReadToken();
                    if (!tok.Validate("small", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse euler's small BOOL");
                    bool canCompress = tok.GetBoolean();

                    Euler euler = new Euler()
                    {
                        mass = euler_mass,
                        I = euler_I,
                    };

                    if (!canCompress)
                    {
                        tok = reader.ReadToken();
                        if (!tok.Validate("v", BinaryFieldType.DATA_VEC3D)) throw new Exception("Failed to parse euler's VEC3D");
                        euler.v = tok.GetVector3D();

                        tok = reader.ReadToken();
                        if (!tok.Validate("omega", BinaryFieldType.DATA_VEC3D)) throw new Exception("Failed to parse euler's VEC3D");
                        euler.omega = tok.GetVector3D();

                        tok = reader.ReadToken();
                        if (!tok.Validate("Accel", BinaryFieldType.DATA_VEC3D)) throw new Exception("Failed to parse euler's VEC3D");
                        euler.Accel = tok.GetVector3D();

                        tok = reader.ReadToken();
                        if (!tok.Validate("Alpha", BinaryFieldType.DATA_VEC3D)) throw new Exception("Failed to parse euler's VEC3D");
                        euler.Alpha = tok.GetVector3D();
                    }
                    else
                    {
                        euler.InitLoadSave();
                    }

                    tok = reader.ReadToken();
                    if (!tok.Validate("Pos", BinaryFieldType.DATA_VEC3D)) throw new Exception("Failed to parse euler's VEC3D");
                    Vector3D euler_Pos = tok.GetVector3D();

                    tok = reader.ReadToken();
                    if (!tok.Validate("Att", BinaryFieldType.DATA_QUAT)) throw new Exception("Failed to parse euler's QUAT");
                    
                    throw new NotImplementedException("Euler Save");
                    //Quaternion euler_Att = tok.GetQuaternion();

                    //euler.Pos = euler_Pos;
                    //euler.Att = euler_Att;

                    // And, reconstruct unsaved params now
                    //euler.CalcMassIInv();
                    //euler.CalcVMag();

                    //return euler;
                }
            }
            throw new NotImplementedException("Euler Save");
        }
    }

    public class AiCmdInfo : IMalformable
    {
        public uint priority { get; set; }
        public uint what { get; set; }
        public int who { get; set; }
        public uint where { get; set; }
        public ulong param { get; set; }


        private readonly IMalformable.MalformationManager _malformationManager;
        public IMalformable.MalformationManager Malformations => _malformationManager;

        public AiCmdInfo()
        {
            _malformationManager = new IMalformable.MalformationManager(this);
        }
    }
}
