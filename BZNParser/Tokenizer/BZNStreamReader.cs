using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static BZNParser.Tokenizer.BZNStreamReader;

namespace BZNParser.Tokenizer
{
    public class StreamDefect
    {
        public UInt32? TypeGarbage { get; set; }
        public string? EndPadGarbage { get; set; }
        public byte[]? TruncatedBytesType { get; set; }
        public byte[]? TruncatedBytesSize { get; set; }
        public byte[]? TruncatedBytesData { get; set; }
        public uint? BytesOversized { get; set; }
        public bool IsEmpty()
        {
            if (TypeGarbage.HasValue)
                return false;
            if (EndPadGarbage != null)
                return false;
            if (TruncatedBytesType != null)
                return false;
            if (TruncatedBytesSize != null)
                return false;
            if (TruncatedBytesData != null)
                return false;
            if (BytesOversized.HasValue)
                return false;
            return true;
        }
    }

    public struct AtlasData
    {
        public long Offset { get; set; }
        public long Length { get; set; }
        public bool IsBinary { get; set; }


        public StreamDefect? Defect { get; set; }
        public UInt32? Defect_TypeGarbage
        {
            get
            {
                return Defect?.TypeGarbage;
            }
            set
            {
                if (Defect == null)
                    Defect = new StreamDefect();
                Defect.TypeGarbage = value;
                if (Defect.IsEmpty())
                    Defect = null;
            }
        }
        public string? Defect_EndPadGarbage
        {
            get
            {
                return Defect?.EndPadGarbage;
            }
            set
            {
                if (Defect == null)
                    Defect = new StreamDefect();
                Defect.EndPadGarbage = value;
                if (Defect.IsEmpty())
                    Defect = null;
            }
        }
        public byte[]? Defect_TruncatedBytesType
        {
            get
            {
                return Defect?.TruncatedBytesType;
            }
            set
            {
                if (Defect == null)
                    Defect = new StreamDefect();
                Defect.TruncatedBytesType = value;
                if (Defect.IsEmpty())
                    Defect = null;
            }
        }
        public byte[]? Defect_TruncatedBytesSize
        {
            get
            {
                return Defect?.TruncatedBytesSize;
            }
            set
            {
                if (Defect == null)
                    Defect = new StreamDefect();
                Defect.TruncatedBytesSize = value;
                if (Defect.IsEmpty())
                    Defect = null;
            }
        }
        public byte[]? Defect_TruncatedBytesData
        {
            get
            {
                return Defect?.TruncatedBytesData;
            }
            set
            {
                if (Defect == null)
                    Defect = new StreamDefect();
                Defect.TruncatedBytesData = value;
                if (Defect.IsEmpty())
                    Defect = null;
            }
        }
        public uint? Defect_BytesOversized
        {
            get
            {
                return Defect?.BytesOversized;
            }
            set
            {
                if (Defect == null)
                    Defect = new StreamDefect();
                Defect.BytesOversized = value;
                if (Defect.IsEmpty())
                    Defect = null;
            }
        }
    }

    public class BZNStreamReader : IDisposable
    {
        public enum FloatTextFormat
        {
            G, // common, sometimes appears when new format should
            _9e2, // new format
            _9e3, // sometimes appears in place of new format
        };

        // Format used in ASCII mode for float text
        public FloatTextFormat FloatFormat { get; private set; }
        public bool FloatFormatUnreliable { get; private set; }




        /// <summary>
        /// Value used to indicate that the BZN file does not contain binary data.
        /// This is should be a very high number so a basic offset check can be used to determine if binary data is being read or not.
        /// </summary>
        const long MAGIC_NO_BINARY = long.MaxValue;

        /// <summary>
        /// Lookup for complex variables in ASCII BZN files, the value is the number of sub-tokens that make up the complex variable.
        /// </summary>
        private static readonly Dictionary<string, int> ComplexStringTokenSizeMap = new Dictionary<string, int>
        {
            {"points", 2},
            {"pos", 3},
            {"v", 3},
            {"omega", 3},
            {"Accel", 3},
            {"euler", 9}, // 5 or 9 in a savegame
            {"dropMat", 12},
            {"transform", 12},
            {"startMat", 12},
            {"saveMatrix", 12},
            {"buildMatrix", 12},
            {"bumpers", 3}, // VEC3
            {"Att", 4}, // QUAT
        };

        private Stream BaseStream { get; set; } // Underlying Stream
        public bool EndOfFile()
        {
            return BaseStream.Position >= BaseStream.Length;
        }

        private readonly BookmarkManager _bookmarkManager;
        public BookmarkManager Bookmark => _bookmarkManager;
        public class BookmarkManager
        {
            private readonly BZNStreamReader _reader;
            private readonly Stack<long> _offsets = new();

