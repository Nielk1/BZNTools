using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "torpedo")]
    public class ClassTorpedo2Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassTorpedo2(preamble, classLabel);
            ClassTorpedo2.Hydrate(parent, reader, obj as ClassTorpedo2);
            return true;
        }
    }
    public class ClassTorpedo2 : ClassGameObject
    {
        public float lifeTimer { get; set; }

        public ClassTorpedo2(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassTorpedo2? obj)
        {
            IBZNToken tok;

            tok = reader.ReadToken();
            if (!tok.Validate("lifeTimer", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse lifeTimer/FLOAT");
            if (obj != null) obj.lifeTimer = tok.GetSingle();

            ClassGameObject.Hydrate(parent, reader, obj as ClassGameObject);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassTorpedo2 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            writer.WriteFloats("lifeTimer", obj.lifeTimer);
            ClassGameObject.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
