using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    public interface IClassFactory
    {
        bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true);
    }
}
