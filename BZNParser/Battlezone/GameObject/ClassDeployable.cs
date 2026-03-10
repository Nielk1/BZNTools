using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "deployable")]
    public class ClassDeployableFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassDeployable(preamble, classLabel);
            ClassDeployable.Hydrate(parent, reader, obj as ClassDeployable);
            return true;
        }
    }
    public class ClassDeployable : ClassHoverCraft
    {
        public float deployTimer { get; set; }

        public ClassDeployable(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassDeployable? obj)
        {
            IBZNToken tok;

            // this class doesn't exist in BZ1
            //if (reader.Format == BZNFormat.Battlezone2)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("state", BinaryFieldType.DATA_VOID)) throw new Exception("Failed to parse state/VOID"); // type not confirmed
                if (obj != null) obj.state = (VEHICLE_STATE)tok.GetUInt32HR();

                tok = reader.ReadToken();
                if (!tok.Validate("deployTimer", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse deployTimer/FLOAT");
                if (obj != null) obj.deployTimer = tok.GetSingle();

                if (parent.SaveType == 0)
                {
                    // setup stuff where some vars are generated
                }
                else
                {
                    /*if (a2[2].vftable)
                    {
                        (a2->vftable->read_long)(a2, this + 2336, 4, "changeState");
                        (a2->vftable->out_bool)(a2, this + 2344, 1, "lockMode");
                        (a2->vftable->out_bool)(a2, this + 2345, 1, "lockModeDeployed");
                    }*/
                }
            }

            ClassHoverCraft.Hydrate(parent, reader, obj as ClassHoverCraft);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassDeployable obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            // this class doesn't exist in BZ1
            //if (writer.Format == BZNFormat.Battlezone2)
            {
                writer.WriteVoidBytes("state", (UInt32)obj.state);
                writer.WriteFloats("deployTimer", preserveMalformations ? obj.Malformations : null, obj.deployTimer);
                if (parent.SaveType == 0)
                {
                    // setup stuff where some vars are generated
                }
                else
                {
                    /*(a2->vftable->write_long)(a2, this + 2336, 4, "changeState");
                    (a2->vftable->in_bool)(a2, this + 2344, 1, "lockMode");
                    (a2->vftable->in_bool)(a2, this + 2345, 1, "lockModeDeployed");*/
                }
            }
            ClassHoverCraft.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
