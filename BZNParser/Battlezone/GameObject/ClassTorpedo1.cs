using BZNParser.Tokenizer;
using System.Numerics;
using System.Reflection.PortableExecutable;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "torpedo")]
    [ObjectClass(BZNFormat.BattlezoneN64, "torpedo")]
    public class ClassTorpedo1Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassTorpedo1(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                ClassTorpedo1.Hydrate(parent, reader, obj as ClassTorpedo1);
                return true;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassTorpedo1 : ClassPowerUp
    {
        public Int32 abandoned { get; set; } // only used in older code paths where Torpedo is actually Craft based

        public ClassTorpedo1(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
            abandoned = 0;
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


        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassTorpedo1? obj)
        {
            if (reader.Version < 1031)
            {
                if (reader.Version < 1019)
                {
                    // obsolete
                    IBZNToken? tok;

                    tok = reader.ReadToken();
                    tok = reader.ReadToken();
                    tok = reader.ReadToken();
                    tok = reader.ReadToken();
                    tok = reader.ReadToken();
                    tok = reader.ReadToken();

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_VEC3D))
                        throw new Exception("Failed to parse ???/VEC3D");
                    // there are 6 vectors here, but we don't know what they are for and are probably able to be forgotten

                    throw new NotImplementedException();
                }
                else if (reader.Version > 1027)
                {
                    // read in abandoned flag
                    IBZNToken? tok;
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("abandoned", BinaryFieldType.DATA_LONG))
                        throw new Exception("Failed to parse abandoned/LONG");
                    if (tok.GetCount() != 1)
                        throw new Exception("Failed to parse abandoned/LONG (wrong entry count)");
                    tok.ApplyInt32(obj, x => x.abandoned);
                }
            }

            if (reader.Format == BZNFormat.Battlezone && reader.Version < 1031)
            {
                ClassGameObject.Hydrate(parent, reader, obj as ClassGameObject);
                return;
            }
            ClassPowerUp.Hydrate(parent, reader, obj as ClassPowerUp);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassTorpedo1 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (writer.Version < 1031)
            {
                if (writer.Version < 1019)
                {
                    // not implemented
                    throw new NotImplementedException();
                }
                else if (writer.Version > 1027)
                {
                    writer.WriteInt32("abandoned", obj, x => x.abandoned);
                }
            }

            if (writer.Format == BZNFormat.Battlezone && writer.Version < 1031)
            {
                ClassGameObject.Dehydrate(obj, parent, writer, binary, save);
                return;
            }
            ClassPowerUp.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
