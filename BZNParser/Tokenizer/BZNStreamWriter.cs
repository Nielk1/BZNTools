using BZNParser.Battlezone;
using System;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static BZNParser.Tokenizer.BZNStreamReader;
using static System.Runtime.InteropServices.JavaScript.JSType;
using UInt8 = byte;

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
            if (float.IsNaN(value)) return "-1.#QNAN";
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

        public Dictionary<int, StreamDefect>? StreamDefects { get; set; }
        public int TokenIndex { get; private set; }
        public string NewLine { get; set; }

        public BZNStreamWriter(Stream stream, BZNFormat format, int version, Dictionary<int, StreamDefect>? preserveDefects = null)
        {
            BaseStream = stream;
            Format = format;
            Version = version;
            FloatFormat = FloatTextFormat.G;
            if (preserveDefects != null && preserveDefects.Count > 0)
                StreamDefects = preserveDefects;
            TokenIndex = 0;

            NewLine = "\r\n";

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

        internal static TProp ExtractPropertyValue<T, TProp>(T parent, Expression<Func<T, TProp>> property)
        {
            if (parent == null)
                throw new ArgumentException("Parent object cannot be null", nameof(parent));

            if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo)
                return (TProp)propInfo.GetValue(parent)!;

            throw new ArgumentException("Expression is not a property", nameof(property));
        }

        /// <summary>
        /// Write a UInt8 to the BZN
        /// </summary>
        /// <remarks>
        /// Handles the following malformations: <see cref="Malformation.INCORRECT_TEXT"/>
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="property"></param>
        public (UInt8 written, TProp stored) WriteUInt8<T, TProp>(string name, T parent, Expression<Func<T, TProp>> property, Func<TProp, UInt8>? convert = null) where T : IMalformable
        {
            TProp valueInternal = ExtractPropertyValue(parent, property);
            UInt8 value = 0;

            if (convert != null)
            {
                value = convert(valueInternal);
            }
            else if (typeof(TProp) == typeof(UInt8) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt8))
            {
                value = (UInt8)(object)valueInternal!;
            }
            else
            {
                throw new Exception("Property type is not compatible with boolean writing and no conversion provided");
            }

            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_CHAR);
                InternalWriteBinarySize(1);
                BaseStream.WriteByte((byte)(value));
                InternalAlignBinary();
                TokenIndex++;
                return (value, valueInternal);
            }

            string textValue = value.ToString();

            // handle incorrect raw value
            (bool hasIncorrectRaw, string? incorrectText) = parent.Malformations.GetIncorrectTextParse(property);
            if (hasIncorrectRaw)
                textValue = incorrectText ?? string.Empty;

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} [1] ="));
            InternalWriteNewline();
            BaseStream.Write(BZNEncoding.win1252.GetBytes(textValue));
            InternalWriteNewline();
            TokenIndex++;

            return (value, valueInternal);
        }

        /// <summary>
        /// Write a chars string to the BZN
        /// </summary>
        /// <remarks>
        /// Handles the following malformations: <see cref="Malformation.INCORRECT_RAW"/>, <see cref="Malformation.RIGHT_TRIM"/>, <see cref="Malformation.INCORRECT_NAME"/>
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="property"></param>
        /// <param name="oneLiner"></param>
        public void WriteChars<T>(string name, T parent, Expression<Func<T, SizedString>> property) where T : IMalformable
        {
            SizedString wrappedValue = ExtractPropertyValue(parent, property);
            string value = wrappedValue.Value; // we don't care about the size as we're a normal char print
            byte[] rawValue = BZNEncoding.win1252.GetBytes(value);

            // handle incorrect raw value
            (bool hasIncorrectRaw, byte[]? incorrectRaw) = parent.Malformations.GetIncorrectRaw(property);
            if (hasIncorrectRaw)
                rawValue = incorrectRaw ?? [];

            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_ID);
                InternalWriteBinarySize(rawValue.Length);
                BaseStream.Write(rawValue);
                InternalAlignBinary();
                TokenIndex++;
                return;
            }

            (bool hasIncorrectName, string? incorrectName) = parent.Malformations.GetIncorrectName(property);
            if (hasIncorrectName)
                name = incorrectName!;

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} ="));
            if (!parent.Malformations.IsRightTrimmed(property))
                BaseStream.Write(BZNEncoding.win1252.GetBytes(" "));
            InternalWriteNewline();
            BaseStream.Write(rawValue);
            InternalWriteNewline();
            TokenIndex++;
        }

        /// <summary>
        /// Write a chars string to the BZN
        /// </summary>
        /// <remarks>
        /// Handles the following malformations: <see cref="Malformation.INCORRECT_RAW"/>, <see cref="Malformation.RIGHT_TRIM"/>, <see cref="Malformation.INCORRECT_NAME"/>
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="property"></param>
        /// <param name="oneLiner"></param>
        public void WriteChars<T>(string name, T parent, Expression<Func<T, string>> property) where T : IMalformable
        {
            string value = ExtractPropertyValue(parent, property);
            byte[] rawValue = BZNEncoding.win1252.GetBytes(value);

            // handle incorrect raw value
            (bool hasIncorrectRaw, byte[]? incorrectRaw) = parent.Malformations.GetIncorrectRaw(property);
            if (hasIncorrectRaw)
                rawValue = incorrectRaw ?? [];

            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_ID);
                InternalWriteBinarySize(rawValue.Length);
                BaseStream.Write(rawValue);
                InternalAlignBinary();
                TokenIndex++;
                return;
            }

            (bool hasIncorrectName, string? incorrectName) = parent.Malformations.GetIncorrectName(property);
            if (hasIncorrectName)
                name = incorrectName!;

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} ="));
            if (!parent.Malformations.IsRightTrimmed(property))
                BaseStream.Write(BZNEncoding.win1252.GetBytes(" "));
            InternalWriteNewline();
            BaseStream.Write(rawValue);
            InternalWriteNewline();
            TokenIndex++;
        }

        /// <summary>
        /// Write a Boolean to the BZN
        /// </summary>
        /// <remarks>
        /// Handles the following malformations: <see cref="Malformation.INCORRECT_TEXT"/>
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="property"></param>
        public void WriteBoolean<T, TProp>(string name, T parent, Expression<Func<T, TProp>> property, Func<TProp, bool>? convert = null) where T : IMalformable
        {
            TProp valueInternal = ExtractPropertyValue(parent, property);
            bool value = false;

            if (convert != null)
            {
                value = convert(valueInternal);
            }
            else if (typeof(TProp) == typeof(bool) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(bool))
            {
                value = (bool)(object)valueInternal!;
            }
            else
            {
                throw new Exception("Property type is not compatible with boolean writing and no conversion provided");
            }

            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_ID);
                InternalWriteBinarySize(1);
                BaseStream.WriteByte((byte)(value ? 1 : 0));
                InternalAlignBinary();
                TokenIndex++;
                return;
            }

            string textValue = value ? "true" : "false";

            // handle incorrect raw value
            (bool hasIncorrectRaw, string? incorrectText) = parent.Malformations.GetIncorrectTextParse(property);
            if (hasIncorrectRaw)
                textValue = incorrectText ?? string.Empty;

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} [1] ="));
            InternalWriteNewline();
            BaseStream.Write(BZNEncoding.win1252.GetBytes(textValue));
            InternalWriteNewline();
            TokenIndex++;
        }




        // TODO determine if one-liner mode is actually a differnt token type, only samples we have right now are ASCII so we don't know if the type is not ID
        /// <summary>
        /// Write an ID to the BZN
        /// </summary>
        /// <remarks>
        /// Handles the following malformations: <see cref="Malformation.INCORRECT_RAW"/>
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="property"></param>
        /// <param name="oneLiner"></param>
        public void WriteID<T>(string name, T parent, Expression<Func<T, string>> property, bool oneLiner = false) where T : IMalformable
        {
            string value = ExtractPropertyValue(parent, property);
            byte[] rawValue = BZNEncoding.win1252.GetBytes(value);

            // handle incorrect raw value
            (bool hasIncorrectRaw, byte[]? incorrectRaw) = parent.Malformations.GetIncorrectRaw(property);
            if (hasIncorrectRaw)
                rawValue = incorrectRaw ?? [];

            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_ID);
                InternalWriteBinarySize(rawValue.Length);
                BaseStream.Write(rawValue);
                InternalAlignBinary();
                TokenIndex++;
                return;
            }
            if (oneLiner)
            {
                // maybe one-liner should be a general writer tool so it can use the malformation for no trailing space universally
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} = "));
            }
            else
            {
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} [1] ="));
                InternalWriteNewline();
            }
            BaseStream.Write(rawValue);
            InternalWriteNewline();
            TokenIndex++;
        }














        public void WriteUnknown(string name, string value)
        {
            if (InBinary)
                throw new Exception("Unknown type data cannot be written in binary mode.");
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} = "));
            InternalWriteStringValue(value);
            InternalWriteNewline();
            TokenIndex++;
        }

        public void WriteRaw(string name, byte[] values)
        {
            if (InBinary)
            {
                throw new NotImplementedException("Raw Write only for ASCII");
            }

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} [1] ="));
            InternalWriteNewline();
            BaseStream.Write(values);
            InternalWriteNewline();
            TokenIndex++;
        }

        [Obsolete]
        public void WriteBooleans(string name, IMalformable.MalformationManager? malformations, params bool[] values)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_BOOL);
                InternalWriteBinarySize(values.Length);

                foreach (bool value in values)
                    BaseStream.WriteByte((byte)(value ? 1 : 0));

                InternalAlignBinary();
                TokenIndex++;
                return;
            }

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} [{values.Length}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++) {
                if (malformations != null)
                {
//                    var mal = malformations.GetMalformations(Malformation.INCORRECT_TEXT, name);
//                    if (mal.Any())
//                    {
//                        // TODO index handling
//                        BaseStream.Write(BZNEncoding.win1252.GetBytes((string)mal.First().Fields[0]));
//                        InternalWriteNewline();
//                        continue;
//                    }
                }

                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{(values[i] ? "true" : "false")}"));
                InternalWriteNewline();
            }
            TokenIndex++;
        }

        [Obsolete]
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
                TokenIndex++;
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} [{values.Length}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(BZNEncoding.win1252.GetBytes(values[i].ToString()));
                InternalWriteNewline();
            }
            TokenIndex++;
        }

        [Obsolete]
        public void WriteUnsignedValues(string name, params UInt16[] values)
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
                TokenIndex++;
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} [{values.Length}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(BZNEncoding.win1252.GetBytes(values[i].ToString()));
                InternalWriteNewline();
            }
            TokenIndex++;
        }

        [Obsolete]
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
                TokenIndex++;
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} [{values.Length}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(BZNEncoding.win1252.GetBytes(values[i].ToString("x")));
                InternalWriteNewline();
            }
            TokenIndex++;
        }

        [Obsolete]
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
                TokenIndex++;
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} [{values.Length}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(BZNEncoding.win1252.GetBytes(values[i].ToString("x")));
                InternalWriteNewline();
            }
            TokenIndex++;
        }

        // used for: undefaicmd
        [Obsolete]
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
                TokenIndex++;
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} ="));
            {
                if (value != 0)
                {
                    InternalWriteNewline();
                    BaseStream.Write(BZNEncoding.win1252.GetBytes(value.ToString()));
                }
                InternalWriteNewline();
            }
            TokenIndex++;
        }

        [Obsolete]
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
                TokenIndex++;
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} ="));
            InternalWriteNewline();
            {
                {
                    InternalWriteNewline();
                    BaseStream.Write(BZNEncoding.win1252.GetBytes(value.ToString("x")));
                }
                InternalWriteNewline();
            }
            TokenIndex++;
        }

        [Obsolete]
        public void WriteLongFlags(string name, params UInt32[] values)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_LONG);
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
                TokenIndex++;
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} [{values.Length}] ="));
            {
                foreach (UInt32 value in values)
                {
                    InternalWriteNewline();
                    BaseStream.Write(BZNEncoding.win1252.GetBytes(value.ToString("x")));
                }
                InternalWriteNewline();
            }
            TokenIndex++;
        }

        [Obsolete]
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
                TokenIndex++;
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} [{values.Length}] ="));
            {
                foreach (UInt16 value in values)
                {
                    InternalWriteNewline();
                    BaseStream.Write(BZNEncoding.win1252.GetBytes(value.ToString("x")));
                }
                InternalWriteNewline();
            }
            TokenIndex++;
        }

        [Obsolete]
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
                TokenIndex++;
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} [{values.Length}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(BZNEncoding.win1252.GetBytes(values[i].ToString()));
                InternalWriteNewline();
            }
            TokenIndex++;
        }

        [Obsolete]
        public void WriteVector2Ds(string name, bool preserveMalformations, params Vector2D[] values)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_VEC2D);
                InternalWriteBinarySize(values.Length * sizeof(float) * 2);
                foreach (Vector2D value in values)
                {
                    InternalWriteFloatValue("  x", value.X, preserveMalformations, FloatFormat, value.Malformations);
                    InternalWriteFloatValue("  z", value.Z, preserveMalformations, FloatFormat, value.Malformations);
                }
                InternalAlignBinary();
                TokenIndex++;
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} [{(values.Length)}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  x")} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  x", values[i].X, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();
                
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  z")} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  z", values[i].Z, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();
            }
            TokenIndex++;
        }

        [Obsolete]
        private void InternalWriteFloatValue(string name, float value, bool preserveMalformations, FloatTextFormat floatFormat, IMalformable.MalformationManager malformations)
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
//                var mal = malformations.GetMalformations(Malformation.INCORRECT_TEXT, name);
//                if (mal.Any())
//                {
//                    BaseStream.Write(BZNEncoding.win1252.GetBytes((string)mal.First().Fields[0]));
//                    return;
//                }
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes(value.ToBZNString(FloatFormat)));
        }

        [Obsolete]
        public void WriteVector3Ds(string name, bool preserveMalformations, params Vector3D[] values)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_VEC3D);
                InternalWriteBinarySize(values.Length * sizeof(float) * 3);
                foreach (Vector3D value in values)
                {
                    InternalWriteFloatValue("  x", value.X, preserveMalformations, FloatFormat, value.Malformations);
                    InternalWriteFloatValue("  y", value.Y, preserveMalformations, FloatFormat, value.Malformations);
                    InternalWriteFloatValue("  z", value.Z, preserveMalformations, FloatFormat, value.Malformations);
                }
                InternalAlignBinary();
                TokenIndex++;
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} [{(values.Length)}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  x")} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  x", values[i].X, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  y")} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  y", values[i].Y, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  z")} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  z", values[i].Z, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();
            }
            TokenIndex++;
        }

        [Obsolete]
        public void WriteEuler(string name, bool preserveMalformations, Euler value)
        {
            if (InBinary)
            {
                throw new NotImplementedException("Euler binary save attempt");
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} ="));
            InternalWriteNewline();

            WriteFloats(value.Malformations.CorrectName(preserveMalformations, " mass"), preserveMalformations ? value.Malformations : null, value.mass);
            WriteFloats(value.Malformations.CorrectName(preserveMalformations, " mass_inv"), preserveMalformations ? value.Malformations : null, value.mass_inv);
            WriteFloats(value.Malformations.CorrectName(preserveMalformations, " v_mag"), preserveMalformations ? value.Malformations : null, value.v_mag);
            WriteFloats(value.Malformations.CorrectName(preserveMalformations, " v_mag_inv"), preserveMalformations ? value.Malformations : null, value.v_mag_inv);
            WriteFloats(value.Malformations.CorrectName(preserveMalformations, " I"), preserveMalformations ? value.Malformations : null, value.I);
            WriteFloats(value.Malformations.CorrectName(preserveMalformations, " k_i"), preserveMalformations ? value.Malformations : null, value.I_inv);
            WriteVector3Ds(value.Malformations.CorrectName(preserveMalformations, " v"    ), preserveMalformations, value.v    );
            WriteVector3Ds(value.Malformations.CorrectName(preserveMalformations, " omega"), preserveMalformations, value.omega);
            WriteVector3Ds(value.Malformations.CorrectName(preserveMalformations, " Accel"), preserveMalformations, value.Accel);
            TokenIndex++;
        }

        [Obsolete]
        public void WriteMat3Ds(string name, bool preserveMalformations, params Matrix[] values)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_MAT3D);
                InternalWriteBinarySize(values.Length * sizeof(float) * 16);
                foreach (Matrix value in values)
                {
                    foreach (float x in new float[] {
                        value.rightx, value.righty, value.rightz, value.rightw,
                        value.upx   , value.upy   , value.upz   , value.upw   ,
                        value.frontx, value.fronty, value.frontz, value.frontw,
                        (float)value.positx, (float)value.posity, (float)value.positz, (float)value.positw,
                    })
                    {
                        byte[] bytes = BitConverter.GetBytes(x);
                        if (IsBigEndian)
                            Array.Reverse(bytes);
                        BaseStream.Write(bytes);
                    }
                }
                InternalAlignBinary();
                TokenIndex++;
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} [{(values.Length)}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  right.x")} [1] =")); InternalWriteNewline(); InternalWriteFloatValue("  right.x", values[i].rightx, preserveMalformations, FloatFormat, values[i].Malformations); InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  right.y")} [1] =")); InternalWriteNewline(); InternalWriteFloatValue("  right.x", values[i].righty, preserveMalformations, FloatFormat, values[i].Malformations); InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  right.z")} [1] =")); InternalWriteNewline(); InternalWriteFloatValue("  right.x", values[i].rightz, preserveMalformations, FloatFormat, values[i].Malformations); InternalWriteNewline();

                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  up.x")} [1] =")); InternalWriteNewline(); InternalWriteFloatValue("  up.x", values[i].upx, preserveMalformations, FloatFormat, values[i].Malformations); InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  up.y")} [1] =")); InternalWriteNewline(); InternalWriteFloatValue("  up.x", values[i].upy, preserveMalformations, FloatFormat, values[i].Malformations); InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  up.z")} [1] =")); InternalWriteNewline(); InternalWriteFloatValue("  up.x", values[i].upz, preserveMalformations, FloatFormat, values[i].Malformations); InternalWriteNewline();

                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  front.x")} [1] =")); InternalWriteNewline(); InternalWriteFloatValue("  front.x", values[i].frontx, preserveMalformations, FloatFormat, values[i].Malformations); InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  front.y")} [1] =")); InternalWriteNewline(); InternalWriteFloatValue("  front.x", values[i].fronty, preserveMalformations, FloatFormat, values[i].Malformations); InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  front.z")} [1] =")); InternalWriteNewline(); InternalWriteFloatValue("  front.x", values[i].frontz, preserveMalformations, FloatFormat, values[i].Malformations); InternalWriteNewline();

                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  posit.x")} [1] =")); InternalWriteNewline(); InternalWriteFloatValue("  posit.x", (float)values[i].positx, preserveMalformations, FloatFormat, values[i].Malformations); InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  posit.y")} [1] =")); InternalWriteNewline(); InternalWriteFloatValue("  posit.y", (float)values[i].posity, preserveMalformations, FloatFormat, values[i].Malformations); InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{values[i].Malformations.CorrectName(preserveMalformations, "  posit.z")} [1] =")); InternalWriteNewline(); InternalWriteFloatValue("  posit.z", (float)values[i].positz, preserveMalformations, FloatFormat, values[i].Malformations); InternalWriteNewline();
            }
            TokenIndex++;
        }

        [Obsolete]
        public void WriteMat3DOlds(string name, bool preserveMalformations, params Matrix[] values)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_MAT3DOLD);
                InternalWriteBinarySize(values.Length * sizeof(float) * 12);
                foreach (Matrix value in values)
                {
                    foreach (float x in new float[] {
                        value.rightx, value.righty, value.rightz,
                        value.upx   , value.upy   , value.upz   ,
                        value.frontx, value.fronty, value.frontz,
                        (float)value.positx, (float)value.posity, (float)value.positz,
                    })
                    {
                        byte[] bytes = BitConverter.GetBytes(x);
                        if (IsBigEndian)
                            Array.Reverse(bytes);
                        BaseStream.Write(bytes);
                    }
                }
                InternalAlignBinary();
                TokenIndex++;
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} [{(values.Length)}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"  right_x [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  right_x", values[i].rightx, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"  right_y [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  right_y", values[i].righty, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"  right_z [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  right_z", values[i].rightz, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();

                BaseStream.Write(BZNEncoding.win1252.GetBytes($"  up_x [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  up_x", values[i].upx, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"  up_y [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  up_y", values[i].upy, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"  up_z [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  up_z", values[i].upz, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();

                BaseStream.Write(BZNEncoding.win1252.GetBytes($"  front_x [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  front_x", values[i].frontx, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"  front_y [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  front_y", values[i].fronty, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"  front_z [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  front_z", values[i].frontz, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();

                BaseStream.Write(BZNEncoding.win1252.GetBytes($"  posit_x [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  posit_x", (float)values[i].positx, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"  posit_y [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  posit_y", (float)values[i].posity, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"  posit_z [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  posit_z", (float)values[i].positz, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();
            }
            TokenIndex++;
        }

        [Obsolete]
        public void WriteMat3DOldEnhanceds(string name, bool preserveMalformations, params Matrix[] values)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_MAT3DOLD);
                InternalWriteBinarySize(values.Length * sizeof(float) * 9 + 4 + sizeof(double) * 3);
                foreach (Matrix value in values)
                {
                    foreach (float x in new float[] {
                        value.rightx, value.righty, value.rightz,
                        value.upx   , value.upy   , value.upz   ,
                        value.frontx, value.fronty, value.frontz,
                    })
                    {
                        byte[] bytes = BitConverter.GetBytes(x);
                        if (IsBigEndian)
                            Array.Reverse(bytes);
                        BaseStream.Write(bytes);
                    }
                    {
                        byte[] bytes = BitConverter.GetBytes(value.junk);
                        if (IsBigEndian)
                            Array.Reverse(bytes);
                        BaseStream.Write(bytes);
                    }
                    foreach (double x in new double[] {
                        value.positx, value.posity, value.positz,
                    })
                    {
                        byte[] bytes = BitConverter.GetBytes(x);
                        if (IsBigEndian)
                            Array.Reverse(bytes);
                        BaseStream.Write(bytes);
                    }
                }
                InternalAlignBinary();
                TokenIndex++;
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} [{(values.Length)}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"  right_x [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  right_x", values[i].rightx, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"  right_y [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  right_y", values[i].righty, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"  right_z [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  right_z", values[i].rightz, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();

                BaseStream.Write(BZNEncoding.win1252.GetBytes($"  up_x [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  up_x", values[i].upx, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"  up_y [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  up_y", values[i].upy, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"  up_z [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  up_z", values[i].upz, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();

                BaseStream.Write(BZNEncoding.win1252.GetBytes($"  front_x [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  front_x", values[i].frontx, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"  front_y [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  front_y", values[i].fronty, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"  front_z [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  front_z", values[i].frontz, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();

                // TODO change these to double strings, whatever that looks like
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"  posit_x [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  posit_x", (float)values[i].positx, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"  posit_y [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  posit_y", (float)values[i].posity, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"  posit_z [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue("  posit_z", (float)values[i].positz, preserveMalformations, FloatFormat, values[i].Malformations);
                InternalWriteNewline();
            }
            TokenIndex++;
        }

        [Obsolete]
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
                TokenIndex++;
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} [{values.Length}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++) {
                BaseStream.Write(BZNEncoding.win1252.GetBytes(values[i].ToString()));
                InternalWriteNewline();
            }
            TokenIndex++;
        }

        [Obsolete]
        public void WriteFloats(string name, IMalformable.MalformationManager? malformations, params float[] values)
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
                TokenIndex++;
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} [{values.Length}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                //BaseStream.Write(BZNEncoding.win1252.GetBytes(values[i].ToBZNString(FloatFormat)));
                InternalWriteFloatValue(name, values[i], malformations != null, FloatFormat, malformations);
                InternalWriteNewline();
            }
            TokenIndex++;
        }

        [Obsolete]
        public void WriteIDsBZ1(string name, UInt64 value, IMalformable.MalformationManager malformations)
        {
            byte[] data = BitConverter.GetBytes(value);
            if (malformations != null)
            {
//                var mal = malformations.GetMalformations(Malformation.INCORRECT_RAW, "param");
//                if (mal.Length > 0)
//                {
//                    data = (byte[])mal[0].Fields[0];
//                }
            }

            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_ID);
                InternalWriteBinarySize(data.Length);
                if (IsBigEndian)
                    Array.Reverse(data);
                BaseStream.Write(data);
                InternalAlignBinary();
                TokenIndex++;
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} [1] ="));
            InternalWriteNewline();
            if (value != 0)
            {
                data = data.Reverse().SkipWhile(b => b == 0).Reverse().ToArray();
                BaseStream.Write(data);
            }
            InternalWriteNewline();
            TokenIndex++;
        }

        [Obsolete]
        public void WriteIDsBZ1(string name, byte[] value)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_ID);
                InternalWriteBinarySize(value.Length);
                BaseStream.Write(value);
                InternalAlignBinary();
                TokenIndex++;
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} [1] ="));
            InternalWriteNewline();
            BaseStream.Write(value);
            InternalWriteNewline();
            TokenIndex++;
        }


        /// <summary>
        /// UInt8 Processed Write
        /// </summary>
        /// <param name="name"></param>
        /// <param name="values"></param>
        [Obsolete]
        public void WriteUnsignedValues(string name, params byte[] values)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_CHAR);
                InternalWriteBinarySize(values.Length);
                BaseStream.Write(values);
                InternalAlignBinary();
                TokenIndex++;
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} [{values.Length}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(BZNEncoding.win1252.GetBytes(values[i].ToString()));
                InternalWriteNewline();
            }
            TokenIndex++;
        }

        /// <summary>
        /// UInt8 Raw write
        /// </summary>
        /// <remarks>
        /// This is for when ASCII BZNs contains the raw bytes
        /// </remarks>
        /// <param name="name"></param>
        /// <param name="values"></param>
        [Obsolete]
        public void WriteUnsignedRawValues(string name, params byte[] values)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_CHAR);
                InternalWriteBinarySize(values.Length);
                BaseStream.Write(values);
                InternalAlignBinary();
                TokenIndex++;
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} [{values.Length}] ="));
            InternalWriteNewline();
            for (int i = 0; i < values.Length; i++)
            {
                BaseStream.Write(new byte[] { values[i] }, 0, 1); // can be written more efficently but written this way for now for when we copypasta later
                InternalWriteNewline();
            }
            TokenIndex++;
        }

        [Obsolete]
        public void WriteChars(string name, string value, IMalformable.MalformationManager malformations)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_CHAR);
                byte[] stringBytes = malformations != null ? BZNEncoding.win1252.GetBytes(malformations.CheckBinaryMessString(name, value)) : BZNEncoding.win1252.GetBytes(value);
                InternalWriteBinarySize(stringBytes.Length);
                BaseStream.Write(stringBytes);
                InternalAlignBinary();
                TokenIndex++;
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} ="));

