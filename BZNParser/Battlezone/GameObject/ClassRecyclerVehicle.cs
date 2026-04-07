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
            {
                obj = new ClassRecyclerVehicle(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                ClassRecyclerVehicle.Hydrate(parent, reader, obj as ClassRecyclerVehicle);
                return true;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassRecyclerVehicle : ClassDeployBuilding
    {
        public float nextRepair { get; set; }
        public float buildDoneTime { get; set; }
        public bool buildActive { get; set; }
        //public SizedString[] buildItems { get; set; }
        public MalformableArray<ClassRecyclerVehicle, SizedString> buildItems { get; private set; }

        public ClassRecyclerVehicle(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
            nextRepair = 0;
            buildDoneTime = 0;
            buildActive = false;
            //buildItems = Array.Empty<SizedString>();
            buildItems = new MalformableArray<ClassRecyclerVehicle, SizedString>(this, x => x.buildItems, 0);
        }

        public override void ClearMalformations()
        {
            Malformations.Clear();
            //buildItems.ClearMalformations(); // Already cleared by above Clear as this is a window into our Malformations
            foreach (var buildItem in buildItems)
                buildItem?.ClearMalformations();
            base.ClearMalformations();
        }

        public override void DisableMalformationAutoFix()
        {
            buildItems.DisableMalformationAutoFix();
            foreach (var buildItem in buildItems)
                buildItem?.DisableMalformationAutoFix();
            base.DisableMalformationAutoFix();
        }

        public override void EnableMalformationAutoFix()
        {
            buildItems.EnableMalformationAutoFix();
            foreach (var buildItem in buildItems)
                buildItem?.EnableMalformationAutoFix();
            base.EnableMalformationAutoFix();
        }



        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassRecyclerVehicle? obj)
        {
            if (reader.Version == 1047)
            {
                IBZNToken? tok;

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("nextRepair", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse nextRepair/FLOAT");
                tok.ApplySingle(obj, x => x.nextRepair);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("buildDoneTime", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse buildDoneTime/FLOAT");
                tok.ApplySingle(obj, x => x.buildDoneTime);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("buildActive", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse buildActive/BOOL");
                tok.ApplyBoolean(obj, x => x.buildActive);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("buildCount", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse buildCount/LONG");
                int buildCount = tok.GetInt32();

                //if (obj != null) obj.buildItems = new SizedString[buildCount];
                if (obj != null) obj.buildItems = new MalformableArray<ClassRecyclerVehicle, SizedString>(obj, x => x.buildItems, buildCount);

                for (int i = 0; i < buildCount; i++)
                {
                    //v5 = std::deque < GameObjectClass const *>::operator[] (v4);
                    //ILoadSaveVisitor::out(a2, *v5, "buildItem");
                    //++v4;

                    //string item = reader.ReadGameObjectClass_BZ2(parent, "buildItem", obj?.Malformations); // TODO another problem for malformations without idx
                    //if (obj != null) obj.buildItems[i] = item;
                 
                    reader.ReadGameObjectClass_BZ2(parent.SaveType, "buildItem", obj, x => x.buildItems, i);
                }
            }

            ClassDeployBuilding.Hydrate(parent, reader, obj as ClassDeployBuilding);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassRecyclerVehicle obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (writer.Version == 1047)
            {
                writer.WriteSingle("nextRepair", obj, x => x.nextRepair);
                writer.WriteSingle("buildDoneTime", obj, x => x.buildDoneTime);
                writer.WriteBoolean("buildActive", obj, x => x.buildActive);
                writer.WriteLength("buildCount", obj, x => x.buildItems);
                //for (int i = 0; i < obj.buildItems.Length; i++)
                for (int i = 0; i < obj.buildItems.Count; i++)
                {
                    //writer.WriteGameObjectClass_BZ2(parent, obj.buildItems[i], "buildItem", obj.Malformations);
                    writer.WriteSizedString("buildItem", obj, x => x.buildItems, (v) => v[i]);
                }
            }
            ClassDeployBuilding.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
