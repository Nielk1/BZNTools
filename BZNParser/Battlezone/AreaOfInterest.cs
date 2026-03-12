using BZNParser.Battlezone.GameObject;
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
            IBZNToken? tok;

            if (!reader.InBinary)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.IsValidationOnly() || !tok.Validate("AOI", BinaryFieldType.DATA_UNKNOWN))
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
                if (tok == null || !tok.Validate("undefptr", BinaryFieldType.DATA_PTR))
                    throw new Exception("Failed to parse undefptr/PTR");
                if (obj != null) obj.path = tok.GetUInt32H();
            }
            if (reader.Format == BZNFormat.Battlezone2)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("path", BinaryFieldType.DATA_PTR))
                    throw new Exception("Failed to parse path/PTR");
                if (obj != null) obj.path = tok.GetUInt32H();
            }

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("team", BinaryFieldType.DATA_LONG))
                throw new Exception("Failed to parse team/LONG");
            if (obj != null) obj.team = tok.GetUInt32();

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("interesting", BinaryFieldType.DATA_BOOL))
                throw new Exception("Failed to parse interesting/BOOL");
            tok.ReadBoolean(obj, x => x.interesting);

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("inside", BinaryFieldType.DATA_BOOL))
                throw new Exception("Failed to parse inside/BOOL");
            tok.ReadBoolean(obj, x => x.inside);

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("value", BinaryFieldType.DATA_LONG))
                throw new Exception("Failed to parse value/LONG");
            if (obj != null) obj.value = tok.GetInt32();

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("force", BinaryFieldType.DATA_LONG))
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
            //    if (tok == null || !tok.Validate("name", BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse name/CHAR");
            //    string name = tok.GetString();
            //    if (name != "AOI")
            //    {
            //        throw new Exception("Failed to parse AOI"); // untested/unconfirmed assumption
            //    }
            //}

            if (writer.Format == BZNFormat.Battlezone)
            {
                writer.WriteBZ1_Ptr("undefptr", path);
            }
            if (writer.Format == BZNFormat.Battlezone2)
            {
                writer.WritePtr32("path", path);
            }

            writer.WriteUnsignedValues("team", team);
            writer.WriteBoolean("interesting", this, x => x.interesting);
            writer.WriteBoolean("inside", this, x => x.inside);
            writer.WriteSignedValues("value", value);
            writer.WriteUnsignedValues("force", force);
        }
    }
}
