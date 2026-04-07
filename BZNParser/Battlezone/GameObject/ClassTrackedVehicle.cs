using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "tracked")]
    public class ClassTrackedVehicleFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassTrackedVehicle(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                ClassTrackedVehicle.Hydrate(parent, reader, obj as ClassTrackedVehicle);
                return true;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassTrackedVehicle : ClassCraft
    {
        public ClassTrackedVehicle(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassTrackedVehicle? obj)
        {
            //IBZNToken tok;

            if (reader.Format == BZNFormat.Battlezone2)
            {
                /*if (a2[2].vftable)
                {
                    //(a2->vftable->out_bool)(a2, this + 2128, 1, "undefbool");
                    tok = reader.ReadToken();
                    if (!tok.Validate("undefbool", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse undefbool/BOOL");
                    //undefbool = tok.GetBoolean();

                    //(a2->vftable->field_24)(a2, this + 848, 12, "undefvector_3d");
                    tok = reader.ReadToken();
                    if (!tok.Validate("undefvector_3d", BinaryFieldType.DATA_VEC3D)) throw new Exception("Failed to parse undefvector_3d/VEC3D");
                    //pos = tok.GetVector3D();

                    //(a2->vftable->field_18)(a2, this + 784, 16, "undefquat");
                    tok = reader.ReadToken();
                    if (!tok.Validate("undefquat", BinaryFieldType.DATA_QUAT)) throw new Exception("Failed to parse undefquat/QUAT");
                    //pos = tok.GetVector3D();

                    //(a2->vftable->out_bool)(a2, this + 2276, 6, "Contact_Region");
                    tok = reader.ReadToken();
                    if (!tok.Validate("Contact_Region", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse Contact_Region/BOOL");
                    //undefbool = tok.GetBoolean();

                    //(a2->vftable->out_float)(a2, this + 2284, 24, "Prev_Spring_Offset");
                    tok = reader.ReadToken();
                    if (!tok.Validate("Prev_Spring_Offset", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse Prev_Spring_Offset/FLOAT");
                    //undeffloat5 = tok.GetSingle();


                    //if ( a2[2].vftable == 2 || a2[2].vftable == 3 )
                    //{
                    //    (a2->vftable->out_float)(a2, this + 2156, 4, "ncb");
                    //    (a2->vftable->out_float)(a2, this + 2160, 4, "ncb");
                    //}

                    //(a2->vftable->read_long)(a2, this + 2516, 4, "CollisionHandle");
                    tok = reader.ReadToken();
                    if (!tok.Validate("CollisionHandle", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse CollisionHandle/LONG");

                    //(a2->vftable->read_long)(a2, this + 2520, 4, "CollisionTime");
                    tok = reader.ReadToken();
                    if (!tok.Validate("CollisionTime", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse CollisionTime/LONG");
                }*/
            }

            ClassCraft.Hydrate(parent, reader, obj as ClassCraft);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassTrackedVehicle obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (parent.SaveType != SaveType.BZN)
            {
                // 
            }
            ClassCraft.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
