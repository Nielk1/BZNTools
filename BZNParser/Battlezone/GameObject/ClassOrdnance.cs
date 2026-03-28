using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    public class ClassOrdnance : Entity // TODO this linkage to GameObject is fake just to get us compiling, when we add logic for saves we will need to revisit this and maybe make the entity root object
    {
        public ClassOrdnance(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassOrdnance? obj)
        {

        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassOrdnance obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
        }
    }
}
