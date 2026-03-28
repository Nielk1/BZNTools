using BZNParser.Tokenizer;
using System.Diagnostics.Contracts;
using System.Reflection.PortableExecutable;

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
        public SizedString[] buildItems { get; private set; }
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
            IBZNToken? tok;

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("buildDoneTime", BinaryFieldType.DATA_FLOAT))
                throw new Exception("Failed to parse buildDoneTime/FLOAT");
            tok.ApplySingle(obj, x => x.buildDoneTime);

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("buildActive", BinaryFieldType.DATA_BOOL))
                throw new Exception("Failed to parse buildActive/BOOL");
            tok.ApplyBoolean(obj, x => x.buildActive);

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("buildCount", BinaryFieldType.DATA_LONG))
                throw new Exception("Failed to parse buildCount/LONG");
            int buildCount = tok.GetInt32();

            if (obj != null) obj.buildItems = new SizedString[buildCount];

            for (int i = 0; i < buildCount; i++)
            {
                //string? item = reader.ReadGameObjectClass_BZ2(parent, "buildItem", obj?.Malformations); // TODO this isn't optimal for malformation reading since we're not dealing with indexes
                //if (obj != null) obj.buildQueue.Enqueue(item);

                reader.ReadGameObjectClass_BZ2(parent.SaveType, "buildItem", obj, x => x.buildItems, i);
            }
            if (parent.SaveType != SaveType.BZN)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("buildStall", BinaryFieldType.DATA_BOOL))
                    throw new Exception("Failed to parse buildStall/BOOL");
                tok.ApplyBoolean(obj, x => x.buildStall);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("buildRally", BinaryFieldType.DATA_VEC3D))
                    throw new Exception("Failed to parse buildRally/VECTOR");
                tok.ApplyVector3D(obj, x => x.buildRally);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("navHandle", BinaryFieldType.DATA_LONG))
                    throw new Exception("Failed to parse navHandle/LONG");
                tok.ApplyInt32(obj, x => x.navHandle);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("launchHandle", BinaryFieldType.DATA_LONG))
                    throw new Exception("Failed to parse launchHandle/LONG");
                tok.ApplyInt32(obj, x => x.launchHandle);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("launchTarget", BinaryFieldType.DATA_VEC3D))
                    throw new Exception("Failed to parse launchTarget/VECTOR");
                tok.ApplyVector3D(obj, x => x.launchTarget);
            }

            if (parent.SaveType == 0)
            {
                if (reader.Version >= 1135)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("navHandle", BinaryFieldType.DATA_LONG))
                        throw new Exception("Failed to parse navHandle/LONG");
                    tok.ApplyInt32(obj, x => x.navHandle);
                }
            }

            ClassPoweredBuilding.Hydrate(parent, reader, obj as ClassPoweredBuilding);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassArmory2 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            writer.WriteSingle("buildDoneTime", obj, x => x.buildDoneTime);
            writer.WriteBoolean("buildActive", obj, x => x.buildActive);
            writer.WriteSignedValues("buildCount", obj.buildItems.Length);

            //foreach (string? item in obj.buildItems)
            for (int i = 0; i < obj.buildItems.Length; i++)
            {
                //writer.WriteGameObjectClass_BZ2(parent, item, "buildItem", obj.Malformations);
                writer.WriteSizedString("buildItem", obj, x => x.buildItems, (v) => v[i]);
            }
            if (parent.SaveType != SaveType.BZN)
            {
                writer.WriteBoolean("buildStall", obj, x => x.buildStall);
                writer.WriteVector3D("buildRally", obj, x => x.buildRally);
                writer.WriteInt32("navHandle", obj, x => x.navHandle);
                writer.WriteInt32("launchHandle", obj, x => x.launchHandle);
                writer.WriteVector3D("launchTarget", obj, x => x.launchTarget);
            }

            if (parent.SaveType == 0)
            {
                if (writer.Version >= 1135)
                {
                    writer.WriteInt32("navHandle", obj, x => x.navHandle);
                }
            }

            ClassPoweredBuilding.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
