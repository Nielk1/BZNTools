using BZNParser.Battlezone;
using BZNParser.Battlezone.GameObject;
using BZNParser.Tokenizer;
using System.Text;
using Xunit;

namespace BZNParser.Tests;

public class BattlezoneMapTests
{
    private static readonly BattlezoneBZNHints Hints = new()
    {
        Strict = true,
        ClassLabels = new() { ["fixture_craft"] = ["boid"], ["fixture_building"] = ["deposit"] }
    };

    public static IEnumerable<object[]> Layouts()
    {
        foreach (int version in new[] { 1196, 1197, 1200 })
            foreach (bool binary in new[] { false, true })
                foreach (bool extended in new[] { false, true })
                    yield return [version, binary, extended];
    }

    [Theory]
    [MemberData(nameof(Layouts))]
    public void ReadsBothMapLayouts(int version, bool binary, bool extended)
    {
        var map = Read(MapFixture.Create(version, binary, extended, version >= 1197 && extended));
        var craft = Assert.IsType<ClassCraft>(Assert.Single(map.Entities).gameObject);
        Assert.Equal(1, craft.illumination);
        Assert.Equal(2, craft.euler.Mass);
        Assert.Equal(0x1234u, craft.EffectsMask);
        Assert.Equal(extended ? 0.3f : 0, craft.m_ejectRatio);
        Assert.Equal(extended, craft.HasEjectRatio);
        if (version >= 1197) Assert.Equal(extended, craft.HasDampingMask);
        Assert.Equal((ushort)(version >= 1197 && extended ? 0x8123 : 0), craft.isDamped);
        Assert.True(map.SatelliteHasEntered);
        Assert.Equal(new uint[] { 17, 23, 42 }, map.SatelliteOwnerObjects);
        Assert.Equal(new byte[] { 0x12, 0x34, 0x56, 0x78 }, map.PadData);
        Assert.Equal(new byte[] { 0x9a, 0xbc, 0xde, 0xf0 }, map.PadData2);
    }

    [Theory]
    [MemberData(nameof(Layouts))]
    public void WrittenMapCanBeReadAgain(int version, bool binary, bool extended)
    {
        byte[] input = MapFixture.Create(version, binary, extended, version >= 1197 && extended);
        var map = Read(input);
        byte[] output = Write(map, binary);
        var reread = Read(output);
        Assert.Equal(map.Entities.Length, reread.Entities.Length);
        if (binary) Assert.Equal(input, output);
    }

