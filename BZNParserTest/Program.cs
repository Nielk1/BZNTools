using BZNParser;
using BZNParser.Battlezone;
using BZNParser.Reader;

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

            //foreach (string filename in new string[] { }
            //.Concat(Directory.EnumerateFiles(@"D:\Program Files (x86)\GOG Galaxy\Games\Battlezone Combat Commander\bz2r_res", "*.bzn", SearchOption.AllDirectories))
            //.Concat(Directory.EnumerateFiles(@"D:\Program Files (x86)\GOG Galaxy\Games\Battlezone Combat Commander\maps", "*.bzn", SearchOption.AllDirectories))
            //.Concat(Directory.EnumerateFiles(@"F:\Programming\BZRModManager\BZRModManager\BZRModManager\bin\steamcmd\steamapps\workshop\content\624970", "*.bzn", SearchOption.AllDirectories))
            //.Concat(Directory.EnumerateFiles(@"F:\Battlezone\Projects\BZ98R\rotbd\src\mission", "*.bzn", SearchOption.AllDirectories))
            //.Concat(Directory.EnumerateFiles(@"F:\Programming\BZRModManager\GenerateMultiplayerDataExtract\GenerateMultiplayerDataExtract\bin\Debug\BZ98R", "*.bzn", SearchOption.AllDirectories))
            //.Concat(Directory.EnumerateFiles(@"F:\Programming\BZRModManager\BZRModManager\BZRModManager\bin\steamcmd\steamapps\workshop\content\301650", "*.bzn", SearchOption.AllDirectories))
            //.Concat(Directory.EnumerateFiles(@"..\..\..\sample", "*", SearchOption.AllDirectories))
            //.Concat(Directory.EnumerateFiles(@"..\..\..\..\old\sample", "*", SearchOption.AllDirectories))
            //.Concat(Directory.EnumerateFiles(@"..\..\..\..\TempApp\bin\Debug\net8.0\out", "*", SearchOption.AllDirectories))
            //    )
            //foreach (string filename in new string[] { @"F:\Downloads\nsdf01c.bzn", @"F:\Downloads\nsdf01c (1).bzn" })
            //foreach (string filename in Directory.EnumerateFiles(@"lists", "Battlezone *.txt", SearchOption.TopDirectoryOnly).SelectMany(fn => File.ReadAllLines(fn).Where(dr => dr.Length > 0)))
            //foreach (string filename in Directory.EnumerateFiles(@"lists", "BattlezoneN64 *.txt", SearchOption.TopDirectoryOnly).SelectMany(fn => File.ReadAllLines(fn).Where(dr => dr.Length > 0)))
            //foreach (string filename in Directory.EnumerateFiles(@"lists", "Battlezone2 *.txt", SearchOption.TopDirectoryOnly).SelectMany(fn => File.ReadAllLines(fn).Where(dr => dr.Length > 0)))
            //foreach (string filename in File.ReadAllLines(@"lists\Battlezone 2016.txt").Where(dr => dr.Length > 0))
            //foreach (string filename in File.ReadAllLines(@"WARN_ExtraTokens.txt").Where(dr => dr.Length > 0).Select(dr => dr.Split('\t')[1]))
            //string filename = @"F:\Programming\BZRModManager\GenerateMultiplayerDataExtract\GenerateMultiplayerDataExtract\bin\Debug\BZ98R\bzone\misn10.bzn";
            string filename = @"..\..\..\sample\bz98r\misn10.bzn";
            {
                Console.WriteLine(filename);

                //if (Success.Contains(filename))
                //    continue;

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
                                        //bool success = false;
                                        try
                                        {
                                            switch (reader.Format)
                                            {
                                                case BZNFormat.Battlezone:
                                                case BZNFormat.BattlezoneN64:
                                                    new BZNFileBattlezone(reader, Hints: BZ1Hints);
                                                    break;
                                                case BZNFormat.Battlezone2:
                                                    new BZNFileBattlezone(reader, Hints: BZ2Hints);
                                                    break;
                                            }

                                            //success = true;
                                            File.AppendAllText("success.txt", $"{filename}\r\n");
                                            //File.AppendAllText($"{reader.Format.ToString()} {reader.Version.ToString("D4")}.txt", $"{filename}\r\n");
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine($"Error: {ex.Message}");
                                            Console.ResetColor();
                                            //Console.ReadKey(true);
                                            File.AppendAllText("failed.txt", $"{filename}\r\n");
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
            //Console.ReadKey(true);
            /*using (var writer = File.CreateText("files.txt"))
            {
                foreach (KeyValuePair<BznType, List<(string, bool)>> entry in Files.OrderBy(dr => dr.Key.version).ThenBy(dr => dr.Key.version).ThenBy(dr => dr.Key.format))
                {
                    foreach ((string, bool) item in entry.Value.OrderBy(dr => dr))
                    {
                        writer.WriteLine($"{entry.Key.version}\t{entry.Key.binary}\t{entry.Key.format}\t{item.Item2}\t{item.Item1}");
                    }
                }
            }*/
        }
    }
}
