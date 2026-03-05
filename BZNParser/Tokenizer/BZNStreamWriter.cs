using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BZNParser.Tokenizer
{
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

        public void WriteUnsignedValues(string name, params UInt32[] values)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_SHORT);
                InternalWriteBinarySize(values.Length);
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
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].x.ToString()));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes("  z [1] ="));
                InternalWriteNewline();
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].z.ToString()));
                InternalWriteNewline();
            }
        }

        public void WriteSignedValues(string name, params int[] values)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_LONG);
                InternalWriteBinarySize(values.Length);
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
                InternalWriteBinarySize(values.Length);
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
                BaseStream.Write(Encoding.ASCII.GetBytes(values[i].ToString()));
                InternalWriteNewline();
            }
        }

        // 8 bit number
        public void WriteUnsignedValues(string name, params byte[] values)
        {
            if (InBinary)
            {
                InternalWriteBinaryType(BinaryFieldType.DATA_FLOAT);
                InternalWriteBinarySize(values.Length);
                byte[] bytes = values.ToArray();
                if (IsBigEndian)
                    Array.Reverse(bytes);
                BaseStream.Write(bytes);
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
                InternalWriteBinarySize(1);
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
        public void WriteVoidBytes(string name, UInt32 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (IsBigEndian)
                Array.Reverse(bytes);
            WriteVoidBytes(name, bytes);
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

                Array.Copy(rawSize, sizeof(int) - SizeSize, sizeBytes, 0, SizeSize);

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