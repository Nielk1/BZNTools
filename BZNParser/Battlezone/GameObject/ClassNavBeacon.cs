using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "beacon")]
    public class ClassNavBeaconFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassNavBeacon(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                ClassNavBeacon.Hydrate(parent, reader, obj as ClassNavBeacon);
                return true;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassNavBeacon : ClassGameObject
    {
        //public string name { get; set; }
        public int navSlot { get; set; }

        public ClassNavBeacon(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
            //name = string.Empty;
            navSlot = 0;
        }

        public override void ClearMalformations()
        {
            Malformations.Clear();
            base.ClearMalformations();
        }

        public override void DisableMalformationAutoFix()
        {
            base.DisableMalformationAutoFix();
        }

        public override void EnableMalformationAutoFix()
        {
            base.EnableMalformationAutoFix();
        }


        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassNavBeacon? obj)
        {
            IBZNToken? tok;

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("name", BinaryFieldType.DATA_CHAR))
                throw new Exception("Failed to parse name/CHAR");
            tok.ApplyChars(obj, x => x.name);


            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("navSlot", BinaryFieldType.DATA_LONG))
                throw new Exception("Failed to parse navSlot/LONG");
            tok.ApplyInt32(obj, x => x.navSlot);

            if (reader.Version > 1104)
            {
                ClassGameObject.Hydrate(parent, reader, obj as ClassGameObject);
            }
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassNavBeacon obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            writer.WriteChars("name", obj, x => x.name);
            writer.WriteInt32("navSlot", obj, x => x.navSlot);

            if (writer.Version > 1104)
            {
                ClassGameObject.Dehydrate(obj, parent, writer, binary, save);
            }
        }
    }
}
