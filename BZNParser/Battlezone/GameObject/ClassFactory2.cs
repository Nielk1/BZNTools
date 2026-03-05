using BZNParser.Tokenizer;

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
        public ClassFactory2(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassFactory2? obj)
        {
            IBZNToken tok;

            if (reader.Version == 1100 || reader.Version == 1104 || reader.Version == 1105)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("buildDoneTime", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse buildDoneTime/FLOAT");
            }
            else
            {
                tok = reader.ReadToken();
                if (!tok.Validate("buildTime", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse buildTime/FLOAT");
            }

            tok = reader.ReadToken();
            if (!tok.Validate("buildActive", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse buildActive/BOOL");

            tok = reader.ReadToken();
            if (!tok.Validate("buildCount", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse buildCount/LONG");
            int buildCount = tok.GetInt32();

            for (int i = 0; i < buildCount; i++)
            {
                //v5 = std::deque < GameObjectClass const *>::operator[] (v4);
                //ILoadSaveVisitor::out(a2, *v5, "buildItem");
                //++v4;
                string item = reader.ReadGameObjectClass_BZ2(parent, "buildItem");
            }

            //...

            // if parent.SaveType != SaveType.BZN

            if (parent.SaveType == 0)
            {
                if (reader.Version >= 1135)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("navHandle", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse navHandle/LONG");
                    //int navHandle = tok.GetInt32();
                }
            }

            ClassPoweredBuilding.Hydrate(parent, reader, obj as ClassPoweredBuilding);
        }
    }
}
