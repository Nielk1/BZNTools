using BZNParser;
using BZNParser.Battlezone;
using BZNParser.Tokenizer;

namespace BZNParserTest;

internal static class CorpusCheck
{
    internal static int Run(string[] args)
    {
        const string usage = "Usage: BZNParserTest --check [--exact] <file-or-directory> ...";
        if (args.Length == 1 && (args[0] == "--help" || args[0] == "-h"))
        {
            Console.WriteLine(usage);
            Console.WriteLine("Reads maps and checks in-memory write/read round trips; source files are never written.");
            Console.WriteLine("--exact also requires byte-for-byte equality with the original.");
            return 0;
        }
        if (args[0] != "--check" || args.Length < 2 || args.Skip(1).Any(a => a.StartsWith("--") && a != "--exact"))
        {
            Console.Error.WriteLine(usage);
            return 2;
        }

        bool exact = args.Contains("--exact");
        var files = new SortedSet<string>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        int failures = 0;
        foreach (string target in args.Skip(1).Where(a => a != "--exact"))
        {
            try
            {
                if (File.Exists(target)) files.Add(Path.GetFullPath(target));
                else if (Directory.Exists(target))
                {
                    foreach (string path in Directory.EnumerateFiles(target, "*", SearchOption.AllDirectories)
                        .Where(path => Path.GetExtension(path).Equals(".bzn", StringComparison.OrdinalIgnoreCase)))
                        files.Add(Path.GetFullPath(path));
                }
                else throw new FileNotFoundException("File or directory does not exist.", target);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
            {
                Console.Error.WriteLine($"FAIL {target}: {ex.Message}");
                failures++;
            }
        }
        if (files.Count == 0)
        {
            Console.Error.WriteLine("No BZN files found.");
            return failures > 0 ? 1 : 2;
        }

        var bz1Hints = BattlezoneBZNHints.BuildHintsBZ1();
        var bz2Hints = BattlezoneBZNHints.BuildHintsBZ2();
        int identical = 0;
        foreach (string path in files)
        {
            try
            {
                byte[] source = File.ReadAllBytes(path);
                using var input = new MemoryStream(source, writable: false);
                using var reader = new BZNStreamReader(input, path);
                if (reader.Format != BZNFormat.Battlezone && reader.Format != BZNFormat.Battlezone2 && reader.Format != BZNFormat.BattlezoneN64)
                    throw new NotSupportedException($"Unsupported game format: {reader.Format}");
                var hints = reader.Format == BZNFormat.Battlezone2 ? bz2Hints : bz1Hints;
                var map = new BZNFileBattlezone(reader, hints);
                using var output = new MemoryStream();
                using (var writer = new BZNStreamWriter(output, reader.Format, reader.Version,
                    preserveMalformations: true, preserveDefects: reader.GetDefects()))
                    map.Write(writer, binary: reader.HasBinary, save: map.SaveType != SaveType.BZN);
                byte[] written = output.ToArray();
                using var checkStream = new MemoryStream(written, writable: false);
                using var checkReader = new BZNStreamReader(checkStream, path);
                var checkMap = new BZNFileBattlezone(checkReader, hints);
                if (map.Entities.Length != checkMap.Entities.Length || map.AiPaths.Length != checkMap.AiPaths.Length)
                    throw new InvalidDataException("Object or path count changed during the round trip.");
                bool same = source.AsSpan().SequenceEqual(written);
                if (exact && !same) throw new InvalidDataException("Round-trip bytes differ from the original.");
                if (same) identical++;
                Console.WriteLine($"PASS v{reader.Version} {(reader.HasBinary ? "binary" : "text")} {map.Entities.Length} objects, {map.AiPaths.Length} paths{(same ? ", byte-identical" : "")}: {path}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"FAIL {path}: {ex.Message}");
                failures++;
            }
        }
        Console.WriteLine($"Checked {files.Count} files; {identical} byte-identical; {failures} failures.");
        return failures == 0 ? 0 : 1;
    }
}
