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
        public ClassWalker2(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassWalker2? obj)
        {
            IBZNToken tok;

            if (reader.Version == 1041) // version is special case for bz2001.bzn
            {
                tok = reader.ReadToken();
                if (!tok.Validate("Walker_IK", BinaryFieldType.DATA_VOID)) throw new Exception("Failed to parse Walker_IK/VOID");
                byte[] data = tok.GetBytes();

                ClassCraft.Hydrate(parent, reader, obj as ClassCraft);
                return;
            }

            if (reader.Version < 1067)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("Pin_Foot", BinaryFieldType.DATA_VOID)) throw new Exception("Failed to parse Pin_Foot/VOID");

                tok = reader.ReadToken();
                if (!tok.Validate("Current_Index", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse Current_Index/FLOAT");

                tok = reader.ReadToken();
                if (!tok.Validate("Anim_State", BinaryFieldType.DATA_VOID)) throw new Exception("Failed to parse Anim_State/VOID");

                tok = reader.ReadToken();
                if (!tok.Validate("Lead", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse Lead/LONG");

                tok = reader.ReadToken();
                if (!tok.Validate("Tail", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse Tail/LONG");

                tok = reader.ReadToken();
                if (!tok.Validate("Control_Queue", BinaryFieldType.DATA_VOID)) throw new Exception("Failed to parse Control_Queue/VOID");
            }

            // parent.SaveType != SaveType.BZN stuff

            ClassCraft.Hydrate(parent, reader, obj as ClassCraft);
        }
    }
}
