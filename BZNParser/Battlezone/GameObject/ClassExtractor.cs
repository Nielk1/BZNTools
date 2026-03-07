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
        public float scrapTimer { get; set; }
        public bool animStart { get; set; }
        public string saveLabel { get; set; }
        public string saveName { get; set; }

        public ClassExtractor(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassExtractor? obj)
        {
            IBZNToken tok;

            tok = reader.ReadToken();
            if (!tok.Validate("scrapTimer", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse scrapTimer/FLOAT");
            if (obj != null) obj.scrapTimer = tok.GetSingle();

            if (reader.Version < 1147)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("saveClass", BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse saveClass/CHAR");
                string saveClass = tok.GetString();
                if (obj != null) obj.saveClass = saveClass;

                if (!string.IsNullOrEmpty(saveClass))
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("saveTeam", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse saveTeam/LONG");
                    if (obj != null) obj.saveTeam = tok.GetInt32();

                    tok = reader.ReadToken();
                    if (!tok.Validate("saveSeqno", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse saveSeqno/LONG");
                    if (obj != null) obj.saveSeqno = tok.GetInt32H();

                    string saveLabel = reader.ReadSizedString_BZ2_1145("saveLabel", 32);
                    if (obj != null) obj.saveLabel = saveLabel;
                    string saveName = reader.ReadSizedString_BZ2_1145("saveName", 32);
                    if (obj != null) obj.saveName = saveName;
                }
            }

            if (reader.Version > 1102)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("animStart", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse animStart/BOOL");
                if (obj != null) obj.animStart = tok.GetBoolean();
            }

            ClassBuilding.Hydrate(parent, reader, obj as ClassBuilding);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassExtractor obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            writer.WriteFloats("scrapTimer", obj.scrapTimer);
            if (writer.Version < 1147)
            {
                writer.WriteChars("saveClass", obj.saveClass);
                if (!string.IsNullOrEmpty(obj.saveClass))
                {
                    writer.WriteSignedValues("saveTeam", obj.saveTeam);
                    writer.WriteUnsignedHexLValues("saveSeqno", (UInt16)obj.saveSeqno); // unsure if this down-cast is safe, if bool writes LONG instead of SHORT it doesn't
                    writer.WriteSizedString_BZ2_1145("saveLabel", 32, obj.saveLabel); // TODO: figure out what this actually does
                    writer.WriteSizedString_BZ2_1145("saveName", 32, obj.saveName); // TODO: figure out what this actually does
                }
            }
            if (writer.Version > 1102)
            {
                writer.WriteBooleans("animStart", obj.animStart);
            }
            ClassBuilding.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
