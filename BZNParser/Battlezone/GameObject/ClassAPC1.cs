using BZNParser.Tokenizer;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "apc")]
    [ObjectClass(BZNFormat.BattlezoneN64, "apc")]
    public class ClassAPC1Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassAPC1(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                ClassAPC1.Hydrate(parent, reader, obj as ClassAPC1);
                return true;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassAPC1 : ClassHoverCraft
    {
        public int SoldierCount
        {
            get
            {
                return soldierCount;
            }
            set
            {
                if (!blockAutoFixMalformations && value != soldierCount)
                    Malformations.Clear<ClassAPC1, int>(x => x.SoldierCount);
                soldierCount = value;
            }
        }
        private int soldierCount;

        public ClassAPC1(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
            soldierCount = 0;
        }
        public override void ClearMalformations()
        {
            Malformations.Clear();
            base.ClearMalformations();
        }

        private bool blockAutoFixMalformations = false;
        public override void DisableMalformationAutoFix()
        {
            blockAutoFixMalformations = true;
            base.DisableMalformationAutoFix();
        }

        public override void EnableMalformationAutoFix()
        {
            blockAutoFixMalformations = false;
            base.EnableMalformationAutoFix();
        }

        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassAPC1? obj)
        {
            IBZNToken? tok;

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("soldierCount", BinaryFieldType.DATA_LONG))
                throw new Exception("Failed to parse soldierCount/LONG");
            tok.ApplyInt32(obj, x => x.SoldierCount);

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("state", BinaryFieldType.DATA_VOID))
                throw new Exception("Failed to parse state/VOID");
            tok.ApplyVoidBytes(obj, x => x.state, 0, (v) => (VEHICLE_STATE)BitConverter.ToUInt32(v));

            ClassHoverCraft.Hydrate(parent, reader, obj as ClassHoverCraft);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassAPC1 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            writer.WriteInt32("soldierCount", obj, x => x.SoldierCount);
            writer.WriteVoidBytes("state", obj, x => x.state, (v) => BitConverter.GetBytes((UInt32)v));
            ClassHoverCraft.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
