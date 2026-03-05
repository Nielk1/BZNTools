using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "geyser")]
    [ObjectClass(BZNFormat.BattlezoneN64, "geyser")]
    public class ClassGeizerFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassGeizer(preamble, classLabel);
            ClassGeizer.Hydrate(parent, reader, obj as ClassGeizer);
            return true;
        }
    }
    public class ClassGeizer : ClassBuilding
    {
        public UInt32 undefptr { get; set; }

        public ClassGeizer(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassGeizer? obj)
        {
            ClassBuilding.Hydrate(parent, reader, obj as ClassBuilding);
        }
    }
}
