using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Reflection.PortableExecutable;
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

        [Obsolete]
        public static string AddBinaryMessString(this IMalformable.MalformationManager Malformations, string name, string value)
        {
            // TODO make this function usable without a malformation context for if we're not tracking malformations but still need to clean data
            string retVal = value;
            int idx = value.IndexOf('\0');
            if (idx > -1)
            {
                retVal = value.Substring(0, idx);
            }
            if (retVal.Length != value.Length)
            {
                string UnpadOnly = value.TrimEnd('\0');
                if (UnpadOnly == retVal)
                {
                    // we're only padded, so store it as needing a pad
                    Malformations.AddStringPad(name, value.Length);
                }
                else
                {
                    // it's more than just a pad, so store the old value
                    Malformations.AddIncorrect(name, value);
                }
            }
            return retVal;
        }

        public static string CheckBinaryMessString(this IMalformable.MalformationManager Malformations, string name, string value)
        {
            //            var mal = Malformations.GetMalformations(Malformation.STRING_PAD, name);
            //            var mal2 = Malformations.GetMalformations(Malformation.INCORRECT_RAW, name);
            //            if (mal.Length > 0)
            //            {
            //                return value.PadRight((int)mal[0].Fields[0], '\0');
            //            }
            //            else if (mal2.Length > 0)
            //            {
            //                return (string)mal2[0].Fields[0];
            //            }
            //            else
            {
                return value;
            }
        }

        [Obsolete]
        public static uint ReadBZ1_PtrDepricated(this BZNStreamReader reader, string name)
        {
            IBZNToken? tok;

            if (reader.InBinary)
            {
                // untested
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate(name, BinaryFieldType.DATA_VOID))
                    throw new Exception($"Failed to parse {name ?? "???"}/PTR");
                return tok.GetUInt32H();
            }
            else
            {
                // untested
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate(name, BinaryFieldType.DATA_VOID))
                    throw new Exception($"Failed to parse {name ?? "???"}/PTR");
                //return tok.GetUInt32H();
                return tok.GetUInt32Raw(); // might be only version 1001 of BZ1
            }
        }
        [Obsolete]
        public static void WriteBZ1_PtrDepricated(this BZNStreamWriter writer, string name, uint value, bool raw = false)
        {
            if (raw)
            {
                writer.WriteVoidBytesRaw(name, BitConverter.GetBytes(value).ToArray()); // not sure if this is right, probably wrong for binary and god knows what it does to BigEndian
                return;
            }

            // untested
            writer.WriteVoidBytes(name, BitConverter.GetBytes(value).Reverse().ToArray()); // not sure if this is right, probably wrong for binary and god knows what it does to BigEndian
        }
        [Obsolete]
        public static UInt64 ReadBZ1_Ptr(this BZNStreamReader reader, string name, int version)
        {
            IBZNToken tok;
            tok = reader.ReadToken();
            if (tok == null || !tok.Validate(name, BinaryFieldType.DATA_PTR))
                throw new Exception($"Failed to parse {name ?? "???"}/PTR");
            if (version < 2012)
                return (UInt64)tok.GetUInt32H();
            return tok.GetUInt64H();
        }
        [Obsolete]
        public static void WriteBZ1_Ptr(this BZNStreamWriter writer, string name, UInt64 value)
        {
            if (writer.Version < 2012)
            {
                writer.WritePtr32(name, (UInt32)value);
            }
            else
            {
                writer.WritePtr64(name, value);
            }
        }

        [Obsolete]
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

        [Obsolete]
        public static void WriteCompressedNumberFromBinary(this BZNStreamWriter writer, uint value)
        {
            if (value <= byte.MaxValue)
            {
                writer.WriteUnsignedValues(null, (byte)value);
            }
            else if (value <= ushort.MaxValue)
            {
                writer.WriteUnsignedValues(null, (ushort)value);
            }
            else
            {
                writer.WriteUnsignedValues(null, value);
            }
        }
        [Obsolete("it's just read sized string again")]
        public static string? ReadBZ2InputString(this BZNStreamReader reader, string name, IMalformable.MalformationManager? malformations)
        {
            IBZNToken tok;
            if (reader.InBinary)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse ?/CHAR");
                uint length = tok.GetUInt8();

                if (length > 0)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate(name, BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse name/CHAR");
                    return tok.GetString();
                }
                return null;
            }

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate(name, BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse name/CHAR");
            if (malformations != null)
            {
                var tmp = tok as BZNTokenString;
                if (tmp != null)
                {
                    //                    if (tmp.RightTrimmedOneLiner)
                    //                        malformations.AddRightTrimmed(name);
                }
            }
            return tok.GetString();
        }
        [Obsolete("it's just read sized string again")]
        public static string? ReadBZ2StringInSized(this BZNStreamReader reader, string name, int bufferSize)
        {
            IBZNToken tok;
            if (reader.InBinary)
            {
                uint length = reader.ReadCompressedNumberFromBinary();

                if (length > 0)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse name/CHAR");
                    return tok.GetString();
                }
                return null;
            }

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate(name, BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse name/CHAR");
            return tok.GetString();
        }

        [Obsolete]
        public static string? ReadGameObjectClass_BZ2(this BZNStreamReader reader, BZNFileBattlezone parent, string name, IMalformable.MalformationManager? malformations, [System.Runtime.CompilerServices.CallerFilePath] string callerFile = "")
        {
            if (reader.Version < 1145)
            {
                return reader.ReadSizedString_BZ2_1145(name, 16, malformations);
            }
            else
            {
                if (parent.SaveType == SaveType.LOCKSTEP)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    return reader.ReadBZ2InputString(name, malformations);
                }
            }
        }

        [Obsolete]
        public static void WriteGameObjectClass_BZ2(this BZNStreamWriter writer, BZNFileBattlezone parent, string name, string value, IMalformable.MalformationManager malformations, [System.Runtime.CompilerServices.CallerFilePath] string callerFile = "")
        {
            if (writer.Version < 1145)
            {
                writer.WriteSizedString_BZ2_1145(name, 16, value, malformations);
            }
            else
            {
                if (parent.SaveType == SaveType.LOCKSTEP)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    writer.WriteBZ2InputString(name, value, malformations);
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
        [Obsolete]
        public static byte ReadBytePossibleRawPossibleSigned_BZ2(this BZNStreamReader reader, string name)
        {
            IBZNToken tok = reader.ReadToken();
            if (tok == null || !tok.Validate(name, BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse saveFlags/CHAR");
            if (reader.Version >= 1187)
            {
                return tok.GetUInt8();
            }
            else
            {
                return tok.GetRaw(0, 1)[0];
            }
        }

        [Obsolete]
        public static void WriteBytePossibleRawPossibleSigned_BZ2(this BZNStreamWriter writer, string name, byte value)
        {
            if (writer.Version >= 1187)
            {
                writer.WriteUnsignedValues(name, value);
                return;
            }
            else
            {
                if (!writer.InBinary)
                {
                    writer.WriteRaw(name, new byte[] { value });
                    //return tok.GetRaw(0, 1)[0];
                }
                else
                {
                    writer.WriteUnsignedValues(name, value);
                }
                return;
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
        [Obsolete("reader.Version <= 1128 ReadChars, else ReadSizedString")]
        public static string? ReadSizedString_BZ2_1145(this BZNStreamReader reader, string name, int bufferSize, IMalformable.MalformationManager? malformations)// = null)
        {
            if (reader.Format != BZNFormat.Battlezone2)
                throw new NotImplementedException();

            IBZNToken tok;

            // <= 1128 - normal chars write
            // < 1145 - compressed size then, if >0 chars
            // else - compressed size then, if >0 chars
            // Raw <= 1128      Size Prefix < 1145            size prefix
            // Raw <= 1128      Size Prefix

            if (reader.Version <= 1128) // <= 1128 <= 1124 <= 1112 <= 1108 <= 1105?  < 1103
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate(name, BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse name/CHAR");
                string retVal = tok.GetString();
                if (malformations != null)
                {
                    retVal = malformations.AddBinaryMessString(name, retVal);
                }
                return retVal;
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
                return reader.ReadBZ2InputString(name, malformations);
            }
        }

        [Obsolete]
        public static void WriteSizedString_BZ2_1145(this BZNStreamWriter writer, string name, int bufferSize, string value, IMalformable.MalformationManager malformations)
        {
            if (writer.Format != BZNFormat.Battlezone2)
                throw new NotImplementedException();

            if (writer.Version <= 1128) // <= 1128 <= 1124 <= 1112 <= 1108 <= 1105?  < 1103
            {
                if (writer.Version == 1101)
                {
                    // this might be pointless now that we have malformations handling it
                    writer.WriteChars(name, value.PadRight(Math.Max(bufferSize, value.Length), '\0'), malformations);
                }
                else
                {
                    writer.WriteChars(name, value, malformations);
                }
            }
            else if (writer.Version < 1145)
            {
                // bufferSize applies in this branch
                // in
                writer.WriteBZ2StringInSized(name, bufferSize, value, malformations);
            }
            else
            {
                // inputstring
                writer.WriteBZ2InputString(name, value, malformations);
            }
        }

        [Obsolete]
        public static void WriteBZ2StringInSized(this BZNStreamWriter writer, string name, string value, IMalformable.MalformationManager malformations)
        {
            if (writer.InBinary)
            {
                writer.WriteUnsignedValues(null, (byte)value.Length);

                if (value.Length > 0)
                    writer.WriteChars(null, value, malformations);
                return;
            }
            writer.WriteChars(name, value, malformations);
        }
        [Obsolete]
        public static void WriteBZ2StringInSized(this BZNStreamWriter writer, string name, int bufferSize, string value, IMalformable.MalformationManager malformations)
        {
            if (writer.InBinary)
            {
                writer.WriteCompressedNumberFromBinary((uint)value.Length);

                if (value.Length > 0)
                    writer.WriteChars(null, value, malformations);
                return;
            }
            writer.WriteChars(name, value, malformations);
        }
        [Obsolete]
        public static void WriteBZ2InputString(this BZNStreamWriter writer, string name, int bufferSize, string value, IMalformable.MalformationManager malformations)
        {
            if (writer.InBinary)
            {
                writer.WriteCompressedNumberFromBinary((uint)value.Length);

                if (value.Length > 0)
                    writer.WriteChars(name, value, malformations);
                return;
            }
            writer.WriteChars(name, value, malformations);
        }

        [Obsolete]
        public static void WriteBZ2InputString(this BZNStreamWriter writer, string name, string value, IMalformable.MalformationManager malformations)
        {
            if (writer.InBinary)
            {
                writer.WriteUnsignedValues(null, (byte)value.Length);

                if (value.Length > 0)
                {
                    writer.WriteChars(null, value, malformations);
                }
                return;
            }

            writer.WriteChars(name, value, malformations);
        }

        public static AiCmdInfo GetAiCmdInfo(this BZNStreamReader reader)
        {
            AiCmdInfo retVal = new AiCmdInfo();

            IBZNToken? tok = reader.ReadToken();
            if (tok == null || !tok.Validate("priority", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse priority/LONG");
            //retVal.priority = tok.GetUInt32();
            tok.ApplyInt32(retVal, x => x.priority);

            tok = reader.ReadToken();
            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                if (tok == null || !tok.Validate("what", BinaryFieldType.DATA_VOID)) throw new Exception("Failed to parse what/VOID");
                if (reader.Version == 1001)
                {
                    //retVal.what = tok.GetUInt32Raw();
                    //tok.ApplyVoidBytesRaw(retVal, x => x.what);
                    tok.ApplyVoidBytesRaw(retVal, x => x.what, 0, (v) => BitConverter.ToUInt32(v));
                }
                else
                {
                    //retVal.what = tok.GetUInt32HR();
                    tok.ApplyUInt32H8(retVal, x => x.what);
                }
            }
            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version < 1145)
                {
                    if (tok == null || !tok.Validate("what", BinaryFieldType.DATA_VOID)) throw new Exception("Failed to parse what/VOID");
                    tok.ApplyVoidBytes(retVal, x => x.what, 0, (v) => BitConverter.ToUInt32(v));
                }
                else
                {
                    if (tok == null || !tok.Validate("what", BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse what/CHAR");
                    //tok.ReadUInt8h(retVal, x => x.what);
                    if (reader.InBinary)
                    {
                        //retVal.what = tok.GetUInt8(); // 1 byte in binary
                        tok.ApplyUInt8(retVal, x => x.what);
                    }
                    else
                    {
                        //retVal.what = tok.GetUInt32HR(); // 4 byte raw binary lowercase
                        tok.ApplyVoidBytes(retVal, x => x.what, 0, (v) => BitConverter.ToUInt32(v));
                    }
                }
                //if (reader.InBinary)
                //{
                //    retVal.what = tok.GetUInt8();
                //}
                //else
                //{
                //    //retVal.what = tok.GetUInt32H();
                //    retVal.what = tok.GetUInt32HR();
                //}
            }

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("who", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse who/LONG");
            //retVal.who = tok.GetInt32();
            tok.ApplyInt32(retVal, x => x.who);

            //if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (reader.Format == BZNFormat.Battlezone && (reader.Version == 1001 || reader.Version == 1011 || reader.Version == 1012))
                {
                    if (tok == null || !tok.Validate("undefptr", BinaryFieldType.DATA_PTR)) throw new Exception("Failed to parse undefptr/PTR");
                }
                else
                {
                    if (tok == null || !tok.Validate("where", BinaryFieldType.DATA_PTR)) throw new Exception("Failed to parse where/PTR");
                }
                //retVal.where = tok.GetUInt32H();
                tok.ApplyUInt32H8(retVal, x => x.where);

                tok = reader.ReadToken();
                //if (reader.Format == BZNFormat.Battlezone && reader.Version >= 2016)
                if (reader.Format == BZNFormat.Battlezone && reader.Version >= 2012)
                {
                    if (tok == null || !tok.Validate("param", BinaryFieldType.DATA_ID)) throw new Exception("Failed to parse param/ID");
                    tok.ApplyID(retVal, x => x.param);
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
                            retVal.Malformations.AddIncorrect("param", rawBytes);

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
                    if (tok == null || !tok.Validate("param", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse param/LONG");
                    //retVal.param = tok.GetUInt32();
                    tok.ApplyUInt32(retVal, x => x.param);
                }
            }

            return retVal;
        }

        // Need to rewrap into other code logic later
        public static void WriteAiCmdInfo(this BZNStreamWriter writer, AiCmdInfo value, bool preserveMalformations)
        {
            //writer.WriteUnsignedValues("priority", value.priority);
            writer.WriteInt32("priority", value, x => x.priority);

            if (writer.Format == BZNFormat.Battlezone || writer.Format == BZNFormat.BattlezoneN64)
            {
                // can't write what we don't know how to read, breakpoint until we fix that>	BZNParser.dll!BZNParser.Battlezone.ExtensionsBattlezone.GetAiCmdInfo(BZNParser.Tokenizer.BZNStreamReader reader) Line 470	C#

                //writer.WriteVoidBytes("what", new byte[1] { (byte)value.what });
                if (writer.Version == 1001)
                {
                    writer.WriteVoidBytesRaw("what", value, x => x.what);
                }
                else
                {
                    writer.WriteVoidBytes("what", value, x => x.what);
                }
            }
            if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version < 1145)
                {
                    writer.WriteVoidBytesL("what", value, x => x.what);
                }
                else
                {
                    if (writer.InBinary)
                    {
                        //writer.WriteUnsignedValues("what", (byte)value.what);
                        writer.WriteUInt8h("what", value, x => x.what);
                    }
                    else
                    {
                        writer.WriteVoidBytesL("what", value, x => x.what); // 1 liner 32bit number like VoidBytes but lowercase
                    }
                }
            }

            //writer.WriteSignedValues("who", value.who);
            writer.WriteInt32("who", value, x => x.who);

            //if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                if (writer.Format == BZNFormat.Battlezone && (writer.Version == 1001 || writer.Version == 1011 || writer.Version == 1012))
                {
                    writer.WritePtr("undefptr", value, x => x.where);
                }
                else
                {
                    writer.WritePtr("where", value, x => x.where);
                }

                //if (reader.Format == BZNFormat.Battlezone && reader.Version >= 2016)
                if (writer.Format == BZNFormat.Battlezone && writer.Version >= 2012)
                {
                    writer.WriteIDsBZ1("param", value.param, preserveMalformations ? value.Malformations : null);
                    //writer.WriteID("param", value, x => x.param);
                }
                else
                {
                    //writer.WriteUnsignedValues("param", (UInt32)value.param);
                    writer.WriteUInt32("param", value, x => x.param);
                }
            }

            return;
        }

        [Obsolete]
        public static Euler GetEuler(this BZNStreamReader reader, SaveType saveType)
        {
            if (reader.Format != BZNFormat.Battlezone2 || saveType == SaveType.BZN) // Battlezone 2 has side paths
            {
                if (reader.InBinary)
                {
                    IBZNToken tok = reader.ReadToken();
                    if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse euler's FLOAT");
                    float euler_mass = tok.GetSingle();

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse euler's FLOAT");
                    float euler_mass_inv = tok.GetSingle();

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse euler's FLOAT");
                    float euler_v_mag = tok.GetSingle();

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse euler's FLOAT");
                    float euler_v_mag_inv = tok.GetSingle();

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse euler's FLOAT");
                    float euler_I = tok.GetSingle();

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse euler's FLOAT");
                    float euler_k_i = tok.GetSingle();

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_VEC3D)) throw new Exception("Failed to parse euler's VEC3D");
                    Vector3D euler_v = tok.GetVector3D();
                    tok.CheckMalformationsVector3D(euler_v.Malformations, reader.FloatFormat);

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_VEC3D)) throw new Exception("Failed to parse euler's VEC3D");
                    Vector3D euler_omega = tok.GetVector3D();
                    tok.CheckMalformationsVector3D(euler_omega.Malformations, reader.FloatFormat);

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_VEC3D)) throw new Exception("Failed to parse euler's VEC3D");
                    Vector3D euler_Accel = tok.GetVector3D();
                    tok.CheckMalformationsVector3D(euler_Accel.Malformations, reader.FloatFormat);

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
                    if (tok == null || !tok.Validate("euler")) throw new Exception("Failed to parse euler");
                    
                    Euler euler = tok.GetEuler();
                    tok.CheckMalformationsEuler(euler, reader.FloatFormat);
                    
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
                    if (tok == null || !tok.Validate("mass", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse euler's FLOAT");
                    float euler_mass = tok.GetSingle();

                    //float euler_mass_inv = tok.GetSingle();
                    //float euler_v_mag = tok.GetSingle();
                    //float euler_v_mag_inv = tok.GetSingle();

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("I", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse euler's FLOAT");
                    float euler_I = tok.GetSingle();

                    //float euler_k_i = tok.GetSingle();

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("small", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse euler's small BOOL");
                    bool canCompress = tok.GetBoolean();

                    Euler euler = new Euler()
                    {
                        mass = euler_mass,
                        I = euler_I,
                    };

                    if (!canCompress)
                    {
                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("v", BinaryFieldType.DATA_VEC3D)) throw new Exception("Failed to parse euler's VEC3D");
                        euler.v = tok.GetVector3D();

                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("omega", BinaryFieldType.DATA_VEC3D)) throw new Exception("Failed to parse euler's VEC3D");
                        euler.omega = tok.GetVector3D();

                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("Accel", BinaryFieldType.DATA_VEC3D)) throw new Exception("Failed to parse euler's VEC3D");
                        euler.Accel = tok.GetVector3D();

                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("Alpha", BinaryFieldType.DATA_VEC3D)) throw new Exception("Failed to parse euler's VEC3D");
                        euler.Alpha = tok.GetVector3D();
                    }
                    else
                    {
                        euler.InitLoadSave();
                    }

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("Pos", BinaryFieldType.DATA_VEC3D)) throw new Exception("Failed to parse euler's VEC3D");
                    Vector3D euler_Pos = tok.GetVector3D();

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("Att", BinaryFieldType.DATA_QUAT)) throw new Exception("Failed to parse euler's QUAT");
                    
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

        [Obsolete]
        public static void WriteEulerBZ(this BZNStreamWriter writer, SaveType saveType, bool preserveMalformations, Euler value)
        {
            if (writer.Format != BZNFormat.Battlezone2 || saveType == SaveType.BZN) // Battlezone 2 has side paths
            {
                if (writer.InBinary)
                {
                    writer.WriteFloats(null, null, value.mass);
                    writer.WriteFloats(null, null, value.mass_inv);
                    writer.WriteFloats(null, null, value.v_mag);
                    writer.WriteFloats(null, null, value.v_mag_inv);
                    writer.WriteFloats(null, null, value.I);
                    writer.WriteFloats(null, null, value.I_inv);
                    writer.WriteVector3Ds(null, preserveMalformations, value.v);
                    writer.WriteVector3Ds(null, preserveMalformations, value.omega);
                    writer.WriteVector3Ds(null, preserveMalformations, value.Accel);

                    return;
                }
                else
                {
                    writer.WriteEuler("euler", preserveMalformations, value);

                    return;
                }
            }
            else if (writer.Format == BZNFormat.Battlezone2 && writer.Version < 1145)
            {
                // byte buffer as void*
                throw new NotImplementedException("Version <1145 Euler Save");
            }
            else
            {
                if (writer.InBinary)
                {
                    writer.WriteFloats("mass", preserveMalformations ? value.Malformations : null, value.mass);

                    //float euler_mass_inv = tok.GetSingle();
                    //float euler_v_mag = tok.GetSingle();
                    //float euler_v_mag_inv = tok.GetSingle();

                    writer.WriteFloats("I", preserveMalformations ? value.Malformations : null, value.I);

                    //float euler_k_i = tok.GetSingle();

                    bool canCompress = true;
                    if (canCompress && value.v.Magnitude() != 0) canCompress = false;
                    if (canCompress && value.omega.Magnitude() != 0) canCompress = false;
                    if (canCompress && value.Accel.Magnitude() != 0) canCompress = false;
                    if (canCompress && value.Alpha.Magnitude() != 0) canCompress = false;

                    if (!canCompress)
                    {
                        writer.WriteVector3Ds(" v", preserveMalformations, value.v);
                        writer.WriteVector3Ds(" omega", preserveMalformations, value.omega);
                        writer.WriteVector3Ds(" Accel", preserveMalformations, value.Accel);
                        writer.WriteVector3Ds(" Alpha", preserveMalformations, value.Alpha);
                    }
                    else
                    {
                        writer.WriteBooleans(" small", preserveMalformations ? value.Malformations : null, true);
                    }

                    writer.WriteVector3Ds(" Pos", preserveMalformations, value.Pos);
                    throw new NotImplementedException("Euler Save");
                    //writer.WriteQuat(" Att", value.Att); // no Quat saving written yet
                }
            }
            throw new NotImplementedException("Euler Save");
        }
    }

    public class AiCmdInfo : IMalformable
    {
        public int priority { get; set; }
        public uint what { get; set; } // AiCommand (not sure BZ2 vs BZ1)
        public int who { get; set; }
        public uint where { get; set; } // AiPath*
        public ulong param { get; set; } // long, not unsigned but, whatever


        private readonly IMalformable.MalformationManager _malformationManager;
        public IMalformable.MalformationManager Malformations => _malformationManager;

        public AiCmdInfo()
        {
            _malformationManager = new IMalformable.MalformationManager(this);
        }
    }
}
