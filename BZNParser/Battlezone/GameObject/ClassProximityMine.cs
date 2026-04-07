using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "proximity")]
    [ObjectClass(BZNFormat.BattlezoneN64, "proximity")]
    [ObjectClass(BZNFormat.Battlezone2, "proximity")]
    public class ClassProximityMineFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassProximityMine(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                ClassProximityMine.Hydrate(parent, reader, obj as ClassProximityMine);
                return true;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassProximityMine : ClassMine
    {
        public ClassProximityMine(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassProximityMine? obj)
        {
            ClassMine.Hydrate(parent, reader, obj as ClassMine);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassProximityMine obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            ClassMine.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
