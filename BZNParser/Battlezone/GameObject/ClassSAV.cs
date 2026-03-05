using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "sav")]
    [ObjectClass(BZNFormat.BattlezoneN64, "sav")]
    [ObjectClass(BZNFormat.Battlezone2, "sav")]
    public class ClassSAVFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassSAV(preamble, classLabel);
            ClassSAV.Hydrate(parent, reader, obj as ClassSAV);
            return true;
        }
    }
    public class ClassSAV : ClassHoverCraft
    {
        public ClassSAV(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassSAV? obj)
        {
            ClassHoverCraft.Hydrate(parent, reader, obj as ClassHoverCraft);
        }
    }
}
