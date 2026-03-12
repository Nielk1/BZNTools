using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "recyclervehicle")]
    public class ClassRecyclerVehicleFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassRecyclerVehicle(preamble, classLabel);
            ClassRecyclerVehicle.Hydrate(parent, reader, obj as ClassRecyclerVehicle);
            return true;
        }
    }
    public class ClassRecyclerVehicle : ClassDeployBuilding
    {
        public float nextRepair { get; set; }
        public float buildDoneTime { get; set; }
        public bool buildActive { get; set; }
        public string[] buildItems { get; set; }

        public ClassRecyclerVehicle(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }

        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassRecyclerVehicle? obj)
        {
            if (reader.Version == 1047)
            {
                IBZNToken? tok;

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("nextRepair", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse nextRepair/FLOAT");
                if (obj != null) obj.nextRepair = tok.GetSingle();

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("buildDoneTime", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse buildDoneTime/FLOAT");
                if (obj != null) obj.buildDoneTime = tok.GetSingle();

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("buildActive", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse buildActive/BOOL");
                tok.ReadBoolean(obj, x => x.buildActive);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("buildCount", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse buildCount/LONG");
                int buildCount = tok.GetInt32();

                if (obj != null) obj.buildItems = new string[buildCount];

                for (int i = 0; i < buildCount; i++)
                {
                    //v5 = std::deque < GameObjectClass const *>::operator[] (v4);
                    //ILoadSaveVisitor::out(a2, *v5, "buildItem");
                    //++v4;
                    string item = reader.ReadGameObjectClass_BZ2(parent, "buildItem", obj?.Malformations); // TODO another problem for malformations without idx
                    if (obj != null) obj.buildItems[i] = item;
                }
            }

            ClassDeployBuilding.Hydrate(parent, reader, obj as ClassDeployBuilding);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassRecyclerVehicle obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            if (writer.Version == 1047)
            {
                writer.WriteFloats("nextRepair", preserveMalformations ? obj.Malformations : null, obj.nextRepair);
                writer.WriteFloats("buildDoneTime", preserveMalformations ? obj.Malformations : null, obj.buildDoneTime);
                writer.WriteBoolean("buildActive", obj, x => x.buildActive);
                writer.WriteSignedValues("buildCount", obj.buildItems.Length);
                for (int i = 0; i < obj.buildItems.Length; i++)
                {
                    writer.WriteGameObjectClass_BZ2(parent, obj.buildItems[i], "buildItem", obj.Malformations);
                }
            }
            ClassDeployBuilding.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