//            if (malformations != null && malformations.GetMalformations(Malformation.RIGHT_TRIM, name).Any())
//            {
//                // missing space after the =, so just do nothing
//            }
//            else
            {
                BaseStream.Write(BZNEncoding.win1252.GetBytes(" ")); // only have the trailing space if the value exists
            }

            InternalWriteStringValue(value);
            InternalWriteNewline();
            TokenIndex++;
        }

        [Obsolete]
        public void WritePtr32(string name, UInt32 value)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_PTR);
                InternalWriteBinarySize(sizeof(UInt32));
                byte[] bytes = BitConverter.GetBytes(value);
                if (IsBigEndian)
                    Array.Reverse(bytes);
                BaseStream.Write(bytes);
                InternalAlignBinary();
                TokenIndex++;
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} = "));
            //InternalWriteStringDirectValue(value.ToString("X8"));
            BaseStream.Write(BZNEncoding.win1252.GetBytes(value.ToString("X8")));
            InternalWriteNewline();
            TokenIndex++;
        }

        [Obsolete]
        public void WritePtr64(string name, UInt64 value)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_PTR);
                InternalWriteBinarySize(sizeof(UInt64));
                byte[] bytes = BitConverter.GetBytes(value);
                if (IsBigEndian)
                    Array.Reverse(bytes);
                BaseStream.Write(bytes);
                InternalAlignBinary();
                TokenIndex++;
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} = "));
            //InternalWriteStringDirectValue(value.ToString("X16"));
            if (value > 0x100000000)
            {
                BaseStream.Write(BZNEncoding.win1252.GetBytes(value.ToString("X16")));
            }
            else
            {
                BaseStream.Write(BZNEncoding.win1252.GetBytes(((UInt32)value).ToString("X8")));
            }
            InternalWriteNewline();
            TokenIndex++;
        }

        // this might be bogus, will know later
        [Obsolete]
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
                TokenIndex++;
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} [{values.Length}] = "));
            InternalWriteNewline();
            foreach (uint value in values)
            {
                //InternalWriteStringValue(value.ToString("X8"));
                BaseStream.Write(BZNEncoding.win1252.GetBytes(value.ToString("X8")));
                InternalWriteNewline();
            }
            TokenIndex++;
        }
        [Obsolete]
        public void WriteVoidBytesL(string name, UInt32 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (IsBigEndian)
                Array.Reverse(bytes);
            WriteVoidBytesL(name, bytes);
        }
        [Obsolete]
        public void WriteVoidBytes(string name, UInt32 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (IsBigEndian)
                Array.Reverse(bytes);
            WriteVoidBytes(name, bytes);
        }
        [Obsolete]
        public void WriteVoidBytesL(string name, byte[] value)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_VOID);
                InternalWriteBinarySize(value.Length);
                BaseStream.Write(value);
                InternalAlignBinary();
                TokenIndex++;
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} = "));
            //InternalWriteStringValue(BitConverter.ToString(value).Replace("-", string.Empty).ToLowerInvariant());
            BaseStream.Write(BZNEncoding.win1252.GetBytes(BitConverter.ToString(value).Replace("-", string.Empty).ToLowerInvariant()));
            InternalWriteNewline();
            TokenIndex++;
        }
        [Obsolete]
        public void WriteVoidBytes(string name, byte[] value)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_VOID);
                InternalWriteBinarySize(value.Length);
                BaseStream.Write(value);
                InternalAlignBinary();
                TokenIndex++;
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} = "));
            //InternalWriteStringValue(BitConverter.ToString(value).Replace("-", string.Empty));
            BaseStream.Write(BZNEncoding.win1252.GetBytes(BitConverter.ToString(value).Replace("-", string.Empty)));
            InternalWriteNewline();
            TokenIndex++;
        }

        [Obsolete]
        public void WriteVoidBytesRaw(string name, byte[] value)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_VOID);
                InternalWriteBinarySize(value.Length);
                BaseStream.Write(value);
                InternalAlignBinary();
                TokenIndex++;
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} = "));
            BaseStream.Write(value);
            InternalWriteNewline();
            TokenIndex++;
        }

        public void WriteValidation(string name)
        {
            if (InBinary)
                return;

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"[{name}]"));
            InternalWriteNewline();
            TokenIndex++;
        }

        private void InternalWriteBinaryType(BinaryFieldType type)
        {
            // todo Type size is only 1 byte no matter what here, put a breadcrumb so we can find all the places this is true just in case
            if (TypeSize > 0)
            {
                byte[] number = new byte[TypeSize];
                number[0] = (byte)type;

                if (StreamDefects != null)
                {
                    if (StreamDefects.ContainsKey(TokenIndex))
                    {
                        StreamDefect defect = StreamDefects[TokenIndex];
                        if (TypeSize > 1 && defect.TypeGarbage.HasValue)
                        {
                            // type size is big enough to have defects and we have one
                            if ((defect.TypeGarbage.Value & 0xff) == number[0])
                            {
                                // defected type is the same as the original, so lets apply it
                                number = BitConverter.GetBytes(defect.TypeGarbage.Value).Take(TypeSize).ToArray();
                            }
                            else
                            {
                                // probably broke something
                            }
                        }
                    }
                }

                if (IsBigEndian)
                    number.Reverse();
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

        /// <summary>
        /// For when a string is stored as a string, so if quotes are active we need quotes
        /// </summary>
        /// <param name="value"></param>
        private void InternalWriteStringValue(string value)
        {
            if (QuoteStrings)// || value.Contains(' ') || value.Contains('\t'))
            {
                // Escape quotes in the string
                string escapedValue = value.Replace("\"", "\\\"");
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"\"{escapedValue}\""));
            }
            else
            {
                BaseStream.Write(BZNEncoding.win1252.GetBytes(value));
            }
        }

        private void InternalWriteNewline()
        {
            // TODO deal with newline malformation here
            byte[] newline = BZNEncoding.win1252.GetBytes(NewLine);
            BaseStream.Write(newline);
        }
    }
}