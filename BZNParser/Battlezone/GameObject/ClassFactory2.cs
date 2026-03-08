using BZNParser.Tokenizer;
using System.Reflection.PortableExecutable;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "factory")]
    public class ClassFactory2Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassFactory2(preamble, classLabel);
            ClassFactory2.Hydrate(parent, reader, obj as ClassFactory2);
            return true;
        }
    }
    public class ClassFactory2 : ClassPoweredBuilding
    {
        public float buildTime { get; set; }
        public bool buildActive { get; set; }
        public string[] buildItems { get; set; }
        public Int32 navHandle { get; set; }

        public ClassFactory2(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }

        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassFactory2? obj)
        {
            IBZNToken tok;

            if (reader.Version == 1100 || reader.Version == 1104 || reader.Version == 1105)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("buildDoneTime", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse buildDoneTime/FLOAT");
                if (obj != null) obj.buildTime = tok.GetSingle();
            }
            else
            {
                tok = reader.ReadToken();
                if (!tok.Validate("buildTime", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse buildTime/FLOAT");
                if (obj != null) obj.buildTime = tok.GetSingle();
            }

            tok = reader.ReadToken();
            if (!tok.Validate("buildActive", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse buildActive/BOOL");
            if (obj != null) obj.buildActive = tok.GetBoolean();

            tok = reader.ReadToken();
            if (!tok.Validate("buildCount", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse buildCount/LONG");
            int buildCount = tok.GetInt32();
            if (obj != null) obj.buildItems = new string[buildCount];

            for (int i = 0; i < buildCount; i++)
            {
                //v5 = std::deque < GameObjectClass const *>::operator[] (v4);
                //ILoadSaveVisitor::out(a2, *v5, "buildItem");
                //++v4;
                string item = reader.ReadGameObjectClass_BZ2(parent, "buildItem");
                if (obj != null) obj.buildItems[i] = item;
            }

            //...

            // if parent.SaveType != SaveType.BZN

            if (parent.SaveType == 0)
            {
                if (reader.Version >= 1135)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("navHandle", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse navHandle/LONG");
                    if (obj != null) obj.navHandle = tok.GetInt32();
                }
            }

            ClassPoweredBuilding.Hydrate(parent, reader, obj as ClassPoweredBuilding);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassFactory2 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            if (writer.Version == 1100 || writer.Version == 1104 || writer.Version == 1105)
            {
                writer.WriteFloats("buildDoneTime", obj.buildTime);
            }
            else
            {
                writer.WriteFloats("buildTime", obj.buildTime);
            }
            writer.WriteBooleans("buildActive", obj.buildActive);
            writer.WriteSignedValues("buildCount", obj.buildItems.Length);
            for (int i = 0; i < obj.buildItems.Length; i++)
            {
                writer.WriteGameObjectClass_BZ2(parent, obj.buildItems[i], "buildItem");
            }

            if (parent.SaveType == 0)
            {
                if (writer.Version >= 1135)
                {
                    writer.WriteSignedValues("navHandle", obj.navHandle);
                }
            }

            ClassPoweredBuilding.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
