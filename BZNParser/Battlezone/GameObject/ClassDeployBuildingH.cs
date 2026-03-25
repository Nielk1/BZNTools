using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "recyclervehicleh")]
    [ObjectClass(BZNFormat.Battlezone2, "deploybuildingh")]
    public class ClassDeployBuildingHFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassDeployBuildingH(preamble, classLabel);
            ClassDeployBuildingH.Hydrate(parent, reader, obj as ClassDeployBuildingH);
            return true;
        }
    }
    public class ClassDeployBuildingH : ClassDeployable
    {
        public Matrix dropMat { get; set; }

        public ClassDeployBuildingH(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }

        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassDeployBuildingH? obj)
        {
            //IBZNToken? tok;

            if (parent.SaveType != SaveType.BZN) { }

            //if ( a2[2].vftable )
            //{
            //    (a2->vftable->out_bool)(a2, this + 2560, 1, "buildActive");
            //    (a2->vftable->out_float)(a2, this + 2576, 4, "buildTime");
            //}
            //(a2->vftable->field_1C)(a2, this + 2592, 64, "buildMatrix");
            //tok = reader.ReadToken();
            //if (!tok.Validate("buildMatrix", BinaryFieldType.DATA_MAT3D)) throw new Exception("Failed to parse buildMatrix/MAT3D"); // type unconfirmed
                                                                                                                                    //dropMat = tok.GetMatrix()
            reader.ReadMatrix("buildMatrix", obj, x => x.dropMat);

            ClassDeployable.Hydrate(parent, reader, obj as ClassDeployable);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassDeployBuildingH obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            if (parent.SaveType != SaveType.BZN) { }
            //writer.WriteMat3Ds("buildMatrix", preserveMalformations, obj.transform);
            writer.WriteMatrix("buildMatrix", obj, x => x.dropMat);
            ClassDeployable.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
