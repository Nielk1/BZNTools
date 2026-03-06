using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "minelayer")]
    [ObjectClass(BZNFormat.BattlezoneN64, "minelayer")]
    [ObjectClass(BZNFormat.Battlezone2, "minelayer")]
    public class ClassMinelayerFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassMinelayer(preamble, classLabel);
            ClassMinelayer.Hydrate(parent, reader, obj as ClassMinelayer);
            return true;
        }
    }
    public class ClassMinelayer : ClassHoverCraft
    {
        public ClassMinelayer(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassMinelayer? obj)
        {
            ClassHoverCraft.Hydrate(parent, reader, obj as ClassHoverCraft);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassMinelayer obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            ClassHoverCraft.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
