using BZNParser;
using BZNParser.Battlezone;
using BZNParser.Battlezone.GameObject;
using BZNParser.Tokenizer;
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
            Dictionary<BznType, List<(string, bool)>> Files = new Dictionary<BznType, List<(string, bool)>>();

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
                            //File.Delete(filename);
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
            new string[] { }
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

                .Concat(Directory.EnumerateFiles(@"..\..\..\..\test_files\Battlezone2", "*.bzn", SearchOption.AllDirectories)
                //.Concat(Directory.EnumerateFiles(@"..\..\..\..\test_files\Battlezone", "*.bzn", SearchOption.AllDirectories)
                .Where(dr => Path.GetFileName(Path.GetDirectoryName(dr)).Length == 5)
                .OrderByDescending(dr => Path.GetDirectoryName(dr))
                .ThenBy(dr => Path.GetFileName(dr)))

                //.Where(dr => dr != @"F:\Programming\BZRModManager\BZRModManager\BZRModManager\bin\steamcmd\steamapps\workshop\content\301650\3660662144\testmap.bzn")
                //.AsParallel().WithDegreeOfParallelism(24).ForAll(filename =>
                .ToList().ForEach(filename =>
            {
                Console.WriteLine(filename);
                Console.Title = filename;

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
                                                using (BZNStreamWriter writer = new BZNStreamWriter(ms, reader.Format, reader.Version, reader.GetDefects()))
                                                {
                                                    bzn.Write(writer, binary: reader.HasBinary, save: false, preserveMalformations: true);
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
            });
            //Console.ReadKey(true);
        }
    }
}
