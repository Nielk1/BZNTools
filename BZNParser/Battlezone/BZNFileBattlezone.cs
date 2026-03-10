using BZNParser.Battlezone.GameObject;
using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;
using static BZNParser.Tokenizer.BZNStreamReader;
using static BZNParser.Tokenizer.IMalformable;

namespace BZNParser.Battlezone
{
    public enum SaveType
    {
        BZN = 0, // ST_MISSION in BZ2, MissionSave False in BZ1 (I might have MissionSave bool backwards in BZ1, not sure)
        SAVE = 1, // ST_SAVE in BZ2, MissionSave True in BZ1 (I might have MissionSave bool backwards in BZ1, not sure)
        JOIN = 2, // ST_JOIN in BZ2
        LOCKSTEP = 3, // ST_LOCKSTEP in BZ2
        VISUAL = 4, // ST_SWITCHSHOW in BZ2
        NONE = 5, // ST_NONE in BZ2
    }
    public class BattlezoneBZNHints
    {
        /// <summary>
        /// Is the reader in strict mode where the class MUST be in the hint list?
        /// </summary>
        public bool Strict { get; set; }
        public Dictionary<string, HashSet<string>>? ClassLabels { get; set; }
        public Dictionary<UInt16, string?>? EnumerationPrjID { get; set; }

