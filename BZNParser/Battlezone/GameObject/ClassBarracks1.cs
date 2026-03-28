using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "barracks")]
    [ObjectClass(BZNFormat.BattlezoneN64, "barracks")]
    public class ClassBarracks1Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassBarracks1(preamble, classLabel);
            ClassBarracks1.Hydrate(parent, reader, obj as ClassBarracks1);
            return true;
        }
    }
    public class ClassBarracks1 : ClassBuilding
    {
        protected Int32 nextEmptyCheck { get; set; }
        public ClassBarracks1(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassBarracks1? obj)
        {
            if (parent.SaveType != SaveType.BZN)
            {
                IBZNToken? tok = reader.ReadToken();
                if (tok == null || !tok.Validate("nextEmptyCheck", BinaryFieldType.DATA_LONG))
                    throw new Exception("Failed to parse nextEmptyCheck/LONG");
                tok.ApplyInt32(obj, x => x.nextEmptyCheck);
            }

            ClassBuilding.Hydrate(parent, reader, obj as ClassBuilding);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassBarracks1 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (parent.SaveType != SaveType.BZN)
            {
                writer.WriteInt32("nextEmptyCheck", obj, x => x.nextEmptyCheck);
            }
            ClassBuilding.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
