using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "iv_walker")]
    [ObjectClass(BZNFormat.Battlezone2, "fv_walker")]
    public class ClassWalker2Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassWalker2(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                ClassWalker2.Hydrate(parent, reader, obj as ClassWalker2);
                return true;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassWalker2 : ClassCraft
    {
        public byte[]? Walker_IK { get; set; }
        public uint Pin_Foot { get; set; }
        public float Current_Index { get; set; }
        public uint Anim_State { get; set; }
        public uint Lead { get; set; }
        public uint Tail { get; set; }
        public uint Control_Queue { get; set; }

        public ClassWalker2(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
            Pin_Foot = 0;
            Current_Index = 0;
            Anim_State = 0;
            Lead = 0;
            Tail = 0;
            Control_Queue = 0;
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


        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassWalker2? obj)
        {
            IBZNToken? tok;

            if (reader.Version == 1041) // version is special case for bz2001.bzn
            {
                // 108 bytes of stuff
                // DE-7B-37-40-EA-FF-69-3E-98-B3-EC-9A-5B-EC-48-C0-C3-B5-32-C0-08-00-00-00-91-0A-86-3F-00-00-10-41-00-00-50-41-00-00-20-41-00-00-80-3F-DA-0F-49-3F-DA-0F-49-3F-91-0A-86-3F-00-00-10-41-00-00-50-41-00-00-20-41-00-00-80-3F-DA-0F-49-3F-DA-0F-49-3F-91-0A-86-3F-00-00-10-41-00-00-50-41-00-00-20-41-00-00-80-3F-DA-0F-49-3F-DA-0F-49-3F
                // 3B-75-8F-41-4B-59-89-BE-03-00-00-80-00-00-00-00-00-00-00-80-08-00-00-00-91-0A-86-3F-00-00-10-41-00-00-50-41-00-00-20-41-00-00-80-3F-DA-0F-49-3F-DA-0F-49-3F-91-0A-86-3F-00-00-10-41-00-00-50-41-00-00-20-41-00-00-80-3F-DA-0F-49-3F-DA-0F-49-3F-91-0A-86-3F-00-00-10-41-00-00-50-41-00-00-20-41-00-00-80-3F-DA-0F-49-3F-DA-0F-49-3F
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("Walker_IK", BinaryFieldType.DATA_VOID)) throw new Exception("Failed to parse Walker_IK/VOID");
                tok.ApplyVoidBytes(obj, x => x.Walker_IK);

                ClassCraft.Hydrate(parent, reader, obj as ClassCraft);
                return;
            }

            if (reader.Version < 1067)
            {
                // this path is currently untested it seems?

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("Pin_Foot", BinaryFieldType.DATA_VOID)) throw new Exception("Failed to parse Pin_Foot/VOID");
                tok.ApplyVoidBytes(obj, x => x.Pin_Foot, 0, (v) => BitConverter.ToUInt32(v));

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("Current_Index", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse Current_Index/FLOAT");
                tok.ApplySingle(obj, x => x.Current_Index);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("Anim_State", BinaryFieldType.DATA_VOID)) throw new Exception("Failed to parse Anim_State/VOID");
                tok.ApplyVoidBytes(obj, x => x.Anim_State, 0, (v) => BitConverter.ToUInt32(v));

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("Lead", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse Lead/LONG");
                tok.ApplyUInt32(obj, x => x.Lead);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("Tail", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse Tail/LONG");
                tok.ApplyUInt32(obj, x => x.Tail);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("Control_Queue", BinaryFieldType.DATA_VOID)) throw new Exception("Failed to parse Control_Queue/VOID");
                tok.ApplyVoidBytes(obj, x => x.Control_Queue, 0, (v) => BitConverter.ToUInt32(v));
            }

            ClassCraft.Hydrate(parent, reader, obj as ClassCraft);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassWalker2 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (writer.Version == 1041) // version is special case for bz2001.bzn
            {
                writer.WriteVoidBytesL("Walker_IK", obj, x => x.Walker_IK);
                ClassCraft.Dehydrate(obj, parent, writer, binary, save);
                return;
            }
            if (writer.Version < 1067)
            {
                writer.WriteVoidBytes("Pin_Foot", obj, x => x.Pin_Foot);
                writer.WriteSingle("Current_Index", obj, x => x.Current_Index);
                writer.WriteVoidBytes("Anim_State", obj, x => x.Anim_State);
                writer.WriteUInt32("Lead", obj, x => x.Lead);
                writer.WriteUInt32("Tail", obj, x => x.Tail);
                writer.WriteVoidBytes("Control_Queue", obj, x => x.Control_Queue);
            }

            // parent.SaveType != SaveType.BZN stuff

            ClassCraft.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