            public BookmarkManager(BZNStreamReader reader)
            {
                _reader = reader;
            }

            public void Mark()
            {
                _offsets.Push(_reader.BaseStream.Position);
            }

            public void RevertToBookmark()
            {
                // use bookmark and remove it (return, we don't need it anymore)
                if (_offsets.TryPop(out var pos))
                {
                    _reader.BaseStream.Position = pos;
                    _reader.TokenIndex = GetTokenIndex(pos);
                }
            }
            public void RewindToBookmark()
            {
                // use bookmark withoug removing it
                if (_offsets.TryPeek(out var pos))
                {
                    _reader.BaseStream.Position = pos;
                    _reader.TokenIndex = GetTokenIndex(pos);
                }
            }

            public void Commit()
            {
                // keep the current position, so we don't need to do anything but remove the bookmark
                _offsets.TryPop(out _);
            }

            // Set the offset, used for multi-offset situations
            public void Set(long offset)
            {
                _reader.BaseStream.Position = offset;
                _reader.TokenIndex = GetTokenIndex(offset);
            }

            // Get the offset, used for multi-offset situations
            public long Get()
            {
                return _reader.BaseStream.Position;
            }

            private int GetTokenIndex(long offset)
            {
                int idx = _reader.Atlas.FindIndex(dr => dr.Offset <= offset && dr.Offset + dr.Length > offset);
                if (idx >= 0)
                    return idx;
                return _reader.Atlas.Count;
            }
        }

        public int TokenIndex { get; private set; }
        private List<AtlasData> Atlas; // stores token offsets, later will be an object with more metadata for Stream Defects

        /// <summary>
        /// BZN file started in binary.
        /// Only used for Battlezone N64 files.
        /// </summary>
        public bool StartBinary { get { return binaryDataStartOffset == 0; } }
        /// <summary>
        /// BZN file has binary fields.
        /// Normal BZNs always start in ASCII mode and switch to binary mode later.
        /// </summary>
        public bool HasBinary { get { return binaryDataStartOffset != MAGIC_NO_BINARY; } }
        /// <summary>
        /// Stream is currently in binary mode.
        /// </summary>
        public bool InBinary { get { return BaseStream.Position >= binaryDataStartOffset; } }
        /// <summary>
        /// BZN file is big endian.
        /// This applies to Battlezone N64 files.
        /// </summary>
        public bool IsBigEndian { get; private set; }
        /// <summary>
        /// Size of the size field in the binary tokens.
        /// In Battlezone N64 this is 0 (not present).
        /// In Battlezone this is 2 (The type enumeration only needs one byte).
        /// In Battlezone II this is 1.
        /// In Star Trek Armada this is 4 (garbage possible in all but lowest sig byte).
        /// In Star Trek Armada 2 this is 4 (garbage possible in all but lowest sig byte).
        /// </summary>
        public byte TypeSize { get; private set; }
        /// <summary>
        /// Size of the type field in the binary tokens.
        /// In Battlezone N64 this is 2.
        /// In Battlezone this is 2.
        /// In Battlezone II this is 2.
        /// In Star Trek Armada this is 4.
        /// In Star Trek Armada 2 this is 4.
        /// </summary>
        public byte SizeSize { get; private set; }
        /// <summary>
        /// Version of the BZN file.
        /// This can be inconsistent between different BZN types but has rare utility the tokenizer level such as maximum ASCII line length control.
        /// </summary>
        public int Version { get; private set; }
        /// <summary>
        /// Is this a save or a map? Not very useful for the tokenizer and might be removed later.
        /// </summary>
        //public int SaveType { get; private set; }
        /// <summary>
        /// Data alignment in bytes.
        /// </summary>
        public byte AlignmentBytes { get; private set; }
        /// <summary>
        /// Game specific variant of the BZN format.
        /// </summary>
        public BZNFormat Format { get; set; }

        /// <summary>
        /// Where binary fields start.
        /// </summary>
        private long binaryDataStartOffset = MAGIC_NO_BINARY;

        public bool QuoteStrings { get; private set; }
        public byte PointerSize { get; private set; }
        public bool MatrixBigPosit { get; private set; }


        public string filename; // temporary


        // TODO Battlezone BZNs should always end in CRLF, though we have seen some that don't so we need to figure out if they're damaged or not
        // It is unknown if this matters for non Battlezone BZNs
        public long CountCR { get; private set; }
        public long CountLF { get; private set; }
        public long CountCRLF { get; private set; }

