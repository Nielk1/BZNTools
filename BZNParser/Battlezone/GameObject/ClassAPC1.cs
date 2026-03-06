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
                obj = new ClassAPC1(preamble, classLabel);
            ClassAPC1.Hydrate(parent, reader, obj as ClassAPC1);
            return true;
        }
    }
    public class ClassAPC1 : ClassHoverCraft
    {
        public int soldierCount { get; set; }
        public ClassAPC1(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassAPC1? obj)
        {
            IBZNToken tok;

            tok = reader.ReadToken();
            if (!tok.Validate("soldierCount", BinaryFieldType.DATA_LONG))
                throw new Exception("Failed to parse soldierCount/LONG");
            if (obj != null) obj.soldierCount = tok.GetInt32();

            tok = reader.ReadToken();
            if (!tok.Validate("state", BinaryFieldType.DATA_VOID))
                throw new Exception("Failed to parse state/VOID");
            if (obj != null) obj.state = (VEHICLE_STATE)tok.GetUInt32(); // state

            ClassHoverCraft.Hydrate(parent, reader, obj as ClassHoverCraft);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassAPC1 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            writer.WriteSignedValues("soldierCount", obj.soldierCount);
            writer.WriteVoidBytes("state", (UInt32)obj.state);
            ClassHoverCraft.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
