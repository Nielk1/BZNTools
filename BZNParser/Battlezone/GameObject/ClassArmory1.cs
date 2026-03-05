using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "armory")]
    [ObjectClass(BZNFormat.BattlezoneN64, "armory")]
    public class ClassArmory1Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassArmory1(preamble, classLabel);
            ClassArmory1.Hydrate(parent, reader, obj as ClassArmory1);
            return true;
        }
    }
    public class ClassArmory1 : ClassProducer
    {
        public ClassArmory1(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassArmory1? obj)
        {
            ClassProducer.Hydrate(parent, reader, obj as ClassProducer);
        }
    }
}
