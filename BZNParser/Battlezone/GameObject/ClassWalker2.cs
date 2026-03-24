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
                obj = new ClassWalker2(preamble, classLabel);
            ClassWalker2.Hydrate(parent, reader, obj as ClassWalker2);
            return true;
        }
    }
    public class ClassWalker2 : ClassCraft
    {
        public byte[] Walker_IK { get; set; }
        public uint Pin_Foot { get; set; }
        public float Current_Index { get; set; }
        public uint Anim_State { get; set; }
        public uint Lead { get; set; }
        public uint Tail { get; set; }
        public uint Control_Queue { get; set; }

        public ClassWalker2(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassWalker2? obj)
        {
            IBZNToken? tok;

            if (reader.Version == 1041) // version is special case for bz2001.bzn
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("Walker_IK", BinaryFieldType.DATA_VOID)) throw new Exception("Failed to parse Walker_IK/VOID");
                if (obj != null) obj.Walker_IK = tok.GetBytes();

                ClassCraft.Hydrate(parent, reader, obj as ClassCraft);
                return;
            }

            if (reader.Version < 1067)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("Pin_Foot", BinaryFieldType.DATA_VOID)) throw new Exception("Failed to parse Pin_Foot/VOID");
                if (obj != null) obj.Pin_Foot = tok.GetUInt32H();

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("Current_Index", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse Current_Index/FLOAT");
                tok.ApplySingle(obj, x => x.Current_Index);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("Anim_State", BinaryFieldType.DATA_VOID)) throw new Exception("Failed to parse Anim_State/VOID");
                if (obj != null) obj.Anim_State = tok.GetUInt32H();

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("Lead", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse Lead/LONG");
                //if (obj != null) obj.Lead = tok.GetUInt32H(); // I don't think this should be hex, but do confirm, if so the writer needs fixing too
                tok.ApplyUInt32(obj, x => x.Current_Index);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("Tail", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse Tail/LONG");
                //if (obj != null) obj.Tail = tok.GetUInt32H(); // I don't think this should be hex, but do confirm, if so the writer needs fixing too
                tok.ApplyUInt32(obj, x => x.Current_Index);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("Control_Queue", BinaryFieldType.DATA_VOID)) throw new Exception("Failed to parse Control_Queue/VOID");
                if (obj != null) obj.Control_Queue = tok.GetUInt32H();
            }

            // parent.SaveType != SaveType.BZN stuff

            ClassCraft.Hydrate(parent, reader, obj as ClassCraft);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassWalker2 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            if (writer.Version == 1041) // version is special case for bz2001.bzn
            {
                writer.WriteVoidBytesL("Walker_IK", obj.Walker_IK);
                ClassCraft.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
                return;
            }
            if (writer.Version < 1067)
            {
                writer.WriteVoidBytes("Pin_Foot", obj.Pin_Foot);
                writer.WriteSingle("Current_Index", obj, x => x.Current_Index);
                writer.WriteVoidBytes("Anim_State", obj.Anim_State);
                writer.WriteUInt32("Lead", obj, x => x.Lead);
                writer.WriteUInt32("Tail", obj, x => x.Tail);
                writer.WriteVoidBytes("Control_Queue", obj.Control_Queue);
            }

            // parent.SaveType != SaveType.BZN stuff

            ClassCraft.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
