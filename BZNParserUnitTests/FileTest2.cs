using BZNParser;
using BZNParser.Battlezone;
using BZNParser.Tokenizer;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace BZNParserUnitTests;

public class GlobalTestDataFixture2 : IDisposable
{
    public BattlezoneBZNHints? BZ1Hints { get; private set; }
    public BattlezoneBZNHints? BZ2Hints { get; private set; }

    public GlobalTestDataFixture2()
    {
        BZ1Hints = BattlezoneBZNHints.BuildHintsBZ1();
        BZ2Hints = BattlezoneBZNHints.BuildHintsBZ2();
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}

public class FileTest2 : IClassFixture<GlobalTestDataFixture2>
{
    private readonly GlobalTestDataFixture2 _fixture;
    public FileTest2(GlobalTestDataFixture2 fixture)
    {
        _fixture = fixture;
    }

    public static IEnumerable<object[]> BZn64Files()
    {
        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\test_files\BattlezoneN64");
        if (Directory.Exists(folderPath))
        {
            foreach (var filePath in Directory.EnumerateFiles(folderPath, "*.bzn", SearchOption.AllDirectories))
            {
                yield return new object[] { filePath };
            }
        }
    }

    public static IEnumerable<object[]> BZ1Files()
    {
        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\test_files\Battlezone");
        if (Directory.Exists(folderPath))
        {
            foreach (var filePath in Directory.EnumerateFiles(folderPath, "*.bzn", SearchOption.AllDirectories))
            {
                yield return new object[] { filePath };
            }
        }
    }

    public static IEnumerable<object[]> BZ2Files()
    {
        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\test_files\Battlezone2");
        if (Directory.Exists(folderPath))
        {
            foreach (var filePath in Directory.EnumerateFiles(folderPath, "*.bzn", SearchOption.AllDirectories))
            {
                yield return new object[] { filePath };
            }
        }
    }

    [Theory]
    [MemberData(nameof(BZn64Files))]
    public void WriteAllVersions_BZn64(string filePath)
    {
        var bz1Versions = new[] { 1001, 1011, 1012, 1017, 1018, 1022, 1029, 1030, 1032, 1033, 1034, 1035, 1036, 1037, 1038, 1039, 1040, 1041, 1043, 1044, 1045, 1047, 1048, 1049, 2003, 2004, 2010, 2011, 2015, 2016 };
        var errors = new List<string>();

        foreach (var version in bz1Versions)
        {
            foreach (var binary in new[] { false, true })
            {
                try
                {
                    using var file = File.OpenRead(filePath);
                    using var reader = new BZNStreamReader(file, filePath);
                    var bzn = new BZNFileBattlezone(reader, Hints: _fixture.BZ1Hints);
                    using var ms = new MemoryStream();
                    using var writer = new BZNStreamWriter(ms, BZNFormat.BattlezoneN64, version);
                    bzn.Write(writer, binary: binary, save: false);
                }
                catch (Exception ex)
                {
                    errors.Add($"v{version} {(binary ? "bin" : "asc")}: {ex.Message}");
                }
            }
        }

        if (errors.Count > 0)
        {
            Assert.False(true, $"File: {filePath}\nFailures:\n{string.Join("\n", errors)}");
        }
    }

    [Theory]
    [MemberData(nameof(BZ1Files))]
    public void WriteAllVersions_BZ1(string filePath)
    {
        var bz1Versions = new[] { 1001, 1011, 1012, 1017, 1018, 1022, 1029, 1030, 1032, 1033, 1034, 1035, 1036, 1037, 1038, 1039, 1040, 1041, 1043, 1044, 1045, 1047, 1048, 1049, 2003, 2004, 2010, 2011, 2015, 2016 };
        var errors = new List<string>();

        foreach (var version in bz1Versions)
        {
            foreach (var binary in new[] { false, true })
            {
                try
                {
                    using var file = File.OpenRead(filePath);
                    using var reader = new BZNStreamReader(file, filePath);
                    var bzn = new BZNFileBattlezone(reader, Hints: _fixture.BZ1Hints);
                    using var ms = new MemoryStream();
                    using var writer = new BZNStreamWriter(ms, BZNFormat.Battlezone, version);
                    bzn.Write(writer, binary: binary, save: false);
                }
                catch (Exception ex)
                {
                    errors.Add($"v{version} {(binary ? "bin" : "asc")}: {ex.Message}");
                }
            }
        }

        if (errors.Count > 0)
        {
            Assert.False(true, $"File: {filePath}\nFailures:\n{string.Join("\n", errors)}");
        }
    }

    [Theory]
    [MemberData(nameof(BZ2Files))]
    public void WriteAllVersions_BZ2(string filePath)
    {
        var bz2Versions = new[] { 1041, 1047, 1070, 1100, 1101, 1103, 1104, 1105, 1108, 1109, 1112, 1120, 1121, 1122, 1123, 1124, 1126, 1127, 1128, 1135, 1137, 1138, 1141, 1142, 1143, 1145, 1147, 1148, 1149, 1150, 1151, 1154, 1160, 1164, 1165, 1167, 1169, 1170, 1171, 1173, 1178, 1179, 1180, 1182, 1183, 1185, 1186, 1187, 1188, 1189, 1190, 1192, 1193, 1194, 1196, 1197 };
        var errors = new List<string>();

        foreach (var version in bz2Versions)
        {
            foreach (var binary in new[] { false, true })
            {
                try
                {
                    using var file = File.OpenRead(filePath);
                    using var reader = new BZNStreamReader(file, filePath);
                    var bzn = new BZNFileBattlezone(reader, Hints: _fixture.BZ2Hints);
                    using var ms = new MemoryStream();
                    using var writer = new BZNStreamWriter(ms, BZNFormat.Battlezone2, version);
                    bzn.Write(writer, binary: binary, save: false);
                }
                catch (Exception ex)
                {
                    errors.Add($"v{version} {(binary ? "bin" : "asc")}: {ex.Message}");
                }
            }
        }

        if (errors.Count > 0)
        {
            Assert.False(true, $"File: {filePath}\nFailures:\n{string.Join("\n", errors)}");
        }
    }
}