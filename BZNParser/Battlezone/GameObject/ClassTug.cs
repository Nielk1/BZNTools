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
            {
                obj = new ClassTug(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                return ClassTug.Hydrate(parent, reader, obj as ClassTug).Success;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassTug : ClassHoverCraft
    {
        public UInt32 cargo { get; set; }

        public ClassTug(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
            cargo = 0;
        }

        public override void ClearMalformations()
        {
            Malformations.Clear();
            base.ClearMalformations();
        }

        public override void DisableMalformationAutoFix()
        {
            base.DisableMalformationAutoFix();
        }

        public override void EnableMalformationAutoFix()
        {
            base.EnableMalformationAutoFix();
        }


        public static ParseResult Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassTug? obj)
        {
            IBZNToken? tok;

            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (reader.Format == BZNFormat.Battlezone)
                {
                    // This is due to bvapc26, assumed to be a tug,in "bdmisn26.bzn"
                    if (tok == null)
                        return ParseResult.Fail("Failed to parse undefptr/state/PTR");
                    if (!tok.Validate("undefptr", BinaryFieldType.DATA_PTR))
                    {
                        if (reader.Version == 1045 && tok.Validate("state", BinaryFieldType.DATA_PTR))
                        {
                            obj?.Malformations?.AddIncorrectName<ClassTug, UInt32>(x => x.cargo, "state");
                        }
                        else
                        {
                            return ParseResult.Fail("Failed to parse undefptr/state/PTR");
                        }
                    }
                    tok.ApplyUInt32H8(obj, x => x.cargo);
                }
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

            var tmp = ClassHoverCraft.Hydrate(parent, reader, obj as ClassHoverCraft);
            if (!tmp.Success)
                return tmp;

			if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version >= 1109)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("state", BinaryFieldType.DATA_VOID))
                        return ParseResult.Fail("Failed to parse state/VOID");
                    //if (obj != null) obj.state = (VEHICLE_STATE)tok.GetUInt32HR();
                    tok.ApplyVoidBytes(obj, x => x.state, 0, (v) => (VEHICLE_STATE)BitConverter.ToUInt32(v));

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("cargoHandle", BinaryFieldType.DATA_LONG))
                        return ParseResult.Fail("Failed to parse cargoHandle/LONG");
                    //if (obj != null) obj.cargo = tok.GetUInt32();
                    tok.ApplyUInt32(obj, x => x.cargo);

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

            return ParseResult.Ok();
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassTug obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (writer.Format == BZNFormat.Battlezone || writer.Format == BZNFormat.BattlezoneN64)
            {
                writer.WritePtr("undefptr", obj, x => x.cargo);
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

            ClassHoverCraft.Dehydrate(obj, parent, writer, binary, save);

            if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version >= 1109)
                {
                    writer.WriteVoidBytes("state", obj, x => x.state, (v) => BitConverter.GetBytes((UInt32)v));
                    writer.WriteUInt32("cargoHandle", obj, x => x.cargo);

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
