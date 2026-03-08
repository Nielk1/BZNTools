using System;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static BZNParser.Tokenizer.BZNStreamReader;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BZNParser.Tokenizer
{
    public static class SingleExtension
    {

        private static Regex pattern_9e2 = new Regex(@"^-?[0-9]\.[0-9]{8}e[+-][0-9]{2}$");
        private static Regex pattern_9e3 = new Regex(@"^-?[0-9]\.[0-9]{8}e[+-][0-9]{3}$");
        public static FloatTextFormat GetFloatTextFormat(string value)
        {
            if (pattern_9e2.IsMatch(value))
            {
                return FloatTextFormat._9e2;
            }
            else if (pattern_9e3.IsMatch(value))
            {
                return FloatTextFormat._9e3;
            }
            return FloatTextFormat.G;
        }

        public static (int sign, int exponent, uint mantissa) Extract(this float value)
        {
            uint bits = BitConverter.SingleToUInt32Bits(value);

            int sign = (int)(bits >> 31);
            int exponent = (int)((bits >> 23) & 0xFF);
            uint mantissa = bits & 0x7FFFFF;

            return (sign, exponent, mantissa);
        }

        public static string ToBZNString(this float value, FloatTextFormat format)
        {
            if (float.IsNaN(value)) return "nan";
            if (float.IsPositiveInfinity(value)) return "inf";
            if (float.IsNegativeInfinity(value)) return "-inf";

            (int sign, int exponent, uint mantissa) = value.Extract();
            switch (format)
            {
                case FloatTextFormat._9e2:
                    return Format1e8(sign, exponent, mantissa, 2);
                case FloatTextFormat._9e3:
                    return Format1e8(sign, exponent, mantissa, 3);
                case FloatTextFormat.G:
                default:
                    return FormatG6(sign, exponent, mantissa);
            }
        }

        private static string Format1e8(int sign, int exponent, uint mantissa, int exponentSize)
        {
            if (exponent == 255)
            {
                if (mantissa == 0)
                    return sign != 0 ? "-inf" : "inf";
                return "nan";
            }

            if (exponent == 0 && mantissa == 0)
                return sign != 0 ? $"-0.00000000e+{new string('0', exponentSize)}" : $"0.00000000e+{new string('0', exponentSize)}";

            BigInteger num;
            int exp2;

            if (exponent == 0)
            {
                // subnormal
                num = mantissa;
                exp2 = -149;
            }
            else
            {
                // normal
                num = ((BigInteger)1 << 23) | mantissa;
                exp2 = exponent - 150;
            }

            BigInteger den = BigInteger.One;

            if (exp2 >= 0)
                num <<= exp2;
            else
                den <<= -exp2;

            // Normalize so 1 <= num/den < 10
            int exp10 = 0;

            while (num >= den * 10)
            {
                den *= 10;
                exp10++;
            }

            while (num < den)
            {
                num *= 10;
                exp10--;
            }

            // Optional sanity check:
            // At this point the normalized significand must be in [1, 10).
            if (!(num >= den && num < den * 10))
                throw new InvalidOperationException("Normalization failed.");

            // Generate 10 digits total:
            // 9 to keep, 1 guard digit for rounding
            char[] allDigits = new char[10];

            for (int i = 0; i < allDigits.Length; i++)
            {
                BigInteger d = num / den;
                allDigits[i] = (char)('0' + (int)d);
                num %= den;
                num *= 10;
            }

            // Keep first 9 digits
            char[] digits = new char[9];
            Array.Copy(allDigits, digits, 9);

            // Round using guard digit + sticky remainder
            int guard = allDigits[9] - '0';
            bool sticky = num != 0;

            // round-half-away-from-zero on magnitude
            bool roundUp = guard > 5 || (guard == 5 && sticky) || (guard == 5 && !sticky);

            if (roundUp)
            {
                int i = digits.Length - 1;
                while (i >= 0)
                {
                    if (digits[i] != '9')
                    {
                        digits[i]++;
                        break;
                    }

                    digits[i] = '0';
                    i--;
                }

                if (i < 0)
                {
                    digits[0] = '1';
                    for (int j = 1; j < digits.Length; j++)
                        digits[j] = '0';
                    exp10++;
                }
            }

            var sb = new StringBuilder(16);

            if (sign != 0)
                sb.Append('-');

            sb.Append(digits[0]);
            sb.Append('.');
            for (int i = 1; i < digits.Length; i++)
                sb.Append(digits[i]);

            sb.Append('e');
            sb.Append(exp10 >= 0 ? '+' : '-');
            sb.Append(Math.Abs(exp10).ToString($"D{exponentSize}"));

            return sb.ToString();
        }

        private static string FormatG6(int sign, int exponent, uint mantissa)
        {
            const int precision = 6;

            // Special cases
            if (exponent == 255)
            {
                if (mantissa == 0)
                    return sign != 0 ? "-inf" : "inf";
                return "nan";
            }

            if (exponent == 0 && mantissa == 0)
                return sign != 0 ? "-0" : "0";

            // Build exact rational value = num / den
            BigInteger num;
            int exp2;

            if (exponent == 0)
            {
                // subnormal: mantissa * 2^-149
                num = mantissa;
                exp2 = -149;
            }
            else
            {
                // normal: (2^23 + mantissa) * 2^(exponent - 127 - 23)
                num = ((BigInteger)1 << 23) | mantissa;
                exp2 = exponent - 150; // exponent - 127 - 23
            }

            BigInteger den = BigInteger.One;

            if (exp2 >= 0)
                num <<= exp2;
            else
                den <<= -exp2;

            // Normalize to decimal scientific form: 1 <= num/den < 10
            int exp10 = 0;

            while (num >= den * 10)
            {
                den *= 10;
                exp10++;
            }

            while (num < den)
            {
                num *= 10;
                exp10--;
            }

            // Generate exactly 'precision' significant digits, then round manually
            char[] digits = new char[precision];

            for (int i = 0; i < precision; i++)
            {
                BigInteger d = num / den;
                digits[i] = (char)('0' + (int)d);
                num %= den;
                num *= 10;
            }

            // Remainder now represents discarded tail as num / (10*den)
            // Round half away from zero on magnitude
            bool roundUp = (num * 2) >= (den * 10);

            if (roundUp)
            {
                int i = precision - 1;
                while (i >= 0)
                {
                    if (digits[i] != '9')
                    {
                        digits[i]++;
                        break;
                    }

                    digits[i] = '0';
                    i--;
                }

                // 9.99999 -> 1.00000e+01 style carry
                if (i < 0)
                {
                    digits[0] = '1';
                    for (int j = 1; j < precision; j++)
                        digits[j] = '0';
                    exp10++;
                }
            }

            // %g rule: use exponent form if exponent < -4 or exponent >= precision
            bool useExponent = exp10 < -4 || exp10 >= precision;

            string body = useExponent
                ? BuildExponentForm(digits, exp10)
                : BuildFixedForm(digits, exp10);

            return sign != 0 ? "-" + body : body;
        }

        private static string BuildExponentForm(char[] digits, int exp10)
        {
            var sb = new StringBuilder();

            sb.Append(digits[0]);

            if (digits.Length > 1)
            {
                sb.Append('.');
                for (int i = 1; i < digits.Length; i++)
                    sb.Append(digits[i]);

                TrimTrailingZerosAndDot(sb);
            }

            sb.Append('e');
            sb.Append(exp10 >= 0 ? '+' : '-');
            sb.Append(Math.Abs(exp10).ToString("D3")); // pad to 3 digits

            return sb.ToString();
        }

        private static string BuildFixedForm(char[] digits, int exp10)
        {
            // digits represent:
            // digits[0].digits[1..] × 10^exp10
            //
            // Place decimal point after (exp10 + 1) digits from the left.
            int decimalPos = exp10 + 1;
            int n = digits.Length;

            var sb = new StringBuilder();

            if (decimalPos <= 0)
            {
                sb.Append('0');
                sb.Append('.');
                for (int i = 0; i < -decimalPos; i++)
                    sb.Append('0');
                for (int i = 0; i < n; i++)
                    sb.Append(digits[i]);
            }
            else if (decimalPos >= n)
            {
                for (int i = 0; i < n; i++)
                    sb.Append(digits[i]);
                for (int i = 0; i < decimalPos - n; i++)
                    sb.Append('0');
            }
            else
            {
                for (int i = 0; i < decimalPos; i++)
                    sb.Append(digits[i]);
                sb.Append('.');
                for (int i = decimalPos; i < n; i++)
                    sb.Append(digits[i]);
            }

            TrimTrailingZerosAndDot(sb);
            return sb.ToString();
        }

        private static void TrimTrailingZerosAndDot(StringBuilder sb)
        {
            int dot = -1;
            for (int i = 0; i < sb.Length; i++)
            {
                if (sb[i] == '.')
                {
                    dot = i;
                    break;
                }
            }

            // No decimal point => nothing fractional to trim
            if (dot < 0)
                return;

            int end = sb.Length - 1;

            while (end > dot && sb[end] == '0')
                end--;

            if (end == dot)
                end--; // remove decimal point too

            sb.Length = end + 1;
        }
    }
    public class BZNStreamWriter : IDisposable
    {
        private static Encoding win1252;
        static BZNStreamWriter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            win1252 = Encoding.GetEncoding(1252);
        }



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
        public FloatTextFormat FloatFormat { get; set; }

        public BZNStreamWriter(Stream stream, BZNFormat format, int version)
        {
            BaseStream = stream;
            Format = format;
            Version = version;
            FloatFormat = FloatTextFormat.G;

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
                    if (Version >= 1182)
                    {
                        FloatFormat = FloatTextFormat._9e2;
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
            BaseStream.Write(win1252.GetBytes($"{name} = "));
            InternalWriteStringValue(value);
            InternalWriteNewline();
        }

        public void WriteRaw(string name, byte[] values)
        {
            if (InBinary)
            {
                throw new NotImplementedException("Raw Write only for ASCII");
            }

            BaseStream.Write(win1252.GetBytes($"{name} [1] ="));
            InternalWriteNewline();
            BaseStream.Write(values);
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
            BaseStream.Write(win1252.GetBytes($"{name} [{values.Length}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++) {
                BaseStream.Write(win1252.GetBytes($"{(values[i] ? "true" : "false")}"));
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
            BaseStream.Write(win1252.GetBytes($"{name} [{values.Length}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(win1252.GetBytes(values[i].ToString()));
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
            BaseStream.Write(win1252.GetBytes($"{name} [{values.Length}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(win1252.GetBytes(values[i].ToString()));
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
            BaseStream.Write(win1252.GetBytes($"{name} [{values.Length}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(win1252.GetBytes(values[i].ToString("x")));
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
            BaseStream.Write(win1252.GetBytes($"{name} [{values.Length}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(win1252.GetBytes(values[i].ToString("x")));
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
            BaseStream.Write(win1252.GetBytes($"{name} ="));
            {
                if (value != 0)
                {
                    InternalWriteNewline();
                    BaseStream.Write(win1252.GetBytes(value.ToString()));
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
            BaseStream.Write(win1252.GetBytes($"{name} ="));
            InternalWriteNewline();
            {
                {
                    InternalWriteNewline();
                    BaseStream.Write(win1252.GetBytes(value.ToString("x")));
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
            BaseStream.Write(win1252.GetBytes($"{name} [{values.Length}] ="));
            {
                foreach (UInt32 value in values)
                {
                    InternalWriteNewline();
                    BaseStream.Write(win1252.GetBytes(value.ToString("x")));
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
            BaseStream.Write(win1252.GetBytes($"{name} [{values.Length}] ="));
            {
                foreach (UInt16 value in values)
                {
                    InternalWriteNewline();
                    BaseStream.Write(win1252.GetBytes(value.ToString("x")));
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
            BaseStream.Write(win1252.GetBytes($"{name} [{values.Length}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(win1252.GetBytes(values[i].ToString()));
                InternalWriteNewline();
            }
        }

        public void WriteVector2Ds(string name, bool preserveMalformations, params Vector2D[] values)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_VEC2D);
                InternalWriteBinarySize(values.Length * sizeof(float) * 2);
                foreach (Vector2D value in values)
                {
                    byte[] xBytes = BitConverter.GetBytes(value.X);
                    byte[] zBytes = BitConverter.GetBytes(value.Z);
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
            BaseStream.Write(win1252.GetBytes($"{name} [{(values.Length)}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  x")} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  x", values[i].X, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();
                
                BaseStream.Write(win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  z")} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  z", values[i].Z, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();
            }
        }

        public void InternalWriteFloatValue(string name, float value, bool preserveMalformations, FloatTextFormat floatFormat, IMalformable.MalformationManager malformations)
        {
            if (InBinary)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                if (IsBigEndian)
                    Array.Reverse(bytes);
                BaseStream.Write(bytes);
                return;
            }

            if (preserveMalformations)
            {
                var mal = malformations.GetMalformations(Malformation.INCORRECT_TEXT, name);
                if (mal.Any())
                {
                    BaseStream.Write(win1252.GetBytes((string)mal.First().Fields[0]));
                    return;
                }
            }
            BaseStream.Write(win1252.GetBytes(value.ToBZNString(FloatFormat)));
        }

        public void WriteVector3Ds(string name, bool preserveMalformations, params Vector3D[] values)
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
            BaseStream.Write(win1252.GetBytes($"{name} [{(values.Length)}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  x")} [1] ="));
                InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes(values[i].x.ToBZNString(FloatFormat)));
                InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  y")} [1] ="));
                InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes(values[i].y.ToBZNString(FloatFormat)));
                InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  z")} [1] ="));
                InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes(values[i].z.ToBZNString(FloatFormat)));
                InternalWriteNewline();
            }
        }

        public void WriteEuler(string name, bool preserveMalformations, Euler value)
        {
            if (InBinary)
            {
                throw new NotImplementedException("Euler binary save attempt");
                return;
            }
            BaseStream.Write(win1252.GetBytes($"{name} ="));
            InternalWriteNewline();

            WriteFloats(value.Malformations.CorrectName(preserveMalformations, " mass"), value.mass);
            WriteFloats(value.Malformations.CorrectName(preserveMalformations, " mass_inv"), value.mass_inv);
            WriteFloats(value.Malformations.CorrectName(preserveMalformations, " v_mag"), value.v_mag);
            WriteFloats(value.Malformations.CorrectName(preserveMalformations, " v_mag_inv"), value.v_mag_inv);
            WriteFloats(value.Malformations.CorrectName(preserveMalformations, " I"), value.I);
            WriteFloats(value.Malformations.CorrectName(preserveMalformations, " k_i"), value.I_inv);
            WriteVector3Ds(value.Malformations.CorrectName(preserveMalformations, " v"    ), preserveMalformations, value.v    );
            WriteVector3Ds(value.Malformations.CorrectName(preserveMalformations, " omega"), preserveMalformations, value.omega);
            WriteVector3Ds(value.Malformations.CorrectName(preserveMalformations, " Accel"), preserveMalformations, value.Accel);
        }

        public void WriteMat3Ds(string name, bool preserveMalformations, params Matrix[] values)
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
            BaseStream.Write(win1252.GetBytes($"{name} [{(values.Length)}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  right.x")} [1] =")); InternalWriteNewline(); InternalWriteFloatValue("  right.x", values[i].right.x, preserveMalformations, FloatFormat, values[i].Malformations); InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  right.y")} [1] =")); InternalWriteNewline(); InternalWriteFloatValue("  right.x", values[i].right.y, preserveMalformations, FloatFormat, values[i].Malformations); InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  right.z")} [1] =")); InternalWriteNewline(); InternalWriteFloatValue("  right.x", values[i].right.z, preserveMalformations, FloatFormat, values[i].Malformations); InternalWriteNewline();

                BaseStream.Write(win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  up.x")} [1] =")); InternalWriteNewline(); InternalWriteFloatValue("  up.x", values[i].up.x, preserveMalformations, FloatFormat, values[i].Malformations); InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  up.y")} [1] =")); InternalWriteNewline(); InternalWriteFloatValue("  up.x", values[i].up.y, preserveMalformations, FloatFormat, values[i].Malformations); InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  up.z")} [1] =")); InternalWriteNewline(); InternalWriteFloatValue("  up.x", values[i].up.z, preserveMalformations, FloatFormat, values[i].Malformations); InternalWriteNewline();

                BaseStream.Write(win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  front.x")} [1] =")); InternalWriteNewline(); InternalWriteFloatValue("  front.x", values[i].front.x, preserveMalformations, FloatFormat, values[i].Malformations); InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  front.y")} [1] =")); InternalWriteNewline(); InternalWriteFloatValue("  front.x", values[i].front.y, preserveMalformations, FloatFormat, values[i].Malformations); InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  front.z")} [1] =")); InternalWriteNewline(); InternalWriteFloatValue("  front.x", values[i].front.z, preserveMalformations, FloatFormat, values[i].Malformations); InternalWriteNewline();

                BaseStream.Write(win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  posit.x")} [1] =")); InternalWriteNewline(); InternalWriteFloatValue("  posit.x", values[i].posit.x, preserveMalformations, FloatFormat, values[i].Malformations); InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  posit.y")} [1] =")); InternalWriteNewline(); InternalWriteFloatValue("  posit.y", values[i].posit.y, preserveMalformations, FloatFormat, values[i].Malformations); InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  posit.z")} [1] =")); InternalWriteNewline(); InternalWriteFloatValue("  posit.z", values[i].posit.z, preserveMalformations, FloatFormat, values[i].Malformations); InternalWriteNewline();
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
            BaseStream.Write(win1252.GetBytes($"{name} [{(values.Length)}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(win1252.GetBytes($"  right_x [1] ="));
                InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes(values[i].right.x.ToBZNString(FloatFormat)));
                InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes($"  right_y [1] ="));
                InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes(values[i].right.y.ToBZNString(FloatFormat)));
                InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes($"  right_z [1] ="));
                InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes(values[i].right.z.ToBZNString(FloatFormat)));
                InternalWriteNewline();

                BaseStream.Write(win1252.GetBytes($"  up_x [1] ="));
                InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes(values[i].up.x.ToBZNString(FloatFormat)));
                InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes($"  up_y [1] ="));
                InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes(values[i].up.y.ToBZNString(FloatFormat)));
                InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes($"  up_z [1] ="));
                InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes(values[i].up.z.ToBZNString(FloatFormat)));
                InternalWriteNewline();

                BaseStream.Write(win1252.GetBytes($"  front_x [1] ="));
                InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes(values[i].front.x.ToBZNString(FloatFormat)));
                InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes($"  front_y [1] ="));
                InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes(values[i].front.y.ToBZNString(FloatFormat)));
                InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes($"  front_z [1] ="));
                InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes(values[i].front.z.ToBZNString(FloatFormat)));
                InternalWriteNewline();

                BaseStream.Write(win1252.GetBytes($"  posit_x [1] ="));
                InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes(values[i].posit.x.ToBZNString(FloatFormat)));
                InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes($"  posit_y [1] ="));
                InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes(values[i].posit.y.ToBZNString(FloatFormat)));
                InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes($"  posit_z [1] ="));
                InternalWriteNewline();
                BaseStream.Write(win1252.GetBytes(values[i].posit.z.ToBZNString(FloatFormat)));
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
            BaseStream.Write(win1252.GetBytes($"{name} [{values.Length}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++) {
                BaseStream.Write(win1252.GetBytes(values[i].ToString()));
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
            BaseStream.Write(win1252.GetBytes($"{name} [{values.Length}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(win1252.GetBytes(values[i].ToBZNString(FloatFormat)));
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
            BaseStream.Write(win1252.GetBytes($"{name} [1] ="));
            InternalWriteNewline();
            BaseStream.Write(win1252.GetBytes(value.ToString("x")));
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
            BaseStream.Write(win1252.GetBytes($"{name} [1] ="));
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
                BaseStream.Write(win1252.GetBytes(value));
                InternalAlignBinary();
                return;
            }
            BaseStream.Write(win1252.GetBytes($"{name} [1] ="));
            InternalWriteNewline();
            BaseStream.Write(win1252.GetBytes(value));
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
            BaseStream.Write(win1252.GetBytes($"{name} [{values.Length}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(win1252.GetBytes(values[i].ToString()));
                InternalWriteNewline();
            }
        }

        public void WriteChars(string name, string value)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_CHAR);
                byte[] stringBytes = win1252.GetBytes(value);
                InternalWriteBinarySize(stringBytes.Length);
                BaseStream.Write(stringBytes);
                InternalAlignBinary();
                return;
            }
            BaseStream.Write(win1252.GetBytes($"{name} = "));

            // this is actually a malformation to not have the space, from what we can tell, so we need to figure out this one
            //if (value.Length > 0)
            //    BaseStream.Write(win1252.GetBytes(" ")); // only have the trailing space if the value exists

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
            BaseStream.Write(win1252.GetBytes($"{name} = "));
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
            BaseStream.Write(win1252.GetBytes($"{name} [{values.Length}] = "));
            InternalWriteNewline();
            foreach (uint value in values)
            {
                InternalWriteStringValue(value.ToString("X8"));
                InternalWriteNewline();
            }
        }
        public void WriteVoidBytesL(string name, UInt32 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (IsBigEndian)
                Array.Reverse(bytes);
            WriteVoidBytesL(name, bytes);
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
            BaseStream.Write(win1252.GetBytes($"{name} = "));
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
            BaseStream.Write(win1252.GetBytes($"{name} = "));
            InternalWriteStringValue(BitConverter.ToString(value).Replace("-", string.Empty));
            InternalWriteNewline();
        }

        public void WriteValidation(string name)
        {
            if (InBinary)
                return;

            BaseStream.Write(win1252.GetBytes($"[{name}]"));
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
            if (QuoteStrings)// || value.Contains(' ') || value.Contains('\t'))
            {
                // Escape quotes in the string
                string escapedValue = value.Replace("\"", "\\\"");
                BaseStream.Write(win1252.GetBytes($"\"{escapedValue}\""));
            }
            else
            {
                BaseStream.Write(win1252.GetBytes(value));
            }
        }
        private void InternalWriteNewline()
        {
            // TODO deal with newline malformation here
            byte[] newline = win1252.GetBytes("\r\n");
            BaseStream.Write(newline);
        }
    }
}