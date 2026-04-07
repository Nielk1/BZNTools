using BZNParser.Battlezone.GameObject;
using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
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
        public AreaOfInterest()
        {
            this.path = 0;
            this.team = 0;
            this.interesting = false;
            this.inside = false;
            this.value = 0;
            this.force = 0;
            this._malformationManager = new IMalformable.MalformationManager(this);
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

        public static bool Create(BZNFileBattlezone parent, BZNStreamReader reader, int countLeft, out AreaOfInterest? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new AreaOfInterest();
                obj.DisableMalformationAutoFix();
            }
            try
            {
                AreaOfInterest.Hydrate(parent, reader, countLeft, obj);
                return true;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
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

            if (reader.Format == BZNFormat.Battlezone)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("undefptr", BinaryFieldType.DATA_PTR))
                    throw new Exception("Failed to parse undefptr/PTR");
                tok.ApplyUInt32H8(obj, x => x.path);
            }
            if (reader.Format == BZNFormat.Battlezone2)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("path", BinaryFieldType.DATA_PTR))
                    throw new Exception("Failed to parse path/PTR");
                tok.ApplyUInt32H8(obj, x => x.path);
            }

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("team", BinaryFieldType.DATA_LONG))
                throw new Exception("Failed to parse team/LONG");
            tok.ApplyUInt32(obj, x => x.team);

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("interesting", BinaryFieldType.DATA_BOOL))
                throw new Exception("Failed to parse interesting/BOOL");
            tok.ApplyBoolean(obj, x => x.interesting);

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("inside", BinaryFieldType.DATA_BOOL))
                throw new Exception("Failed to parse inside/BOOL");
            tok.ApplyBoolean(obj, x => x.inside);

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("value", BinaryFieldType.DATA_LONG))
                throw new Exception("Failed to parse value/LONG");
            tok.ApplyInt32(obj, x => x.value);

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("force", BinaryFieldType.DATA_LONG))
                throw new Exception("Failed to parse force/LONG");
            tok.ApplyUInt32(obj, x => x.force);

            return true;
        }

        public void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            writer.WriteValidation("AOI");

            if (writer.Format == BZNFormat.Battlezone)
            {
                writer.WritePtr("undefptr", this, x => x.path);
            }
            if (writer.Format == BZNFormat.Battlezone2)
            {
                writer.WritePtr("path", this, x => x.path);
            }

            writer.WriteUInt32("team", this, x => x.team);
            writer.WriteBoolean("interesting", this, x => x.interesting);
            writer.WriteBoolean("inside", this, x => x.inside);
            writer.WriteInt32("value", this, x => x.value);
            writer.WriteUInt32("force", this, x => x.force);
        }
    }
}
