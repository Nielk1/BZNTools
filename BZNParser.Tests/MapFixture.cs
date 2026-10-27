using System.Globalization;
using System.Text;

namespace BZNParser.Tests;

// A small independently encoded map, containing no game or third-party assets.
// Binary record types and sizes are explicit so the parser and writer cannot
// agree on the same wrong layout and accidentally make a regression pass.
internal sealed class MapFixture
{
    private readonly MemoryStream stream = new();
    private readonly bool binary;

    private MapFixture(int version, bool binary)
    {
        this.binary = binary;
        Text($"version [1] =\r\n{version}\r\nsaveType [1] =\r\n0\r\nbinarySave [1] =\r\n{binary.ToString().ToLowerInvariant()}\r\n");
    }

    internal static byte[] Create(int version, bool binary, bool ejectRatio,
        bool dampingMask, bool satellite = true, int extraFloats = 0,
        uint ratioBits = 0x3e99999a, ushort mask = 0x8123, int objectCount = 1,
        bool? satelliteTurnOff = null, bool building = false, bool badIndependence = false)
    {
        var f = new MapFixture(version, binary);
        f.String("msn_filename", "fixture.bzn");
        f.Long("seq_count", 1);
        f.Long("saveType", 0);
        f.Field("g_TerrainName", 2, Encoding.ASCII.GetBytes("fixture".PadRight(100, '\0')), "fixture", false);
        f.Long("size", (uint)objectCount);
        for (int objectIndex = 0; objectIndex < objectCount; objectIndex++)
            f.Craft(version, (objectIndex & 1) == 0 ? ejectRatio : !ejectRatio,
                (objectIndex & 2) == 0 ? dampingMask : !dampingMask,
                extraFloats, ratioBits, mask, (uint)objectIndex + 1, building, badIndependence);
        f.Field("groupTargets", 0, new byte[640], new string('0', 1280), false);
        f.String("dllName", "Fixture.dll");
        f.Validation("AiMission");
        f.Validation("AOIs");
        f.Long("size", 0);
        f.Validation("AiPaths");
        f.Long("count", 0);
        if (satellite)
        {
            f.Boolean("hasEntered", true);
            f.Long("ownerObj", 17);
            f.Long("ownerObj", 23);
            f.Long("ownerObj", 42);
        }
        if (satelliteTurnOff.HasValue) f.Boolean("turnOff", satelliteTurnOff.Value);
        f.Field("PadData", 0, [0x12, 0x34, 0x56, 0x78], "12345678", false);
        f.Field("PadData2", 0, [0x9a, 0xbc, 0xde, 0xf0], "9ABCDEF0", false);
        return f.stream.ToArray();
    }

    private void Craft(int version, bool ejectRatio, bool dampingMask,
        int extraFloats, uint ratioBits, ushort mask, uint sequence, bool building, bool badIndependence)
    {
        var f = this;
        f.Validation("GameObject");
        f.String("objClass", building ? "fixture_building" : "fixture_craft");
        f.Field("seqno", 4, BitConverter.GetBytes(0x800000u | sequence), sequence.ToString("x"));
        f.Byte("team", 1);
        if (!binary) f.String("label", "");
        f.Boolean("isUser", false);
        f.Field("objAddr", 8, BitConverter.GetBytes(1u), "00000001", false);
        var transform = new float[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
        if (binary)
            f.Record(12, transform.SelectMany(BitConverter.GetBytes).ToArray());
        else
        {
            f.Text("transform [1] =\r\n");
            string[] axes = ["right", "up", "front", "posit"];
            for (int i = 0; i < axes.Length; i++)
                for (int j = 0; j < 3; j++)
                    f.Float($"  {axes[i]}.{(char)('x' + j)}", transform[i * 4 + j]);
        }
        if (building)
            f.String("saveClass", "");
        else
        {
            f.Long("abandoned", 0);
            f.Float("curAmmo", 100);
            f.Float("maxAmmo", 200);
            f.Float("addAmmo", 1);
            f.String("curPilot", "ispilo");
            if (ejectRatio)
                f.Field(version == 1195 ? "m_ejectRatio" : "ejectRatio", 5,
                    BitConverter.GetBytes(ratioBits), BitConverter.UInt32BitsToSingle(ratioBits).ToString("R", CultureInfo.InvariantCulture));
        }
        for (int i = 0; i < extraFloats; i++)
            f.Float("unknownExtension", 123);
        f.Float("illumination", 1);
        if (!binary) f.Text("euler =\r\n");
        string[] scalars = ["mass", "mass_inv", "v_mag", "v_mag_inv", "I", "k_i"];
        for (int i = 0; i < scalars.Length; i++) f.Float($" {scalars[i]}", i + 2);
        string[] vectors = ["v", "omega", "Accel"];
        for (int i = 0; i < vectors.Length; i++)
        {
            if (binary)
                f.Record(9, new float[] { i + 1, i + 2, i + 3 }.SelectMany(BitConverter.GetBytes).ToArray());
            else
            {
                f.Text($" {vectors[i]} [1] =\r\n");
                for (int j = 0; j < 3; j++) f.Float($"  {(char)('x' + j)}", i + j + 1);
            }
        }
        f.String("name", "Fixture");
        f.Byte("saveFlags", 12); // defaultShot and defaultHealth
        f.Short("isVisible", 65535);
        if (dampingMask) f.Short("isDamped", mask);
        f.Long("EffectsMask", 0x1234);
        f.Short("isSeen", 0);
        f.Byte("groupNumber", 0);
        if (!binary) f.Text("undefaicmd =\r\n");
        f.Long("priority", 0);
        f.Field("what", 2, [0], "00000000", false);
        f.Long("who", 0);
        f.Field("where", 8, BitConverter.GetBytes(0u), "00000000", false);
        f.Field("param", 4, BitConverter.GetBytes(0u), "00000000", false);
        f.Boolean("aiProcess", false);
        if (badIndependence) f.Boolean("badIndependence", false);
        else f.Byte("independence", 0);
    }

    private void Byte(string name, byte value) => Field(name, 2, [value], value.ToString(CultureInfo.InvariantCulture));
    private void Short(string name, ushort value) => Field(name, 3, BitConverter.GetBytes(value), value.ToString(CultureInfo.InvariantCulture));
    private void Long(string name, uint value) => Field(name, 4, BitConverter.GetBytes(value), value.ToString(CultureInfo.InvariantCulture));
    private void Float(string name, float value) => Field(name, 5, BitConverter.GetBytes(value), value.ToString("R", CultureInfo.InvariantCulture));
    private void Boolean(string name, bool value) => Field(name, 1, [value ? (byte)1 : (byte)0], value.ToString().ToLowerInvariant());
    private void Validation(string name) { if (!binary) Text($"[{name}]\r\n"); }
    private void String(string name, string value)
    {
        if (!binary) { Text($"{name} = {value}\r\n"); return; }
        byte[] bytes = Encoding.ASCII.GetBytes(value);
        Record(2, [(byte)bytes.Length]);
        if (bytes.Length > 0) Record(2, bytes);
    }
    private void Field(string name, byte type, byte[] bytes, string text, bool multiline = true)
    {
        if (binary) Record(type, bytes);
        else Text(multiline ? $"{name} [1] =\r\n{text}\r\n" : $"{name} = {text}\r\n");
    }
    private void Record(byte type, byte[] bytes)
    {
        stream.WriteByte(type);
        stream.Write(BitConverter.GetBytes((ushort)bytes.Length));
        stream.Write(bytes);
    }
    private void Text(string value) => stream.Write(Encoding.ASCII.GetBytes(value));
}
