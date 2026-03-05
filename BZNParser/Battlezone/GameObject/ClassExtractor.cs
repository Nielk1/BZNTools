using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "extractor")]
    public class ClassExtractorFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassExtractor(preamble, classLabel);
            ClassExtractor.Hydrate(parent, reader, obj as ClassExtractor);
            return true;
        }
    }
    public class ClassExtractor : ClassBuilding
    {
        public ClassExtractor(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassExtractor? obj)
        {
            IBZNToken tok;

            tok = reader.ReadToken();
            if (!tok.Validate("scrapTimer", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse scrapTimer/FLOAT");
            //saveClass = tok.GetSingle();

            if (reader.Version < 1147)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("saveClass", BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse saveClass/CHAR");
                string saveClass = tok.GetString();

                if (!string.IsNullOrEmpty(saveClass))
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("saveTeam", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse saveTeam/LONG");
                    //tok.GetUInt32();

                    tok = reader.ReadToken();
                    if (!tok.Validate("saveSeqno", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse saveSeqno/LONG");
                    //seqno = tok.GetUInt32H();

                    string saveLabel = reader.ReadSizedString_BZ2_1145("saveLabel", 32);
                    string saveName = reader.ReadSizedString_BZ2_1145("saveName", 32);
                }
            }

            if (reader.Version > 1102)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("animStart", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse animStart/BOOL");
            }

            ClassBuilding.Hydrate(parent, reader, obj as ClassBuilding);
        }
    }
}
