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
            {
                obj = new ClassBomber(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                ClassBomber.Hydrate(parent, reader, obj as ClassBomber);
                return true;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassBomber : ClassHoverCraft
    {
        public float m_ReloadTime { get; set; }

        public ClassBomber(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
            m_ReloadTime = 0;
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


        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassBomber? obj)
        {
            IBZNToken? tok;

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("state", BinaryFieldType.DATA_VOID)) throw new Exception("Failed to parse state/VOID");
            tok.ApplyVoidBytes(obj, x => x.state, 0, (v) => (VEHICLE_STATE)BitConverter.ToUInt32(v));

            if (parent.SaveType != SaveType.BZN)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("m_ReloadTime", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse m_ReloadTime/FLOAT");
                tok.ApplySingle(obj, x => x.m_ReloadTime);
            }

            ClassHoverCraft.Hydrate(parent, reader, obj as ClassHoverCraft);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassBomber obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            writer.WriteVoidBytes("state", obj, x => x.state, (v) => BitConverter.GetBytes((UInt32)v));

            if (parent.SaveType != SaveType.BZN)
            {
                writer.WriteSingle("m_ReloadTime", obj, x => x.m_ReloadTime);
            }

            ClassHoverCraft.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