        public BZNStreamReader(Stream stream, string filename)
        {
            this.filename = filename;

            _bookmarkManager = new BookmarkManager(this);
            Atlas = new List<AtlasData>();

            long startPosition = stream.Position;
            TokenIndex = 0;

            Format = BZNFormat.Battlezone;
            FloatFormat = FloatTextFormat.G; // default float text format
            FloatFormatUnreliable = false;

            this.BaseStream = stream;

            CountCR = 0;
            CountLF = 0;
            CountCRLF = 0;

            PointerSize = 4;
            MatrixBigPosit = false;

            long position = stream.Position;
            BinaryReader reader = new BinaryReader(stream);
            {
                // assume BZNs always start with a version, check for the string format
                char[] versionname = reader.ReadChars(13);
                if(!versionname.All(c => !char.IsControl(c)))
                    binaryDataStartOffset = 0;

                //stream.Position = position;
                Bookmark.Set(position);

                bool TypeSizeSet = false;
                //TypeSize = 0; // BZn64
                //TypeSize = 1; // BZ2
                TypeSize = 2; // BZ1
                SizeSize = 2; // BZ

                // we are starting in binary, so check for BigEndian since this could be an n64 file
                if (StartBinary)
                {
                    byte[] First2Bytes = new byte[2];
                    reader.Read(First2Bytes, 0, 2);
                    if (First2Bytes[0] == 0x00 && First2Bytes[1] != 0x00)
                    {
                        IsBigEndian = true;
                        TypeSize = 0; // BZn64
                        AlignmentBytes = 2;
                        TypeSizeSet = true;
                        Format = BZNFormat.BattlezoneN64;
                    }
                    //stream.Position = position;
                    Bookmark.Set(position);
                }

                long tmpPosition = stream.Position;
                if (Format == BZNFormat.BattlezoneN64)
                {
                    IBZNToken? SeqNoToken = ReadToken(); // 4 byte number

                    IBZNToken? MissionSaveToken = ReadToken();
                    if (MissionSaveToken == null)
                        throw new Exception("Failed to parse MissionSaveToken");
                    bool MissionSave = MissionSaveToken.GetBoolean();
                    //SaveType = MissionSave ? 0 : 1;

                    IBZNToken? TerrainOrMissionName = ReadToken(); // long, probably 64 bytes of text
                }
                else
                {
                    IBZNToken? VersionToken = ReadToken();
                    if (VersionToken != null)
                    {
                        Version = VersionToken.GetInt32();

                        tmpPosition = position = stream.Position;
                        IBZNToken? SaveTypeToken = ReadToken();
                        if (SaveTypeToken != null && !InBinary && SaveTypeToken.Validate("saveType"))
                        {
                            //SaveType = SaveTypeToken.GetInt32();
                            TypeSize = 1;
                            TypeSizeSet = true;
                            Format = BZNFormat.Battlezone2;
                        }
                        else if (SaveTypeToken != null && !InBinary && SaveTypeToken.Validate("saveGameDesc"))
                        {
                            TypeSize = 4; // Star Trek Armada, 3 bytes are garbage
                            TypeSizeSet = true;
                            SizeSize = 4;
                            Format = BZNFormat.StarTrekArmada;
                        }
                        else
                        {
                            // we didn't read a saveType, walk back
                            //stream.Position = position;
                            Bookmark.Set(position);
                        }
                    }
                    else
                    {
                        // null on the read, so just walk back even thoug we're probably screwed at this point
                        Bookmark.Set(position);
                    }

                    //if (Version > 1022)
                    {
                        tmpPosition = stream.Position;
                        IBZNToken? BinaryToken = ReadToken();
                        if (BinaryToken != null && BinaryToken.Validate("binarySave"))
                        {
                            if (BinaryToken.GetBoolean())
                                binaryDataStartOffset = stream.Position;

                            long tmpPosition2 = stream.Position;

                            IBZNToken? tok = ReadToken();
                            if (tok != null && tok.Validate("msn_filename", BinaryFieldType.DATA_CHAR))
                            {
                                tok = ReadToken();
                                if (tok != null && tok.Validate("seq_count", BinaryFieldType.DATA_LONG))
                                {
                                    tok = ReadToken();
                                    if (tok != null && tok.Validate("missionSave", BinaryFieldType.DATA_BOOL))
                                    {
                                        //SaveType = tok.GetBoolean() ? 0 : 1; // TODO we had an impossible BZN so let's ignore this for the moment
                                        if (!InBinary || tok.GetUInt8() <= 1)
                                        {
                                            TypeSize = 2;
                                            TypeSizeSet = true;
                                            SizeSize = 2;
                                            Format = BZNFormat.Battlezone;
                                        }
                                    }
                                }
                            }

                            //stream.Position = tmpPosition2;
                            Bookmark.Set(tmpPosition2);
                        }
                        else if (BinaryToken != null && BinaryToken.Validate("BinaryMode"))
                        {
                            if (BinaryToken.GetBoolean())
                                binaryDataStartOffset = stream.Position;
                            TypeSize = 4; // Star Trek Armada, 3 bytes are garbage
                            TypeSizeSet = true;
                            SizeSize = 4;
                            Format = BZNFormat.StarTrekArmada2;
                        }
                        else if (BinaryToken != null && BinaryToken.Validate("seq_count", BinaryFieldType.DATA_LONG))
                        {
                            IBZNToken? tok;
                            tok = ReadToken();
                            if (tok != null && tok.Validate("missionSave", BinaryFieldType.DATA_BOOL))
                            {
                                //SaveType = tok.GetBoolean() ? 0 : 1; // TODO we had an impossible BZN so let's ignore this for the moment
                                if (!InBinary || tok.GetUInt8() <= 1)
                                {
                                    TypeSize = 2;
                                    TypeSizeSet = true;
                                    SizeSize = 2;
                                    Format = BZNFormat.Battlezone;
                                }
                            }
                            else if (tok != null && tok.Validate("size"))
                            {
                                tok = ReadToken();
                                if (tok != null && tok.IsValidationOnly() && tok.Validate("GameObject"))
                                {
                                    // SaveType here is obviously 0
                                    TypeSize = 2;
                                    TypeSizeSet = true;
                                    SizeSize = 2;
                                    Format = BZNFormat.Battlezone;
                                }
                            }
                            else if (tok != null && tok.Validate("TerrainName"))
                            {
                                tok = ReadToken();
                                if (tok != null && tok.Validate("start_time"))
                                {
                                    tok = ReadToken();
                                    if (tok != null && tok.Validate("size"))
                                    {
                                        tok = ReadToken();
                                        if (tok != null && tok.IsValidationOnly() && tok.Validate("GameObject"))
                                        {
                                            // SaveType here is obviously 0
                                            TypeSize = 2;
                                            TypeSizeSet = true;
                                            SizeSize = 2;
                                            Format = BZNFormat.Battlezone;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            //stream.Position = position;
                            Bookmark.Set(position);
                        }
                    }
                }

                // check for special case BZ2's bz2001.bzn
                // we might be the "bz2001.bzn" file from BZ2 that is not in the BZ1 patch continuum but we register as a BZ1 type BZN
                if (Format == BZNFormat.Battlezone && !HasBinary)
                {
                    IBZNToken? tok = ReadToken();
                    if (tok != null && tok.Validate("msn_filename"))
                    {
                        tok = ReadToken();
                        if (tok != null && tok.Validate("seq_count", BinaryFieldType.DATA_LONG))
                        {
                            tok = ReadToken();
                            if (tok != null && tok.Validate("saveType", BinaryFieldType.DATA_LONG))
                            {
                                //SaveType = tok.GetInt32();
                                TypeSize = 1;
                                TypeSizeSet = true;
                                SizeSize = 2;
                                Format = BZNFormat.Battlezone2;
                            }
                        }
                    }
                }

                // We're the default type size, so let's inspect our size
                if (!TypeSizeSet)
                {
                    // interrogate, might need this to repeat earlier if the file starts binary but unsure
                }

                if (Format == BZNFormat.Battlezone2 && Version == 1160)
                {
                    // Breadcrumb BZ2-1160-QUIRK
                    QuoteStrings = true;
                }

                if (Format == BZNFormat.Battlezone)
                {
                    PointerSize = Version >= 2012 ? (byte)8 : (byte)4;
                    MatrixBigPosit = Version >= 0; // at some point it changed but IDK when
                }

                if (Format == BZNFormat.Battlezone2 && !HasBinary)
                {
                    // < 1182 we can probably assume G
                    // 1182 .8e3
                    // 1183 .8e2
                    // 1183 .8e3
                    // 1187 .8e2
                    // 1188 .8e2
                    // 1192 .8e2
                    // 1192 FormatG6 (rare)
                    // 1193 .8e2
                    // 1194 .8e2
                    // 1196 .8e2
                    // 1197 .8e2
                    if (Version >= 1182)
                    {
                        FloatFormat = FloatTextFormat._9e2;

                        //stream.Position = startPosition;
                        Bookmark.Set(startPosition);

                        // float format could have changed, so lets try to agressive scan for floats

                        Dictionary<FloatTextFormat, UInt32> FloatCounts = new Dictionary<FloatTextFormat, uint>()
                        {
                            { FloatTextFormat.G, 0 },
                            { FloatTextFormat._9e2, 0 },
                            { FloatTextFormat._9e3, 0 }
                        };

                        IBZNToken? tok;
                        while ((tok = ReadToken()) != null)
                        {
                            ProcessTokenForFloats(tok, FloatCounts);
                        }

                        // if more than one float count is greater than 0, mark a flag that float confusion exists
                        // then note the most common format as the main format
                        if (FloatCounts.Count(kv => kv.Value > 0) > 1)
                        {
                            FloatFormatUnreliable = true;
                        }
                        FloatFormat = FloatCounts.Where(kv => kv.Value > 0).OrderByDescending(kv => kv.Value).Select(kv => kv.Key).Append(FloatFormat).FirstOrDefault();
                    }
                }

                //stream.Position = startPosition;
                Bookmark.Set(startPosition);
            }
        }

        private void ProcessTokenForFloats(IBZNToken? token, Dictionary<FloatTextFormat, uint> floatCounts)
        {
            if (token == null)
                return;

            // TODO for now always assume 32bit, it will chew some garbage but it will do for now
            for (int i = 0; i < token.GetCount(); i++)
            {
                if (token.GetSubCount(i) == 0)
                {
                    // 0 subtokens, so it must be a normal field, check if it has singles
                    string s = token.GetString(i);
                    if (float.TryParse(s, out _) && s.Contains(".")) // so we ignore integers to be safe
                    {
                        float v = token.GetSingle(i);
                        floatCounts[SingleExtension.GetFloatTextFormat(s)]++;
                    }
                }
                else
                {
                    for (int j = 0; j < token.GetSubCount(i); j++)
                    {
                        IBZNToken? subTok = token.GetSubToken(i, j);
                        ProcessTokenForFloats(subTok, floatCounts);
                    }
                }
            }
        }

        public void Dispose()
        {
            if (BaseStream != null) BaseStream.Close();
        }


        public Dictionary<int, StreamDefect> GetDefects()
        {
            // check for discontinuity
            long offset = 0;
            for (int i = 0; i < Atlas.Count; i++)
            {
                AtlasData ad = Atlas[i];
                if (offset != ad.Offset)
                    throw new Exception("Atlas Damaged");
                offset += ad.Length;
            }

            // this is probably horribly inefficent but whatever
            return Atlas.Select((ad, index) => new { ad, index }).Where(x => x.ad.Defect != null).ToDictionary(x => x.index, x => x.ad.Defect!);
        }
         

        /// <summary>
        /// Read the next token from the stream.
        /// </summary>
        /// <returns></returns>
        public IBZNToken? ReadToken()
        {
            if (InBinary)
            {
                return ReadBinaryToken(BaseStream);
            }
            else
            {
                return ReadStringToken(BaseStream);
            }
        }

        /// <summary>
        /// Read a string value or string validation token from the file stream.
        /// </summary>
        /// <param name="filestream"></param>
        /// <returns></returns>
        private IBZNToken? ReadStringToken(Stream filestream)
        {
            if (filestream.Position >= filestream.Length) return null;

            if (TokenIndex > Atlas.Count)
                throw new Exception("Atlas Discontinuity"); // Discontinuity
            AtlasData ad = Atlas.Count >= TokenIndex ? new AtlasData() { Offset = BaseStream.Position } : Atlas[TokenIndex];

            for (; filestream.Position < filestream.Length; )
            {
                string rawLine = ReadStringLine(filestream);

                if (rawLine.Length > 0)
                {
                    if (rawLine.StartsWith("[") && rawLine.EndsWith("]"))
                    {
                        ad.Length = filestream.Position - ad.Offset;
                        ad.IsBinary = false;
                        if (TokenIndex == Atlas.Count)
                            Atlas.Add(ad);
                        TokenIndex++;
                        return new BZNTokenValidation(rawLine.Substring(1, rawLine.Length - 2));
                    }

                    IBZNToken tok = ReadStringValueToken(filestream, rawLine);
                    ad.Length = filestream.Position - ad.Offset;
                    ad.IsBinary = false;
                    if (tok is BZNTokenString strTok)
                    {
                        string val = strTok.values[0];
                        string valT = val.TrimEnd(new char[] { '\r', '\n' });
                        if (val != valT) {
                            strTok.values[0] = valT;
                            ad.Defect_EndPadGarbage = val.Substring(valT.Length);
                        }
                    }
                    if (TokenIndex == Atlas.Count)
                        Atlas.Add(ad);
                    TokenIndex++;
                    return tok;
                }
            }
            return null;
        }

        private static string[] SmartStringSplit(string input, int count)
        {
            if (input == null)
                return Array.Empty<string>();

            string trimmed = input.TrimStart();
            int leadingSpaceCount = input.Length - trimmed.Length;

            // Trim leading spaces, then split by spaces, removing empty entries
            string[] retVal = trimmed.Split(' ', count);
            retVal[0] = new string(' ', leadingSpaceCount) + retVal[0];
            return retVal;
        }

        /// <summary>
        /// Read a string value from the file stream.
        /// </summary>
        /// <param name="filestream"></param>
        /// <param name="rawLine"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private IBZNToken ReadStringValueToken(Stream filestream, string rawLine)
        {
            long pos = filestream.Position;

            if (!rawLine.EndsWith(" =") && !rawLine.Contains(" = ") && rawLine.Contains('='))
            {
                // fucky wucky
                rawLine = rawLine.Replace("=", " = ");
            }

            //string[] line = rawLine.Split(' ', 4);
            string[] line = SmartStringSplit(rawLine, 4);

            if (line[1] == "=")
            {
                //line = rawLine.Split(' ', 3);
                line = SmartStringSplit(rawLine, 3);
                string name = line[0];

                int countIndentedLines = 0;
                {
                    HashSet<string> SeenBeforeKeys = new HashSet<string>();
                    long offsetStartChildren = filestream.Position;
                    int countSpacesHead = 0;
                    for (; ; )
                    {
                        string nextRawLine = ReadStringLine(filestream);
                        if (nextRawLine.StartsWith(" ") && nextRawLine.Contains("="))
                        {
                            int countSpacesHead2 = nextRawLine.Length - nextRawLine/*.TrimStart()*/.Length;
                            if (countSpacesHead == 0)
                                countSpacesHead = countSpacesHead2;
                            string key = nextRawLine.Split(new char[] { '=', '[' })[0].TrimEnd();
                            if (countSpacesHead == countSpacesHead2 && SeenBeforeKeys.Add(key))
                            {
                                countIndentedLines++;
                                IBZNToken tok = ReadStringValueToken(filestream, nextRawLine);
                            }
                            else
                            {
                                filestream.Position = offsetStartChildren;
                                break;
                            }
                        }
                        else
                        {
                            filestream.Position = offsetStartChildren;
                            break;
                        }
                    }
                }

                // fix for fucked up BZNs that removed leading spaces from complex ASCII tokens
                if (countIndentedLines == 0 && ComplexStringTokenSizeMap.ContainsKey(name.Trim()))
                    countIndentedLines = ComplexStringTokenSizeMap[name.Trim()];

                if (countIndentedLines > 0)
                {
                    int count = 1;

                    IBZNToken[][] values = new IBZNToken[count][];
                    for (int subSectionCounter = 0; subSectionCounter < count; subSectionCounter++)
                    {
                        values[subSectionCounter] = new IBZNToken[countIndentedLines];
                        for (int constructCounter = 0; constructCounter < countIndentedLines; constructCounter++)
                        {
                            string rawLineInner = ReadStringLine(filestream).TrimEnd('\r', '\n')/*.TrimStart()*/;
                            if (rawLineInner.Length != 0)
                                values[subSectionCounter][constructCounter] = ReadStringValueToken(filestream, rawLineInner);
                        }
                    }

                    return new BZNTokenNestedString(name, values);
                }
                else
                {
                    if (line.Length == 2)
                    {
                        // because there is no array size indicator we assume the value is on the same line, and there isn't one
                        // this is basically a badly written file where a no-value single-liner has the end trimed
                        return new BZNTokenString(name, new string[] { string.Empty }) { RightTrimmedOneLiner = true };
                    }

                    string value = line[2];
                    if (QuoteStrings)
                    {
                        value = value.Trim();
                        if (value.StartsWith('"') && value.EndsWith('"'))
                            value = value.Substring(1, value.Length - 2);
                    }

                    return new BZNTokenString(name, new string[] { value });
                }
            }
            else if (line[2] == "=")
            {
                string name = line[0];
                int count = int.Parse(line[1].Substring(1, line[1].Length - 2));

                if (count == 0) return new BZNTokenString(name, new string[0]);

                int countIndentedLines = 0;
                {
                    HashSet<string> SeenBeforeKeys = new HashSet<string>();
                    long offsetStartChildren = filestream.Position;
                    int countSpacesHead = 0;
                    for (; ; )
                    {
                        string nextRawLine = ReadStringLine(filestream);
                        if (nextRawLine.StartsWith(" ") && nextRawLine.Contains("="))
                        {
                            int countSpacesHead2 = nextRawLine.Length - nextRawLine.TrimStart().Length;
                            if (countSpacesHead == 0)
                                countSpacesHead = countSpacesHead2;
                            string key = nextRawLine.Split(new char[] { '=', '[' })[0].TrimEnd();
                            if (countSpacesHead == countSpacesHead2 && SeenBeforeKeys.Add(key))
                            {
                                countIndentedLines++;
                                IBZNToken tok = ReadStringValueToken(filestream, nextRawLine);
                            }
                            else
                            {
                                filestream.Position = offsetStartChildren;
                                break;
                            }
                        }
                        else
                        {
                            filestream.Position = offsetStartChildren;
                            break;
                        }
                    }
                }

                // fix for fucked up BZNs that removed leading spaces from complex ASCII tokens
                if (countIndentedLines == 0 && ComplexStringTokenSizeMap.ContainsKey(name.Trim()))
                    countIndentedLines = ComplexStringTokenSizeMap[name.Trim()];

                if (countIndentedLines > 0)
                {
                    IBZNToken[][] values = new IBZNToken[count][];
                    for (int subSectionCounter = 0; subSectionCounter < count; subSectionCounter++)
                    {
                        values[subSectionCounter] = new IBZNToken[countIndentedLines];
                        for (int constructCounter = 0; constructCounter < countIndentedLines; constructCounter++)
                        {
                            string rawLineInner = ReadStringLine(filestream).TrimEnd('\r', '\n')/*.TrimStart()*/;
                            if (rawLineInner.Length != 0)
                                values[subSectionCounter][constructCounter] = ReadStringValueToken(filestream, rawLineInner);
                        }
                    }

                    return new BZNTokenNestedString(name, values);
                }
                else
                {
                    string[] values = new string[count];

                    for (int lineNum = 0; lineNum < count; lineNum++)
                    {
                        long new_pos = filestream.Position;
                        string new_rawLine = ReadStringLine(filestream).TrimEnd('\r', '\n');
                        //string[] new_line = new_rawLine.TrimStart().Split(' ', 4);
                        string[] new_line = SmartStringSplit(new_rawLine, 4);

                        if ((new_line.Length > 1 && new_line[1] == "=") || (new_line.Length > 2 && new_line[2] == "="))
                        {
                            // special case where we didn't have a value for some reason (buggy printed VEC2Ds

                            // points [1] =
                            //   x [1] =
                            // 32
                            //   z [1] =
                            // pathType = 00000000
                            // ...
                            // points [1] =
                            //   x [1] =
                            //   z [1] =
                            // -32
                            // pathType = 00000000

                            values[lineNum] = string.Empty;
                            filestream.Position = new_pos;
                        }
                        else
                        {
                            values[lineNum] = new_rawLine;
                        }
                    }

                    return new BZNTokenString(name, values);
                }
            }
            else
            {
                throw new Exception("Error reading ASCII data, \"=\" not found where expected.");
            }
        }

        /// <summary>
        /// Read a binary token from the file stream.
        /// </summary>
        /// <param name="filestream"></param>
        /// <returns></returns>
        private IBZNToken? ReadBinaryToken(Stream filestream)
        {
            if (filestream.Position >= filestream.Length) return null;

            if (TokenIndex > Atlas.Count)
                throw new Exception("Atlas Discontinuity"); // Discontinuity
            AtlasData ad = Atlas.Count >= TokenIndex ? new AtlasData() { Offset = BaseStream.Position } : Atlas[TokenIndex];

            byte[] number = new byte[4];
            uint type = 0;
            uint typeClean = 0;
            if (TypeSize > 0)
            {
                if (IsBigEndian)
                {
                    filestream.Read(number, sizeof(uint) - TypeSize, TypeSize);
                    type = BitConverter.ToUInt32(number.Reverse().ToArray(), 0);
                }
                else
                {
                    int readSize = filestream.Read(number, 0, TypeSize);
                    type = BitConverter.ToUInt32(number, 0);

                    // deal with rare truncation
                    if (readSize != TypeSize)
                        ad.Defect_TruncatedBytesType = number.Take(readSize).ToArray();
                }
                typeClean = type & 0xff;
            }
            else
            {
                type = (uint)BinaryFieldType.DATA_UNKNOWN;
                typeClean = type;
            }
            uint Size = 0;
            if (IsBigEndian)
            {
                int readSize = filestream.Read(number, sizeof(uint) - SizeSize, SizeSize);
                Size = BitConverter.ToUInt32(number.Reverse().ToArray(), 0);
            }
            else
            {
                int readSize = filestream.Read(number, 0, SizeSize);
                Size = BitConverter.ToUInt32(number, 0);

                // deal with rare truncation
                if (readSize != SizeSize)
                    ad.Defect_TruncatedBytesSize = number.Take(readSize).ToArray();
            }

            byte[] data = new byte[Size];
            {
                int readSize = filestream.Read(data, 0, (int)Size);

                // deal with rare truncation
                if (readSize != Size)
                    ad.Defect_TruncatedBytesData = data.Take((int)readSize).ToArray();
            }

            if (ad.Defect_TruncatedBytesData == null)
            {
                // lets just avoid this code if we had a stream defect, simpler that way
                if (typeClean == (uint)BinaryFieldType.DATA_CHAR)
                {
                    long pos = filestream.Position;
                    int readSize = 0;

                    // read next type
                    uint type2 = 0;
                    if (IsBigEndian)
                    {
                        readSize = filestream.Read(number, 0, TypeSize);
                        type2 = BitConverter.ToUInt32(number.Reverse().ToArray(), 0);
                    }
                    else
                    {
                        readSize = filestream.Read(number, 0, TypeSize);
                        type2 = BitConverter.ToUInt32(number, 0);
                    }
                    uint typeClean2 = type2 & 0xff;

                    if (readSize != TypeSize)
                    {
                        // abort, something is too fucky
                        filestream.Seek(pos, SeekOrigin.Begin);
                    }
                    else
                    {
                        if (!Enum.IsDefined(typeof(BinaryFieldType), (byte)typeClean2))
                        {
                            // invalid type, so try backing off by 1
                            // if we ever add dynamic token discovery by signiture this section can be removed
                            filestream.Seek(pos, SeekOrigin.Begin);
                            filestream.Seek(-1, SeekOrigin.Current);

                            if (IsBigEndian)
                            {
                                filestream.Read(number, 0, TypeSize);
                                type2 = BitConverter.ToUInt32(number.Reverse().ToArray(), 0);
                            }
                            else
                            {
                                filestream.Read(number, 0, TypeSize);
                                type2 = BitConverter.ToUInt32(number, 0);
                            }
                            typeClean2 = type2 & 0xff;
                            if (!Enum.IsDefined(typeof(BinaryFieldType), (byte)typeClean))
                            {
                                // give up
                                filestream.Seek(pos, SeekOrigin.Begin);
                            }
                            else
                            {
                                // ah we're valid now, which means our current token is oversized by 1
                                ad.Defect_BytesOversized = Size;
                                data = data.Take((int)Size - 1).ToArray();
                                filestream.Seek(pos, SeekOrigin.Begin);
                                filestream.Seek(-1, SeekOrigin.Current);
                            }
                        }
                        else
                        {
                            filestream.Seek(pos, SeekOrigin.Begin);
                        }
                    }
                }
            }

            if (AlignmentBytes > 0)
            {
                // Only known case of padding right now is BZn64's 16bit alignment, which due to Sizes being 2 bytes (and thus aligned) only requires the data to be padded.
                // Still fully implemented a record pad here, though it might need to be broken into more sections if we find everything needs alignment
                int pad = (int)((TypeSize + SizeSize + Size) % AlignmentBytes);
                for (int i = 0; i < pad; i++)
                    filestream.ReadByte(); // deal with padding
            }

            // early tokens will probably read with the wrong byte BitSize, but it's ok since it can read binary tokens without it, it only affects the var counter
            IBZNToken tok = new BZNTokenBinary((BinaryFieldType)typeClean, data, IsBigEndian, PointerSize, MatrixBigPosit) { rawType = type != typeClean ? type : null };
            ad.Length = filestream.Position - ad.Offset;
            ad.IsBinary = true;
            if (type != typeClean)
            {
                ad.Defect_TypeGarbage = type;
            }
            if (TokenIndex == Atlas.Count)
                Atlas.Add(ad);
            TokenIndex++;
            return tok;
        }

        /// <summary>
        /// Read a string from the file stream.
        /// </summary>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        private string ReadStringLine(Stream fileStream)
        {
            string buffer = string.Empty;

            for (int idx = 0; fileStream.Position < fileStream.Length; idx++)
            {
                byte character = (byte)fileStream.ReadByte();

                if (character == 0x0D)
                {
                    // 1022, 1033, 1037, 1038, 1048, 1105, 1108, 1018, 1128, 1034, 1135, 1137, 1143, 1045, 1148, 1149, 1154, 1169, 1171, 1179, 1180, 1182, 1183, 1186, 1187, 1188, 1192, 2016
                    
                    CountCR++;

                    int nextBytes = fileStream.ReadByte(); // 0x0A

                    if (nextBytes == 0x0A)
                    {
                        CountLF++;
                        CountCRLF++;
                    }
                    else if (nextBytes == 0x0D)
                    {
                        if (CountCRLF == CountLF && CountCR == CountLF + 1)
                        {
                            // double CR, likely a CR CR LF
                            CountCR--;
                            buffer += BZNEncoding.win1252.GetChars(new byte[] { character })[0];
                            fileStream.Seek(-1, SeekOrigin.Current);
                            continue;
                        }
                    }

                    //if (nextBytes != 0x0A)
                    //{
                    //    idx = -1;
                    //    buffer += (char)character;
                    //
                    //    continue;
                    //}

                    // Version 1180 line width 4095
                    // Version 1192 line width uncapped?
                    if (Version <= 1180 && idx == 4095)
                    {
                        idx = -1;

                        continue;
                    }

                    // 1171

                    break;
                }
                else if (character == 0x0A)
                {
                    // 1045

                    // TODO this is likely a bug in the BZN file as it should always be CRLF, but this is just LF which is invalid
                    CountLF++;

                    // strange, how is this even possible, maybe only BZ1?
                    if (Version <= 1180 && idx == 4095)
                    {
                        idx = -1;

                        continue;
                    }

                    break;
                }
                else
                {
                    //buffer += (char)character;
                    buffer += BZNEncoding.win1252.GetChars(new byte[] { character })[0];
                }
            }

            return buffer;
        }
    }
}
