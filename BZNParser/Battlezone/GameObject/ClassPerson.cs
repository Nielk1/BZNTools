using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "person")]
    [ObjectClass(BZNFormat.BattlezoneN64, "person")]
    [ObjectClass(BZNFormat.Battlezone2, "person")] // ?
    public class ClassPersonFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassPerson(preamble, classLabel);
            ClassPerson.Hydrate(parent, reader, obj as ClassPerson);
            return true;
        }
    }
    public class ClassPerson : ClassCraft
    {
        public float nextScream { get; set; }

        public ClassPerson(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassPerson? obj)
        {
            IBZNToken? tok;

            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("nextScream", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse nextScream/FLOAT");
                tok.ApplySingle(obj, x => x.nextScream);
            }
            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version == 1047)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("nextScream", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse nextScream/FLOAT");
                    tok.ApplySingle(obj, x => x.nextScream);
                }
                else
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("state", BinaryFieldType.DATA_VOID)) throw new Exception("Failed to parse state/VOID"); // type not confirmed
                    tok.ApplyVoidBytes(obj, x => x.state, 0, (v) => (VEHICLE_STATE)BitConverter.ToUInt32(v));

                    /*if (a2[2].vftable)
                    {
                        (a2->vftable->field_8)(a2, this + 2140, 4, "animMode");
                        (a2->vftable->field_8)(a2, this + 2144, 4, "animStance");
                        (a2->vftable->out_bool)(a2, this + 2200, 1, "fixedRate");
                        (a2->vftable->out_float)(a2, this + 2188, 4, "forceFps");
                        (a2->vftable->out_float)(a2, this + 2192, 4, "forceDir");
                        (a2->vftable->out_bool)(a2, this + 2201, 1, "wasFlying");
                        (a2->vftable->out_bool)(a2, this + 2202, 1, "Alive");
                        (a2->vftable->out_float)(a2, this + 2196, 4, "Dying_Timer");
                        (a2->vftable->out_bool)(a2, this + 2203, 1, "Explosion");
                    }*/
                }
            }

            ClassCraft.Hydrate(parent, reader, obj as ClassCraft);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassPerson obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            if (writer.Format == BZNFormat.Battlezone || writer.Format == BZNFormat.BattlezoneN64)
            {
                writer.WriteSingle("nextScream", obj, x => x.nextScream);
            }
            if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version == 1047)
                {
                    writer.WriteSingle("nextScream", obj, x => x.nextScream);
                }
                else
                {
                    writer.WriteVoidBytes("state", obj, x => x.state, (v) => BitConverter.GetBytes((UInt32)v));

                    /*if (a2[2].vftable)
                    {
                        (a2->vftable->field_8)(a2, this + 2140, 4, "animMode");
                        (a2->vftable->field_8)(a2, this + 2144, 4, "animStance");
                        (a2->vftable->out_bool)(a2, this + 2200, 1, "fixedRate");
                        (a2->vftable->out_float)(a2, this + 2188, 4, "forceFps");
                        (a2->vftable->out_float)(a2, this + 2192, 4, "forceDir");
                        (a2->vftable->out_bool)(a2, this + 2201, 1, "wasFlying");
                        (a2->vftable->out_bool)(a2, this + 2202, 1, "Alive");
                        (a2->vftable->out_float)(a2, this + 2196, 4, "Dying_Timer");
                        (a2->vftable->out_bool)(a2, this + 2203, 1, "Explosion");
                    }*/
                }
            }

            ClassCraft.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
