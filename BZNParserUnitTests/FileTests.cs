using BZNParser;
using BZNParser.Battlezone;
using BZNParser.Tokenizer;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace BZNParserUnitTests;

public class GlobalTestDataFixture : IDisposable
{
    public BattlezoneBZNHints? BZ1Hints { get; private set; }
    public BattlezoneBZNHints? BZ2Hints { get; private set; }

    public GlobalTestDataFixture()
    {
        BZ1Hints = BattlezoneBZNHints.BuildHintsBZ1();
        BZ2Hints = BattlezoneBZNHints.BuildHintsBZ2();
    }

    public void Dispose()
    {
        //SharedConfiguration.Clear(); // Cleanup once all tests finish
    }
}

public class FileTests : IClassFixture<GlobalTestDataFixture>
{
    private readonly GlobalTestDataFixture _fixture;
    public FileTests(GlobalTestDataFixture fixture)
    {
        // This is called for every file, but 'fixture' is the same shared instance
        _fixture = fixture;
    }

    public static IEnumerable<object[]> BZNs_BZn64()
    {
        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\test_files\BattlezoneN64");

        if (Directory.Exists(folderPath))
        {
            return Directory.EnumerateFiles(folderPath, "*.bzn", SearchOption.AllDirectories)
                //.Where(dr => Path.GetFileName(Path.GetDirectoryName(dr))!.Length == 5)
                .Select(file => new object[] { Path.GetFileName(file), file });
        }

        return Enumerable.Empty<object[]>();
    }
    public static IEnumerable<object[]> BZNs_BZ1()
    {
        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\test_files\Battlezone");

        if (Directory.Exists(folderPath))
        {
            return Directory.EnumerateFiles(folderPath, "*.bzn", SearchOption.AllDirectories)
                //.Where(dr => Path.GetFileName(Path.GetDirectoryName(dr))!.Length == 5)
                .Select(file => new object[] { Path.GetFileName(file), file });
        }

        return Enumerable.Empty<object[]>();
    }
    public static IEnumerable<object[]> BZNs_BZ2()
    {
        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\test_files\Battlezone2");

        List<object[]> items = new List<object[]>();
        if (Directory.Exists(folderPath))
        {
            return Directory.EnumerateFiles(folderPath, "*.bzn", SearchOption.AllDirectories)
                //.Where(dr => Path.GetFileName(Path.GetDirectoryName(dr))!.Length == 5)
                .Select(file => new object[] { Path.GetFileName(file), file });
        }

        return Enumerable.Empty<object[]>();
    }

