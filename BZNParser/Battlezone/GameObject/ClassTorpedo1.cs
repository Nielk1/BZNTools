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
                obj = new ClassTorpedo1(preamble, classLabel);
            ClassTorpedo1.Hydrate(parent, reader, obj as ClassTorpedo1);
            return true;
        }
    }
    public class ClassTorpedo1 : ClassPowerUp
    {
        public ClassTorpedo1(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
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
                }
                else if (reader.Version > 1027)
                {
                    // read in abandoned flag
                    IBZNToken? tok;
                    tok = reader.ReadToken();
                }
            }

            if (reader.Format == BZNFormat.Battlezone && reader.Version < 1031)
            {
                ClassGameObject.Hydrate(parent, reader, obj as ClassGameObject);
                return;
            }
            ClassPowerUp.Hydrate(parent, reader, obj as ClassPowerUp);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassTorpedo1 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            if (writer.Version < 1031)
            {
                if (writer.Version < 1019)
                {
                    // not implemented
                }
                else if (writer.Version > 1027)
                {
                    // not implemented
                }
            }

            if (writer.Format == BZNFormat.Battlezone && writer.Version < 1031)
            {
                ClassGameObject.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
                return;
            }
            ClassPowerUp.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
