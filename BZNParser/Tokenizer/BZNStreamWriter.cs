using BZNParser.Battlezone;
using System;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static BZNParser.Tokenizer.BZNStreamReader;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Int8 = sbyte;
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
        public byte PointerSize { get; private set; }

        public Dictionary<int, StreamDefect>? StreamDefects { get; set; }
        public int TokenIndex { get; private set; }
        public string NewLine { get; set; }
        public bool PreserveMalformations { get; set; }

        public BZNStreamWriter(Stream stream, BZNFormat format, int version, bool preserveMalformations = false, Dictionary<int, StreamDefect>? preserveDefects = null)
        {
            BaseStream = stream;
            Format = format;
            Version = version;
            FloatFormat = FloatTextFormat.G;
            if (preserveDefects != null && preserveDefects.Count > 0)
                StreamDefects = preserveDefects;
            PreserveMalformations = preserveMalformations;
            TokenIndex = 0;
            PointerSize = 4;

            NewLine = "\r\n";

            switch (format)
            {
                case BZNFormat.Battlezone:
                    TypeSize = 2;
                    SizeSize = 2;
                    AlignmentBytes = 0;
                    IsBigEndian = false;
                    if (Version >= 2012)
                        PointerSize = 8;
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

        public (Int32 written, Int32 stored) WriteLength<T, TProp>(string name, T parent, Expression<Func<T, TProp>> property, Func<TProp, Int32>? convert = null) where T : IMalformable
        {
            if (!InBinary && name == null)
                throw new InvalidOperationException("Cannot write a text token with a null name");

            TProp valueInternal = ExtractPropertyValue(parent, property);
            Int32 value = 0;

            if (convert != null)
            {
                value = convert(valueInternal);
            }
            else if (typeof(TProp).IsArray && valueInternal is Array array)
            {
                value = array.Length;
            }
            else if (typeof(TProp).IsGenericType
                && typeof(TProp).GetGenericTypeDefinition() == typeof(MalformableArray<,>)
                && typeof(TProp).GetGenericArguments()[1] == typeof(Vector3D))
            {
                dynamic arr = (dynamic)(object)valueInternal!;
                value = arr!.ToArray();
            }
            else if (valueInternal is System.Collections.IEnumerable enumerable)
            {
                value = enumerable.Cast<object>().Count();
            }
            else
            {
                throw new Exception("Property type is not compatible with Length writing and no conversion provided");
            }

            int valueOriginal = value;

            if (PreserveMalformations)
            {
                (bool hasIncorrectLength, int? incorrectLength) = parent.Malformations.GetIncorrectLength(property);
                if (hasIncorrectLength && incorrectLength.HasValue)
                    value = incorrectLength.Value;
            }

            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_LONG);
                InternalWriteBinarySize(4);
                byte[] buff = BitConverter.GetBytes(value);
                if (IsBigEndian)
                    Array.Reverse(buff);
                BaseStream.Write(buff, 0, 4);
                InternalAlignBinary();
                TokenIndex++;
                return (value, valueOriginal);
            }

            string textValue = value.ToString();

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(name!, parent, property)} [1] ="));
            InternalWriteNewline();
            BaseStream.Write(BZNEncoding.win1252.GetBytes(textValue));
            InternalWriteNewline();
            TokenIndex++;
            return (value, valueOriginal);
        }

        /// <summary>
        /// Write a Single to the BZN
        /// </summary>
        /// <remarks>
        /// Handles the following malformations: <see cref="Malformation.INCORRECT_TEXT"/>, <see cref="Malformation.INCORRECT_NAME"/>
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="property"></param>
        public (Single written, TProp stored) WriteSingle<T, TProp>(string? name, T parent, Expression<Func<T, TProp>> property, Func<TProp, Single>? convert = null) where T : IMalformable
        {
            if (!InBinary && name == null)
                throw new InvalidOperationException("Cannot write a text token with a null name");

            TProp valueInternal = ExtractPropertyValue(parent, property);
            Single value = 0;

            if (convert != null)
            {
                value = convert(valueInternal);
            }
            else if (typeof(TProp) == typeof(Single) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(Single))
            {
                value = (Single)(Single)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(Double) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(Double))
            {
                value = (Single)(Double)(object)valueInternal!;
            }
            else if (typeof(TProp).IsGenericType && typeof(TProp).GetGenericTypeDefinition() == typeof(DualModeValue<,>))
            {
                var genericArgs = typeof(TProp).GetGenericArguments();
                if (genericArgs[0] == typeof(Single) || genericArgs[1] == typeof(Single))
                {
                    // Use dynamic to call Get<Single>() on the DualModeValue instance
                    dynamic dual = valueInternal!;
                    value = dual.Get<Single>();
                }
                else
                {
                    throw new Exception("DualModeValue does not contain a Single type and no conversion provided");
                }
            }
            else
            {
                throw new Exception("Property type is not compatible with Single writing and no conversion provided");
            }

            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_FLOAT);
                InternalWriteBinarySize(4);
                InternalWriteFloatValue(parent, property);
                InternalAlignBinary();
                TokenIndex++;
                return (value, valueInternal);
            }

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(name!, parent, property)} [1] ="));
            InternalWriteNewline();
            InternalWriteFloatValue(parent, property);
            InternalWriteNewline();
            TokenIndex++;
            return (value, valueInternal);
        }

        /// <summary>
        /// Write a UInt32 to the BZN
        /// </summary>
        /// <remarks>
        /// Handles the following malformations: <see cref="Malformation.INCORRECT_TEXT"/>, <see cref="Malformation.INCORRECT_NAME"/>
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="property"></param>
        public (Int32 written, TProp stored) WriteInt32<T, TProp>(string name, T parent, Expression<Func<T, TProp>> property, Func<TProp, Int32>? convert = null) where T : IMalformable
        {
            TProp valueInternal = ExtractPropertyValue(parent, property);
            Int32 value = 0;

            if (convert != null)
            {
                value = convert(valueInternal);
            }
            else if (typeof(TProp) == typeof(Int8) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(Int8))
            {
                value = (Int32)(Int8)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(Int16) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(Int16))
            {
                value = (Int32)(Int16)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(Int32) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(Int32))
            {
                value = (Int32)(Int32)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(Int64) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(Int64))
            {
                value = (Int32)(Int64)(object)valueInternal!;
            }
            else if (typeof(TProp).IsGenericType && typeof(TProp).GetGenericTypeDefinition() == typeof(DualModeValue<,>))
            {
                var genericArgs = typeof(TProp).GetGenericArguments();
                if (genericArgs[0] == typeof(Int32) || genericArgs[1] == typeof(Int32))
                {
                    if (valueInternal == null)
                    {
                        value = default;
                    }
                    else
                    {
                        dynamic dual = valueInternal;
                        value = dual.Get<Int32>();
                    }
                }
                else
                {
                    throw new Exception("DualModeValue does not contain a Single type and no conversion provided");
                }
            }
            else
            {
                throw new Exception("Property type is not compatible with Single writing and no conversion provided");
            }

            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_LONG);
                InternalWriteBinarySize(4);
                byte[] buff = BitConverter.GetBytes(value);
                if (IsBigEndian)
                    Array.Reverse(buff);
                BaseStream.Write(buff, 0, 4);
                InternalAlignBinary();
                TokenIndex++;
                return (value, valueInternal);
            }

            string textValue = value.ToString();

            if (PreserveMalformations)
            {
                // handle incorrect raw value
                (bool hasIncorrectRaw, string? incorrectText) = parent.Malformations.GetIncorrectTextParse(property);
                if (hasIncorrectRaw)
                    textValue = incorrectText ?? string.Empty;
            }

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(name, parent, property)} [1] ="));
            InternalWriteNewline();
            BaseStream.Write(BZNEncoding.win1252.GetBytes(textValue));
            InternalWriteNewline();
            TokenIndex++;

            return (value, valueInternal);
        }


        public (UInt32 written, TProp stored) WriteUInt32h<T, TProp>(string name, T parent, Expression<Func<T, TProp>> property, Func<TProp, UInt32>? convert = null) where T : IMalformable
        {
            TProp valueInternal = ExtractPropertyValue(parent, property);

            // Handle array or nullable array case
            if (typeof(TProp).IsArray && typeof(TProp).GetElementType() == typeof(UInt32))
            {
                var arr = (UInt32[]?)(object?)valueInternal;
                int length = arr?.Length ?? 0;

                if (InBinary)
                {
                    InternalWriteBinaryType(BinaryFieldType.DATA_LONG);
                    InternalWriteBinarySize(4 * length);
                    if (arr != null)
                    {
                        foreach (var v in arr)
                        {
                            byte[] buff = BitConverter.GetBytes(v);
                            if (IsBigEndian)
                                Array.Reverse(buff);
                            BaseStream.Write(buff, 0, 4);
                        }
                    }
                    InternalAlignBinary();
                    TokenIndex++;
                    return (arr != null && arr.Length > 0 ? arr[0] : 0, valueInternal);
                }

                // Text mode
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(name, parent, property)} [{length}] ="));
                InternalWriteNewline();
                if (arr != null)
                {
                    foreach (var v in arr)
                    {
                        string textValue = v.ToString("x");
                        BaseStream.Write(BZNEncoding.win1252.GetBytes(textValue));
                        InternalWriteNewline();
                    }
                }
                TokenIndex++;
                return (arr != null && arr.Length > 0 ? arr[0] : 0, valueInternal);
            }

            // Single value case (original logic)
            UInt32 value = 0;
            if (convert != null)
            {
                value = convert(valueInternal);
            }
            else if (typeof(TProp) == typeof(UInt8) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt8))
            {
                value = (UInt32)(UInt8)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(UInt16) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt16))
            {
                value = (UInt32)(UInt16)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(UInt32) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt32))
            {
                value = (UInt32)(UInt32)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(UInt64) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt64))
            {
                value = (UInt32)(UInt64)(object)valueInternal!;
            }
            else
            {
                throw new Exception("Property type is not compatible with boolean writing and no conversion provided");
            }

            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_LONG);
                InternalWriteBinarySize(4);
                byte[] buff = BitConverter.GetBytes(value);
                if (IsBigEndian)
                    Array.Reverse(buff);
                BaseStream.Write(buff, 0, 4);
                InternalAlignBinary();
                TokenIndex++;
                return (value, valueInternal);
            }

            string singleTextValue = value.ToString("x");

            if (PreserveMalformations)
            {
                // handle incorrect raw value
                (bool hasIncorrectRaw, string? incorrectText) = parent.Malformations.GetIncorrectTextParse(property);
                if (hasIncorrectRaw)
                    singleTextValue = incorrectText ?? string.Empty;
            }

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(name, parent, property)} [1] ="));
            InternalWriteNewline();
            BaseStream.Write(BZNEncoding.win1252.GetBytes(singleTextValue));
            InternalWriteNewline();
            TokenIndex++;

            return (value, valueInternal);
        }

        public (byte[] written, TProp stored) WriteVoidBytesRaw<T, TProp>(string name, T parent, Expression<Func<T, TProp>> property, Func<TProp, byte[]>? convert = null) where T : IMalformable
        {
            TProp valueInternal = ExtractPropertyValue(parent, property);
            byte[] value;

            if (convert != null)
            {
                value = convert(valueInternal);
            }
            else if (valueInternal is byte[] bytes)
            {
                value = bytes;
            }
            else if (valueInternal is UInt32[] u32arr)
            {
                value = MemoryMarshal.AsBytes(new Span<UInt32>(u32arr)).ToArray();
            }
            else if (valueInternal is Int32[] i32arr)
            {
                value = MemoryMarshal.AsBytes(new Span<Int32>(i32arr)).ToArray();
            }
            else if (valueInternal is UInt16[] u16arr)
            {
                value = MemoryMarshal.AsBytes(new Span<UInt16>(u16arr)).ToArray();
            }
            else if (valueInternal is Int16[] i16arr)
            {
                value = MemoryMarshal.AsBytes(new Span<Int16>(i16arr)).ToArray();
            }
            else if (valueInternal is float[] farr)
            {
                value = MemoryMarshal.AsBytes(new Span<float>(farr)).ToArray();
            }
            else if (valueInternal is double[] darr)
            {
                value = MemoryMarshal.AsBytes(new Span<double>(darr)).ToArray();
            }
            else if (valueInternal is sbyte[] s8arr)
            {
                value = MemoryMarshal.AsBytes(new Span<sbyte>(s8arr)).ToArray();
            }
            else if (typeof(TProp).IsPrimitive || typeof(TProp).IsValueType)
            {
                if (valueInternal is byte v)
                {
                    value = new byte[] { v };
                }
                else if (valueInternal is UInt32 v1)
                {
                    value = BitConverter.GetBytes(v1);
                }
                else if (valueInternal is Int32 v2)
                {
                    value = BitConverter.GetBytes(v2);
                }
                else if (valueInternal is UInt16 v3)
                {
                    value = BitConverter.GetBytes(v3);
                }
                else if (valueInternal is Int16 v4)
                {
                    value = BitConverter.GetBytes(v4);
                }
                else if (valueInternal is float v5)
                {
                    value = BitConverter.GetBytes(v5);
                }
                else if (valueInternal is double v6)
                {
                    value = BitConverter.GetBytes(v6);
                }
                else if (valueInternal is sbyte sb)
                {
                    value = new byte[] { (byte)sb };
                }
                else
                {
                    throw new Exception("Property type is not compatible with byte array writing and no conversion provided");
                }
            }
            else
            {
                throw new Exception("Property type is not compatible with byte array writing and no conversion provided");
            }

            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_VOID);
                InternalWriteBinarySize(value.Length);
                BaseStream.Write(value, 0, value.Length);
                InternalAlignBinary();
                TokenIndex++;
                return (value, valueInternal);
            }

            //string textValue = BitConverter.ToString(value).Replace("-", string.Empty).ToUpperInvariant(); // replace this with nicer logic

            // if (PreserveMalformations) {
            //// handle incorrect raw value
            //(bool hasIncorrectRaw, string? incorrectText) = parent.Malformations.GetIncorrectTextParse(property);
            //if (hasIncorrectRaw)
            //    textValue = incorrectText ?? string.Empty;
            // }

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(name, parent, property)} = "));
            //BaseStream.Write(BZNEncoding.win1252.GetBytes(textValue));
            BaseStream.Write(value);
            InternalWriteNewline();
            TokenIndex++;

            return (value, valueInternal);
        }
        public (byte[] written, TProp stored) WriteVoidBytes<T, TProp>(string name, T parent, Expression<Func<T, TProp>> property, Func<TProp, byte[]>? convert = null) where T : IMalformable
        {
            TProp valueInternal = ExtractPropertyValue(parent, property);
            byte[] value;

            if (convert != null)
            {
                value = convert(valueInternal);
            }
            else if (valueInternal is byte[] bytes)
            {
                value = bytes;
            }
            else if (valueInternal is UInt32[] u32arr)
            {
                value = MemoryMarshal.AsBytes(new Span<UInt32>(u32arr)).ToArray();
            }
            else if (valueInternal is Int32[] i32arr)
            {
                value = MemoryMarshal.AsBytes(new Span<Int32>(i32arr)).ToArray();
            }
            else if (valueInternal is UInt16[] u16arr)
            {
                value = MemoryMarshal.AsBytes(new Span<UInt16>(u16arr)).ToArray();
            }
            else if (valueInternal is Int16[] i16arr)
            {
                value = MemoryMarshal.AsBytes(new Span<Int16>(i16arr)).ToArray();
            }
            else if (valueInternal is float[] farr)
            {
                value = MemoryMarshal.AsBytes(new Span<float>(farr)).ToArray();
            }
            else if (valueInternal is double[] darr)
            {
                value = MemoryMarshal.AsBytes(new Span<double>(darr)).ToArray();
            }
            else if (valueInternal is sbyte[] s8arr)
            {
                value = MemoryMarshal.AsBytes(new Span<sbyte>(s8arr)).ToArray();
            }
            else if (typeof(TProp).IsPrimitive || typeof(TProp).IsValueType)
            {
                if (valueInternal is byte v)
                {
                    value = new byte[] { v };
                }
                else if (valueInternal is UInt64 v0)
                {
                    value = BitConverter.GetBytes(v0);
                }
                else if (valueInternal is UInt32 v1)
                {
                    value = BitConverter.GetBytes(v1);
                }
                else if (valueInternal is Int32 v2)
                {
                    value = BitConverter.GetBytes(v2);
                }
                else if (valueInternal is UInt16 v3)
                {
                    value = BitConverter.GetBytes(v3);
                }
                else if (valueInternal is Int16 v4)
                {
                    value = BitConverter.GetBytes(v4);
                }
                else if (valueInternal is float v5)
                {
                    value = BitConverter.GetBytes(v5);
                }
                else if (valueInternal is double v6)
                {
                    value = BitConverter.GetBytes(v6);
                }
                else if (valueInternal is sbyte sb)
                {
                    value = new byte[] { (byte)sb };
                }
                else
                {
                    throw new Exception("Property type is not compatible with byte array writing and no conversion provided");
                }
            }
            else
            {
                throw new Exception("Property type is not compatible with byte array writing and no conversion provided");
            }

            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_VOID);
                InternalWriteBinarySize(value.Length);
                BaseStream.Write(value, 0, value.Length);
                InternalAlignBinary();
                TokenIndex++;
                return (value, valueInternal);
            }

            string textValue = BitConverter.ToString(value).Replace("-", string.Empty).ToUpperInvariant(); // replace this with nicer logic

            if (PreserveMalformations)
            {
                // handle incorrect raw value
                (bool hasIncorrectRaw, string? incorrectText) = parent.Malformations.GetIncorrectTextParse(property);
                if (hasIncorrectRaw)
                    textValue = incorrectText ?? string.Empty;
            }

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(name, parent, property)} = "));
            BaseStream.Write(BZNEncoding.win1252.GetBytes(textValue));
            InternalWriteNewline();
            TokenIndex++;

            return (value, valueInternal);
        }

        // This function might be replacable if it's only ever used to write raw bytes of UInt32s except in one specific version+game+binary case where it's one char
        public (byte[] written, TProp stored) WriteVoidBytesL<T, TProp>(string name, T parent, Expression<Func<T, TProp>> property, Func<TProp, byte[]>? convert = null) where T : IMalformable
        {
            TProp valueInternal = ExtractPropertyValue(parent, property);
            byte[] value;

            if (convert != null)
            {
                value = convert(valueInternal);
            }
            else if (valueInternal is byte[] bytes)
            {
                value = bytes;
            }
            else if (valueInternal is UInt32[] u32arr)
            {
                value = MemoryMarshal.AsBytes(new Span<UInt32>(u32arr)).ToArray();
                if (IsBigEndian)
                    for (int i = 0; i < value.Length; i += sizeof(UInt32))
                    {
                        byte[] tmp = new byte[sizeof(UInt32)];
                        Array.Copy(value, i, tmp, 0, sizeof(UInt32));
                        Array.Reverse(tmp);
                        Array.Copy(tmp, 0, value, i, sizeof(UInt32));
                    }
            }
            else if (valueInternal is Int32[] i32arr)
            {
                value = MemoryMarshal.AsBytes(new Span<Int32>(i32arr)).ToArray();
                if (IsBigEndian)
                    for (int i = 0; i < value.Length; i += sizeof(Int32))
                    {
                        byte[] tmp = new byte[sizeof(Int32)];
                        Array.Copy(value, i, tmp, 0, sizeof(Int32));
                        Array.Reverse(tmp);
                        Array.Copy(tmp, 0, value, i, sizeof(Int32));
                    }
            }
            else if (valueInternal is UInt16[] u16arr)
            {
                value = MemoryMarshal.AsBytes(new Span<UInt16>(u16arr)).ToArray();
                if (IsBigEndian)
                    for (int i = 0; i < value.Length; i += sizeof(UInt16))
                    {
                        byte[] tmp = new byte[sizeof(UInt16)];
                        Array.Copy(value, i, tmp, 0, sizeof(UInt16));
                        Array.Reverse(tmp);
                        Array.Copy(tmp, 0, value, i, sizeof(UInt16));
                    }
            }
            else if (valueInternal is Int16[] i16arr)
            {
                value = MemoryMarshal.AsBytes(new Span<Int16>(i16arr)).ToArray();
                if (IsBigEndian)
                    for (int i = 0; i < value.Length; i += sizeof(Int16))
                    {
                        byte[] tmp = new byte[sizeof(Int16)];
                        Array.Copy(value, i, tmp, 0, sizeof(Int16));
                        Array.Reverse(tmp);
                        Array.Copy(tmp, 0, value, i, sizeof(Int16));
                    }
            }
            else if (valueInternal is float[] farr)
            {
                value = MemoryMarshal.AsBytes(new Span<float>(farr)).ToArray();
                if (IsBigEndian)
                    for (int i = 0; i < value.Length; i += sizeof(float))
                    {
                        byte[] tmp = new byte[sizeof(float)];
                        Array.Copy(value, i, tmp, 0, sizeof(float));
                        Array.Reverse(tmp);
                        Array.Copy(tmp, 0, value, i, sizeof(float));
                    }
            }
            else if (valueInternal is double[] darr)
            {
                value = MemoryMarshal.AsBytes(new Span<double>(darr)).ToArray();
                if (IsBigEndian)
                    for (int i = 0; i < value.Length; i += sizeof(double))
                    {
                        byte[] tmp = new byte[sizeof(double)];
                        Array.Copy(value, i, tmp, 0, sizeof(double));
                        Array.Reverse(tmp);
                        Array.Copy(tmp, 0, value, i, sizeof(double));
                    }
            }
            else if (valueInternal is sbyte[] s8arr)
            {
                value = MemoryMarshal.AsBytes(new Span<sbyte>(s8arr)).ToArray();
            }
            else if (typeof(TProp).IsPrimitive || typeof(TProp).IsValueType)
            {
                if (valueInternal is byte v)
                {
                    value = new byte[] { v };
                }
                else if (valueInternal is UInt32 v1)
                {
                    value = BitConverter.GetBytes(v1);
                    if (IsBigEndian)
                        value.Reverse();
                }
                else if (valueInternal is Int32 v2)
                {
                    value = BitConverter.GetBytes(v2);
                    if (IsBigEndian)
                        value.Reverse();
                }
                else if (valueInternal is UInt16 v3)
                {
                    value = BitConverter.GetBytes(v3);
                    if (IsBigEndian)
                        value.Reverse();
                }
                else if (valueInternal is Int16 v4)
                {
                    value = BitConverter.GetBytes(v4);
                    if (IsBigEndian)
                        value.Reverse();
                }
                else if (valueInternal is float v5)
                {
                    value = BitConverter.GetBytes(v5);
                    if (IsBigEndian)
                        value.Reverse();
                }
                else if (valueInternal is double v6)
                {
                    value = BitConverter.GetBytes(v6);
                    if (IsBigEndian)
                        value.Reverse();
                }
                else if (valueInternal is sbyte sb)
                {
                    value = new byte[] { (byte)sb };
                }
                else
                {
                    throw new Exception("Property type is not compatible with byte array writing and no conversion provided");
                }
            }
            else
            {
                throw new Exception("Property type is not compatible with byte array writing and no conversion provided");
            }

            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_VOID);
                InternalWriteBinarySize(value.Length);
                BaseStream.Write(value, 0, value.Length);
                InternalAlignBinary();
                TokenIndex++;
                return (value, valueInternal);
            }

            string textValue = BitConverter.ToString(value).Replace("-", string.Empty).ToLowerInvariant(); // replace this with nicer logic

            // handle incorrect caseing
            if (PreserveMalformations)
            {
                (bool hasIncorrectCase, char? incorrectCase) = parent.Malformations.GetIncorrectCase(property);
                if (hasIncorrectCase && incorrectCase != null)
                    switch (incorrectCase.Value)
                    {
                        case 'U': textValue = textValue.ToUpperInvariant(); break;
                        case 'L': textValue = textValue.ToLowerInvariant(); break;
                    }

                // handle incorrect raw value
                (bool hasIncorrectRaw, string? incorrectText) = parent.Malformations.GetIncorrectTextParse(property);
                if (hasIncorrectRaw)
                    textValue = incorrectText ?? string.Empty;
            }

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(name, parent, property)} = "));
            BaseStream.Write(BZNEncoding.win1252.GetBytes(textValue));
            InternalWriteNewline();
            TokenIndex++;

            return (value, valueInternal);
        }


        // Except for BZ98R this is a 32bit pointer, but for the common API we need to output a 64bit value.
        // always single-line
        public (UInt64 written, TProp stored) WritePtr<T, TProp>(string name, T parent, Expression<Func<T, TProp>> property, Func<TProp, UInt64>? convert = null) where T : IMalformable
        {
            TProp valueInternal = ExtractPropertyValue(parent, property);
            UInt64 value = 0;
            
            if (convert != null)
            {
                value = convert(valueInternal);
            }
            else if (typeof(TProp) == typeof(UInt8) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt8))
            {
                value = (UInt64)(UInt8)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(UInt16) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt16))
            {
                value = (UInt64)(UInt16)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(UInt32) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt32))
            {
                value = (UInt64)(UInt32)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(UInt64) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt64))
            {
                value = (UInt64)(UInt64)(object)valueInternal!;
            }
            else
            {
                throw new Exception("Property type is not compatible with boolean writing and no conversion provided");
            }

            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_PTR);
                InternalWriteBinarySize(PointerSize);
                byte[] buff = BitConverter.GetBytes(value).Take(PointerSize).ToArray(); // truncate to correct size
                    if (IsBigEndian)
                        Array.Reverse(buff);
                    BaseStream.Write(buff, 0, PointerSize);
                InternalAlignBinary();
                TokenIndex++;
                return (value, valueInternal);
            }

            string textValue = value.ToString("X8");
            if (value > 0xFFFFFFFF)
                textValue = value.ToString("X16");

            if (PreserveMalformations)
            {
                // handle incorrect raw value
                (bool hasIncorrectRaw, string? incorrectText) = parent.Malformations.GetIncorrectTextParse(property);
                if (hasIncorrectRaw)
                    textValue = incorrectText ?? string.Empty;
            }

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(name, parent, property)} = "));
            BaseStream.Write(BZNEncoding.win1252.GetBytes(textValue));
            InternalWriteNewline();
            TokenIndex++;

            return (value, valueInternal);
        }

        // always multi-line
        public (UInt64 written, TProp stored) WritePtrs<T, TProp>(string? name, T parent, Expression<Func<T, TProp>> property, Func<TProp, UInt32>? convert = null) where T : IMalformable
        {
            if (!InBinary && name == null)
                throw new InvalidOperationException("Cannot write a text token with a null name");

            TProp valueInternal = ExtractPropertyValue(parent, property);

            UInt64[] values;
            if (typeof(TProp).IsArray && typeof(TProp).GetElementType() != null)
            {
                var arr = (Array)(object)valueInternal!;
                int arrLen = arr.Length;
                values = new UInt64[arrLen];
                for (int i = 0; i < arrLen; i++)
                {
                    object? element = arr.GetValue(i);
                    if (convert != null)
                    {
                        values[i] = convert((TProp)element!);
                    }
                    else if (element is UInt8 u8)
                    {
                        values[i] = u8;
                    }
                    else if (element is UInt16 u16)
                    {
                        values[i] = u16;
                    }
                    else if (element is UInt32 u32)
                    {
                        values[i] = u32;
                    }
                    else if (element is UInt64 u64)
                    {
                        values[i] = u64;
                    }
                    else
                    {
                        throw new Exception("Property type is not compatible with pointer array writing and no conversion provided");
                    }
                }
            }
            else
            {
                UInt64 value = 0;
                if (convert != null)
                {
                    value = convert(valueInternal);
                }
                else if (typeof(TProp) == typeof(UInt8) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt8))
                {
                    value = (UInt64)(UInt8)(object)valueInternal!;
                }
                else if (typeof(TProp) == typeof(UInt16) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt16))
                {
                    value = (UInt64)(UInt16)(object)valueInternal!;
                }
                else if (typeof(TProp) == typeof(UInt32) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt32))
                {
                    value = (UInt64)(UInt32)(object)valueInternal!;
                }
                else if (typeof(TProp) == typeof(UInt64) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt64))
                {
                    value = (UInt64)(UInt64)(object)valueInternal!;
                }
                else
                {
                    throw new Exception("Property type is not compatible with boolean writing and no conversion provided");
                }
                values = new[] { value };
            }

            int length = values.Length;

            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_PTR);
                InternalWriteBinarySize(PointerSize * length);
                foreach (var value in values)
                {
                    byte[] buff = BitConverter.GetBytes(value).Take(PointerSize).ToArray();
                    if (IsBigEndian)
                        Array.Reverse(buff);
                    BaseStream.Write(buff, 0, PointerSize);
                }
                InternalAlignBinary();
                TokenIndex++;
                return (values.Length > 0 ? values[0] : 0, valueInternal);
            }

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(name, parent, property)} = "));
            for (int i = 0; i < length; i++)
            {
                string textValue = values[i].ToString("X8");
                if (values[i] > 0xFFFFFFFF)
                    textValue = values[i].ToString("X16");

                if (PreserveMalformations)
                {
                    // handle incorrect raw value
                    (bool hasIncorrectRaw, string? incorrectText) = parent.Malformations.GetIncorrectTextParse(property, i);
                    if (hasIncorrectRaw)
                        textValue = incorrectText ?? string.Empty;
                }
                BaseStream.Write(BZNEncoding.win1252.GetBytes(textValue));
                InternalWriteNewline();
            }
            TokenIndex++;

            return (values.Length > 0 ? values[0] : 0, valueInternal);
        }

        /// <summary>
        /// Write a UInt32 to the BZN
        /// </summary>
        /// <remarks>
        /// Handles the following malformations: <see cref="Malformation.INCORRECT_TEXT"/>, <see cref="Malformation.INCORRECT_NAME"/>
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="property"></param>
        public (UInt32 written, TProp stored) WriteUInt32<T, TProp>(string? name, T parent, Expression<Func<T, TProp>> property, Func<TProp, UInt32>? convert = null) where T : IMalformable
        {
            if (!InBinary && name == null)
                throw new InvalidOperationException("Cannot write a text token with a null name");

            TProp valueInternal = ExtractPropertyValue(parent, property);
            UInt32 value = 0;

            if (convert != null)
            {
                value = convert(valueInternal);
            }
            else if (typeof(TProp) == typeof(UInt8) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt8))
            {
                value = (UInt32)(UInt8)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(UInt16) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt16))
            {
                value = (UInt32)(UInt16)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(UInt32) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt32))
            {
                value = (UInt32)(UInt32)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(UInt64) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt64))
            {
                value = (UInt32)(UInt64)(object)valueInternal!;
            }
            else
            {
                throw new Exception("Property type is not compatible with boolean writing and no conversion provided");
            }

            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_LONG);
                InternalWriteBinarySize(4);
                byte[] buff = BitConverter.GetBytes(value);
                if (IsBigEndian)
                    Array.Reverse(buff);
                BaseStream.Write(buff, 0, 4);
                InternalAlignBinary();
                TokenIndex++;
                return (value, valueInternal);
            }

            string textValue = value.ToString();

            if (PreserveMalformations)
            {
                // handle incorrect raw value
                (bool hasIncorrectRaw, string? incorrectText) = parent.Malformations.GetIncorrectTextParse(property);
                if (hasIncorrectRaw)
                    textValue = incorrectText ?? string.Empty;
            }

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(name, parent, property)} [1] ="));
            InternalWriteNewline();
            BaseStream.Write(BZNEncoding.win1252.GetBytes(textValue));
            InternalWriteNewline();
            TokenIndex++;

            return (value, valueInternal);
        }


        public (UInt16 written, TProp stored) WriteUInt16h<T, TProp>(string name, T parent, Expression<Func<T, TProp>> property, Func<TProp, UInt16>? convert = null) where T : IMalformable
        {
            TProp valueInternal = ExtractPropertyValue(parent, property);
            UInt16 value = 0;

            if (convert != null)
            {
                value = convert(valueInternal);
            }
            else if (typeof(TProp) == typeof(UInt8) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt8))
            {
                value = (UInt16)(UInt8)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(UInt16) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt16))
            {
                value = (UInt16)(UInt16)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(UInt32) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt32))
            {
                value = (UInt16)(UInt32)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(UInt64) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt64))
            {
                value = (UInt16)(UInt64)(object)valueInternal!;
            }
            else
            {
                throw new Exception("Property type is not compatible with boolean writing and no conversion provided");
            }

            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_SHORT);
                InternalWriteBinarySize(2);
                byte[] buff = BitConverter.GetBytes(value);
                if (IsBigEndian)
                    Array.Reverse(buff);
                BaseStream.Write(buff, 0, 2);
                InternalAlignBinary();
                TokenIndex++;
                return (value, valueInternal);
            }

            string textValue = value.ToString("x");

            if (PreserveMalformations)
            {
                // handle incorrect raw value
                (bool hasIncorrectRaw, string? incorrectText) = parent.Malformations.GetIncorrectTextParse(property);
                if (hasIncorrectRaw)
                    textValue = incorrectText ?? string.Empty;
            }

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(name, parent, property)} [1] ="));
            InternalWriteNewline();
            BaseStream.Write(BZNEncoding.win1252.GetBytes(textValue));
            InternalWriteNewline();
            TokenIndex++;

            return (value, valueInternal);
        }

        /// <summary>
        /// Write a UInt16 to the BZN
        /// </summary>
        /// <remarks>
        /// Handles the following malformations: <see cref="Malformation.INCORRECT_TEXT"/>, <see cref="Malformation.INCORRECT_NAME"/>
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="property"></param>
        public (UInt16 written, TProp stored) WriteUInt16<T, TProp>(string name, T parent, Expression<Func<T, TProp>> property, Func<TProp, UInt16>? convert = null) where T : IMalformable
        {
            TProp valueInternal = ExtractPropertyValue(parent, property);
            UInt16 value = 0;

            if (convert != null)
            {
                value = convert(valueInternal);
            }
            else if (typeof(TProp) == typeof(UInt8) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt8))
            {
                value = (UInt16)(UInt8)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(UInt16) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt16))
            {
                value = (UInt16)(UInt16)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(UInt32) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt32))
            {
                value = (UInt16)(UInt32)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(UInt64) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt64))
            {
                value = (UInt16)(UInt64)(object)valueInternal!;
            }
            else
            {
                throw new Exception("Property type is not compatible with boolean writing and no conversion provided");
            }

            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_SHORT);
                InternalWriteBinarySize(2);
                byte[] buff = BitConverter.GetBytes(value);
                if (IsBigEndian)
                    Array.Reverse(buff);
                BaseStream.Write(buff, 0, 2);
                InternalAlignBinary();
                TokenIndex++;
                return (value, valueInternal);
            }

            string textValue = value.ToString();

            if (PreserveMalformations)
            {
                // handle incorrect raw value
                (bool hasIncorrectRaw, string? incorrectText) = parent.Malformations.GetIncorrectTextParse(property);
                if (hasIncorrectRaw)
                    textValue = incorrectText ?? string.Empty;
            }

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(name, parent, property)} [1] ="));
            InternalWriteNewline();
            BaseStream.Write(BZNEncoding.win1252.GetBytes(textValue));
            InternalWriteNewline();
            TokenIndex++;

            return (value, valueInternal);
        }
        public (UInt8 written, TProp stored) WriteSaveFlags<T, TProp>(string name, T parent, Expression<Func<T, TProp>> property, Func<TProp, UInt8>? convert = null) where T : IMalformable
        {
            TProp valueInternal = ExtractPropertyValue(parent, property);
            UInt8 value = 0;

            if (convert != null)
            {
                value = convert(valueInternal);
            }
            else if (typeof(TProp) == typeof(UInt8) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt8))
            {
                value = (UInt8)(UInt8)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(UInt16) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt16))
            {
                value = (UInt8)(UInt16)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(UInt32) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt32))
            {
                value = (UInt8)(UInt32)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(UInt64) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt64))
            {
                value = (UInt8)(UInt64)(object)valueInternal!;
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

            if (PreserveMalformations)
            {
                // handle incorrect raw value
                (bool hasIncorrectRaw, string? incorrectText) = parent.Malformations.GetIncorrectTextParse(property);
                if (hasIncorrectRaw)
                    textValue = incorrectText ?? string.Empty;
            }

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(name, parent, property)} [1] ="));
            InternalWriteNewline();
            if (Format != BZNFormat.Battlezone2 || Version >= 1187)
                BaseStream.Write(BZNEncoding.win1252.GetBytes(textValue));
            else
                BaseStream.WriteByte((byte)(value));
            InternalWriteNewline();
            TokenIndex++;

            return (value, valueInternal);
        }
        public (Int8 written, TProp stored) WriteInt8<T, TProp>(string name, T parent, Expression<Func<T, TProp>> property, Func<TProp, Int8>? convert = null) where T : IMalformable
        {
            TProp valueInternal = ExtractPropertyValue(parent, property);
            Int8 value = 0;

            if (convert != null)
            {
                value = convert(valueInternal);
            }
            else if (typeof(TProp) == typeof(Int8) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(Int8))
            {
                value = (Int8)(Int8)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(Int16) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(Int16))
            {
                value = (Int8)(Int16)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(Int32) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(Int32))
            {
                value = (Int8)(Int32)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(Int64) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(Int64))
            {
                value = (Int8)(Int64)(object)valueInternal!;
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

            if (PreserveMalformations)
            {
                // handle incorrect raw value
                (bool hasIncorrectRaw, string? incorrectText) = parent.Malformations.GetIncorrectTextParse(property);
                if (hasIncorrectRaw)
                    textValue = incorrectText ?? string.Empty;
            }

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(name, parent, property)} [1] ="));
            InternalWriteNewline();
            BaseStream.Write(BZNEncoding.win1252.GetBytes(textValue));
            InternalWriteNewline();
            TokenIndex++;

            return (value, valueInternal);
        }

        /// <summary>
        /// Write a UInt8 to the BZN
        /// </summary>
        /// <remarks>
        /// Handles the following malformations: <see cref="Malformation.INCORRECT_TEXT"/>, <see cref="Malformation.INCORRECT_NAME"/>
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="property"></param>
        public (UInt8 written, TProp stored) WriteUInt8<T, TProp>(string? name, T parent, Expression<Func<T, TProp>> property, Func<TProp, UInt8>? convert = null) where T : IMalformable
        {
            if (!InBinary && name == null)
                throw new InvalidOperationException("Cannot write a text token with a null name");

            TProp valueInternal = ExtractPropertyValue(parent, property);
            UInt8 value = 0;

            if (convert != null)
            {
                value = convert(valueInternal);
            }
            else if (typeof(TProp) == typeof(UInt8) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt8))
            {
                value = (UInt8)(UInt8)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(UInt16) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt16))
            {
                value = (UInt8)(UInt16)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(UInt32) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt32))
            {
                value = (UInt8)(UInt32)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(UInt64) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt64))
            {
                value = (UInt8)(UInt64)(object)valueInternal!;
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

            if (PreserveMalformations)
            {
                // handle incorrect raw value
                (bool hasIncorrectRaw, string? incorrectText) = parent.Malformations.GetIncorrectTextParse(property);
                if (hasIncorrectRaw)
                    textValue = incorrectText ?? string.Empty;
            }

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(name!, parent, property)} [1] ="));
            InternalWriteNewline();
            BaseStream.Write(BZNEncoding.win1252.GetBytes(textValue));
            InternalWriteNewline();
            TokenIndex++;

            return (value, valueInternal);
        }

        public (UInt8 written, TProp stored) WriteUInt8h<T, TProp>(string name, T parent, Expression<Func<T, TProp>> property, Func<TProp, UInt8>? convert = null) where T : IMalformable
        {
            TProp valueInternal = ExtractPropertyValue(parent, property);
            UInt8 value = 0;

            if (convert != null)
            {
                value = convert(valueInternal);
            }
            else if (typeof(TProp) == typeof(UInt8) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt8))
            {
                value = (UInt8)(UInt8)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(UInt16) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt16))
            {
                value = (UInt8)(UInt16)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(UInt32) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt32))
            {
                value = (UInt8)(UInt32)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(UInt64) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt64))
            {
                value = (UInt8)(UInt64)(object)valueInternal!;
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

            string textValue = value.ToString("x");

            if (PreserveMalformations)
            {
                // handle incorrect raw value
                (bool hasIncorrectRaw, string? incorrectText) = parent.Malformations.GetIncorrectTextParse(property);
                if (hasIncorrectRaw)
                    textValue = incorrectText ?? string.Empty;
            }

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(name, parent, property)} [1] ="));
            InternalWriteNewline();
            BaseStream.Write(BZNEncoding.win1252.GetBytes(textValue));
            InternalWriteNewline();
            TokenIndex++;

            return (value, valueInternal);
        }

        public (UInt8 written, TProp stored) WriteUInt8Raw<T, TProp>(string name, T parent, Expression<Func<T, TProp>> property, Func<TProp, UInt8>? convert = null) where T : IMalformable
        {
            TProp valueInternal = ExtractPropertyValue(parent, property);
            UInt8 value = 0;

            if (convert != null)
            {
                value = convert(valueInternal);
            }
            else if (typeof(TProp) == typeof(UInt8) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt8))
            {
                value = (UInt8)(UInt8)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(UInt16) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt16))
            {
                value = (UInt8)(UInt16)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(UInt32) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt32))
            {
                value = (UInt8)(UInt32)(object)valueInternal!;
            }
            else if (typeof(TProp) == typeof(UInt64) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt64))
            {
                value = (UInt8)(UInt64)(object)valueInternal!;
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

            if (PreserveMalformations)
            {
                // handle incorrect raw value
                (bool hasIncorrectRaw, string? incorrectText) = parent.Malformations.GetIncorrectTextParse(property);
                if (hasIncorrectRaw)
                    textValue = incorrectText ?? string.Empty;
            }

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(name, parent, property)} [1] ="));
            InternalWriteNewline();
            BaseStream.WriteByte(value);
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
        public void WriteChars<T>(string name, T parent, Expression<Func<T, SizedString>> property, Func<SizedString, byte[]>? convert = null) where T : IMalformable
        {
            SizedString wrappedValue = ExtractPropertyValue(parent, property);
            string value = wrappedValue?.Value ?? string.Empty; // we don't care about the size as we're a normal char print
            byte[] rawValue = BZNEncoding.win1252.GetBytes(value);

            if (convert != null)
            {
                rawValue = convert(wrappedValue);
            }

            if (PreserveMalformations)
            {
                // handle incorrect raw value
                (bool hasIncorrectRaw, byte[]? incorrectRaw) = parent.Malformations.GetIncorrectRaw(property);
                if (hasIncorrectRaw)
                    rawValue = incorrectRaw ?? [];
            }

            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_CHAR);
                InternalWriteBinarySize(rawValue.Length);
                BaseStream.Write(rawValue);
                InternalAlignBinary();
                TokenIndex++;
                return;
            }

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(name, parent, property)} ="));
            if (!PreserveMalformations || !parent.Malformations.IsRightTrimmed(property))
                BaseStream.Write(BZNEncoding.win1252.GetBytes(" "));
            //InternalWriteNewline();
            if (QuoteStrings)
                BaseStream.Write(BZNEncoding.win1252.GetBytes("\""));
            BaseStream.Write(rawValue);

            if (StreamDefects != null)
            {
                if (StreamDefects.ContainsKey(TokenIndex))
                {
                    StreamDefect defect = StreamDefects[TokenIndex];
                    if (defect.EndPadGarbage != null)
                    {
                        BaseStream.Write(BZNEncoding.win1252.GetBytes(defect.EndPadGarbage));
                    }
                }
            }

            if (QuoteStrings)
                BaseStream.Write(BZNEncoding.win1252.GetBytes("\""));
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
        public void WriteChars<T>(string name, T parent, Expression<Func<T, string>> property, Func<string, byte[]>? convert = null) where T : IMalformable
        {
            string? value = ExtractPropertyValue(parent, property);
            byte[] rawValue;

            if (convert != null)
            {
                rawValue = convert(value);
            }
            else
            {
                rawValue = BZNEncoding.win1252.GetBytes(value);
            }

            if (PreserveMalformations)
            {
                // handle incorrect raw value
                (bool hasIncorrectRaw, byte[]? incorrectRaw) = parent.Malformations.GetIncorrectRaw(property);
                if (hasIncorrectRaw)
                    rawValue = incorrectRaw ?? [];
            }

            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_CHAR);
                InternalWriteBinarySize(rawValue.Length);
                BaseStream.Write(rawValue);
                InternalAlignBinary();
                TokenIndex++;
                return;
            }

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(name, parent, property)} ="));
            if (!PreserveMalformations || !parent.Malformations.IsRightTrimmed(property))
                BaseStream.Write(BZNEncoding.win1252.GetBytes(" "));
            //InternalWriteNewline();
            if (QuoteStrings)
                BaseStream.Write(BZNEncoding.win1252.GetBytes("\""));
            BaseStream.Write(rawValue);

            if (StreamDefects != null)
            {
                if (StreamDefects.ContainsKey(TokenIndex))
                {
                    StreamDefect defect = StreamDefects[TokenIndex];
                    if (defect.EndPadGarbage != null)
                    {
                        BaseStream.Write(BZNEncoding.win1252.GetBytes(defect.EndPadGarbage));
                    }
                }
            }

            if (QuoteStrings)
                BaseStream.Write(BZNEncoding.win1252.GetBytes("\""));
            InternalWriteNewline();
            TokenIndex++;
        }

        /// <summary>
        /// Write a Boolean to the BZN
        /// </summary>
        /// <remarks>
        /// Handles the following malformations: <see cref="Malformation.INCORRECT_TEXT"/>, <see cref="Malformation.INCORRECT_NAME"/>
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
                InternalWriteBinaryType(BinaryFieldType.DATA_BOOL);
                InternalWriteBinarySize(1);
                BaseStream.WriteByte((byte)(value ? 1 : 0));
                InternalAlignBinary();
                TokenIndex++;
                return;
            }

            string textValue = value ? "true" : "false";

            if (PreserveMalformations)
            {
                // handle incorrect raw value
                (bool hasIncorrectRaw, string? incorrectText) = parent.Malformations.GetIncorrectTextParse(property);
                if (hasIncorrectRaw)
                    textValue = incorrectText ?? string.Empty;
            }

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(name, parent, property)} [1] ="));
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
        /// Handles the following malformations: <see cref="Malformation.INCORRECT_RAW"/>, <see cref="Malformation.INCORRECT_NAME"/>
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="property"></param>
        /// <param name="oneLiner"></param>
        public void WriteID<T, TProp>(string name, T parent, Expression<Func<T, TProp>> property/*, bool oneLiner = false*/) where T : IMalformable
        {
            TProp value = ExtractPropertyValue(parent, property);
            byte[] rawValue;

            if (typeof(TProp) == typeof(string) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(string))
            {
                string str = (string?)(object?)value ?? "";
                rawValue = BZNEncoding.win1252.GetBytes(str);
                if (rawValue.Length > 8)
                    rawValue = rawValue.Take(8).ToArray();
                else if (rawValue.Length < 8)
                    rawValue = rawValue.Concat(new byte[8 - rawValue.Length]).ToArray();
            }
            else if (typeof(TProp) == typeof(SizedString) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(SizedString))
            {
                string str = ((SizedString?)(object?)value)?.Value ?? "";
                rawValue = BZNEncoding.win1252.GetBytes(str);
                if (rawValue.Length > 8)
                    rawValue = rawValue.Take(8).ToArray();
                else if (rawValue.Length < 8)
                    rawValue = rawValue.Concat(new byte[8 - rawValue.Length]).ToArray();
            }
            else if (typeof(TProp) == typeof(UInt64) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt64))
            {
                // Convert UInt64 to 8 bytes (little-endian)
                rawValue = BitConverter.GetBytes((UInt64)(object)value!);
            }
            else
            {
                throw new NotImplementedException("Unimplemented ID type");
            }

            if (PreserveMalformations)
            {
                // handle incorrect raw value
                (bool hasIncorrectRaw, byte[]? incorrectRaw) = parent.Malformations.GetIncorrectRaw(property);
                if (hasIncorrectRaw)
                    rawValue = incorrectRaw ?? [];
            }

            if (InBinary)
            {
                // Always write 8 bytes in binary mode
                if (rawValue.Length < 8)
                    rawValue = rawValue.Concat(new byte[8 - rawValue.Length]).ToArray();
                else if (rawValue.Length > 8)
                    rawValue = rawValue.Take(8).ToArray();

                InternalWriteBinaryType(BinaryFieldType.DATA_ID);
                InternalWriteBinarySize(8);
                BaseStream.Write(rawValue, 0, 8);
                InternalAlignBinary();
                TokenIndex++;
                return;
            }

            // In text mode, write up to the first null (or all 8 if no null)
            //int textLen = Array.IndexOf(rawValue, (byte)0);
            //if (textLen < 0)
            //    textLen = Math.Min(rawValue.Length, 8);
            //else
            //    textLen = Math.Min(textLen, 8);

            if (Format != BZNFormat.Battlezone || Version != 1001)
            {
                int textLen = Array.IndexOf(rawValue, (byte)0);
                if (textLen >= 0)
                    rawValue = rawValue.Take(textLen).ToArray();
            }
            // text mode garbage probably doesn't work right here, consider text malformation in that case

            //if (oneLiner)
            if (Format == BZNFormat.Battlezone && Version == 1001)
            {
                //// maybe one-liner should be a general writer tool so it can use the malformation for no trailing space universally
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(name, parent, property)} = "));
            }
            else
            {
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(name, parent, property)} [1] ="));
                InternalWriteNewline();
            }
            BaseStream.Write(rawValue);
            InternalWriteNewline();
            TokenIndex++;
        }




        private string InternalFixName<T, TProp>(string name, T parent, Expression<Func<T, TProp>> property) where T : IMalformable
        {
            if (PreserveMalformations)
            {
                (bool hasIncorrectName, string? incorrectName) = parent.Malformations.GetIncorrectName(property);
                if (hasIncorrectName && incorrectName != null)
                    name = incorrectName;
            }
            return name!;
        }
        private void InternalWriteVector2DValue(Vector2D value)
        {
            if (InBinary)
            {
                InternalWriteFloatValue(value, x => x.X);
                InternalWriteFloatValue(value, x => x.Z);
                return;
            }

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName("  x", value, x => x.X)} [1] ="));
            InternalWriteNewline();
            InternalWriteFloatValue(value, x => x.X);
            InternalWriteNewline();

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName("  z", value, x => x.Z)} [1] ="));
            InternalWriteNewline();
            InternalWriteFloatValue(value, x => x.Z);
            InternalWriteNewline();
        }
        private void InternalWriteVector3DValue(Vector3D value)
        {
            if (InBinary)
            {
                InternalWriteFloatValue(value, x => x.X);
                InternalWriteFloatValue(value, x => x.Y);
                InternalWriteFloatValue(value, x => x.Z);
                return;
            }

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName("  x", value, x => x.X)} [1] ="));
            InternalWriteNewline();
            InternalWriteFloatValue(value, x => x.X);
            InternalWriteNewline();

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName("  y", value, x => x.Y)} [1] ="));
            InternalWriteNewline();
            InternalWriteFloatValue(value, x => x.Y);
            InternalWriteNewline();

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName("  z", value, x => x.Z)} [1] ="));
            InternalWriteNewline();
            InternalWriteFloatValue(value, x => x.Z);
            InternalWriteNewline();
        }
        private void InternalWriteDoubleValue<T, TProp>(T parent, Expression<Func<T, TProp>> property) where T : IMalformable
        {
            TProp value_ = BZNStreamWriter.ExtractPropertyValue(parent, property);
            double value = (double)(object)value_!;

            if (InBinary)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                if (IsBigEndian)
                    Array.Reverse(bytes);
                BaseStream.Write(bytes);
                return;
            }

            string textValue = value.ToString(); // figure out if doubles have a format

            if (PreserveMalformations)
            {
                // handle incorrect text value
                (bool hasIncorrectRaw, string? incorrectText) = parent.Malformations.GetIncorrectTextParse(property);
                if (hasIncorrectRaw)
                    textValue = incorrectText ?? string.Empty;
            }

            BaseStream.Write(BZNEncoding.win1252.GetBytes(textValue));
        }
        private void InternalWriteFloatValue<T, TProp>(T parent, Expression<Func<T, TProp>> property) where T : IMalformable
        {
            TProp value_ = BZNStreamWriter.ExtractPropertyValue(parent, property);
            float value;
            if (value_ is float f)
                value = f;
            else if (value_ is double d)
                value = (float)d;
            else if (typeof(TProp).IsGenericType && typeof(TProp).GetGenericTypeDefinition() == typeof(DualModeValue<,>))
            {
                var genericArgs = typeof(TProp).GetGenericArguments();
                if (genericArgs[0] == typeof(float) || genericArgs[1] == typeof(float))
                {
                    if (value_ == null)
                    {
                        value = default;
                    }
                    else
                    {
                        dynamic dual = value_;
                        value = dual.Get<float>();
                    }
                }
                else if (genericArgs[0] == typeof(double) || genericArgs[1] == typeof(double))
                {
                    if (value_ == null)
                    {
                        value = default;
                    }
                    else
                    {
                        dynamic dual = value_;
                        value = (float)dual.Get<double>();
                    }
                }
                else
                {
                    throw new InvalidCastException($"DualModeValue does not contain float or double: {typeof(TProp)}");
                }
            }
            else
                throw new InvalidCastException($"Unsupported type: {typeof(TProp)}");

            if (InBinary)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                if (IsBigEndian)
                    Array.Reverse(bytes);
                BaseStream.Write(bytes);
                return;
            }

            string textValue = value.ToBZNString(FloatFormat);

            if (PreserveMalformations)
            {
                // handle incorrect text value
                (bool hasIncorrectRaw, string? incorrectText) = parent.Malformations.GetIncorrectTextParse(property);
                if (hasIncorrectRaw)
                    textValue = incorrectText ?? string.Empty;
            }

            BaseStream.Write(BZNEncoding.win1252.GetBytes(textValue));
        }
        public (Vector2D written, TProp stored) WriteVector2D<T, TProp>(string? name, T parent, Expression<Func<T, TProp>> property) where T : IMalformable
        {
            if (!InBinary && name == null)
                throw new InvalidOperationException("Cannot write a text token with a null name");

            TProp propValue = ExtractPropertyValue(parent, property);
            Vector2D[] vectors;
            if (typeof(TProp).IsArray && typeof(TProp).GetElementType() == typeof(Vector2D))
            {
                vectors = (Vector2D[])(object)propValue!;
            }
            else if (typeof(TProp).IsGenericType
                && typeof(TProp).GetGenericTypeDefinition() == typeof(MalformableArray<,>)
                && typeof(TProp).GetGenericArguments()[1] == typeof(Vector2D))
            {
                dynamic arr = (dynamic)(object)propValue!;
                vectors = arr!.ToArray();
            }
            else
            {
                vectors = new Vector2D[] { (Vector2D)(object)propValue! };
            }
            int length = vectors!.Length;

            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_VEC2D);
                InternalWriteBinarySize(sizeof(float) * 2 * length);

                bool writeTruncate = false;
                if (StreamDefects != null)
                {
                    if (StreamDefects.ContainsKey(TokenIndex))
                    {
                        StreamDefect defect = StreamDefects[TokenIndex];
                        if (defect.TruncatedBytesData != null)
                        {
                            // fucky wucky, this might break something if you edit the field, but any edits should invalidate the entire StreamDefects collection 
                            BaseStream.Write(defect.TruncatedBytesData);
                            writeTruncate = true;
                        }
                    }
                }
                if (!writeTruncate)
                    foreach (var vec in vectors)
                        InternalWriteVector2DValue(vec);
                InternalAlignBinary();
            }
            else
            {
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(name!, parent, property)} [{length}] ="));
                InternalWriteNewline();
                foreach (var vec in vectors)
                {
                    InternalWriteVector2DValue(vec);
                }
            }
            TokenIndex++;
            return (vectors.Length > 0 ? vectors[0] : default(Vector2D), propValue)!;
        }
        public (Vector3D written, TProp stored) WriteVector3D<T, TProp>(string? name, T parent, Expression<Func<T, TProp>> property) where T : IMalformable
        {
            if (!InBinary && name == null)
                throw new InvalidOperationException("Cannot write a text token with a null name");

            TProp propValue = ExtractPropertyValue(parent, property);
            Vector3D[] vectors;
            if (typeof(TProp).IsArray && typeof(TProp).GetElementType() == typeof(Vector3D))
            {
                vectors = (Vector3D[])(object)propValue!;
            }
            else if (typeof(TProp).IsGenericType
                && typeof(TProp).GetGenericTypeDefinition() == typeof(MalformableArray<,>)
                && typeof(TProp).GetGenericArguments()[1] == typeof(Vector3D))
            {
                dynamic arr = (dynamic)(object)propValue!;
                vectors = arr!.ToArray();
            }
            else
            {
                vectors = new Vector3D[] { (Vector3D)(object)propValue! };
            }
            int length = vectors.Length;

            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_VEC3D);
                InternalWriteBinarySize(sizeof(float) * 3 * length);
                foreach (var vec in vectors)
                {
                    InternalWriteVector3DValue(vec);
                }
                InternalAlignBinary();
            }
            else
            {
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(name, parent, property)} [{length}] ="));
                InternalWriteNewline();
                foreach (var vec in vectors)
                {
                    InternalWriteVector3DValue(vec);
                }
            }
            TokenIndex++;
            return (vectors.Length > 0 ? vectors[0] : default(Vector3D), propValue)!;
        }
        public Euler WriteEuler<T>(string name, T parent, Expression<Func<T, Euler>> property) where T : IMalformable
        {
            Euler value = BZNStreamWriter.ExtractPropertyValue(parent, property);

            if (InBinary)
            {
                WriteSingle(null, value, x => x.Mass);
                WriteSingle(null, value, x => x.MassInv);
                WriteSingle(null, value, x => x.VMag);
                WriteSingle(null, value, x => x.VMagInv);
                WriteSingle(null, value, x => x.I);
                WriteSingle(null, value, x => x.IInv);
                WriteVector3D(null, value, x => x.v);
                WriteVector3D(null, value, x => x.omega);
                WriteVector3D(null, value, x => x.Accel);
                return value;
            }

            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(name, parent, property)} ="));
            InternalWriteNewline();
            {
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(" mass", value, x => x.Mass)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.Mass);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(" mass_inv", value, x => x.MassInv)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.MassInv);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(" v_mag", value, x => x.VMag)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.VMag);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(" v_mag_inv", value, x => x.VMagInv)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.VMagInv);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(" I", value, x => x.I)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.I);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(" k_i", value, x => x.IInv)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.IInv);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(" v", value, x => x.v)} [1] ="));
                InternalWriteNewline();
                InternalWriteVector3DValue(value.v);
                //InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(" omega", value, x => x.omega)} [1] ="));
                InternalWriteNewline();
                InternalWriteVector3DValue(value.omega);
                //InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(" Accel", value, x => x.Accel)} [1] ="));
                InternalWriteNewline();
                InternalWriteVector3DValue(value.Accel);
                //InternalWriteNewline();
            }
            TokenIndex++;
            return value;
        }

        public void WriteMatrix<T, TProp>(string name, T parent, Expression<Func<T, TProp>> property, Func<TProp, Matrix>? convert = null) where T : IMalformable
        {
            TProp valueInternal = ExtractPropertyValue(parent, property);
            Matrix value;

            if (convert != null)
            {
                value = convert(valueInternal);
            }
            else if (/*typeof(TProp) == typeof(Matrix) &&*/ valueInternal is Matrix m)
            {
                value = m;
            }
            else
            {
                throw new Exception("Property type is not compatible with Length writing and no conversion provided");
            }

            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_MAT3D);
                InternalWriteBinarySize(sizeof(float) * 16);
                {
                    InternalWriteFloatValue(value, x => x.RightX);
                    InternalWriteFloatValue(value, x => x.RightY);
                    InternalWriteFloatValue(value, x => x.RightZ);
                    InternalWriteFloatValue(value, x => x.RightW);
                    InternalWriteFloatValue(value, x => x.UpX);
                    InternalWriteFloatValue(value, x => x.UpY);
                    InternalWriteFloatValue(value, x => x.UpZ);
                    InternalWriteFloatValue(value, x => x.UpW);
                    InternalWriteFloatValue(value, x => x.FrontX);
                    InternalWriteFloatValue(value, x => x.FrontY);
                    InternalWriteFloatValue(value, x => x.FrontZ);
                    InternalWriteFloatValue(value, x => x.FrontW);
                    InternalWriteFloatValue(value, x => x.PositX);
                    InternalWriteFloatValue(value, x => x.PositY);
                    InternalWriteFloatValue(value, x => x.PositZ);
                    InternalWriteFloatValue(value, x => x.PositW);
                }
                InternalAlignBinary();
                TokenIndex++;
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(name, parent, property)} [1] ="));
            InternalWriteNewline();
            {
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName("  right.x", value, x => x.RightX)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.RightX);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName("  right.y", value, x => x.RightY)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.RightY);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName("  right.z", value, x => x.RightZ)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.RightZ);
                InternalWriteNewline();

                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName("  up.x", value, x => x.UpX)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.UpX);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName("  up.y", value, x => x.UpY)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.UpY);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName("  up.z", value, x => x.UpZ)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.UpZ);
                InternalWriteNewline();

                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName("  front.x", value, x => x.FrontX)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.FrontX);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName("  front.y", value, x => x.FrontY)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.FrontY);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName("  front.z", value, x => x.FrontZ)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.FrontZ);
                InternalWriteNewline();

                // TODO change these to double strings, whatever that looks like
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName("  posit.x", value, x => x.PositX)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.PositX);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName("  posit.y", value, x => x.PositY)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.PositY);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName("  posit.z", value, x => x.PositZ)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.PositZ);
                InternalWriteNewline();
            }
            TokenIndex++;
        }
        public void WriteMatrixOld<T>(string name, T parent, Expression<Func<T, Matrix>> property) where T : IMalformable
        {
            Matrix value = BZNStreamWriter.ExtractPropertyValue(parent, property);

            if (InBinary)
            {
                if (Format == BZNFormat.Battlezone && Version > 0) // BREADCRUMB VER_BIGPOSIT
                {
                    InternalWriteBinaryType(BinaryFieldType.DATA_MAT3DOLD);
                    InternalWriteBinarySize(sizeof(float) * 9 + 4 + sizeof(double) * 3);
                    {
                        InternalWriteFloatValue(value, x => x.RightX);
                        InternalWriteFloatValue(value, x => x.RightY);
                        InternalWriteFloatValue(value, x => x.RightZ);
                        InternalWriteFloatValue(value, x => x.UpX);
                        InternalWriteFloatValue(value, x => x.UpY);
                        InternalWriteFloatValue(value, x => x.UpZ);
                        InternalWriteFloatValue(value, x => x.FrontX);
                        InternalWriteFloatValue(value, x => x.FrontY);
                        InternalWriteFloatValue(value, x => x.FrontZ);

                        // TODO zero this if we aren't preserving malformations, consider adding it to malformations though it would need a new type, like "excess bytes"
                        if (PreserveMalformations)
                        {
                            byte[] bytes = BitConverter.GetBytes(value.junk);
                            if (IsBigEndian)
                                Array.Reverse(bytes);
                            BaseStream.Write(bytes);
                        }
                        else
                        {
                            BaseStream.WriteByte(0x00);
                            BaseStream.WriteByte(0x00);
                            BaseStream.WriteByte(0x00);
                            BaseStream.WriteByte(0x00);
                        }

                        InternalWriteDoubleValue(value, x => x.PositX);
                        InternalWriteDoubleValue(value, x => x.PositY);
                        InternalWriteDoubleValue(value, x => x.PositZ);
                    }
                    InternalAlignBinary();
                    TokenIndex++;
                    return;
                }
                else
                {
                    InternalWriteBinaryType(BinaryFieldType.DATA_MAT3DOLD);
                    InternalWriteBinarySize(sizeof(float) * 12);
                    {
                        InternalWriteFloatValue(value, x => x.RightX);
                        InternalWriteFloatValue(value, x => x.RightY);
                        InternalWriteFloatValue(value, x => x.RightZ);
                        InternalWriteFloatValue(value, x => x.UpX);
                        InternalWriteFloatValue(value, x => x.UpY);
                        InternalWriteFloatValue(value, x => x.UpZ);
                        InternalWriteFloatValue(value, x => x.FrontX);
                        InternalWriteFloatValue(value, x => x.FrontY);
                        InternalWriteFloatValue(value, x => x.FrontZ);
                        InternalWriteFloatValue(value, x => x.PositX);
                        InternalWriteFloatValue(value, x => x.PositY);
                        InternalWriteFloatValue(value, x => x.PositZ);
                    }
                    InternalAlignBinary();
                    TokenIndex++;
                    return;
                }
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName(name, parent, property)} [1] ="));
            InternalWriteNewline();
            {
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName("  right_x", value, x => x.RightX)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.RightX);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName("  right_y", value, x => x.RightY)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.RightY);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName("  right_z", value, x => x.RightZ)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.RightZ);
                InternalWriteNewline();

                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName("  up_x", value, x => x.UpX)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.UpX);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName("  up_y", value, x => x.UpY)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.UpY);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName("  up_z", value, x => x.UpZ)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.UpZ);
                InternalWriteNewline();

                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName("  front_x", value, x => x.FrontX)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.FrontX);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName("  front_y", value, x => x.FrontY)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.FrontY);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName("  front_z", value, x => x.FrontZ)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.FrontZ);
                InternalWriteNewline();

                // TODO change these to double strings, whatever that looks like
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName("  posit_x", value, x => x.PositX)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.PositX);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName("  posit_y", value, x => x.PositY)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.PositY);
                InternalWriteNewline();
                BaseStream.Write(BZNEncoding.win1252.GetBytes($"{InternalFixName("  posit_z", value, x => x.PositZ)} [1] ="));
                InternalWriteNewline();
                InternalWriteFloatValue(value, x => x.PositZ);
                InternalWriteNewline();
            }
            TokenIndex++;
        }

        public void WriteCmdDummy(string name)
        {
            if (InBinary)
            {
                TokenIndex++;
                return;
            }
            BaseStream.Write(BZNEncoding.win1252.GetBytes($"{name} ="));
            InternalWriteNewline();
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
                        if (TypeSize > 1)
                        {
                            if (defect.TruncatedBytesType != null)
                            {
                                // fucky wucky, this might break something if you edit the field, but any edits should invalidate the entire StreamDefects collection 
                                BaseStream.Write(defect.TruncatedBytesType);
                                return;
                            }
                            else if (defect.TypeGarbage.HasValue)
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
                if (StreamDefects != null)
                {
                    if (StreamDefects.ContainsKey(TokenIndex))
                    {
                        StreamDefect defect = StreamDefects[TokenIndex];
                        if (defect.TruncatedBytesSize != null)
                        {
                            // fucky wucky, this might break something if you edit the field, but any edits should invalidate the entire StreamDefects collection 
                            BaseStream.Write(defect.TruncatedBytesSize);
                            return;
                        }
                        if (defect.BytesOversized.HasValue)
                        {
                            size = (int)defect.BytesOversized.Value;
                        }
                    }
                }

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