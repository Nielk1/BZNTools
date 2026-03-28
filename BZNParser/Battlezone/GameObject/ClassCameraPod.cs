using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "camerapod")]
    [ObjectClass(BZNFormat.BattlezoneN64, "camerapod")]
    [ObjectClass(BZNFormat.Battlezone2, "camerapod")]
    public class ClassCameraPodFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassCameraPod(preamble, classLabel);
            ClassCameraPod.Hydrate(parent, reader, obj as ClassCameraPod);
            return true;
        }
    }
    public class ClassCameraPod : ClassPowerUp
    {
        protected int navSlot { get; set; }
        public ClassCameraPod(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassCameraPod? obj)
        {
            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version >= 1148)
                {
                    IBZNToken? tok;

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("navSlot", BinaryFieldType.DATA_LONG))
                        throw new Exception("Failed to parse navSlot/LONG");
                    tok.ApplyInt32(obj, x => x.navSlot);
                }
            }

            ClassPowerUp.Hydrate(parent, reader, obj as ClassPowerUp);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassCameraPod obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version >= 1148)
                {
                    writer.WriteInt32("navSlot", obj, x => x.navSlot);
                }
            }
            ClassPowerUp.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