    // 2. Use [Theory] to run this method for every item in the collection
    [Theory]
    [MemberData(nameof(BZNs_BZn64))]
    [MemberData(nameof(BZNs_BZ1))]
    [MemberData(nameof(BZNs_BZ2))]
    public void ArchiveFullCycle(string fileName, string filePath)
    {
        using (FileStream file = File.OpenRead(filePath))
        {
            using (BZNStreamReader reader = new BZNStreamReader(file, filePath))
            {
                switch (reader.Format)
                {
                    case BZNFormat.Battlezone:
                    case BZNFormat.BattlezoneN64:
                    case BZNFormat.Battlezone2:
                        {
                            BZNFileBattlezone? bzn = null;
                            switch (reader.Format)
                            {
                                case BZNFormat.Battlezone:
                                case BZNFormat.BattlezoneN64:
                                    bzn = new BZNFileBattlezone(reader, Hints: _fixture.BZ1Hints);
                                    break;
                                case BZNFormat.Battlezone2:
                                    bzn = new BZNFileBattlezone(reader, Hints: _fixture.BZ2Hints);
                                    break;
                            }

                            if (bzn != null)
                            {
                                using (FileStream compareStream = File.OpenRead(filePath))
                                using (MatchesStream ms = new MatchesStream(compareStream))
                                using (BZNStreamWriter writer = new BZNStreamWriter(ms, reader.Format, reader.Version, true, reader.GetDefects()))
                                {
                                    bzn.Write(writer, binary: reader.HasBinary, save: false);
                                    // The write must have consumed the ENTIRE comparison stream; an
                                    // incomplete run (missing tail fields) would otherwise look valid.
                                    ms.AssertComplete();
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
    }

    // --- CR -> LF (newline conversion) binary damage ---
    // A text tool rewrote CR (0x0D) bytes to LF (0x0A). In the binary section that shrank a SIZE
    // field (0x0D -> 0x0A) so the payload read short and the stream desynced. The reader recovers
    // the true size and mirror mode must reproduce the exact damaged bytes.
    private static string? DamagedBZNPath { get { return @"G:\Downloads\bridges2i.bzn"; } }

    /*[Fact]
    public void DamagedNewline_RepairsFilenameAndConfirmsDamage()
    {
        string? path = DamagedBZNPath;
        if (path == null || !File.Exists(path))
            return; // sample not present in this environment

        using (BZNStreamReader reader = new BZNStreamReader(File.OpenRead(path), path))
        {
            Assert.Equal(BZNFormat.Battlezone2, reader.Format);

            bool foundFullFilename = false;
            for (int i = 0; i < 500; i++)
            {
                IBZNToken? tok = reader.ReadToken();
                if (tok == null)
                    break;
                if (tok is BZNTokenBinary bin)
                {
                    try { if (bin.GetString() == "bridges2i.bzn") foundFullFilename = true; } catch { }
                }
            }

            Assert.True(foundFullFilename, "damaged binary filename should be recovered as 'bridges2i.bzn'");
            Assert.True(reader.NewlineDamageConfirmed, "reader should confirm binary newline damage");

            // GetDefects() walks the Atlas and throws on any discontinuity -- a clean desync check.
            Dictionary<int, StreamDefect> defects = reader.GetDefects();
            Assert.Contains(defects.Values, d => d.NewlineDamaged);
            StreamDefect damaged = defects.Values.First(d => d.NewlineDamaged);
            Assert.True(damaged.NewlineSizeCorrected > 0, "a corrected size should have been recorded");
        }
    }

    [Fact]
    public void DamagedNewline_ModelSurfacesMalformation()
    {
        string? path = DamagedBZNPath;
        if (path == null || !File.Exists(path))
            return; // sample not present in this environment

        using (BZNStreamReader reader = new BZNStreamReader(File.OpenRead(path), path))
        {
            BZNFileBattlezone bzn = new BZNFileBattlezone(reader, Hints: _fixture.BZ2Hints);

            (bool found, byte[]? origBytes, uint? corrected) =
                bzn.Malformations.GetBinaryNewlineCorruption<BZNFileBattlezone, SizedString>(x => x.msn_filename);
            Assert.True(found, "the model should surface a binary newline corruption on msn_filename");
            Assert.NotNull(origBytes);
            // The corrected size (13) is larger than the damaged on-disk size (10)
            Assert.True(corrected!.Value > 0);
        }
    }

    [Fact]
    public void DamagedNewline_WriterEmitsOriginalDamagedSizeInMirrorMode()
    {
        // A NewlineDamaged defect must make the writer emit the exact on-disk SIZE bytes
        // (NewlineSizeOriginal) in place of the corrected size, so the damaged token
        // round-trips byte-for-byte in mirror mode. Payload here is 5 * sizeof(UInt32) = 20.
        var defects = new Dictionary<int, StreamDefect>
        {
            [0] = new StreamDefect
            {
                NewlineDamaged = true,
                NewlineSizeCorrected = 20,                 // the true, recovered payload size
                NewlineSizeOriginal = new byte[] { 0x0A, 0x00 },  // the damaged on-disk size (10)
            },
        };

        byte[] mirrored;
        using (MemoryStream ms = new MemoryStream())
        using (BZNStreamWriter writer = new BZNStreamWriter(ms, BZNFormat.Battlezone2, 1185, true, defects))
        {
            writer.SetBinary();
            writer.WriteUnsignedValues("x", 1, 2, 3, 4, 5);
            mirrored = ms.ToArray();
        }
        Assert.True(mirrored.Length >= 3, "expected a binary type+size header");
        // SIZE must be the damaged bytes (0x0A 0x00), not the corrected 20 (0x14 0x00)
        Assert.Equal(0x0A, mirrored[1]);
        Assert.Equal(0x00, mirrored[2]);

        byte[] normal;
        using (MemoryStream ms = new MemoryStream())
        using (BZNStreamWriter writer = new BZNStreamWriter(ms, BZNFormat.Battlezone2, 1185, true, null))
        {
            writer.SetBinary();
            writer.WriteUnsignedValues("x", 1, 2, 3, 4, 5);
            normal = ms.ToArray();
        }
        Assert.Equal(0x14, normal[1]); // 20 -- corrected size when there is no defect
        Assert.Equal(0x00, normal[2]);
    }*/
}