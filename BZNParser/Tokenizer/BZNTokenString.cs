using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BZNParser.Tokenizer
{
    public class BZNTokenString : IBZNToken
    {
        public string Name { get => name; }

        private string[] values;
        private string name;

        /// <summary>
        /// This is a one-liner token with an empty value, but is missing the space between the value and '=' as it was right trimmed.
        /// </summary>
        public bool RightTrimmedOneLiner { get; set; } // TODO figure out how to show this nicer as this is a carrier defect if it doesn't equal type

        public BZNTokenString(string name, string[] values)
        {
            this.name = name;
            this.values = values;
        }

        public bool IsBinary => false;

        public int GetCount(int PtrSize)
        {
            return values.Length;
        }
        public int GetSubCount(int index = 0) => 0;
        public IBZNToken GetSubToken(int index = 0, int subIndex = 0) { throw new InvalidOperationException("Basic String Tokens have no sub tokens."); }

        public bool GetBoolean(int index = 0)
        {
            if (index >= values.Length) throw new ArgumentOutOfRangeException();
            if (values[index] == "0") return false;
            if (values[index] == "1") return true;
            if (values[index] == "00000000")
                return false; // unlikely to be real so lets check for it
            if (values[index] == "00000001")
                return true; // unlikely to be real so lets check for it
            return bool.Parse(values[index]);
        }
        public UInt64 GetUInt64(int index = 0)
        {
            if (index >= values.Length) throw new ArgumentOutOfRangeException();
            if (values[index].StartsWith('-'))
            {
                return unchecked((UInt64)Int64.Parse(values[index]));
            }
            return UInt64.Parse(values[index]);
        }
        public UInt64 GetUInt64H(int index = 0)
        {
            if (index >= values.Length) throw new ArgumentOutOfRangeException();
            return UInt64.Parse(values[index], System.Globalization.NumberStyles.HexNumber);
        }

        public Int32 GetInt32(int index = 0)
        {
            if (index >= values.Length) throw new ArgumentOutOfRangeException();
            return Int32.Parse(values[index]);
        }
        public Int32 GetInt32H(int index = 0)
        {
            if (index >= values.Length) throw new ArgumentOutOfRangeException();
            if (values[index].StartsWith('-'))
            {
                return Int32.Parse(values[index], System.Globalization.NumberStyles.HexNumber);
            }
            return unchecked((Int32)UInt32.Parse(values[index], System.Globalization.NumberStyles.HexNumber));
        }
        public UInt32 GetUInt32(int index = 0)
        {
            if (index >= values.Length) throw new ArgumentOutOfRangeException();
            if (values[index].StartsWith('-'))
            {
                return unchecked((UInt32)Int32.Parse(values[index]));
            }
            return UInt32.Parse(values[index]);
        }

        public UInt32 GetUInt32H(int index = 0)
        {
            if (index >= values.Length) throw new ArgumentOutOfRangeException();
            return UInt32.Parse(values[index], System.Globalization.NumberStyles.HexNumber);
        }

        public UInt32 GetUInt32HR(int index = 0)
        {
            if (index >= values.Length) throw new ArgumentOutOfRangeException();
            UInt32 value = UInt32.Parse(values[index], System.Globalization.NumberStyles.HexNumber);
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public UInt32 GetUInt32Raw(int index = 0)
        {
            // can't be BigEndian as we're text which doesn't exist on IsBigEndian platforms (thank god or the string token parser would need to know the bit order for these edge cases)
            return BitConverter.ToUInt32(GetRaw(index * 4, 4));
        }

        public Int16 GetInt16(int index = 0)
        {
            if (index >= values.Length) throw new ArgumentOutOfRangeException();
            return Int16.Parse(values[index]);
        }

        public UInt16 GetUInt16(int index = 0)
        {
            if (index >= values.Length) throw new ArgumentOutOfRangeException();
            if (values[index].StartsWith('-'))
            {
                return unchecked((UInt16)Int16.Parse(values[index]));
            }
            return UInt16.Parse(values[index]);
        }

        public UInt16 GetUInt16H(int index = 0)
        {
            if (index >= values.Length) throw new ArgumentOutOfRangeException();
            return UInt16.Parse(values[index], System.Globalization.NumberStyles.HexNumber);
        }

        public SByte GetInt8(int index = 0)
        {
            if (index >= values.Length) throw new ArgumentOutOfRangeException();
            return SByte.Parse(values[index]);
        }

        public byte GetUInt8(int index = 0)
        {
            if (index >= values.Length) throw new ArgumentOutOfRangeException();
            if (values[index].StartsWith('-'))
            {
                return unchecked((byte)SByte.Parse(values[index]));
            }
            return byte.Parse(values[index]);
        }
        public float GetSingle(int index = 0)
        {
            if (index >= values.Length) throw new ArgumentOutOfRangeException();
            if (values[index] == "-1.#QNAN")
                return float.NaN;
            if (values[index] == string.Empty)
                return 0f;
            return Single.Parse(values[index], System.Globalization.CultureInfo.InvariantCulture);
        }

        public string GetString(int index = 0)
        {
            if (index > values.Length) throw new ArgumentOutOfRangeException();
            return values[index];
        }

        public Vector3D GetVector3D(int index = 0)
        {
            throw new InvalidOperationException();
        }

        public Vector2D GetVector2D(int index = 0)
        {
            throw new InvalidOperationException();
        }

        public Matrix GetMatrixOld(int index = 0)
        {
            throw new InvalidOperationException();
        }

        public Matrix GetMatrix(int index = 0)
        {
            throw new InvalidOperationException();
        }

        public Euler GetEuler(int index = 0)
        {
            throw new InvalidOperationException();
        }

        public byte[] GetBytes(int index = 0, int length = -1) {
            if (length == -1)
                length = (values[0].Length / 2) - index;
            if (index + length > values[0].Length / 2) throw new ArgumentOutOfRangeException();
            char[] rawDataArray = values[0].Skip(index * 2).Take(length * 2).ToArray();
            byte[] dataOut = new byte[rawDataArray.Length / 2];
            for(int x=0;x<dataOut.Length;x++)
            {
                dataOut[x] = byte.Parse("" + rawDataArray[x * 2 + 0] + rawDataArray[x * 2 + 1], System.Globalization.NumberStyles.HexNumber);
            }
            return dataOut;
        }

        public byte[] GetRaw(int index = 0, int length = -1)
        {
            if (values.Length > 1) throw new ArgumentOutOfRangeException();
            if (length == -1) return values[0].Skip(index).Select(x => (byte)x).ToArray();
            return values[0].Skip(index).Take(length).Select(x => (byte)x).ToArray();
        }

        public string GetName()
        {
            return name.Trim();
        }

        public string GetRawName()
        {
            return name;
        }

        public bool IsValidationOnly() { return false; }

        public override string ToString()
        {
            if (values == null)
                return $"ASCII \tName: {name.PadRight(13)}\tnull";

            string retVal = $"ASCII \tName: {name.PadRight(13)}\tValue: {values[0].Substring(0, Math.Min(59, values[0].Length))}{(values[0].Length > 59 ? "..." : string.Empty)}";
            for (int x = 1; x < values.Length; x++)
            {
                retVal += $"     \t                         \t{values[x].Substring(0, Math.Min(59, values[x].Length))}{(values[x].Length > 59 ? "..." : string.Empty)}";
            }
            return retVal;
        }

        public bool Validate(string? name, BinaryFieldType type = BinaryFieldType.DATA_UNKNOWN)
        {
            //return this.name == name;
            if (this.name.Trim() == name)
                return true;

            // malformed extra line/space
            //if (this.name.Trim() == name)
            //    return true;

            // typo
            if (MatchesAllButOne(name, this.name))
                return true;

            return false;
        }

        private static bool MatchesAllButOne(string reference, string candidate)
        {
            string referenceT = reference.Trim();
            if (candidate.Length != referenceT.Length - 1)
                return false;

            for (int i = 0; i < referenceT.Length; i++)
            {
                string modified = referenceT.Remove(i, 1);
                if (modified == candidate)
                    return true;
            }
            return false;
        }
    }
}
