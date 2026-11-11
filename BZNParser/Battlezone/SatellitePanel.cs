using BZNParser.Tokenizer;
using System;

namespace BZNParser.Battlezone
{
    /// <summary>
    /// BZ2 SatellitePanel tail data (version &gt;= 1125): <c>hasEntered</c>, <c>ownerObj[3]</c>, and
    /// <c>turnOff</c> (when version &gt;= 1198).
    /// <para>
    /// The version spec requires this data to be present. When a source BZN omits it, the object still
    /// exists but every member that should be there is tagged <see cref="BZNParser.Tokenizer.Malformation.MISSING_FIELD"/>
    /// so the writer reproduces the omission in mirror mode instead of fabricating a value.
    /// </para>
    /// </summary>
    public class SatellitePanel : IMalformable
    {
        public bool hasEntered { get; set; }
        public Int32 ownerObj0 { get; set; }
        public Int32 ownerObj1 { get; set; }
        public Int32 ownerObj2 { get; set; }
        public bool turnOff { get; set; }

        private readonly IMalformable.MalformationManager _malformationManager;
        public IMalformable.MalformationManager Malformations => _malformationManager;
        public SatellitePanel()
        {
            this._malformationManager = new IMalformable.MalformationManager(this);
        }
        public void ClearMalformations()
        {
            Malformations.Clear();
        }
        private bool blockAutoFixMalformations = false;
        public void DisableMalformationAutoFix()
        {
            blockAutoFixMalformations = true;
        }
        public void EnableMalformationAutoFix()
        {
            blockAutoFixMalformations = false;
        }

        /// <summary>
        /// Reads a SatellitePanel. The object is always produced when the version spec requires it; when
        /// the source omits the data its members are tagged missing (and the stream is rewound so the
        /// caller can read the following PadData). Returns false only on a genuine parse failure.
        /// </summary>
        public static bool Create(BZNFileBattlezone parent, BZNStreamReader reader, out SatellitePanel? obj)
        {
            obj = new SatellitePanel();
            obj.DisableMalformationAutoFix();
            try
            {
                return SatellitePanel.Hydrate(parent, reader, obj).Success;
            }
            finally
            {
                obj.EnableMalformationAutoFix();
            }
        }

        public static ParseResult Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, SatellitePanel? obj)
        {
            IBZNToken? tok;

            reader.Bookmark.Mark();

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("hasEntered", BinaryFieldType.DATA_BOOL))
            {
                if (tok != null && tok.Validate("PadData", BinaryFieldType.DATA_VOID))
                {
                    // The SatellitePanel data is missing even though the version requires it. Rewind so the
                    // caller reads PadData, and tag every member that should exist as missing so the writer
                    // reproduces the omission in mirror mode.
                    reader.Bookmark.RevertToBookmark();
                    MarkAllMissing(reader.Version, obj);
                    return ParseResult.Ok();
                }
                else
                {
                    reader.Bookmark.Commit();
                    return ParseResult.Fail("Failed to parse hasEntered/BOOL");
                }
            }
            else
            {
                reader.Bookmark.Commit();
                tok.ApplyBoolean(obj, x => x.hasEntered);
                for (int i = 0; i < 3/*MAX_WORLDS*/; i++)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("ownerObj", BinaryFieldType.DATA_LONG))
                        return ParseResult.Fail("Failed to parse ownerObj/LONG");
                    if (i == 0) tok.ApplyInt32(obj, x => x.ownerObj0);
                    else if (i == 1) tok.ApplyInt32(obj, x => x.ownerObj1);
                    else tok.ApplyInt32(obj, x => x.ownerObj2);
                }

                if (reader.Version >= 1198)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("turnOff", BinaryFieldType.DATA_BOOL))
                        return ParseResult.Fail("Failed to parse turnOff/BOOL");
                    tok.ApplyBoolean(obj, x => x.turnOff);
                }
            }

            return ParseResult.Ok();
        }

        /// <summary>
        /// Tags each member that the given version requires as <see cref="BZNParser.Tokenizer.Malformation.MISSING_FIELD"/>.
        /// </summary>
        private static void MarkAllMissing(int version, SatellitePanel? obj)
        {
            if (obj == null) return;
            obj.Malformations.AddMissingField<SatellitePanel, bool>(x => x.hasEntered, 0);
            obj.Malformations.AddMissingField<SatellitePanel, Int32>(x => x.ownerObj0, 0);
            obj.Malformations.AddMissingField<SatellitePanel, Int32>(x => x.ownerObj1, 0);
            obj.Malformations.AddMissingField<SatellitePanel, Int32>(x => x.ownerObj2, 0);
            if (version >= 1198)
                obj.Malformations.AddMissingField<SatellitePanel, bool>(x => x.turnOff, 0);
        }

        public void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            writer.WriteBoolean("hasEntered", this, x => x.hasEntered);
            writer.WriteInt32("ownerObj", this, x => x.ownerObj0);
            writer.WriteInt32("ownerObj", this, x => x.ownerObj1);
            writer.WriteInt32("ownerObj", this, x => x.ownerObj2);
            if (writer.Version >= 1198)
                writer.WriteBoolean("turnOff", this, x => x.turnOff);
        }
    }
}
