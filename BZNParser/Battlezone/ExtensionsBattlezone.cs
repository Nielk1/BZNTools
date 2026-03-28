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
                    //tok.ApplyUInt32H8(retVal, x => x.what);
                    tok.ApplyVoidBytes(retVal, x => x.what, 0, (v) => BitConverter.ToUInt32(v), expectedCase: 'L');
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
                    //string tmp = tok.GetString();
                    //if (tmp == string.Empty)
                    //{
                    //    retVal.param = 0;
                    //}
                    //else
                    //{
                    //    //param = tok.GetUInt32();
                    //    byte[] rawBytes = tok.GetRaw(0, -1);
                    //    if (rawBytes.Length > 8)
                    //    {
                    //        // bugged path!
                    //        // Probably not converting these properly
                    //        retVal.Malformations.AddIncorrect("param", rawBytes);
                    //
                    //        string utf8Str = Encoding.UTF8.GetString(rawBytes);
                    //        byte[] newRawBytes = win1252.GetBytes(utf8Str);
                    //        rawBytes = newRawBytes;
                    //    }
                    //    byte[] raw2 = new byte[8];
                    //    Array.Copy(rawBytes, 0, raw2, 0, Math.Min(8, rawBytes.Length));
                    //    retVal.param = BitConverter.ToUInt64(raw2, 0);
                    //}
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
                    writer.WriteVoidBytesL("what", value, x => x.what);
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
                    //writer.WriteIDsBZ1("param", value.param, preserveMalformations ? value.Malformations : null);
                    writer.WriteID("param", value, x => x.param);
                }
                else
                {
                    //writer.WriteUnsignedValues("param", (UInt32)value.param);
                    writer.WriteUInt32("param", value, x => x.param);
                }
            }

            return;
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
