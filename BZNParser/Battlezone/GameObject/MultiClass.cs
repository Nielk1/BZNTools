using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    public class MultiClass : Entity
    {
        public List<(Entity Object, bool Expected, long Next, string Name)> Candidates { get; private set; }
        public override string ClassLabel { get { return $"[{string.Join(',', Candidates.Select(dr => dr.Object.ClassLabel))}]"; } }


        public MultiClass(EntityDescriptor preamble, List<(Entity Object, bool Expected, long Next, string Name)> candidates) : base(preamble, null)
        {
            this.Candidates = candidates;
        }
        public override string ToString()
        {
            return $"{base.ToString()} [{string.Join(',', Candidates.Select(dr => dr.Object.ToString()))}]";
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            throw new Exception(label + ": MultiClass cannot be written");
        }
    }
}
