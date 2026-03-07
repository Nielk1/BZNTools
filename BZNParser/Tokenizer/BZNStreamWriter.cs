
using System;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BZNParser.Tokenizer
{
    public static class SingleExtension
    {
        public static string ToBZNString(this float value, int version, BZNFormat format)
        {

            if (float.IsNaN(value)) return "nan";
            if (float.IsPositiveInfinity(value)) return "inf";
            if (float.IsNegativeInfinity(value)) return "-inf";

            if (format == BZNFormat.Battlezone2 && version >= 1183)
            {
                // Preserve signed zero
                if (value == 0f)
                {
                    bool negZero = BitConverter.SingleToInt32Bits(value) < 0;
                    return negZero ? "-0.00000000e+00" : "0.00000000e+00";
                }

                // Get exponent
                int exponent = (int)Math.Floor(Math.Log10(Math.Abs(value)));
                double mantissa = value / Math.Pow(10, exponent);

                // Round mantissa to the desired significant digits
                string mantissaStr = mantissa.ToString("F8", System.Globalization.CultureInfo.InvariantCulture);

                // Remove trailing zeros and possible trailing decimal point
                if (mantissaStr.Contains('.'))
                    mantissaStr = mantissaStr.TrimEnd('.');

                // Format exponent with sign and at least two digits
                string expStr = exponent.ToString("+#00;-#00");

                return $"{mantissaStr}e{expStr}";
            }

            // .NET general format with 6 significant digits
            string s = value.ToString("g6", CultureInfo.InvariantCulture);

            // C++ sample uses lowercase e
            s = s.Replace('E', 'e');

            // Normalize exponent to at least 3 digits, matching your sample:
            // e+6   -> e+006
            // e-5   -> e-005
            // e+30  -> e+030
            s = Regex.Replace(s, @"e([+-])(\d+)$", m =>
                $"e{m.Groups[1].Value}{int.Parse(m.Groups[2].Value):000}");

            return s;
        }
    }
    public class BZNStreamWriter : IDisposable
    {
        private Stream BaseStream { get; set; }
        public BZNFormat Format { get; private set; }
        public bool IsBigEndian { get; private set; }
        public byte TypeSize { get; private set; }
        public byte SizeSize { get; private set; }
        public int Version { get; private set; }
        public byte AlignmentBytes { get; private set; }
        public bool QuoteStrings { get; private set; }
        public bool HasBinary { get; private set; }
        public bool InBinary { get; private set; }

        public BZNStreamWriter(Stream stream, BZNFormat format, int version)
        {
            BaseStream = stream;
            Format = format;
            Version = version;

            switch (format)
            {
                case BZNFormat.Battlezone:
                    TypeSize = 2;
                    SizeSize = 2;
                    AlignmentBytes = 0;
                    IsBigEndian = false;
                    break;
                case BZNFormat.Battlezone2:
                    TypeSize = 1;
                    SizeSize = 2;
                    AlignmentBytes = 0;
                    IsBigEndian = false;
                    if (Version == 1160)
                    {
                        // Breadcrumb BZ2-1160-QUIRK
                        QuoteStrings = true;
                    }
                    break;
                case BZNFormat.BattlezoneN64:
                    TypeSize = 0;
                    SizeSize = 2;
                    AlignmentBytes = 2;
                    IsBigEndian = true;
                    break;
                case BZNFormat.StarTrekArmada:
                case BZNFormat.StarTrekArmada2:
                    TypeSize = 4;
                    SizeSize = 4;
                    AlignmentBytes = 0;
                    IsBigEndian = false;
                    break;
                default:
                    throw new Exception("Unknown BZNFormat");
            }
        }

        /// <summary>
        /// Switch the writer to binary mode. Once in binary mode, the writer will write binary data instead of text. This is used for writing the binary data section of a BZN file. Once in binary mode, the writer cannot be switched back to text mode.
        /// </summary>
        public void SetBinary()
        {
            HasBinary = true;
            InBinary = true;
        }

        public void Dispose()
        {
            BaseStream?.Dispose();
        }

        public void WriteUnknown(string name, string value)
        {
            if (InBinary)
                throw new Exception("Unknown type data cannot be written in binary mode.");
            BaseStream.Write(Encoding.ASCII.GetBytes($"{name} = "));
            InternalWriteStringValue(value);
            InternalWriteNewline();
        }

        public void WriteBooleans(string name, params bool[] values)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_BOOL);
                InternalWriteBinarySize(values.Length);

                foreach (bool value in values)
                    BaseStream.WriteByte((byte)(value ? 1 : 0));

                InternalAlignBinary();
                return;
            }
            BaseStream.Write(Encoding.ASCII.GetBytes($"{name} [{values.Length}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++) {
                BaseStream.Write(Encoding.ASCII.GetBytes($"{(values[i] ? "true" : "false")}"));
                InternalWriteNewline();
            }
        }

        public void WriteUnsignedValues(string name, params UInt64[] values)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_SHORT);
                InternalWriteBinarySize(values.Length);
                foreach (UInt64 value in values)
                {
                    byte[] bytes = BitConverter.GetBytes(value);
                    if (IsBigEndian)
                        Array.Reverse(bytes);
                    BaseStream.Write(bytes);
                }
                InternalAlignBinary();
                return;
            }
            BaseStream.Write(Encoding.ASCII.GetBytes($"{name} [{values.Length}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].ToString()));
                InternalWriteNewline();
            }
        }

        public void WriteUnsignedValues(string name, params UInt16[] values)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_SHORT);
                InternalWriteBinarySize(values.Length);
                foreach (UInt16 value in values)
                {
                    byte[] bytes = BitConverter.GetBytes(value);
                    if (IsBigEndian)
                        Array.Reverse(bytes);
                    BaseStream.Write(bytes);
                }
                InternalAlignBinary();
                return;
            }
            BaseStream.Write(Encoding.ASCII.GetBytes($"{name} [{values.Length}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].ToString()));
                InternalWriteNewline();
            }
        }

        public void WriteUnsignedHexLValues(string name, params UInt16[] values)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_SHORT);
                InternalWriteBinarySize(sizeof(UInt16) * values.Length);
                foreach (UInt16 value in values)
                {
                    byte[] bytes = BitConverter.GetBytes(value);
                    if (IsBigEndian)
                        Array.Reverse(bytes);
                    BaseStream.Write(bytes);
                }
                InternalAlignBinary();
                return;
            }
            BaseStream.Write(Encoding.ASCII.GetBytes($"{name} [{values.Length}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].ToString("x")));
                InternalWriteNewline();
            }
        }

        public void WriteUnsignedHexLValues(string name, params UInt32[] values)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_LONG);
                InternalWriteBinarySize(sizeof(UInt32) * values.Length);
                foreach (UInt32 value in values)
                {
                    byte[] bytes = BitConverter.GetBytes(value);
                    if (IsBigEndian)
                        Array.Reverse(bytes);
                    BaseStream.Write(bytes);
                }
                InternalAlignBinary();
                return;
            }
            BaseStream.Write(Encoding.ASCII.GetBytes($"{name} [{values.Length}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].ToString("x")));
                InternalWriteNewline();
            }
        }

        // used for: undefaicmd
        public void WriteCmd(string name, UInt32 value)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_LONG);
                InternalWriteBinarySize(sizeof(UInt32));
                {
                    byte[] bytes = BitConverter.GetBytes(value);
                    if (IsBigEndian)
                        Array.Reverse(bytes);
                    BaseStream.Write(bytes);
                }
                InternalAlignBinary();
                return;
            }
            BaseStream.Write(Encoding.ASCII.GetBytes($"{name} ="));
            {
                if (value != 0)
                {
                    InternalWriteNewline();
                    BaseStream.Write(Encoding.ASCII.GetBytes(value.ToString()));
                }
                InternalWriteNewline();
            }
        }

        public void WriteShortFlag(string name, UInt16 value)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_SHORT);
                InternalWriteBinarySize(sizeof(UInt16));
                {
                    {
                        byte[] bytes = BitConverter.GetBytes(value);
                        if (IsBigEndian)
                            Array.Reverse(bytes);
                        BaseStream.Write(bytes);
                    }
                }
                InternalAlignBinary();
                return;
            }
            BaseStream.Write(Encoding.ASCII.GetBytes($"{name} ="));
            InternalWriteNewline();
            {
                {
                    InternalWriteNewline();
                    BaseStream.Write(Encoding.ASCII.GetBytes(value.ToString("x")));
                }
                InternalWriteNewline();
            }
        }

        public void WriteLongFlags(string name, params UInt32[] values)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_SHORT);
                InternalWriteBinarySize(sizeof(UInt32) * values.Length);
                {
                    foreach (UInt32 value in values)
                    {
                        byte[] bytes = BitConverter.GetBytes(value);
                        if (IsBigEndian)
                            Array.Reverse(bytes);
                        BaseStream.Write(bytes);
                    }
                }
                InternalAlignBinary();
                return;
            }
            BaseStream.Write(Encoding.ASCII.GetBytes($"{name} [{values.Length}] ="));
            {
                foreach (UInt32 value in values)
                {
                    InternalWriteNewline();
                    BaseStream.Write(Encoding.ASCII.GetBytes(value.ToString("x")));
                }
                InternalWriteNewline();
            }
        }

        public void WriteShortFlags(string name, params UInt16[] values)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_SHORT);
                InternalWriteBinarySize(sizeof(UInt16) * values.Length);
                {
                    foreach (UInt16 value in values)
                    {
                        byte[] bytes = BitConverter.GetBytes(value);
                        if (IsBigEndian)
                            Array.Reverse(bytes);
                        BaseStream.Write(bytes);
                    }
                }
                InternalAlignBinary();
                return;
            }
            BaseStream.Write(Encoding.ASCII.GetBytes($"{name} [{values.Length}] ="));
            {
                foreach (UInt16 value in values)
                {
                    InternalWriteNewline();
                    BaseStream.Write(Encoding.ASCII.GetBytes(value.ToString("x")));
                }
                InternalWriteNewline();
            }
        }

        public void WriteUnsignedValues(string name, params UInt32[] values)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_LONG);
                InternalWriteBinarySize(sizeof(UInt32) * values.Length);
                foreach (UInt32 value in values)
                {
                    byte[] bytes = BitConverter.GetBytes(value);
                    if (IsBigEndian)
                        Array.Reverse(bytes);
                    BaseStream.Write(bytes);
                }
                InternalAlignBinary();
                return;
            }
            BaseStream.Write(Encoding.ASCII.GetBytes($"{name} [{values.Length}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].ToString()));
                InternalWriteNewline();
            }
        }

        public void WriteVector2Ds(string name, params Vector2D[] values)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_VEC2D);
                InternalWriteBinarySize(values.Length * sizeof(float) * 2);
                foreach (Vector2D value in values)
                {
                    byte[] xBytes = BitConverter.GetBytes(value.x);
                    byte[] zBytes = BitConverter.GetBytes(value.z);
                    if (IsBigEndian)
                    {
                        Array.Reverse(xBytes);
                        Array.Reverse(zBytes);
                    }
                    BaseStream.Write(xBytes);
                    BaseStream.Write(zBytes);
                }
                InternalAlignBinary();
                return;
            }
            BaseStream.Write(Encoding.ASCII.GetBytes($"{name} [{(values.Length)}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(Encoding.ASCII.GetBytes("  x [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].x.ToBZNString(Version, Format)));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes("  z [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].z.ToBZNString(Version, Format)));
                InternalWriteNewline();
            }
        }

        public void WriteVector3Ds(string name, params Vector3D[] values)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_VEC3D);
                InternalWriteBinarySize(values.Length * sizeof(float) * 3);
                foreach (Vector3D value in values)
                {
                    byte[] xBytes = BitConverter.GetBytes(value.x);
                    byte[] yBytes = BitConverter.GetBytes(value.y);
                    byte[] zBytes = BitConverter.GetBytes(value.z);
                    if (IsBigEndian)
                    {
                        Array.Reverse(xBytes);
                        Array.Reverse(yBytes);
                        Array.Reverse(zBytes);
                    }
                    BaseStream.Write(xBytes);
                    BaseStream.Write(yBytes);
                    BaseStream.Write(zBytes);
                }
                InternalAlignBinary();
                return;
            }
            BaseStream.Write(Encoding.ASCII.GetBytes($"{name} [{(values.Length)}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(Encoding.ASCII.GetBytes("  x [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].x.ToBZNString(Version, Format)));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes("  y [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].y.ToBZNString(Version, Format)));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes("  z [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].z.ToBZNString(Version, Format)));
                InternalWriteNewline();
            }
        }

        public void WriteEuler(string name, Euler value)
        {
            if (InBinary)
            {
                throw new NotImplementedException("Euler binary save attempt");
                return;
            }
            BaseStream.Write(Encoding.ASCII.GetBytes($"{name} ="));
            InternalWriteNewline();

            WriteFloats(" mass", value.mass);
            WriteFloats(" mass_inv", value.mass_inv);
            WriteFloats(" v_mag", value.v_mag);
            WriteFloats(" v_mag_inv", value.v_mag_inv);
            WriteFloats(" I", value.I);
            WriteFloats(" k_i", value.I_inv);
            WriteVector3Ds(" v", value.v);
            WriteVector3Ds(" omega", value.omega);
            WriteVector3Ds(" Accel", value.Accel);
        }

        public void WriteMat3Ds(string name, params Matrix[] values)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_MAT3D);
                InternalWriteBinarySize(values.Length * sizeof(float) * 16);
                foreach (Matrix value in values)
                {
                    foreach (float x in new float[] {
                        value.right.x, value.right.y, value.right.z, value.rightw,
                        value.up.x   , value.up.y   , value.up.z   , value.upw   ,
                        value.front.x, value.front.y, value.front.z, value.frontw,
                        value.posit.x, value.posit.y, value.posit.z, value.positw
                    })
                    {
                        byte[] bytes = BitConverter.GetBytes(x);
                        if (IsBigEndian)
                            Array.Reverse(bytes);
                        BaseStream.Write(bytes);
                    }
                }
                InternalAlignBinary();
                return;
            }
            BaseStream.Write(Encoding.ASCII.GetBytes($"{name} [{(values.Length)}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(Encoding.ASCII.GetBytes($"  right.x [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].right.x.ToBZNString(Version, Format)));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes($"  right.y [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].right.y.ToBZNString(Version, Format)));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes($"  right.z [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].right.z.ToBZNString(Version, Format)));
                InternalWriteNewline();

                BaseStream.Write(Encoding.ASCII.GetBytes($"  up.x [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].up.x.ToBZNString(Version, Format)));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes($"  up.y [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].up.y.ToBZNString(Version, Format)));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes($"  up.z [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].up.z.ToBZNString(Version, Format)));
                InternalWriteNewline();

                BaseStream.Write(Encoding.ASCII.GetBytes($"  front.x [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].front.x.ToBZNString(Version, Format)));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes($"  front.y [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].front.y.ToBZNString(Version, Format)));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes($"  front.z [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].front.z.ToBZNString(Version, Format)));
                InternalWriteNewline();

                BaseStream.Write(Encoding.ASCII.GetBytes($"  posit.x [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].posit.x.ToBZNString(Version, Format)));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes($"  posit.y [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].posit.y.ToBZNString(Version, Format)));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes($"  posit.z [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].posit.z.ToBZNString(Version, Format)));
                InternalWriteNewline();
            }
        }

        public void WriteMat3DOlds(string name, params Matrix[] values)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_MAT3DOLD);
                InternalWriteBinarySize(values.Length * sizeof(float) * 16);
                foreach (Matrix value in values)
                {
                    foreach (float x in new float[] {
                        value.right.x, value.right.y, value.right.z,
                        value.up.x   , value.up.y   , value.up.z   ,
                        value.front.x, value.front.y, value.front.z,
                        value.posit.x, value.posit.y, value.posit.z
                    })
                    {
                        byte[] bytes = BitConverter.GetBytes(x);
                        if (IsBigEndian)
                            Array.Reverse(bytes);
                        BaseStream.Write(bytes);
                    }
                }
                InternalAlignBinary();
                return;
            }
            BaseStream.Write(Encoding.ASCII.GetBytes($"{name} [{(values.Length)}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(Encoding.ASCII.GetBytes($"  right_x [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].right.x.ToBZNString(Version, Format)));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes($"  right_y [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].right.y.ToBZNString(Version, Format)));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes($"  right_z [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].right.z.ToBZNString(Version, Format)));
                InternalWriteNewline();

                BaseStream.Write(Encoding.ASCII.GetBytes($"  up_x [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].up.x.ToBZNString(Version, Format)));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes($"  up_y [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].up.y.ToBZNString(Version, Format)));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes($"  up_z [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].up.z.ToBZNString(Version, Format)));
                InternalWriteNewline();

                BaseStream.Write(Encoding.ASCII.GetBytes($"  front_x [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].front.x.ToBZNString(Version, Format)));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes($"  front_y [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].front.y.ToBZNString(Version, Format)));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes($"  front_z [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].front.z.ToBZNString(Version, Format)));
                InternalWriteNewline();

                BaseStream.Write(Encoding.ASCII.GetBytes($"  posit_x [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].posit.x.ToBZNString(Version, Format)));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes($"  posit_y [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].posit.y.ToBZNString(Version, Format)));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes($"  posit_z [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].posit.z.ToBZNString(Version, Format)));
                InternalWriteNewline();
            }
        }

        public void WriteSignedValues(string name, params int[] values)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_LONG);
                InternalWriteBinarySize(sizeof(int) * values.Length);
                foreach (int value in values)
                {
                    byte[] bytes = BitConverter.GetBytes(value);
                    if (IsBigEndian)
                        Array.Reverse(bytes);
                    BaseStream.Write(bytes);
                }
                InternalAlignBinary();
                return;
            }
            BaseStream.Write(Encoding.ASCII.GetBytes($"{name} [{values.Length}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++) {
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].ToString()));
                InternalWriteNewline();
            }
        }

        public void WriteFloats(string name, params float[] values)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_FLOAT);
                InternalWriteBinarySize(sizeof(float) * values.Length);
                foreach (float value in values)
                {
                    byte[] bytes = BitConverter.GetBytes(value);
                    if (IsBigEndian)
                        Array.Reverse(bytes);
                    BaseStream.Write(bytes);
                }
                InternalAlignBinary();
                return;
            }
            BaseStream.Write(Encoding.ASCII.GetBytes($"{name} [{values.Length}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].ToBZNString(Version, Format)));
                InternalWriteNewline();
            }
        }

        public void WriteIDs(string name, UInt64 value)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_ID);
                InternalWriteBinarySize(sizeof(UInt64));
                byte[] data = BitConverter.GetBytes(value);
                if (IsBigEndian)
                    Array.Reverse(data);
                BaseStream.Write(data);
                InternalAlignBinary();
                return;
            }
            BaseStream.Write(Encoding.ASCII.GetBytes($"{name} [1] ="));
            InternalWriteNewline();
            BaseStream.Write(Encoding.ASCII.GetBytes(value.ToString("x")));
            InternalWriteNewline();
        }

        public void WriteIDs(string name, byte[] value)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_ID);
                InternalWriteBinarySize(value.Length);
                BaseStream.Write(value);
                InternalAlignBinary();
                return;
            }
            BaseStream.Write(Encoding.ASCII.GetBytes($"{name} [1] ="));
            InternalWriteNewline();
            BaseStream.Write(value);
            InternalWriteNewline();
        }

        public void WriteIDs(string name, string value)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_ID);
                InternalWriteBinarySize(value.Length);
                BaseStream.Write(Encoding.ASCII.GetBytes(value));
                InternalAlignBinary();
                return;
            }
            BaseStream.Write(Encoding.ASCII.GetBytes($"{name} [1] ="));
            InternalWriteNewline();
            BaseStream.Write(Encoding.ASCII.GetBytes(value));
            InternalWriteNewline();
        }

        // 8 bit number
        public void WriteUnsignedValues(string name, params byte[] values)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_CHAR);
                InternalWriteBinarySize(values.Length);
                //byte[] bytes = values.ToArray();
                //if (IsBigEndian)
                //    Array.Reverse(bytes);
                BaseStream.Write(values);
                InternalAlignBinary();
                return;
            }
            BaseStream.Write(Encoding.ASCII.GetBytes($"{name} [{values.Length}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].ToString()));
                InternalWriteNewline();
            }
        }

        public void WriteChars(string name, string value)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_CHAR);
                byte[] stringBytes = Encoding.ASCII.GetBytes(value);
                InternalWriteBinarySize(stringBytes.Length);
                BaseStream.Write(stringBytes);
                InternalAlignBinary();
                return;
            }
            BaseStream.Write(Encoding.ASCII.GetBytes($"{name} = "));
            InternalWriteStringValue(value);
            InternalWriteNewline();
        }

        public void WritePtr(string name, uint value)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_PTR);
                InternalWriteBinarySize(4);
                byte[] bytes = BitConverter.GetBytes(value);
                if (IsBigEndian)
                    Array.Reverse(bytes);
                BaseStream.Write(bytes);
                InternalAlignBinary();
                return;
            }
            BaseStream.Write(Encoding.ASCII.GetBytes($"{name} = "));
            InternalWriteStringValue(value.ToString("X8"));
            InternalWriteNewline();
        }

        // this might be bogus, will know later
        public void WritePtrs(string name, params uint[] values)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_PTR);
                InternalWriteBinarySize(4 * values.Length);
                for (int i = 0; i < values.Length; i++)
                {
                    byte[] bytes = BitConverter.GetBytes(values[i]);
                    if (IsBigEndian)
                        Array.Reverse(bytes);
                    BaseStream.Write(bytes);
                }
                InternalAlignBinary();
                return;
            }
            BaseStream.Write(Encoding.ASCII.GetBytes($"{name} [{values.Length}] = "));
            InternalWriteNewline();
            foreach (uint value in values)
            {
                InternalWriteStringValue(value.ToString("X8"));
                InternalWriteNewline();
            }
        }
        public void WriteVoidBytes(string name, UInt32 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (IsBigEndian)
                Array.Reverse(bytes);
            WriteVoidBytes(name, bytes);
        }

        public void WriteVoidBytesL(string name, byte[] value)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_VOID);
                InternalWriteBinarySize(value.Length);
                BaseStream.Write(value);
                InternalAlignBinary();
                return;
            }
            BaseStream.Write(Encoding.ASCII.GetBytes($"{name} = "));
            InternalWriteStringValue(BitConverter.ToString(value).Replace("-", string.Empty).ToLowerInvariant());
            InternalWriteNewline();
        }
        public void WriteVoidBytes(string name, byte[] value)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_VOID);
                InternalWriteBinarySize(value.Length);
                BaseStream.Write(value);
                InternalAlignBinary();
                return;
            }
            BaseStream.Write(Encoding.ASCII.GetBytes($"{name} = "));
            InternalWriteStringValue(BitConverter.ToString(value).Replace("-", string.Empty));
            InternalWriteNewline();
        }

        public void WriteValidation(string name)
        {
            if (InBinary)
                return;

            BaseStream.Write(Encoding.ASCII.GetBytes($"[{name}]"));
            InternalWriteNewline();
        }

        private void InternalWriteBinaryType(BinaryFieldType type)
        {
            // todo Type size is only 1 byte no matter what here, put a breadcrumb so we can find all the places this is true just in case
            if (TypeSize > 0)
            {
                byte[] number = new byte[TypeSize];
                if (IsBigEndian)
                {
                    number[number.Length - 1] = (byte)type;
                }
                else
                {
                    number[0] = (byte)type;
                }
                BaseStream.Write(number);
            }
        }

        private void InternalAlignBinary()
        {
            // todo likely untested
            if (AlignmentBytes > 0)
            {
                long position = BaseStream.Position;
                long paddingNeeded = (AlignmentBytes - (position % AlignmentBytes)) % AlignmentBytes;
                if (paddingNeeded > 0)
                {
                    BaseStream.Write(new byte[paddingNeeded]);
                }
            }
        }
        private void InternalWriteBinarySize(int size)
        {
            if (SizeSize > 0)
            {
                byte[] sizeBytes = new byte[SizeSize];
                byte[] rawSize = BitConverter.GetBytes(size);

                Array.Copy(rawSize, 0, sizeBytes, 0, SizeSize);

                if (IsBigEndian)
                    Array.Reverse(sizeBytes);

                BaseStream.Write(sizeBytes);
            }
        }
        private void InternalWriteStringValue(string value)
        {
            if (QuoteStrings || value.Contains(' ') || value.Contains('\t'))
            {
                // Escape quotes in the string
                string escapedValue = value.Replace("\"", "\\\"");
                BaseStream.Write(Encoding.ASCII.GetBytes($"\"{escapedValue}\""));
            }
            else
            {
                BaseStream.Write(Encoding.ASCII.GetBytes(value));
            }
        }
        private void InternalWriteNewline()
        {
            // TODO deal with newline malformation here
            byte[] newline = Encoding.ASCII.GetBytes("\r\n");
            BaseStream.Write(newline);
        }
    }
}