        private static bool TryParseFlexibleUInt16(string input, out ushort value)
        {
            if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return UInt16.TryParse(
                    input.Substring(2),
                    NumberStyles.AllowHexSpecifier,
                    CultureInfo.InvariantCulture,
                    out value
                );
            }
            return UInt16.TryParse(input, out value);
        }
        public static BattlezoneBZNHints? BuildHintsBZ1()
        {
            BattlezoneBZNHints BZ1Hints = new BattlezoneBZNHints();
            BZ1Hints.Strict = true;
            BZ1Hints.ClassLabels = new Dictionary<string, HashSet<string>>();
            bool AnyHints = false;
            if (File.Exists("BZ1_ClassLabels.txt"))
            {
                AnyHints = true;

                HashSet<string> ValidClassLabelsBZ1 = new HashSet<string>();
                foreach (Type type in typeof(BZNFileBattlezone).Assembly.GetTypes())
                {
                    var attrs = type.GetCustomAttributes(typeof(ObjectClassAttribute), true);
                    foreach (ObjectClassAttribute attr in attrs)
                        if (attr.Format == BZNFormat.Battlezone || attr.Format == BZNFormat.BattlezoneN64)
                            ValidClassLabelsBZ1.Add(attr.ClassName);
                }

                foreach (string line in File.ReadAllLines("BZ1_ClassLabels.txt"))
                {
                    string[] parts = line.Split(new char[] { '\t' }, 3);
                    if (parts.Length == 2 || parts.Length == 3)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();
                        if (!ValidClassLabelsBZ1.Contains(value))
                            continue;
                        if (!BZ1Hints.ClassLabels.ContainsKey(key))
                            BZ1Hints.ClassLabels[key] = new HashSet<string>();
                        BZ1Hints.ClassLabels[key].Add(value);
                    }
                }
            }
            if (File.Exists("BZN64_Enum_PrjID.txt"))
            {
                AnyHints = true;

                Dictionary<UInt16, string?> EnumPrjID = new Dictionary<UInt16, string?>();

                foreach (string line in File.ReadAllLines("BZN64_Enum_PrjID.txt"))
                {
                    string[] parts = line.Split(new char[] { '\t' }, 3);
                    if (parts.Length == 2 || parts.Length == 3)
                    {
                        string keyS = parts[0].Trim();
                        UInt16 key;
                        if (TryParseFlexibleUInt16(keyS, out key))
                        {
                            string? value = parts[1].Trim();
                            if (value.Length == 0)
                                value = null;
                            EnumPrjID[key] = value;
                        }
                    }
                }
                if (EnumPrjID.Any())
                    BZ1Hints.EnumerationPrjID = EnumPrjID;
            }
            if (AnyHints)
                return BZ1Hints;
            return null;
        }
        public static BattlezoneBZNHints? BuildHintsBZ2()
        {
            if (File.Exists("BZ2_ClassLabels.txt"))
            {
                BattlezoneBZNHints BZ2Hints = new BattlezoneBZNHints();
                BZ2Hints.Strict = true;
                BZ2Hints.ClassLabels = new Dictionary<string, HashSet<string>>();

                foreach (string line in File.ReadAllLines("BZ2_ClassLabels.txt"))
                {
                    string[] parts = line.Split(new char[] { '\t' }, 3);
                    if (parts.Length == 2 || parts.Length == 3)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();
                        if (!BZ2Hints.ClassLabels.ContainsKey(key))
                            BZ2Hints.ClassLabels[key] = new HashSet<string>();
                        BZ2Hints.ClassLabels[key].Add(value);
                    }
                }

                HashSet<string> ValidClassLabelsBZ2 = new HashSet<string>();
                foreach (Type type in typeof(BZNFileBattlezone).Assembly.GetTypes())
                {
                    var attrs = type.GetCustomAttributes(typeof(ObjectClassAttribute), true);
                    foreach (ObjectClassAttribute attr in attrs)
                        if (attr.Format == BZNFormat.Battlezone2)
                            ValidClassLabelsBZ2.Add(attr.ClassName);
                }
                foreach (string key in BZ2Hints.ClassLabels.Keys.ToList())
                    if (ValidClassLabelsBZ2.Contains(key))
                        BZ2Hints.ClassLabels[key].Add(key);
                for (; ; )
                {
                    bool found = false;
                    int size = 0;
                    foreach (string key in BZ2Hints.ClassLabels.Keys.ToList())
                    {
                        HashSet<string> classLabels = BZ2Hints.ClassLabels[key];

                        // these classLabels might be other ODF names instead of class labels, so lets expand them
                        foreach (string item in classLabels.ToList()) // make a new list so we can alter it while looping
                        {
                            if (!ValidClassLabelsBZ2.Contains(item))
                            {
                                // if it's not a valid class label and is thus just an ODF name, remove it from the options
                                // we will still try to walk it to valid classlabels below
                                classLabels.Remove(item);
                            }
                            if (BZ2Hints.ClassLabels.ContainsKey(item))
                            {
                                HashSet<string> newClassLabels = BZ2Hints.ClassLabels[item];
                                foreach (string newLabel in newClassLabels)
                                {
                                    if (!found && !classLabels.Contains(newLabel))
                                    {

                                    }
                                    found = classLabels.Add(newLabel) || found;
                                }
                            }
                        }

                        //BZ2Hints.ClassLabels[key] = classLabels;
                        size += classLabels.Count;
                    }
                    if (!found)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"Cleaning BZ2 ClassLabels {size}");
                    }
                }

                return BZ2Hints;
            }
            return null;
        }
    }
    public class BZNFileBattlezone : IMalformable
    {
        public BattlezoneBZNHints? Hints;


        private readonly IMalformable.MalformationManager _malformationManager;
        public IMalformable.MalformationManager Malformations => _malformationManager;


        private readonly Dictionary<string, IClassFactory> _classLabelMap;
        public Dictionary<string, IClassFactory> ClassLabelMap => _classLabelMap;


        /// <summary>
        /// Version this file was loaded from
        /// </summary>
        public UInt32 Version { get; private set; }
        public bool Binary { get; private set; }
        public SaveType SaveType { get; private set; }

        public string msn_filename { get; set; }
        public UInt32 seq_count { get; set; }
        public string TerrainName { get; set; }
        public float? start_time { get; set; }
        public EntityDescriptor[] Entities { get; private set; }
        public byte[] groupTargets { get; set; }

        /// Mission DLL or internal class
        public string Mission { get; set; }
        public UInt64 sObject { get; set; }

        // BZ1 lua mission state block
        public bool? bz1_luamission_started { get; set; }
        // /BZ1 lua mission state block

        public Int32? AiMissionSize { get; set; }



        public UInt32? UserProcess_sObject { get; set; }
        public Int32? UserProcess_cycle { get; set; }
        public Int32? UserProcess_cycleMax { get; set; }
        public UInt32? UserProcess_selectList { get; set; }
        public UInt32? UserProcess_undefptr_1 { get; set; }
        public UInt32? UserProcess_undefptr_2 { get; set; }
        public bool? UserProcess_exited { get; set; }



        public AreaOfInterest[] AOIs { get; set; }
        public AiPath[] AiPaths { get; set; }


        internal Dictionary<string, HashSet<string>> LongTermClassLabelLookupCache;

        public BZNFileBattlezone(BZNStreamReader reader, BattlezoneBZNHints? Hints = null)
        {
            this.LongTermClassLabelLookupCache = new Dictionary<string, HashSet<string>>();

            this._malformationManager = new IMalformable.MalformationManager(this);
            this._classLabelMap = new Dictionary<string, IClassFactory>();
            this.Hints = Hints;



            // build ClassLabelMap
            foreach (Type type in this.GetType().Assembly.GetTypes())
            {
                // Only consider types that implement IClassFactory and are not interfaces or abstract
                if (!typeof(IClassFactory).IsAssignableFrom(type) || type.IsInterface || type.IsAbstract)
                    continue;

                var attrs = type.GetCustomAttributes(typeof(ObjectClassAttribute), true);
                foreach (ObjectClassAttribute attr in attrs)
                    if (attr.Format == reader.Format)
                        if (ClassLabelMap.ContainsKey(attr.ClassName))
                            throw new Exception($"Duplicate class label: {attr.ClassName} ({type})");
                        else
                            ClassLabelMap[attr.ClassName] = (IClassFactory)Activator.CreateInstance(type)!;
            }

            // extra info to print at start and end
            {
                Console.WriteLine($"---------------- START READER INFO ----------------");
                Console.WriteLine($"StartBinary: {reader.StartBinary}");
                Console.WriteLine($"HasBinary: {reader.HasBinary}");
                Console.WriteLine($"InBinary: {reader.InBinary}");
                Console.WriteLine($"StartBinary: {reader.StartBinary}");
                Console.WriteLine($"IsBigEndian: {reader.IsBigEndian}");
                Console.WriteLine($"TypeSize: {reader.TypeSize}");
                Console.WriteLine($"SizeSize: {reader.SizeSize}");
                Console.WriteLine($"Version: {reader.Version}");
                Console.WriteLine($"AlignmentBytes: {reader.AlignmentBytes}");
                Console.WriteLine($"Format: {reader.Format}");
                Console.WriteLine($"QuoteStrings: {reader.QuoteStrings}");
                Console.WriteLine($"PointerSize: {reader.PointerSize}");
                Console.WriteLine($"MatrixBigPosit: {reader.MatrixBigPosit}");
                Console.WriteLine($"CountCR: {reader.CountCR}");
                Console.WriteLine($"CountLF: {reader.CountLF}");
                Console.WriteLine($"CountCRLF: {reader.CountCRLF}");
                Console.WriteLine($"FloatFormat: {reader.FloatFormat}");
                Console.WriteLine($"----------------- END READER INFO -----------------");
            }

            IBZNToken tok;

            Console.WriteLine($"Format: {reader.Format}");

            if (reader.Format != BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                Version = tok.GetUInt32();
                Console.WriteLine($"Version: {Version}"); // don't bother validating first field maybe?
                if (!tok.IsBinary)
                {
                    string fieldName = (tok as BZNTokenString).Name;
                    if (fieldName != "version")
                        Malformations.AddIncorrectName<BZNFileBattlezone, UInt32>(x => x.Version, fieldName);
                }
            }

            // Breadcrumb BZ2001-QUIRK
            if (reader.Format == BZNFormat.Battlezone2 && reader.Version != 1041 && reader.Version != 1047) // version is special case for bz2001.bzn
            {
                tok = reader.ReadToken();
                if (!tok.Validate("saveType", BinaryFieldType.DATA_UNKNOWN))
                    throw new Exception("Failed to parse saveType/UNKNOWN");
                Console.WriteLine($"saveType: {tok.GetUInt32()}");
                SaveType = (SaveType)tok.GetUInt32();
            }

            if (reader.Format == BZNFormat.Battlezone)
            {
                if (reader.Version > 1022)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("binarySave", BinaryFieldType.DATA_BOOL))
                        throw new Exception("Failed to parse binarySave/BOOL");
                    tok.ReadBoolean<BZNFileBattlezone, bool>(this, x => x.Binary);
                    Console.WriteLine($"binarySave: {Binary}");

                    tok = reader.ReadToken();
                    if (!tok.Validate("msn_filename", BinaryFieldType.DATA_CHAR))
                        throw new Exception("Failed to parse msn_filename/CHAR");
                    //msn_filename = tok.GetString();
                    msn_filename = Malformations.AddBinaryMessString("msn_filename", tok.GetString());
                    Console.WriteLine($"msn_filename: \"{msn_filename}\"");
                }
            }

            if (reader.Format == BZNFormat.Battlezone2)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("binarySave", BinaryFieldType.DATA_BOOL))
                    throw new Exception("Failed to parse binarySave/BOOL");
                tok.ReadBoolean<BZNFileBattlezone, bool>(this, x => x.Binary);
                Console.WriteLine($"binarySave: {Binary}");

                msn_filename = reader.ReadSizedString_BZ2_1145("msn_filename", 16, Malformations);
                Console.WriteLine($"msn_filename: \"{msn_filename}\"");
            }

            // todo this is oddly messy, clean it up and confirm
            if (reader.Format == BZNFormat.BattlezoneN64 || (reader.Format == BZNFormat.Battlezone && reader.Version <= 1001))
            {
                tok = reader.ReadToken();
                if (!tok.Validate("seq_count", BinaryFieldType.DATA_LONG))
                    throw new Exception("Failed to parse seq_count/LONG");
                seq_count = tok.GetUInt32();
                Console.WriteLine($"seq_count: {seq_count}");
            }
            else if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.Battlezone2)
            {
                // Why does SeqCount exist if there's a GameObject counter too?
                // It appears to be the next seqno so we can calculate it for BZN64 via MAX+1.
                tok = reader.ReadToken();
                if (!tok.Validate("seq_count", BinaryFieldType.DATA_LONG))
                    throw new Exception("Failed to parse seq_count/LONG");
                seq_count = tok.GetUInt32();
                Console.WriteLine($"seq_count: {seq_count}");

                if (reader.Format == BZNFormat.Battlezone2)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("saveType", BinaryFieldType.DATA_LONG))
                        throw new Exception("Failed to parse saveType/LONG");
                    Int32 saveType2 = tok.GetInt32();
                    Console.WriteLine($"saveType (redundant?): {saveType2}"); // maybe not if the first one is missing
                }
            }

            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                if (reader.Format == BZNFormat.Battlezone && reader.Version < 1016)
                {
                    //bool missionSave = true;
                    Console.WriteLine($"missionSave: true (assumed)");
                    SaveType = SaveType.BZN;
                }
                //if ((1017 <= reader.Version && reader.Version <= 1037) || reader.Version == 1043 || reader.Version == 1045 || reader.Version == 2003 || reader.Version == 2016)
                else
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("missionSave", BinaryFieldType.DATA_BOOL))
                        throw new Exception("Failed to parse missionSave/BOOL");
                    (_, bool missionSave) = tok.ReadBoolean<BZNFileBattlezone, SaveType>(this, x => x.SaveType, 0, x => x ? SaveType.BZN : SaveType.SAVE);
                    Console.WriteLine($"missionSave: {missionSave}");
                }
            }

            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                if (reader.Format == BZNFormat.BattlezoneN64 || reader.Version != 1001)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("TerrainName", BinaryFieldType.DATA_CHAR))
                        throw new Exception("Failed to parse TerrainName/CHAR");
                    TerrainName = tok.GetString();
                    Console.WriteLine($"TerrainName: {TerrainName}");
                }
            }
            else if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version < 1171)
                {
                    // BZ2: 1123 1124
                    tok = reader.ReadToken();
                    if (!tok.Validate("TerrainName", BinaryFieldType.DATA_CHAR))
                        throw new Exception("Failed to parse TerrainName/CHAR");

                    TerrainName = Malformations.AddBinaryMessString("TerrainName", tok.GetString());
                    Console.WriteLine($"TerrainName: {TerrainName}");
                }
                else if (reader.Version == 1171)
                {
                    // seems to be able to go either way
                    tok = reader.ReadToken();
                    if (!tok.Validate("g_TerrainName", BinaryFieldType.DATA_CHAR))
                    {
                        if (!tok.Validate("TerrainName", BinaryFieldType.DATA_CHAR)) // saw this on a 1171 once, why?
                            throw new Exception("Failed to parse g_TerrainName/CHAR"); // might need to note a safe malformation here
                        Malformations.AddIncorrectName<BZNFileBattlezone, string>(x => x.TerrainName, "TerrainName");
                    }
                    TerrainName = tok.GetString();
                    Console.WriteLine($"TerrainName: {TerrainName}");
                }
                else
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("g_TerrainName", BinaryFieldType.DATA_CHAR))
                        throw new Exception("Failed to parse g_TerrainName/CHAR");
                    TerrainName = tok.GetString();
                    Console.WriteLine($"TerrainName: {TerrainName}");
                }
            }

            if (reader.Format == BZNFormat.Battlezone)
            {
                if (reader.Version == 1011 || reader.Version == 1012)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("start_time", BinaryFieldType.DATA_FLOAT))
                        throw new Exception("Failed to parse start_time/FLOAT");
                    start_time = tok.GetSingle();
                    Console.WriteLine($"start_time: {start_time}");
                }
            }

            if (reader.Format == BZNFormat.Battlezone && SaveType == SaveType.SAVE)
            {
                reader.Bookmark.Mark();
                try
                {
                    Malformations.Push(); // Create a new malformation context
                    Hydrate(reader);
                    reader.Bookmark.Commit();
                    Malformations.Pop(); // Merge the malformation context with the previous
                }
                catch
                {
                    Malformations.Discard(); // Discard the malformation context if we fail to parse

                    // don't bother making a new malformation context since we aren't going to try again if this fails
                    reader.Bookmark.RevertToBookmark();
                    SaveType = SaveType.BZN;
                    LongTermClassLabelLookupCache.Clear();
                    Hydrate(reader);

                    Malformations.AddIncorrect("missionSave", true);
                }
            }
            else
            {
                Hydrate(reader);
            }

            // extra info to print at start and end
            {
                Console.WriteLine($"---------------- START READER INFO ----------------");
                Console.WriteLine($"StartBinary: {reader.StartBinary}");
                Console.WriteLine($"HasBinary: {reader.HasBinary}");
                Console.WriteLine($"InBinary: {reader.InBinary}");
                Console.WriteLine($"StartBinary: {reader.StartBinary}");
                Console.WriteLine($"IsBigEndian: {reader.IsBigEndian}");
                Console.WriteLine($"TypeSize: {reader.TypeSize}");
                Console.WriteLine($"SizeSize: {reader.SizeSize}");
                Console.WriteLine($"Version: {reader.Version}");
                Console.WriteLine($"AlignmentBytes: {reader.AlignmentBytes}");
                Console.WriteLine($"Format: {reader.Format}");
                Console.WriteLine($"QuoteStrings: {reader.QuoteStrings}");
                Console.WriteLine($"PointerSize: {reader.PointerSize}");
                Console.WriteLine($"MatrixBigPosit: {reader.MatrixBigPosit}");
                Console.WriteLine($"CountCR: {reader.CountCR}");
                Console.WriteLine($"CountLF: {reader.CountLF}");
                Console.WriteLine($"CountCRLF: {reader.CountCRLF}");
                Console.WriteLine($"FloatFormat: {reader.FloatFormat}");
                Console.WriteLine($"----------------- END READER INFO -----------------");
            }

            // Battlezone requires CRLF for ASCII BZN portions
            if (reader.CountLF != reader.CountCR || reader.CountCR != reader.CountCRLF)
            {
                if (reader.CountCR == 0 && reader.CountLF > 0)
                {
                    Malformations.SetLineEnding("\n");
                }
                else if (reader.CountLF == 0 && reader.CountCR > 0)
                {
                    Malformations.SetLineEnding("\r");
                }
                else
                {
                    Malformations.SetLineEnding(null);
                }
            }

            if (reader.Format == BZNFormat.Battlezone2 && !reader.HasBinary)
            {
                FloatTextFormat ExpectedFloatFormat = FloatTextFormat.G;
                if (reader.Version >= 1182)
                {
                    ExpectedFloatFormat = FloatTextFormat._9e2;
                }

                if (ExpectedFloatFormat != reader.FloatFormat)
                {
                    Malformations.AddFloatFormat(reader.FloatFormat);//, ExpectedFloatFormat);
                }
            }
        }

        private void Hydrate(BZNStreamReader reader)
        {
            IBZNToken tok;

            // get count of GameObjects
            tok = reader.ReadToken();
            if (!tok.Validate("size", BinaryFieldType.DATA_LONG))
                throw new Exception("Failed to parse size/LONG");
            Int32 CountItems = tok.GetInt32();
            Console.WriteLine($"size: {CountItems}");

            // TODO hoist this up to property and ensure we can scan it for Malformations to be able to do a "has malformations" check
            // malformations, depending on what kind, might also let us rank mulitple options when the class is unclear
            EntityDescriptor[] GameObjects = new EntityDescriptor[CountItems];

            int CntPad = CountItems.ToString().Length;
            Dictionary<string, HashSet<string>> LongTermClassLabelLookupCache = new Dictionary<string, HashSet<string>>();
            Stopwatch w = new Stopwatch();
            w.Start();
            for (int gameObjectCounter = 0; gameObjectCounter < CountItems; gameObjectCounter++)
            {
                //GameObjects[gameObjectCounter] = new BZNGameObjectWrapper(reader, (gameObjectCounter + 1) == CountItems);
                EntityDescriptor? tmpObj;
                if (EntityDescriptor.Create(this, reader, CountItems - gameObjectCounter, out tmpObj, true, Hints: Hints) && tmpObj != null)
                {
                    GameObjects[gameObjectCounter] = tmpObj;
                    if (w.Elapsed > TimeSpan.FromMilliseconds(100))
                        Console.WriteLine($"GameObject[{gameObjectCounter.ToString().PadLeft(CntPad)}]: {GameObjects[gameObjectCounter].seqNo.ToString("X8")} {GameObjects[gameObjectCounter].PrjID.ToString().PadRight(16)} {(GameObjects[gameObjectCounter].gameObject?.ClassLabel ?? string.Empty).PadRight(16)} {GameObjects[gameObjectCounter].gameObject?.ToString()?.Replace(@"BZNParser.Battlezone.GameObject.", string.Empty)}");
                    w.Restart();
                }
            }
            w.Stop();

            this.Entities = GameObjects;

            TailParse(reader);
        }

        public void TailParse(BZNStreamReader reader)
        {
            IBZNToken tok;

            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version > 1165)
                {
                    // 1187, 1188, 1192
                    tok = reader.ReadToken();
                    if (!tok.Validate("groupTargets", BinaryFieldType.DATA_VOID))
                        throw new Exception("Failed to parse groupTargets/VOID");
                    this.groupTargets = tok.GetBytes();
                }
                if (reader.Version == 1100 || reader.Version == 1041 || reader.Version == 1047 || reader.Version == 1070) // not sure what versions this happens
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("name", BinaryFieldType.DATA_CHAR))
                        throw new Exception("Failed to parse name/CHAR");
                    this.Mission = Malformations.AddBinaryMessString("name", tok.GetString());
                    Console.WriteLine($"Mission: {this.Mission}");
                }
                else if (reader.Version < 1145)
                {
                    // max length 40
                    tok = reader.ReadToken();
                    if (!tok.Validate("dllName", BinaryFieldType.DATA_CHAR))
                        throw new Exception("Failed to parse dllName/CHAR");
                    this.Mission = Malformations.AddBinaryMessString("dllName", tok.GetString());
                    Console.WriteLine($"Mission: {this.Mission}");
                }
                else
                {
                    if (reader.InBinary)
                    {
                        tok = reader.ReadToken();
                        if (!tok.Validate(null, BinaryFieldType.DATA_CHAR))
                            throw new Exception("Failed to parse dllName/CHAR");
                    }
                    tok = reader.ReadToken();
                    if (!tok.Validate("dllName", BinaryFieldType.DATA_CHAR))
                        throw new Exception("Failed to parse dllName/CHAR");
                    this.Mission = Malformations.AddBinaryMessString("dllName", tok.GetString());
                    Console.WriteLine($"Mission: {this.Mission}");
                }
            }
            if (reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                string mission = string.Format("BZn64Mission_{0,4:X4}", tok.GetUInt16());
                Console.WriteLine($"Mission: {mission}");
                this.Mission = mission;

                sObject = reader.ReadBZ1_PtrDepricated("sObject");
            }
            if (reader.Format == BZNFormat.Battlezone)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("name", BinaryFieldType.DATA_CHAR))
                    throw new Exception("Failed to parse name/CHAR");
                this.Mission = Malformations.AddBinaryMessString("name", tok.GetString());
                Console.WriteLine($"Mission: {this.Mission}");

                // read the old sObject ptr, not sure what can be done with it
                if (reader.Version < 1002)
                {
                    sObject = reader.ReadBZ1_PtrDepricated("sObject");
                }
                else
                {
                    sObject = reader.ReadBZ1_Ptr("sObject", reader.Version);
                }
            }

            if (reader.Format == BZNFormat.Battlezone)
            {
                // Mission State (unsure if this needs to be later, but the undefbool below for lua is def needed)

                // LuaMission (which is invoked by many stock mission types)
                if (new string[] { "LuaMission", "MultSTMission", "MultDMMission", "Inst4XMission", "Inst03Mission" }.Contains(Mission))
                {
                    if (SaveType == SaveType.BZN ? reader.Version == 1044 : reader.Version >= 1044)
                    {
                        tok = reader.ReadToken();
                        if (!tok.Validate("undefbool", BinaryFieldType.DATA_BOOL))
                            throw new Exception("Failed to parse undefbool/BOOL");
                        bz1_luamission_started = tok.GetBoolean();
                    }
                    else
                    {
                        bz1_luamission_started = SaveType == SaveType.SAVE;
                    }

                    if (SaveType != SaveType.BZN)
                    {
                        // load lua mission state stuff here, LuaMission::Load

                        // load "count"
                        // loop
                        //     LoadValues
                    }
                }
            }

            // TODO determine if this should go below the BZ1 lua mission state or not, it's unclear
            if (!reader.InBinary)
            {
                tok = reader.ReadToken();
                if (!tok.IsValidationOnly() || !tok.Validate("AiMission", BinaryFieldType.DATA_UNKNOWN))
                    throw new Exception("Failed to parse [AiMission]");
            }

            // not sure what this is, probably tied to some mission types
            //if (reader.Format == BZNFormat.Battlezone && (reader.Version == 1001 || reader.Version == 1011 || reader.Version == 1012))
            if (reader.Format == BZNFormat.Battlezone && Mission == @"AiMission")
            {
                tok = reader.ReadToken();
                if (!tok.Validate("size", BinaryFieldType.DATA_LONG))
                    throw new Exception("Failed to parse size/LONG");
                AiMissionSize = tok.GetInt32(); // seems to always be 1
            }

            if (reader.Format == BZNFormat.Battlezone && (reader.Version == 1011 || reader.Version == 1012))
            {
                // this might also be due to the above count being 1 instead of 0, unknown, for now we're using the version

                tok = reader.ReadToken();
                if (!tok.Validate("name", BinaryFieldType.DATA_CHAR))
                    throw new Exception("Failed to parse name/CHAR");
                //tok.GetBytes(); // "AiMission"

                // read the old sObject ptr, not sure what can be done with it
                if (reader.Version < 1002)
                {
                    sObject = reader.ReadBZ1_PtrDepricated("sObject");
                }
                else
                {
                    sObject = reader.ReadBZ1_Ptr("sObject", reader.Version);
                }

                if (!reader.InBinary)
                {
                    tok = reader.ReadToken();
                    if (!tok.IsValidationOnly() || !tok.Validate("UserProcess", BinaryFieldType.DATA_UNKNOWN))
                        throw new Exception("Failed to parse [UserProcess]");
                }

                tok = reader.ReadToken();
                if (!tok.Validate("undefptr", BinaryFieldType.DATA_PTR))
                    throw new Exception("Failed to parse undefptr/PTR");
                UserProcess_sObject = tok.GetUInt32H();

                tok = reader.ReadToken();
                if (!tok.Validate("cycle", BinaryFieldType.DATA_UNKNOWN))
                    throw new Exception("Failed to parse cycle/UNKNOWN");
                UserProcess_cycle = tok.GetInt32();

                tok = reader.ReadToken();
                if (!tok.Validate("cycleMax", BinaryFieldType.DATA_UNKNOWN))
                    throw new Exception("Failed to parse cycleMax/UNKNOWN");
                UserProcess_cycleMax = tok.GetInt32();

                tok = reader.ReadToken();
                if (!tok.Validate("selectList", BinaryFieldType.DATA_UNKNOWN))
                    throw new Exception("Failed to parse selectList/UNKNOWN");
                UserProcess_selectList = tok.GetUInt32H();

                tok = reader.ReadToken();
                if (!tok.Validate("undefptr", BinaryFieldType.DATA_PTR))
                    throw new Exception("Failed to parse undefptr/PTR");
                UserProcess_undefptr_1 = tok.GetUInt32H();

                tok = reader.ReadToken();
                if (!tok.Validate("undefptr", BinaryFieldType.DATA_PTR))
                    throw new Exception("Failed to parse undefptr/PTR");
                UserProcess_undefptr_2 = tok.GetUInt32H();

                tok = reader.ReadToken();
                if (!tok.Validate("exited", BinaryFieldType.DATA_UNKNOWN))
                    throw new Exception("Failed to parse exited/UNKNOWN");
                UserProcess_exited = tok.GetBoolean();
            }

            // if reader.SaveType != 0

            if (!reader.InBinary)
            {
                tok = reader.ReadToken();
                if (!tok.IsValidationOnly() || !tok.Validate("AOIs", BinaryFieldType.DATA_UNKNOWN))
                    throw new Exception("Failed to parse [AOIs]");
            }

            tok = reader.ReadToken();
            if (!tok.Validate("size", BinaryFieldType.DATA_LONG))
                throw new Exception("Failed to parse size/LONG");
            Int32 CountAOIs = tok.GetInt32();

            AreaOfInterest[] AOIs = new AreaOfInterest[CountAOIs];
            for (int aioCounter = 0; aioCounter < CountAOIs; aioCounter++)
            {
                AreaOfInterest? tmpAio;
                if (AreaOfInterest.Create(this, reader, CountAOIs - aioCounter, out tmpAio, true) && tmpAio != null)
                {
                    AOIs[aioCounter] = tmpAio;
                }
            }

            this.AOIs = AOIs;

            if (!reader.InBinary)
            {
                tok = reader.ReadToken();
                if (!tok.IsValidationOnly() || !tok.Validate("AiPaths", BinaryFieldType.DATA_UNKNOWN))
                    throw new Exception("Failed to parse [AiPaths]");
            }

            tok = reader.ReadToken();
            if (!tok.Validate("count", BinaryFieldType.DATA_LONG))
                throw new Exception("Failed to parse count/LONG");
            Int32 CountPaths = tok.GetInt32();

            AiPath[] AiPaths = new AiPath[CountPaths];
            for (int pathCounter = 0; pathCounter < CountPaths; pathCounter++)
            {
                AiPath? tmpAiPath;
                if (AiPath.Create(this, reader, CountPaths, CountPaths - pathCounter, out tmpAiPath, true) && tmpAiPath != null)
                {
                    AiPaths[pathCounter] = tmpAiPath;
                }
            }
            this.AiPaths = AiPaths;

            if (reader.Format == BZNFormat.Battlezone2)
            {
                // SatellitePanel
                if (reader.Version >= 1125) // version 1169 failed to read this, might need a walk back for malformed
                {
                    reader.Bookmark.Mark();

                    // 1188 1192
                    tok = reader.ReadToken();
                    if (!tok.Validate("hasEntered", BinaryFieldType.DATA_BOOL))
                    {
                        if (tok.Validate("PadData", BinaryFieldType.DATA_VOID))
                        {
                            // SatellitePanel data is missing when it must exist, deal with damaged BZN?
                            reader.Bookmark.RevertToBookmark();
                        }
                        else
                        {
                            reader.Bookmark.Commit();
                            throw new Exception("Failed to parse hasEntered/BOOL");
                        }
                    }
                    else
                    {
                        reader.Bookmark.Commit();
                        for (int i = 0; i < 3/*MAX_WORLDS*/; i++)
                        {
                            tok = reader.ReadToken();
                            if (!tok.Validate("ownerObj", BinaryFieldType.DATA_LONG))
                                throw new Exception("Failed to parse ownerObj/LONG");
                            //Int32 pathType = tok.GetUInt32H();
                        }
                    }
                }
            }

            if (reader.Format == BZNFormat.Battlezone && (reader.Version == 1001 || reader.Version == 1011 || reader.Version == 1012))
            {
                if (!reader.InBinary)
                {
                    tok = reader.ReadToken();
                    if (!tok.IsValidationOnly() || !tok.Validate("AiTasks", BinaryFieldType.DATA_UNKNOWN))
                        throw new Exception("Failed to parse [AiTasks]");
                }

                tok = reader.ReadToken();
                if (!tok.Validate("count", BinaryFieldType.DATA_LONG))
                    throw new Exception("Failed to parse count/LONG");
                Int32 CountAiTasks = tok.GetInt32();

                for (int i = 0; i < CountAiTasks; i++)
                {
                }
            }

            if (reader.Format == BZNFormat.Battlezone2)
            {
                // code says this has to be SaveType != 0 to load, but they do always exist, very strange
                if (reader.Version >= 1115)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("PadData", BinaryFieldType.DATA_VOID))
                        throw new Exception("Failed to parse PadData/VOID");

                    if (reader.Version >= 1119)
                    {
                        tok = reader.ReadToken();
                        if (!tok.Validate("PadData2", BinaryFieldType.DATA_VOID))
                            throw new Exception("Failed to parse PadData2/VOID");
                    }
                }
            }

            if (reader.Format == BZNFormat.Battlezone && reader.Version == 1001)
            {
                if (!reader.InBinary)
                {
                    tok = reader.ReadToken();
                    if (!tok.IsValidationOnly() || !tok.Validate("Terrain", BinaryFieldType.DATA_UNKNOWN))
                        throw new Exception("Failed to parse [Terrain]");
                }

                tok = reader.ReadToken();
                TerrainName = tok.GetString();
                if (!tok.Validate("Name", BinaryFieldType.DATA_UNKNOWN))
                    throw new Exception("Failed to parse Name/UNKNOWN");
                Console.WriteLine($"TerrainName: {TerrainName}");
            }

            if (!reader.EndOfFile())
            {
                if (reader.Format == BZNFormat.Battlezone)
                {
                    // odd extra VEC2D at the end of the file with 0,0

                    tok = reader.ReadToken(); // returns null if the stream ends after chewing extra lines
                    if (tok != null)
                    {
                        if (!tok.Validate(null, BinaryFieldType.DATA_VEC2D))
                        {
                            throw new Exception("Tokens left after last known token");
                        }
                        Vector2D point = tok.GetVector2D();
                        if (point.X != 0 || point.Z != 0)
                            throw new Exception("Tokens left after last known token");
                        Malformations.AddExtraField("FILE_END", tok);
                    }
                }
            }

            if (!reader.EndOfFile())
            {
                throw new Exception("Tokens left after last known token");
            }

            // BZ1 version 2016 binary extra DATA_VEC2D at the end, not sure if this is universal
        }

        public void Write(BZNStreamWriter writer, bool binary = true, bool save = false, bool preserveMalformations = false)
        {
            // keep the float format if its non-standard
            if (preserveMalformations)
            {
                FloatTextFormat? floatTextFormat = Malformations.GetFloatTextFormat();
                if (floatTextFormat != null)
                {
                    writer.FloatFormat = floatTextFormat.Value;
                }

                string? newLine = Malformations.GetLineEnding();
                if (newLine != null)
                {
                    writer.NewLine = newLine;
                }
            }

            if (writer.Format != BZNFormat.BattlezoneN64)
            {
                var mal = Malformations.GetMalformations(Malformation.INCORRECT_NAME, "version");
                if (preserveMalformations && mal.Length > 0)
                {
                    writer.WriteSignedValues((string)mal[0].Fields[0], writer.Version);
                }
                else
                {
                    writer.WriteSignedValues("version", writer.Version);
                }
            }

            // Breadcrumb BZ2001-QUIRK
            if (writer.Format == BZNFormat.Battlezone2 && writer.Version != 1041 && writer.Version != 1047) // version is special case for bz2001.bzn
            {
                writer.WriteUnsignedValues("saveType", (UInt32)SaveType); // only in ASCII format in our testing
            }

            if (writer.Format == BZNFormat.Battlezone)
            {
                if (writer.Version > 1022)
                {
                    writer.WriteBooleans("binarySave", preserveMalformations ? Malformations : null, binary);
                    if (binary)
                        writer.SetBinary();

                    writer.WriteChars("msn_filename", msn_filename, Malformations);
                }
            }

            if (writer.Format == BZNFormat.Battlezone2)
            {
                writer.WriteBooleans("binarySave", preserveMalformations ? Malformations : null, binary);
                if (binary)
                    writer.SetBinary();

                writer.WriteSizedString_BZ2_1145("msn_filename", 16, msn_filename, Malformations);
            }

            // todo this is oddly messy, clean it up and confirm
            if (writer.Format == BZNFormat.BattlezoneN64 || (writer.Format == BZNFormat.Battlezone && writer.Version <= 1001))
            {
                writer.WriteUnsignedValues("seq_count", seq_count);
            }
            else if (writer.Format == BZNFormat.Battlezone || writer.Format == BZNFormat.Battlezone2)
            {
                /*if (writer.InBinary)
                {
                    writer.WriteCompressedNumberFromBinary(seq_count);
                }
                else
                {
                    writer.WriteUnsignedValues("seq_count", seq_count);
                }*/
                writer.WriteUnsignedValues("seq_count", seq_count);

                if (writer.Format == BZNFormat.Battlezone2)
                {
                    // duplicate SaveType?
                    writer.WriteSignedValues("saveType", (Int32)SaveType);
                }
            }

            if (writer.Format == BZNFormat.Battlezone || writer.Format == BZNFormat.BattlezoneN64)
            {
                bool AlreadyWroteMissionSave = false;
                if (preserveMalformations)
                {
                    if (writer.Format == BZNFormat.Battlezone)
                    {
                        var mals = Malformations.GetMalformations(Malformation.INCORRECT, "missionSave");
                        if (mals.Length > 0)
                        {
                            // we aren't writing this field because the original lacked it
                            AlreadyWroteMissionSave = true;
                        }
                    }
                }

                if (!AlreadyWroteMissionSave)
                {
                    if (writer.Format == BZNFormat.Battlezone && writer.Version < 1016)
                    {

                    }
                    //if ((1017 <= reader.Version && reader.Version <= 1037) || reader.Version == 1043 || reader.Version == 1045 || reader.Version == 2003 || reader.Version == 2016)
                    else
                    {
                        switch (SaveType)
                        {
                            case SaveType.BZN:
                                writer.WriteBooleans("missionSave", preserveMalformations ? Malformations : null, true);
                                break;
                            case SaveType.SAVE:
                                writer.WriteBooleans("missionSave", preserveMalformations ? Malformations : null, false);
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                }
            }

            if (writer.Format == BZNFormat.Battlezone || writer.Format == BZNFormat.BattlezoneN64)
            {
                if (writer.Format == BZNFormat.BattlezoneN64 || writer.Version != 1001)
                {
                    writer.WriteChars("TerrainName", TerrainName, Malformations);
                }
            }
            else if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version < 1171)
                {
                    // BZ2: 1123 1124s
                    if (preserveMalformations)
                        writer.WriteChars("TerrainName", Malformations.CheckBinaryMessString("TerrainName", TerrainName), Malformations);
                    else
                        writer.WriteChars("TerrainName", TerrainName, Malformations);
                }
                else if (writer.Version == 1171)
                {
                    var mal = Malformations.GetMalformations(Malformation.INCORRECT_NAME, "g_TerrainName");
                    if (preserveMalformations && mal.Length > 0)
                    {
                        writer.WriteChars((string)mal[0].Fields[0], TerrainName, Malformations);
                    }
                    else
                    {
                        writer.WriteChars("g_TerrainName", TerrainName, Malformations);
                    }
                }
                else
                {
                    writer.WriteChars("g_TerrainName", TerrainName, Malformations);
                }
            }

            if (writer.Format == BZNFormat.Battlezone)
            {
                if (writer.Version == 1011 || writer.Version == 1012)
                {
                    writer.WriteFloats("start_time", preserveMalformations ? Malformations : null, start_time ?? 0f);
                }
            }

            //DeHydrate(writer);
            writer.WriteSignedValues("size", Entities.Length);

            int idx = 0;
            foreach (var entity in Entities)
            {
                Console.WriteLine($"Writing entity [{idx}]");
                entity.Write(this, writer, binary, save, preserveMalformations);
                idx++;
            }

            //TailUnParse(writer);
            if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version > 1165)
                {
                    // 1187, 1188, 1192
                    writer.WriteVoidBytesL("groupTargets", groupTargets);
                }
                if (writer.Version == 1100 || writer.Version == 1041 || writer.Version == 1047 || writer.Version == 1070) // not sure what versions this happens
                {
                    writer.WriteChars("name", Malformations.CheckBinaryMessString("name", Mission), Malformations);
                }
                else if (writer.Version < 1145)
                {
                    // max length 40
                    writer.WriteChars("dllName", Malformations.CheckBinaryMessString("dllName", Mission), Malformations);
                }
                else
                {
                    if (writer.InBinary)
                    {
                        writer.WriteUnsignedValues(null, (byte)Mission.Length);
                    }
                    writer.WriteChars("dllName", Malformations.CheckBinaryMessString("dllName", Mission), Malformations);
                }
            }
            if (writer.Format == BZNFormat.BattlezoneN64)
            {
                Match m = Regex.Match(Mission, "BZn64Mission_(?<id>[0-9A-F]{4})");
                if (m.Success)
                    writer.WriteUnsignedValues(null, UInt16.Parse(m.Groups["id"].Value));
                else
                    throw new InvalidCastException("Mission name was not in expected format");

                writer.WriteBZ1_PtrDepricated("sObject", (UInt32)sObject);
            }
            if (writer.Format == BZNFormat.Battlezone)
            {
                writer.WriteChars("name", Mission, Malformations);

                // read the old sObject ptr, not sure what can be done with it
                if (writer.Version < 1002)
                {
                    writer.WriteBZ1_PtrDepricated("sObject", (UInt32)sObject);
                }
                else
                {
                    writer.WriteBZ1_Ptr("sObject", sObject);
                }
            }

            if (writer.Format == BZNFormat.Battlezone)
            {
                // Mission State

                // LuaMission (which is invoked by many stock mission types)
                if (new string[] { "LuaMission", "MultSTMission", "MultDMMission", "Inst4XMission", "Inst03Mission" }.Contains(Mission))
                {
                    if (SaveType == SaveType.BZN ? writer.Version == 1044 : writer.Version >= 1044)
                    {
                        writer.WriteBooleans("undefbool", preserveMalformations ? Malformations : null, bz1_luamission_started ?? SaveType == SaveType.BZN);

                        // TODO other lua state values here?
                    }
                }
            }

            // TODO determine if this should go below the BZ1 lua mission state or not, it's unclear
            writer.WriteValidation("AiMission");

            if (writer.Format == BZNFormat.Battlezone && (writer.Version == 1001 || writer.Version == 1011 || writer.Version == 1012))
            {
                writer.WriteSignedValues("size", AiMissionSize ?? 1);
            }

            if (writer.Format == BZNFormat.Battlezone && (writer.Version == 1011 || writer.Version == 1012))
            {
                // this might also be due to the above count being 1 instead of 0, unknown, for now we're using the version
                
                writer.WriteChars("name", "AiMission", Malformations);

                // read the old sObject ptr, not sure what can be done with it
                if (writer.Version < 1002)
                {
                    writer.WriteBZ1_PtrDepricated("sObject", (UInt32)sObject);
                }
                else
                {
                    writer.WriteBZ1_Ptr("sObject", sObject);
                }

                writer.WriteValidation("UserProcess");
                writer.WriteBZ1_PtrDepricated("undefptr", UserProcess_sObject.Value);
                writer.WriteSignedValues("cycle", UserProcess_cycle.Value);
                writer.WriteSignedValues("cycleMax", UserProcess_cycleMax.Value);
                writer.WriteBZ1_PtrDepricated("selectList", UserProcess_selectList.Value);
                writer.WriteBZ1_PtrDepricated("undefptr", UserProcess_undefptr_1.Value);
                writer.WriteBZ1_PtrDepricated("undefptr", UserProcess_undefptr_2.Value);
                writer.WriteBooleans("exited", preserveMalformations ? Malformations : null, UserProcess_exited.Value);
            }

            writer.WriteValidation("AOIs");
            writer.WriteSignedValues("size", AOIs.Length);
            for (int aioCounter = 0; aioCounter < AOIs.Length; aioCounter++)
            {
                AOIs[aioCounter].Write(this, writer, binary, save, preserveMalformations);
            }

            writer.WriteValidation("AiPaths");
            writer.WriteSignedValues("count", AiPaths.Length);
            for (int i = 0; i < AiPaths.Length; i++)
            {
                AiPaths[i].Write(this, writer, binary, save, preserveMalformations);
            }
        }
    }
}
