using BZNParser.Tokenizer;
using System.Reflection.PortableExecutable;

namespace BZNParser.Battlezone
{
    static class AiCmdInfoExtensions
    {
        public static AiCmdInfo GetAiCmdInfo(this BZNStreamReader reader, string? name = null)
        {
            AiCmdInfo retVal = new AiCmdInfo();
            retVal.DisableMalformationAutoFix();

            try
            {
                IBZNToken? tok;

                if (!reader.InBinary && reader.Format == BZNFormat.Battlezone2)
                {
                    if (name == null) throw new Exception("Name must be provided for non-binary BZ2 parsing");
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate(name)) throw new Exception($"Failed to parse {name}");
                }

                tok = reader.ReadToken();
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
            finally
            {
                retVal.EnableMalformationAutoFix();
            }
        }

        public static void WriteAiCmdInfo(this BZNStreamWriter writer, AiCmdInfo value, string? name = null)
        {
            if (!writer.InBinary && writer.Format == BZNFormat.Battlezone2)
            {
                if (name == null) throw new Exception("Name must be provided for non-binary BZ2 writing");
                writer.WriteCmdDummy(name);
            }

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

    /*
BZ98R:
enum AiCommand {
	CMD_NONE,
	CMD_SELECT,
	CMD_STOP,
	CMD_GO,
	CMD_ATTACK,
	CMD_FOLLOW,
	CMD_FORMATION,
	CMD_PICKUP,
	CMD_DROPOFF,
	CMD_NO_DROPOFF,
	CMD_GET_REPAIR,
	CMD_GET_RELOAD,
	CMD_GET_WEAPON,
	CMD_GET_CAMERA,
	CMD_GET_BOMB,
	CMD_DEFEND,
	CMD_GO_TO_GEYSER,
	CMD_RESCUE,
	CMD_RECYCLE,
	CMD_SCAVENGE,
	CMD_HUNT,
	CMD_BUILD,
	CMD_PATROL,
	CMD_STAGE,
	CMD_SEND,
	CMD_GET_IN,
	CMD_LAY_MINES,
	CMD_CLOAK,
	CMD_DECLOAK,
	NUM_CMD,
};

BZCC:
enum AiCommand {
	CMD_NONE,
	CMD_SELECT,
	CMD_STOP,
	CMD_GO,
	CMD_ATTACK,
	CMD_FOLLOW,
	CMD_FORMATION,
	CMD_PICKUP,
	CMD_DROPOFF,
	CMD_UNDEPLOY,
	CMD_DEPLOY,
	CMD_NO_DEPLOY,
	CMD_GET_REPAIR,
	CMD_GET_RELOAD,
	CMD_GET_WEAPON,
	CMD_GET_CAMERA,
	CMD_GET_BOMB,
	CMD_DEFEND,
	CMD_RESCUE,
	CMD_RECYCLE,
	CMD_SCAVENGE,
	CMD_HUNT,
	CMD_BUILD,
	CMD_PATROL,
	CMD_STAGE,
	CMD_SEND,
	CMD_GET_IN,
	CMD_LAY_MINES,
	CMD_LOOK_AT,
	CMD_SERVICE,
	CMD_UPGRADE,
	CMD_DEMOLISH,
	CMD_POWER,
	CMD_BACK,
	CMD_DONE,
	CMD_CANCEL,
	CMD_SET_GROUP,
	CMD_SET_TEAM,
	CMD_SEND_GROUP,
	CMD_TARGET,
	CMD_INSPECT,
	CMD_SWITCHTEAM,
	CMD_INTERFACE,
	CMD_LOGOFF,
	CMD_AUTOPILOT,
	CMD_MESSAGE,
	CMD_CLOSE,
	CMD_MORPH_SETDEPLOYED, // For morphtanks
	CMD_MORPH_SETUNDEPLOYED, // For morphtanks
	CMD_MORPH_UNLOCK, // For morphtanks
	CMD_BAILOUT,
	CMD_BUILD_ROTATE, // Update building rotations by 90 degrees.
	CMD_CMDPANEL_SELECT,
	CMD_CMDPANEL_DESELECT,

	NUM_CMD // Must be last!
}; // Don't let NUM_COMMAND go past 255, as 1 byte is used for it when sending across network
     */

    public class AiCmdInfo : IMalformable
    {
        public int priority { get; set; }
        public uint what { get; set; } // AiCommand (not sure BZ2 vs BZ1) TODO: handle BZ1 vs BZ2, maybe make this a dual value? or store 2 properties with conversion logic?
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
        private bool blockAutoFixMalformations = false;
        public void DisableMalformationAutoFix()
        {
            blockAutoFixMalformations = true;
        }
        public void EnableMalformationAutoFix()
        {
            blockAutoFixMalformations = false;
        }
    }
}
