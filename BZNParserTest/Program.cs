using BZNParser;
using BZNParser.Battlezone;
using BZNParser.Battlezone.GameObject;
using BZNParser.Tokenizer;
using System.Collections.Concurrent;
using System.Linq;

namespace BZNParserTest
{
    internal class Program
    {
        record struct BznType(int version, bool binary, BZNFormat format);
        static void Main(string[] args)
        {
            BattlezoneBZNHints? BZ1Hints = BattlezoneBZNHints.BuildHintsBZ1();
            BattlezoneBZNHints? BZ2Hints = BattlezoneBZNHints.BuildHintsBZ2();

            HashSet<string> Success = new HashSet<string>();
            if (File.Exists("success.txt"))
                foreach (string line in File.ReadAllLines("success.txt"))
                    Success.Add(line);
            //Dictionary<BznType, List<(string, bool)>> Files = new Dictionary<BznType, List<(string, bool)>>();

            HashSet<string> SuccessTest2 = new HashSet<string>();
            if (File.Exists("success_test2.txt"))
                foreach (string line in File.ReadAllLines("success_test2.txt"))
                    SuccessTest2.Add(line);

            object successTxtLock = new object();
            object errorTxtLock = new object();

            /*
            HashSet<string> KnownFiles = Directory.EnumerateFiles(@"..\..\..\..\test_files", "*.bzn", SearchOption.AllDirectories).Select(dr => Path.GetFileNameWithoutExtension(dr)).ToHashSet<string>();
            foreach (string filename in Directory.EnumerateFiles(@"..\..\..\..\source_test_files", "*.bzn", SearchOption.AllDirectories))
            {
                if (new FileInfo(filename).Length > 0)
                {
                    string testPath = @"F:\Programming\BZNTools\test_files";
                    string? foldername = null;
                    string? hashString = null;

                    using (FileStream file = File.OpenRead(filename))
                    {
                        using (BZNStreamReader reader = new BZNStreamReader(file, filename))
                        {
                            foldername = $@"{reader.Format}\{reader.Version}{(reader.HasBinary ? 'B' : 'A')}";

                            // generate SHA256 of file
                            using (var sha256 = System.Security.Cryptography.SHA256.Create())
                            {
                                using (var stream = File.OpenRead(filename))
                                {
                                    byte[] hashBytes = sha256.ComputeHash(stream);
                                    hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                                }
                            }
                        }
                    }

                    if (foldername != null && hashString != null)
                    {
                        string outFilename = Path.Combine(testPath, foldername, $"{hashString}.bzn");
                        if (KnownFiles.Contains(hashString))
                        {
                            File.Delete(filename);
                        }
                        else
                        {
                            if (!Directory.Exists(Path.GetDirectoryName(outFilename)))
                                Directory.CreateDirectory(Path.GetDirectoryName(outFilename));
                            //File.Move(filename, outFilename, overwrite: true);
                            File.Copy(filename, outFilename, overwrite: true);
                        }
                    }
                }
            }
            */

            /*foreach (string filename in*/
            var fileList = new string[] { }
                //.Concat(Directory.EnumerateFiles(@"..\..\..\..\test_files\Battlezone2\1197B", "*", SearchOption.AllDirectories))
                //.Concat(Directory.EnumerateFiles(@"..\..\..\..\test_files\Battlezone2\1197A", "*", SearchOption.AllDirectories))
                //
                //.Concat(Directory.EnumerateFiles(@"..\..\..\..\test_files\Battlezone2\1196B", "*", SearchOption.AllDirectories))
                //.Concat(Directory.EnumerateFiles(@"..\..\..\..\test_files\Battlezone2\1196A", "*", SearchOption.AllDirectories))
                //
                //.Concat(Directory.EnumerateFiles(@"..\..\..\..\test_files\Battlezone2\1194A", "*", SearchOption.AllDirectories))
                //.Concat(Directory.EnumerateFiles(@"..\..\..\..\test_files\Battlezone2\1193A", "*", SearchOption.AllDirectories))
                //
                //.Concat(Directory.EnumerateFiles(@"..\..\..\..\test_files\Battlezone2\1192B", "*", SearchOption.AllDirectories))
                //.Concat(Directory.EnumerateFiles(@"..\..\..\..\test_files\Battlezone2\1192A", "*", SearchOption.AllDirectories))

                .Concat(Directory.EnumerateFiles(@"..\..\..\..\test_files\Battlezone2", "*.bzn", SearchOption.AllDirectories))
                .Concat(Directory.EnumerateFiles(@"..\..\..\..\test_files\Battlezone", "*.bzn", SearchOption.AllDirectories))
                .Concat(Directory.EnumerateFiles(@"..\..\..\..\test_files\BattlezoneN64", "*.bzn", SearchOption.AllDirectories))
                //.Where(dr => Path.GetFileName(Path.GetDirectoryName(dr))!.Length <= 5)

                .OrderByDescending(dr => Path.GetDirectoryName(dr))
                .ThenBy(dr => Path.GetFileName(dr))
                //.Where(dr => dr != @"F:\Programming\BZRModManager\BZRModManager\BZRModManager\bin\steamcmd\steamapps\workshop\content\301650\3660662144\testmap.bzn")
                ;

            int count = fileList.Count();
            int countWidth = count.ToString().Length;
            int i = 0;
            int j = 0;
            object iLock = new object();
            string lastName;

            //fileList.AsParallel().WithDegreeOfParallelism(24).ForAll(filename =>
            fileList.ToList().ForEach(filename =>
            {
                Console.WriteLine(filename);
                lastName = filename;
                Console.Title = $"{i.ToString().PadLeft(countWidth)}/{count} {j.ToString().PadLeft(countWidth)}/{count} {lastName}";
                lock (iLock)
                    i++;

                try
                {
                    if (Success.Contains(filename))
                        //continue;
                        return;

                    if (new FileInfo(filename).Length > 0)
                    {
                        using (FileStream file = File.OpenRead(filename))
                        {
                            using (BZNStreamReader reader = new BZNStreamReader(file, filename))
                            {
                                //if (reader.HasBinary)
                                //    continue;

                                switch (reader.Format)
                                {
                                    case BZNFormat.Battlezone:
                                    case BZNFormat.BattlezoneN64:
                                    case BZNFormat.Battlezone2:
                                        {
                                            //if (reader.HasBinary)
                                            //    continue;

                                            //bool success = false;
                                            BZNFileBattlezone? bzn = null;
                                            try
                                            {
                                                switch (reader.Format)
                                                {
                                                    case BZNFormat.Battlezone:
                                                    case BZNFormat.BattlezoneN64:
                                                        bzn = new BZNFileBattlezone(reader, Hints: BZ1Hints);
                                                        break;
                                                    case BZNFormat.Battlezone2:
                                                        bzn = new BZNFileBattlezone(reader, Hints: BZ2Hints);
                                                        break;
                                                }

                                                if (bzn != null)
                                                {
                                                    //if (!bzn.Entities.Any(dr => dr.gameObject.GetType() == typeof(MultiClass)))
                                                    //{
                                                    //    //success = true;
                                                    //    File.AppendAllText("success.txt", $"{filename}\r\n");
                                                    //    //File.AppendAllText($"{reader.Format.ToString()} {reader.Version.ToString("D4")}.txt", $"{filename}\r\n");
                                                    //}
                                                    //else
                                                    //{
                                                    //
                                                    //}

                                                    using (FileStream compareStream = File.OpenRead(filename))
                                                    using (MatchesStream ms = new MatchesStream(compareStream))
                                                    using (BZNStreamWriter writer = new BZNStreamWriter(ms, reader.Format, reader.Version, true, reader.GetDefects()))
                                                    {
                                                        bzn.Write(writer, binary: reader.HasBinary, save: false);
                                                    }
                                                    lock (successTxtLock)
                                                    {
                                                        File.AppendAllText("success.txt", $"{filename}\r\n");
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                lock (errorTxtLock)
                                                {
                                                    Console.ForegroundColor = ConsoleColor.Red;
                                                    Console.WriteLine($"Error: {ex.Message}");
                                                    Console.ResetColor();
                                                    //Console.ReadKey(true);
                                                    File.AppendAllText("failed.txt", $"{filename}\t{ex.Message}\r\n");
                                                }
                                            }
                                            finally
                                            {
                                                /*BznType bznType = new BznType(reader.Version, reader.HasBinary, reader.Format);
                                                if (!Files.ContainsKey(bznType))
                                                    Files[bznType] = new List<(string, bool)>();
                                                Files[bznType].Add((filename, success));*/
                                            }
                                        }
                                        break;
                                    case BZNFormat.StarTrekArmada:
                                    case BZNFormat.StarTrekArmada2:
                                        break;
                                }
                            }
                        }
                        //Console.ReadKey(true);
                    }
                }
                finally
                {
                    Console.Title = $"{i.ToString().PadLeft(countWidth)}/{count} {j.ToString().PadLeft(countWidth)}/{count} {lastName}";
                    lock (iLock)
                        j++;
                }
            });
            //Console.ReadKey(true);

            i = 0;
            j = 0;
            ConcurrentDictionary<string, (int, int)> SubCounters = new ConcurrentDictionary<string, (int, int)>();
            fileList.AsParallel().WithDegreeOfParallelism(14).ForAll(filename =>
            //fileList.ToList().ForEach(filename =>
            {
                Console.WriteLine(filename);
                lastName = filename;
                //Console.Title = $"{i.ToString().PadLeft(countWidth)}/{count} {j.ToString().PadLeft(countWidth)}/{count} {lastName}";
                Console.Title = $"{i.ToString().PadLeft(countWidth)}/{count} {j.ToString().PadLeft(countWidth)}/{count} {lastName} - {string.Join(' ', SubCounters.Select(dr => $"{dr.Value.Item1}/{dr.Value.Item2}"))}";
                lock (iLock)
                    i++;

                try
                {
                    if (!SuccessTest2.Contains(filename)) // check if we consider it "all solved"
                    {
                        if (new FileInfo(filename).Length > 0)
                        {
                            using (FileStream file = File.OpenRead(filename))
                            {
                                using (BZNStreamReader reader = new BZNStreamReader(file, filename))
                                {
                                    switch (reader.Format)
                                    {
                                        case BZNFormat.Battlezone:
                                        case BZNFormat.BattlezoneN64:
                                        case BZNFormat.Battlezone2:
                                            {
                                                BZNFileBattlezone? bzn = null;
                                                int[] testList = { };

                                                switch (reader.Format)
                                                {
                                                    case BZNFormat.Battlezone:
                                                    case BZNFormat.BattlezoneN64:
                                                        bzn = new BZNFileBattlezone(reader, Hints: BZ1Hints);
                                                        testList = new int[] { 1001, 1011, 1012, 1017, 1018, 1022, 1029, 1030, 1032, 1033, 1034, 1035, 1036, 1037, 1038, 1039, 1040, 1041, 1043, 1044, 1045, 1047, 1048, 1049, 2003, 2004, 2010, 2011, 2015, 2016 };
                                                        break;
                                                    case BZNFormat.Battlezone2:
                                                        bzn = new BZNFileBattlezone(reader, Hints: BZ2Hints);
                                                        testList = new int[] { 1041, 1047, 1070, 1100, 1101, 1103, 1104, 1105, 1108, 1109, 1112, 1120, 1121, 1122, 1123, 1124, 1126, 1127, 1128, 1135, 1137, 1138, 1141, 1142, 1143, 1145, 1147, 1148, 1149, 1150, 1151, 1154, 1160, 1164, 1165, 1167, 1169, 1170, 1171, 1173, 1178, 1179, 1180, 1182, 1183, 1185, 1186, 1187, 1188, 1189, 1190, 1192, 1193, 1194, 1196, 1197 };
                                                        break;
                                                }

                                                if (bzn != null)
                                                {
                                                    SubCounters[filename] = (0, testList.Length * 2);

                                                    bool failed = false;
                                                    int z = 0;
                                                    foreach (int test in testList.OrderByDescending(dr => dr))
                                                    {
                                                        for (int b = 0; b < 2; b++)
                                                        {
                                                            string testingLine = $"{filename}\t{test}{(b > 0 ? 'B' : 'A')}";
                                                            if (!SuccessTest2.Contains(testingLine))
                                                            {
                                                                try
                                                                {
                                                                    var fmt = reader.Format;
                                                                    if (fmt == BZNFormat.BattlezoneN64)
                                                                        fmt = BZNFormat.Battlezone;
                                                                    using (MemoryStream ms = new MemoryStream())
                                                                    using (BZNStreamWriter writer = new BZNStreamWriter(ms, reader.Format, test))
                                                                    {
                                                                        bzn.Write(writer, binary: b > 0, save: false);
                                                                    }
                                                                    SubCounters[filename] = (SubCounters[filename].Item1 + 1, SubCounters[filename].Item2);
                                                                    Console.Title = $"{i.ToString().PadLeft(countWidth)}/{count} {j.ToString().PadLeft(countWidth)}/{count} {lastName} - {string.Join(' ', SubCounters.Select(dr => $"{dr.Value.Item1}/{dr.Value.Item2}"))}";
                                                                    lock (successTxtLock)
                                                                    {
                                                                        File.AppendAllText("success_test2.txt", $"{testingLine}\r\n");
                                                                    }
                                                                }
                                                                catch (Exception ex)
                                                                {
                                                                    failed = true;
                                                                    lock (errorTxtLock)
                                                                    {
                                                                        Console.ForegroundColor = ConsoleColor.Red;
                                                                        Console.WriteLine($"Error: {ex.Message}");
                                                                        Console.ResetColor();
                                                                        //Console.ReadKey(true);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        z++;
                                                    }
                                                    if (!failed)
                                                    {
                                                        lock (successTxtLock)
                                                        {
                                                            File.AppendAllText("success_test2.txt", $"{filename}\r\n");
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                        case BZNFormat.StarTrekArmada:
                                        case BZNFormat.StarTrekArmada2:
                                            break;
                                    }
                                }
                            }
                            //Console.ReadKey(true);
                        }
                    }
                }
                finally
                {
                    SubCounters.Remove(filename, out _);
                    //Console.Title = $"{i.ToString().PadLeft(countWidth)}/{count} {j.ToString().PadLeft(countWidth)}/{count} {lastName}";
                    Console.Title = $"{i.ToString().PadLeft(countWidth)}/{count} {j.ToString().PadLeft(countWidth)}/{count} {lastName} - {string.Join(' ', SubCounters.Select(dr => $"{dr.Value.Item1}/{dr.Value.Item2}"))}";
                    lock (iLock)
                        j++;
                }
            });
            //Console.ReadKey(true);
        }
    }
}
