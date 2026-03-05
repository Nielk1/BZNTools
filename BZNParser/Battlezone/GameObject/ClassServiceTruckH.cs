using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "serviceh")]
    public class ClassServiceTruckHFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassServiceTruckH(preamble, classLabel);
            ClassServiceTruckH.Hydrate(parent, reader, obj as ClassServiceTruckH);
            return true;
        }
    }
    public class ClassServiceTruckH : ClassHoverCraft
    {
        public ClassServiceTruckH(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassServiceTruckH? obj)
        {
            ClassHoverCraft.Hydrate(parent, reader, obj as ClassHoverCraft);
        }
    }
}