    [Theory]
    [MemberData(nameof(Layouts))]
    public void SwitchingEncodingRetainsFieldPresence(int version, bool binary, bool extended)
    {
        var map = Read(MapFixture.Create(version, binary, extended, version >= 1197 && extended));
        var reread = Read(Write(map, !binary));
        var craft = Assert.IsType<ClassCraft>(Assert.Single(reread.Entities).gameObject);
        Assert.Equal(extended, craft.HasEjectRatio);
        if (version >= 1197) Assert.Equal(extended, craft.HasDampingMask);
        Assert.Equal(map.PadData, reread.PadData);
        Assert.Equal(map.PadData2, reread.PadData2);
        Assert.Equal(map.SatelliteOwnerObjects, reread.SatelliteOwnerObjects);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EachObjectCanHaveItsOwnLayout(bool binary)
    {
        byte[] input = MapFixture.Create(1197, binary, false, false, objectCount: 4);
        var map = Read(input);
        Assert.Equal(4, map.Entities.Length);
        for (int i = 0; i < map.Entities.Length; i++)
        {
            var craft = Assert.IsType<ClassCraft>(map.Entities[i].gameObject);
            Assert.Equal((i & 1) != 0, craft.HasEjectRatio);
            Assert.Equal((i & 2) != 0, craft.HasDampingMask);
        }
        var reread = Read(Write(map, binary));
        Assert.Equal(4, reread.Entities.Length);
        if (binary) Assert.Equal(input, Write(reread, true));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PreservesPresentZeroValuedExtensions(bool binary)
    {
        var map = Read(MapFixture.Create(1197, binary, true, true, ratioBits: 0, mask: 0));
        var reread = Read(Write(map, binary));
        var craft = Assert.IsType<ClassCraft>(Assert.Single(reread.Entities).gameObject);
        Assert.True(craft.HasEjectRatio);
        Assert.True(craft.HasDampingMask);
        Assert.Equal(0, craft.m_ejectRatio);
        Assert.Equal((ushort)0, craft.isDamped);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PreservesMissingSatellitePanel(bool binary)
    {
        byte[] input = MapFixture.Create(1196, binary, true, false, satellite: false);
        var map = Read(input);
        Assert.Null(map.SatelliteHasEntered);
        var reread = Read(Write(map, binary));
        Assert.Null(reread.SatelliteHasEntered);
        Assert.Equal(map.PadData, reread.PadData);
        if (binary) Assert.Equal(input, Write(map, true));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void KeepsVersion1195EjectRatio(bool binary)
    {
        var map = Read(MapFixture.Create(1195, binary, true, false));
        var craft = Assert.IsType<ClassCraft>(Assert.Single(Read(Write(map, binary)).Entities).gameObject);
        Assert.Equal(0.3f, craft.m_ejectRatio);
    }

    [Theory]
    [InlineData(0x80000000u)] // negative zero
    [InlineData(0x7fc12345u)] // a NaN with a non-default payload
    [InlineData(0x7f800000u)] // infinity
    public void PreservesBinaryEjectRatioBits(uint bits)
    {
        byte[] input = MapFixture.Create(1196, true, true, false, ratioBits: bits);
        Assert.Equal(input, Write(Read(input), true));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RejectsUnrecognizedCraftExtension(bool binary)
    {
        Assert.ThrowsAny<Exception>(() => Read(MapFixture.Create(1196, binary, true, false, extraFloats: 1)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RejectsTruncatedTrailingRecord(bool binary)
    {
        byte[] input = MapFixture.Create(1196, binary, true, false);
        // Remove PadData2, which is required for these versions.
        int cut = binary ? 7 : "PadData2 = 9ABCDEF0\r\n".Length;
        Assert.ThrowsAny<Exception>(() => Read(input[..^cut]));
    }

    [Fact]
    public void PreservesPaddingHexSpellingWhenRequested()
    {
        string text = Encoding.ASCII.GetString(MapFixture.Create(1196, false, true, false))
            .Replace("9ABCDEF0", "9abCdEf0", StringComparison.Ordinal);
        var map = Read(Encoding.ASCII.GetBytes(text));
        string written = Encoding.ASCII.GetString(Write(map, false, preserveMalformations: true));
        Assert.Contains("PadData2 = 9abCdEf0\r\n", written);
        Assert.Equal(map.PadData2, Read(Encoding.ASCII.GetBytes(written)).PadData2);
    }

    [Fact]
    public void BuildingCannotHideAFailedCommonObjectRead()
    {
        Assert.ThrowsAny<Exception>(() => Read(MapFixture.Create(1197, true, false, true,
            building: true, badIndependence: true)));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void PreservesSatelliteTurnOff(bool binary, bool turnOff)
    {
        byte[] input = MapFixture.Create(1200, binary, true, true, satelliteTurnOff: turnOff);
        var map = Read(input);
        Assert.Equal(turnOff, map.SatelliteTurnOff);
        var reread = Read(Write(map, binary));
        Assert.Equal(turnOff, reread.SatelliteTurnOff);
        if (binary) Assert.Equal(input, Write(map, true));
    }

    [Fact]
    public void WritingDoesNotClearTheUnnamedObjectFlag()
    {
        byte[] input = MapFixture.Create(1196, true, true, false);
        var map = Read(input);
        uint sequence = map.Entities[0].seqNo;
        Assert.Equal(input, Write(map, true));
        Assert.Equal(sequence, map.Entities[0].seqNo);
        Assert.Equal(input, Write(map, true));
    }

    [Fact]
    public void RejectsWrongSatelliteOwnerCount()
    {
        var map = Read(MapFixture.Create(1196, true, true, false));
        map.SatelliteOwnerObjects = [1, 2];
        Assert.Throws<InvalidOperationException>(() => Write(map, true));
    }

    private static BZNFileBattlezone Read(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        using var reader = new BZNStreamReader(stream, "fixture.bzn");
        return new BZNFileBattlezone(reader, Hints);
    }

    private static byte[] Write(BZNFileBattlezone map, bool binary, bool preserveMalformations = false)
    {
        using var stream = new MemoryStream();
        using (var writer = new BZNStreamWriter(stream, BZNFormat.Battlezone2, map.Version, preserveMalformations))
            map.Write(writer, binary);
        return stream.ToArray();
    }
}
