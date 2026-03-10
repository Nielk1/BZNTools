using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "tug")]
    [ObjectClass(BZNFormat.BattlezoneN64, "tug")]
    [ObjectClass(BZNFormat.Battlezone2, "tug")]
    public class ClassTugFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassTug(preamble, classLabel);
            ClassTug.Hydrate(parent, reader, obj as ClassTug);
            return true;
        }
    }
    public class ClassTug : ClassHoverCraft
    {
        public UInt32 cargo { get; set; }

        public ClassTug(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassTug? obj)
        {
            IBZNToken tok;

            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (reader.Format == BZNFormat.Battlezone && reader.Version == 1045)
                {
                    // This is due to bvapc26, assumed to be a tug,in "bdmisn26.bzn"
                    if (!tok.Validate("undefptr", BinaryFieldType.DATA_PTR))
                        if (!tok.Validate("state", BinaryFieldType.DATA_PTR))
                            throw new Exception("Failed to parse undefptr/state/PTR");
                }
                else
                {
                    //if (!tok.Validate("dropoff", BinaryFieldType.DATA_PTR)) throw new Exception("Failed to parse dropoff/PTR");
                    if (!tok.Validate("undefptr", BinaryFieldType.DATA_PTR)) throw new Exception("Failed to parse undefptr/PTR");
                }
                if (obj != null) obj.cargo = tok.GetUInt32H(); // cargo
            }
            else if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version < 1109)
                {
                    if (parent.SaveType != SaveType.BZN)
                    {
                        // 2 things to read here, cargoHandle and lastPosit
                    }
                }
            }

            ClassHoverCraft.Hydrate(parent, reader, obj as ClassHoverCraft);

            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version >= 1109)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("state", BinaryFieldType.DATA_VOID))
                        throw new Exception("Failed to parse state/VOID");
                    if (obj != null) obj.state = (VEHICLE_STATE)tok.GetUInt32HR();

                    tok = reader.ReadToken();
                    if (!tok.Validate("cargoHandle", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse cargoHandle/LONG");
                    if (obj != null) obj.cargo = tok.GetUInt32();

                    if (parent.SaveType != SaveType.BZN)
                    {
                        //(a2->vftable->field_24)(a2, this + 2364, 12, "lastPosit");
                    }
                }
                if (parent.SaveType == SaveType.JOIN || parent.SaveType == SaveType.LOCKSTEP)
                {
                    //(a2->vftable->out_float)(a2, this + 2340, 4, "dockSpeed");
                    //(a2->vftable->out_float)(a2, this + 2344, 4, "delayTimer");
                    //(a2->vftable->out_float)(a2, this + 2348, 4, "timeDeploy");
                    //(a2->vftable->out_float)(a2, this + 2352, 4, "timeUndeploy");
                }
                if (parent.SaveType == 0)
                {
                    // stuff
                }
            }
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassTug obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            if (writer.Format == BZNFormat.Battlezone || writer.Format == BZNFormat.BattlezoneN64)
            {
                if (writer.Format == BZNFormat.Battlezone && writer.Version == 1045)
                {
                    writer.WritePtr("undefptr", obj.cargo); // dropoff
                }
                else
                {
                    writer.WritePtr("undefptr", obj.cargo);
                }
            }
            else if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version < 1109)
                {
                    if (parent.SaveType != SaveType.BZN)
                    {
                        // 2 things to write here, cargoHandle and lastPosit
                    }
                }
            }

            ClassHoverCraft.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);

            if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version >= 1109)
                {
                    writer.WriteVoidBytes("state", (UInt32)obj.state);
                    writer.WriteUnsignedValues("cargoHandle", obj.cargo);

                    if (parent.SaveType != SaveType.BZN)
                    {
                        //(a2->vftable->field_24)(a2, this + 2364, 12, "lastPosit");
                    }
                }
                if (parent.SaveType == SaveType.JOIN || parent.SaveType == SaveType.LOCKSTEP)
                {
                    //(a2->vftable->out_float)(a2, this + 2340, 4, "dockSpeed");
                    //(a2->vftable->out_float)(a2, this + 2344, 4, "delayTimer");
                    //(a2->vftable->out_float)(a2, this + 2348, 4, "timeDeploy");
                    //(a2->vftable->out_float)(a2, this + 2352, 4, "timeUndeploy");
                }
                if (parent.SaveType == 0)
                {
                    // stuff
                }
            }
        }
    }
}
