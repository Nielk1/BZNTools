using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "bomber")]
    public class ClassBomberFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassBomber(preamble, classLabel);
            ClassBomber.Hydrate(parent, reader, obj as ClassBomber);
            return true;
        }
    }
    public class ClassBomber : ClassHoverCraft
    {
        protected float m_ReloadTime { get; set; }
        public ClassBomber(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassBomber? obj)
        {
            IBZNToken tok;

            tok = reader.ReadToken();
            if (!tok.Validate("state", BinaryFieldType.DATA_VOID)) throw new Exception("Failed to parse state/VOID");
            UInt32 state = tok.GetUInt32H();

            if (parent.SaveType != SaveType.BZN)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("m_ReloadTime", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse m_ReloadTime/FLOAT");
                if (obj != null) obj.m_ReloadTime = tok.GetSingle();
            }

            ClassHoverCraft.Hydrate(parent, reader, obj as ClassHoverCraft);
        }
    }
}
