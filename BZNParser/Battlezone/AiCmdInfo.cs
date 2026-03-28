using BZNParser.Tokenizer;

namespace BZNParser.Battlezone
{
    static class AiCmdInfoExtensions
    {
        public static AiCmdInfo GetAiCmdInfo(this BZNStreamReader reader)
        {
            AiCmdInfo retVal = new AiCmdInfo();

            IBZNToken? tok = reader.ReadToken();
            if (tok == null || !tok.Validate("priority", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse priority/LONG");
            tok.ApplyInt32(retVal, x => x.priority);

            tok = reader.ReadToken();
            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                if (tok == null || !tok.Validate("what", BinaryFieldType.DATA_VOID)) throw new Exception("Failed to parse what/VOID");
                if (reader.Version == 1001)
                {
                    tok.ApplyVoidBytesRaw(retVal, x => x.what, 0, (v) => BitConverter.ToUInt32(v));
                }
                else
                {
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
                    if (reader.InBinary)
                    {
                        tok.ApplyUInt8(retVal, x => x.what);
                    }
                    else
                    {
                        tok.ApplyVoidBytes(retVal, x => x.what, 0, (v) => BitConverter.ToUInt32(v));
                    }
                }
            }

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("who", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse who/LONG");
            tok.ApplyInt32(retVal, x => x.who);

            tok = reader.ReadToken();
            if (reader.Format == BZNFormat.Battlezone && (reader.Version == 1001 || reader.Version == 1011 || reader.Version == 1012))
            {
                if (tok == null || !tok.Validate("undefptr", BinaryFieldType.DATA_PTR)) throw new Exception("Failed to parse undefptr/PTR");
            }
            else
            {
                if (tok == null || !tok.Validate("where", BinaryFieldType.DATA_PTR)) throw new Exception("Failed to parse where/PTR");
            }
            tok.ApplyUInt32H8(retVal, x => x.where);

            tok = reader.ReadToken();
            if (reader.Format == BZNFormat.Battlezone && reader.Version >= 2012)
            {
                if (tok == null || !tok.Validate("param", BinaryFieldType.DATA_ID)) throw new Exception("Failed to parse param/ID");
                tok.ApplyID(retVal, x => x.param);
            }
            else
            {
                if (tok == null || !tok.Validate("param", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse param/LONG");
                tok.ApplyUInt32(retVal, x => x.param);
            }

            return retVal;
        }

        public static void WriteAiCmdInfo(this BZNStreamWriter writer, AiCmdInfo value, bool preserveMalformations)
        {
            writer.WriteInt32("priority", value, x => x.priority);

            if (writer.Format == BZNFormat.Battlezone || writer.Format == BZNFormat.BattlezoneN64)
            {
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
                        writer.WriteUInt8h("what", value, x => x.what);
                    }
                    else
                    {
                        writer.WriteVoidBytesL("what", value, x => x.what); // 1 liner 32bit number like VoidBytes but lowercase
                    }
                }
            }

            writer.WriteInt32("who", value, x => x.who);

            if (writer.Format == BZNFormat.Battlezone && (writer.Version == 1001 || writer.Version == 1011 || writer.Version == 1012))
            {
                writer.WritePtr("undefptr", value, x => x.where);
            }
            else
            {
                writer.WritePtr("where", value, x => x.where);
            }

            if (writer.Format == BZNFormat.Battlezone && writer.Version >= 2012)
            {
                writer.WriteID("param", value, x => x.param);
            }
            else
            {
                writer.WriteUInt32("param", value, x => x.param);
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

        public void ClearMalformations()
        {
            Malformations.Clear();
        }
    }
}
