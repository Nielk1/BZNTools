using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "deploybuilding")]
    public class ClassDeployBuildingFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassDeployBuilding(preamble, classLabel);
            ClassDeployBuilding.Hydrate(parent, reader, obj as ClassDeployBuilding);
            return true;
        }
    }
    public class ClassDeployBuilding : ClassTrackedDeployable
    {
        public Matrix dropMat { get; set; }

        public ClassDeployBuilding(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }

        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassDeployBuilding? obj)
        {
            IBZNToken tok;

            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version != 1047)
                {
                    //if ( a2[2].vftable )
                    //{
                    //    (a2->vftable->out_bool)(a2, this + 2560, 1, "buildActive");
                    //    (a2->vftable->out_float)(a2, this + 2576, 4, "buildTime");
                    //}
                    //(a2->vftable->field_1C)(a2, this + 2592, 64, "buildMatrix");
                    tok = reader.ReadToken();
                    if (!tok.Validate("buildMatrix", BinaryFieldType.DATA_MAT3D)) throw new Exception("Failed to parse buildMatrix/MAT3D"); // type unconfirmed
                    if (obj != null)
                    {
                        obj.dropMat = tok.GetMatrix();
                        tok.CheckMalformationsMatrix(obj.dropMat.Malformations, reader.FloatFormat);
                    }
                }
            }

            ClassTrackedDeployable.Hydrate(parent, reader, obj as ClassTrackedDeployable);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassDeployBuilding obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version != 1047)
                {
                    writer.WriteMat3Ds("buildMatrix", preserveMalformations, obj.dropMat);
                }
            }
            ClassTrackedDeployable.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
