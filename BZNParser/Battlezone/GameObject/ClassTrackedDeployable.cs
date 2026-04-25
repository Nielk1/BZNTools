using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    // BZ2
    public class ClassTrackedDeployableFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassTrackedDeployable(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                return ClassTrackedDeployable.Hydrate(parent, reader, obj as ClassTrackedDeployable).Success;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassTrackedDeployable : ClassTrackedVehicle
    {
        public float deployTimer { get; set; }

        public ClassTrackedDeployable(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
            deployTimer = 0;
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


        public static ParseResult Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassTrackedDeployable? obj)
        {
            IBZNToken? tok;

            if (reader.Format == BZNFormat.Battlezone2)
            //(a2->vftable->field_8)(a2, this + 1424, 4, "state");
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("state", BinaryFieldType.DATA_VOID))
                    return ParseResult.Fail("Failed to parse state/VOID"); // type not confirmed
                tok.ApplyVoidBytes(obj, x => x.state, 0, (v) => (VEHICLE_STATE)BitConverter.ToUInt32(v));

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("deployTimer", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse deployTimer/FLOAT");
                tok.ApplySingle(obj, x => x.deployTimer, format: reader.FloatFormat);

                //if (a2[2].vftable)
                //    (a2->vftable->read_long)(a2, this + 2544, 4, "changeState");
            }

            return ClassTrackedVehicle.Hydrate(parent, reader, obj as ClassTrackedVehicle);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassTrackedDeployable obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (writer.Format == BZNFormat.Battlezone2)
            {
                writer.WriteVoidBytes("state", obj, x => x.state, (v) => BitConverter.GetBytes((UInt32)v));
                writer.WriteSingle("deployTimer", obj, x => x.deployTimer);
            }
            ClassTrackedVehicle.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
