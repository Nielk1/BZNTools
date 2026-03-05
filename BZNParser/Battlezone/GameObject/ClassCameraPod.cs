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
                    IBZNToken tok;

                    tok = reader.ReadToken();
                    if (!tok.Validate("navSlot", BinaryFieldType.DATA_LONG))
                        throw new Exception("Failed to parse navSlot/LONG");
                    if (obj != null) obj.navSlot = tok.GetInt32();
                }
            }

            ClassPowerUp.Hydrate(parent, reader, obj as ClassPowerUp);
        }
    }
}
