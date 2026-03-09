using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace BZNParser.Battlezone
{
    public class AreaOfInterest : IMalformable
    {
        public UInt32 path { get; set; }
        public UInt32 team { get; set; }
        public bool interesting { get; set; }
        public bool inside { get; set; }
        public Int32 value { get; set; }
        public UInt32 force { get; set; }


        private readonly IMalformable.MalformationManager _malformationManager;
        public IMalformable.MalformationManager Malformations => _malformationManager;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public AreaOfInterest()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            this._malformationManager = new IMalformable.MalformationManager(this);
        }

        public static bool Create(BZNFileBattlezone parent, BZNStreamReader reader, int countLeft, out AreaOfInterest? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new AreaOfInterest();
            AreaOfInterest.Hydrate(parent, reader, countLeft, obj);
            return true;
        }

        public static bool Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, int countLeft, AreaOfInterest? obj)
        {
            IBZNToken tok;

            if (!reader.InBinary)
            {
                tok = reader.ReadToken();
                if (!tok.IsValidationOnly() || !tok.Validate("AOI", BinaryFieldType.DATA_UNKNOWN))
                    throw new Exception("Failed to parse [AOI]");
            }
            //if (reader.Format == BZNFormat.Battlezone2)
            //{
            //    tok = reader.ReadToken();
            //    if (!tok.Validate("name", BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse name/CHAR");
            //    string name = tok.GetString();
            //    if (name != "AOI")
            //    {
            //        throw new Exception("Failed to parse AOI"); // untested/unconfirmed assumption
            //    }
            //}

            if (reader.Format == BZNFormat.Battlezone)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("undefptr", BinaryFieldType.DATA_PTR))
                    throw new Exception("Failed to parse dropoff/PTR");
                if (obj != null) obj.path = tok.GetUInt32H();
            }
            if (reader.Format == BZNFormat.Battlezone2)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("path", BinaryFieldType.DATA_PTR))
                    throw new Exception("Failed to parse path/PTR");
                if (obj != null) obj.path = tok.GetUInt32H();
            }

            tok = reader.ReadToken();
            if (!tok.Validate("team", BinaryFieldType.DATA_LONG))
                throw new Exception("Failed to parse team/LONG");
            if (obj != null) obj.team = tok.GetUInt32();

            tok = reader.ReadToken();
            if (!tok.Validate("interesting", BinaryFieldType.DATA_BOOL))
                throw new Exception("Failed to parse interesting/BOOL");
            if (obj != null) obj.interesting = tok.GetBoolean();

            tok = reader.ReadToken();
            if (!tok.Validate("inside", BinaryFieldType.DATA_BOOL))
                throw new Exception("Failed to parse inside/BOOL");
            if (obj != null) obj.inside = tok.GetBoolean();

            tok = reader.ReadToken();
            if (!tok.Validate("value", BinaryFieldType.DATA_LONG))
                throw new Exception("Failed to parse value/LONG");
            if (obj != null) obj.value = tok.GetInt32();

            tok = reader.ReadToken();
            if (!tok.Validate("force", BinaryFieldType.DATA_LONG))
                throw new Exception("Failed to parse force/LONG");
            if (obj != null) obj.force = tok.GetUInt32();

            return true;
        }

        public void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            writer.WriteValidation("AOI");

            //if (reader.Format == BZNFormat.Battlezone2)
            //{
            //    tok = reader.ReadToken();
            //    if (!tok.Validate("name", BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse name/CHAR");
            //    string name = tok.GetString();
            //    if (name != "AOI")
            //    {
            //        throw new Exception("Failed to parse AOI"); // untested/unconfirmed assumption
            //    }
            //}

            if (writer.Format == BZNFormat.Battlezone)
            {
                writer.WritePtr("undefptr", path);
            }
            if (writer.Format == BZNFormat.Battlezone2)
            {
                writer.WritePtr("path", path);
            }

            writer.WriteUnsignedValues("team", team);
            writer.WriteBooleans("interesting", interesting);
            writer.WriteBooleans("inside", inside);
            writer.WriteSignedValues("value", value);
            writer.WriteUnsignedValues("force", force);
        }
    }
}
