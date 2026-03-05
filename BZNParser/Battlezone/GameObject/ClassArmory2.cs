using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "armory")]
    public class ClassArmory2Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassArmory2(preamble, classLabel);
            ClassArmory2.Hydrate(parent, reader, obj as ClassArmory2);
            return true;
        }
    }
    public class ClassArmory2 : ClassPoweredBuilding
    {
        public Queue<string?> buildQueue { get; private set; }
        public float buildDoneTime { get; set; }
        public bool buildActive { get; set; }
        public bool buildStall { get; set; }
        Vector3D buildRally { get; set; }
        public int navHandle { get; set; }
        Vector3D launchTarget { get; set; }
        int launchHandle { get; set; }

        public ClassArmory2(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassArmory2? obj)
        {
            IBZNToken tok;

            tok = reader.ReadToken();
            if (!tok.Validate("buildDoneTime", BinaryFieldType.DATA_FLOAT))
                throw new Exception("Failed to parse buildDoneTime/FLOAT");
            if (obj != null) obj.buildDoneTime = tok.GetSingle();

            tok = reader.ReadToken();
            if (!tok.Validate("buildActive", BinaryFieldType.DATA_BOOL))
                throw new Exception("Failed to parse buildActive/BOOL");
            if (obj != null) obj.buildActive = tok.GetBoolean();

            tok = reader.ReadToken();
            if (!tok.Validate("buildCount", BinaryFieldType.DATA_LONG))
                throw new Exception("Failed to parse buildCount/LONG");
            int buildCount = tok.GetInt32();

            if (obj != null) obj.buildQueue = new Queue<string?>(buildCount);

            for (int i = 0; i < buildCount; i++)
            {
                string? item = reader.ReadGameObjectClass_BZ2(parent, "buildItem");
                if (obj != null) obj.buildQueue.Enqueue(item);
            }
            if (parent.SaveType != SaveType.BZN)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("buildStall", BinaryFieldType.DATA_BOOL))
                    throw new Exception("Failed to parse buildStall/BOOL");
                if (obj != null) obj.buildStall = tok.GetBoolean();

                tok = reader.ReadToken();
                if (!tok.Validate("buildRally", BinaryFieldType.DATA_VEC3D))
                    throw new Exception("Failed to parse buildRally/VECTOR");
                if (obj != null) obj.buildRally = tok.GetVector3D();

                tok = reader.ReadToken();
                if (!tok.Validate("navHandle", BinaryFieldType.DATA_LONG))
                    throw new Exception("Failed to parse navHandle/LONG");
                if (obj != null) obj.navHandle = tok.GetInt32();

                tok = reader.ReadToken();
                if (!tok.Validate("launchHandle", BinaryFieldType.DATA_LONG))
                    throw new Exception("Failed to parse launchHandle/LONG");
                if (obj != null) obj.launchHandle = tok.GetInt32();

                tok = reader.ReadToken();
                if (!tok.Validate("launchTarget", BinaryFieldType.DATA_VEC3D))
                    throw new Exception("Failed to parse launchTarget/VECTOR");
                if (obj != null) obj.launchTarget = tok.GetVector3D();
            }

            if (parent.SaveType == 0)
            {
                if (reader.Version >= 1135)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("navHandle", BinaryFieldType.DATA_LONG))
                        throw new Exception("Failed to parse navHandle/LONG");
                    if (obj != null) obj.navHandle = tok.GetInt32();
                }
            }

            ClassPoweredBuilding.Hydrate(parent, reader, obj as ClassPoweredBuilding);
        }
    }
}
