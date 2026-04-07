using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace BZNParser.Tokenizer
{
    public class BZNTokenBinary : IBZNToken
    {
        private BinaryFieldType type;
        private byte[] data;
        private bool IsBigEndian;
        private int PtrSize;
        private bool BigPosit;

        public uint? rawType { get; set; } // TODO figure out how to show this nicer as this is a carrier defect if it doesn't equal type

        public BZNTokenBinary(BinaryFieldType fieldType, byte[] data, bool isBigEndian, int ptrSize, bool bigPosit)
        {
            // TODO: Complete member initialization
            this.type = fieldType;
            this.data = data;
            this.IsBigEndian = isBigEndian;
            this.PtrSize = ptrSize;
            this.BigPosit = bigPosit;
        }
        public bool IsBinary => true;
        public int GetCount()
        {
            switch (type)
            {
                case BinaryFieldType.DATA_VOID: return data.Length / 4;
                case BinaryFieldType.DATA_BOOL: return data.Length / 1;
                case BinaryFieldType.DATA_CHAR: return data.Length / 1;
                case BinaryFieldType.DATA_SHORT: return data.Length / 2;
                case BinaryFieldType.DATA_LONG: return data.Length / 4;
                case BinaryFieldType.DATA_FLOAT: return data.Length / 4;
                case BinaryFieldType.DATA_DOUBLE: return data.Length / 8;
                case BinaryFieldType.DATA_ID: return data.Length / 4;
                case BinaryFieldType.DATA_PTR: return data.Length / PtrSize;
                case BinaryFieldType.DATA_VEC3D: return data.Length / 4 / 3;
                case BinaryFieldType.DATA_VEC2D: return data.Length / 4 / 2;
                case BinaryFieldType.DATA_MAT3DOLD: throw new NotImplementedException(); // make sure you account for bigPosit if you implement this
                case BinaryFieldType.DATA_MAT3D: throw new NotImplementedException();
                case BinaryFieldType.DATA_STRING: throw new NotImplementedException();
                case BinaryFieldType.DATA_QUAT: throw new NotImplementedException();
                case BinaryFieldType.DATA_UNKNOWN: throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
        public int GetCount(BinaryFieldType type)
        {
            switch (type)
            {
                case BinaryFieldType.DATA_VOID: return data.Length / 4;
                case BinaryFieldType.DATA_BOOL: return data.Length / 1;
                case BinaryFieldType.DATA_CHAR: return data.Length / 1;
                case BinaryFieldType.DATA_SHORT: return data.Length / 2;
                case BinaryFieldType.DATA_LONG: return data.Length / 4;
                case BinaryFieldType.DATA_FLOAT: return data.Length / 4;
                case BinaryFieldType.DATA_DOUBLE: return data.Length / 8;
                case BinaryFieldType.DATA_ID: return data.Length / 4;
                case BinaryFieldType.DATA_PTR: return data.Length / PtrSize;
                case BinaryFieldType.DATA_VEC3D: return data.Length / 4 / 3;
                case BinaryFieldType.DATA_VEC2D: return data.Length / 4 / 2;
                case BinaryFieldType.DATA_MAT3DOLD: throw new NotImplementedException(); // make sure you account for bigPosit if you implement this
                case BinaryFieldType.DATA_MAT3D: throw new NotImplementedException();
                case BinaryFieldType.DATA_STRING: throw new NotImplementedException();
                case BinaryFieldType.DATA_QUAT: throw new NotImplementedException();
                case BinaryFieldType.DATA_UNKNOWN: throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
        public int GetSubCount(int index = 0) => 0;
        public IBZNToken GetSubToken(int index = 0, int subIndex = 0) { throw new InvalidOperationException("Binary Tokens have no sub tokens."); }

        public bool GetBoolean(int index = 0)
        {
            if (index >= data.Length / sizeof(bool)) throw new ArgumentOutOfRangeException();
            if (IsBigEndian) return BitConverter.ToBoolean(data.Skip(index * sizeof(bool)).Take(sizeof(bool)).Reverse().ToArray(), 0);
            return BitConverter.ToBoolean(data, index * sizeof(bool));
        }
        public UInt64 GetUInt64(int index = 0)
        {
            if (index >= data.Length / sizeof(UInt32)) throw new ArgumentOutOfRangeException();
            if (IsBigEndian) return BitConverter.ToUInt64(data.Skip(index * sizeof(UInt64)).Take(sizeof(UInt64)).Reverse().ToArray(), 0);
            return BitConverter.ToUInt64(data, index * sizeof(UInt64));
        }
        public UInt64 GetUInt64H(int index = 0) => GetUInt64(index);

        public Int32 GetInt32(int index = 0)
        {
            if (index >= data.Length / sizeof(Int32)) throw new ArgumentOutOfRangeException();
            if (IsBigEndian) return BitConverter.ToInt32(data.Skip(index * sizeof(Int32)).Take(sizeof(Int32)).Reverse().ToArray(), 0);
            return BitConverter.ToInt32(data, index * sizeof(Int32));
        }
        public Int32 GetInt32H(int index = 0) => GetInt32(index);
        public UInt32 GetUInt32HR(int index = 0)
        {
            if (index >= data.Length / sizeof(UInt32)) throw new ArgumentOutOfRangeException();
            //if (IsBigEndian) return BitConverter.ToUInt32(data.Skip(index * sizeof(UInt32)).Take(sizeof(UInt32)).Reverse().ToArray(), 0);
            return BitConverter.ToUInt32(data, index * sizeof(UInt32));
        }

        public UInt32 GetUInt32(int index = 0)
        {
            if (index >= data.Length / sizeof(UInt32)) throw new ArgumentOutOfRangeException();
            if (IsBigEndian) return BitConverter.ToUInt32(data.Skip(index * sizeof(UInt32)).Take(sizeof(UInt32)).Reverse().ToArray(), 0);
            return BitConverter.ToUInt32(data, index * sizeof(UInt32));
        }

        public UInt32 GetUInt32H(int index = 0) => GetUInt32(index);

        public UInt32 GetUInt32Raw(int index = 0) => GetUInt32(index);

        public UInt32 GetUInt32N64Fix(int index = 0)
        {
            if (index >= data.Length / sizeof(UInt32)) throw new ArgumentOutOfRangeException();
            //if (n64Data) return BitConverter.ToUInt32(data.Skip(index * sizeof(UInt32)).Take(sizeof(UInt32)).Reverse().ToArray(), 0);
            return BitConverter.ToUInt32(data, index * sizeof(UInt32));
        }

        public Int16 GetInt16(int index = 0)
        {
            if (index >= data.Length / sizeof(Int16)) throw new ArgumentOutOfRangeException();
            if (IsBigEndian) return BitConverter.ToInt16(data.Skip(index * sizeof(Int16)).Take(sizeof(Int16)).Reverse().ToArray(), 0);
            return BitConverter.ToInt16(data, index * sizeof(Int16));
        }

        public UInt16 GetUInt16(int index = 0)
        {
            if (index >= data.Length / sizeof(UInt16)) throw new ArgumentOutOfRangeException();
            if (IsBigEndian) return BitConverter.ToUInt16(data.Skip(index * sizeof(UInt16)).Take(sizeof(UInt16)).Reverse().ToArray(), 0);
            return BitConverter.ToUInt16(data, index * sizeof(UInt16));
        }

        public UInt16 GetUInt16H(int index = 0)
        {
            return GetUInt16(index);
        }

        public SByte GetInt8(int index = 0)
        {
            if (index >= data.Length / sizeof(byte)) throw new ArgumentOutOfRangeException();
            return (SByte)data[index];
        }

        public byte GetUInt8(int index = 0)
        {
            if (index >= data.Length / sizeof(byte)) throw new ArgumentOutOfRangeException();
            return data[index];
        }

        public Single GetSingle(int index = 0)
        {
            if (index >= data.Length / sizeof(Single)) throw new ArgumentOutOfRangeException();
            if (IsBigEndian) return BitConverter.ToSingle(data.Skip(index * sizeof(Single)).Take(sizeof(Single)).Reverse().ToArray(), 0);
            return BitConverter.ToSingle(data, index * sizeof(Single));
        }

        public string GetString(int index = 0)
        {
            if (index > 0) throw new ArgumentOutOfRangeException();
            string retVal = BZNEncoding.win1252.GetString(data);
            return retVal;
        }

        public Vector3D GetVector3D(int index = 0)
        {
            if (index >= data.Length / sizeof(Single) / 3) throw new ArgumentOutOfRangeException();
            return new Vector3D() { X = GetSingle(index * 3), Y = GetSingle(index * 3 + 1), Z = GetSingle(index * 3 + 2) };
        }

        public Vector2D GetVector2D(int index = 0)
        {
            if (index >= data.Length / sizeof(Single) / 2) throw new ArgumentOutOfRangeException();
            return new Vector2D() { X = GetSingle(index * 2), Z = GetSingle(index * 2 + 1) };
        }
        private UInt32 GetUInt32Internal(int offset)
        {
            if (offset + sizeof(UInt32) > data.Length) throw new ArgumentOutOfRangeException();
            byte[] buffer = new byte[sizeof(UInt32)];
            Array.Copy(data, offset, buffer, 0, buffer.Length);
            if (IsBigEndian) Array.Reverse(buffer);
            return BitConverter.ToUInt32(buffer);
        }
        private float GetFloatInternal(int offset)
        {
            if (offset + sizeof(float) > data.Length) throw new ArgumentOutOfRangeException();
            byte[] buffer = new byte[sizeof(float)];
            Array.Copy(data, offset, buffer, 0, buffer.Length);
            if (IsBigEndian) Array.Reverse(buffer);
            return BitConverter.ToSingle(buffer);
        }
        private double GetDoubleInternal(int offset)
        {
            if (offset + sizeof(double) > data.Length) throw new ArgumentOutOfRangeException();
            byte[] buffer = new byte[sizeof(double)];
            Array.Copy(data, offset, buffer, 0, buffer.Length);
            if (IsBigEndian) Array.Reverse(buffer);
            return BitConverter.ToDouble(buffer);
        }
        public Matrix GetMatrixOld(int index = 0)
        {
            if (BigPosit)
                return GetMatrixDoubleOld(index);
            return new Matrix()
            {
                RightX = GetSingle(index * 12 +  0),
                RightY = GetSingle(index * 12 +  1),
                RightZ = GetSingle(index * 12 +  2),
                RightW = 0,
                UpX    = GetSingle(index * 12 +  3),
                UpY    = GetSingle(index * 12 +  4),
                UpZ    = GetSingle(index * 12 +  5),
                UpW    = 0,
                FrontX = GetSingle(index * 12 +  6),
                FrontY = GetSingle(index * 12 +  7),
                FrontZ = GetSingle(index * 12 +  8),
                FrontW = 0,
                PositX = GetSingle(index * 12 +  9),
                PositY = GetSingle(index * 12 + 10),
                PositZ = GetSingle(index * 12 + 11),
                PositW = 1,
            };
        }
        private Matrix GetMatrixDoubleOld(int index = 0)
        {
            int stride = sizeof(float) * 8 + sizeof(UInt32) + sizeof(double) * 3;
            return new Matrix()
            {
                RightX = GetFloatInternal(stride * index + 0 * sizeof(float)),
                RightY = GetFloatInternal(stride * index + 1 * sizeof(float)),
                RightZ = GetFloatInternal(stride * index + 2 * sizeof(float)),
                RightW = 0,
                UpX    = GetFloatInternal(stride * index + 3 * sizeof(float)),
                UpY    = GetFloatInternal(stride * index + 4 * sizeof(float)),
                UpZ    = GetFloatInternal(stride * index + 5 * sizeof(float)),
                UpW    = 0,
                FrontX = GetFloatInternal(stride * index + 6 * sizeof(float)),
                FrontY = GetFloatInternal(stride * index + 7 * sizeof(float)),
                FrontZ = GetFloatInternal(stride * index + 8 * sizeof(float)),
                FrontW = 0,
                junk   = GetUInt32Internal(stride * index + 9 * sizeof(float) + 0 * sizeof(UInt32)), // junk padding
                PositX = GetDoubleInternal(stride * index + 9 * sizeof(float) + 1 * sizeof(UInt32) + 0 * sizeof(double)),
                PositY = GetDoubleInternal(stride * index + 9 * sizeof(float) + 1 * sizeof(UInt32) + 1 * sizeof(double)),
                PositZ = GetDoubleInternal(stride * index + 9 * sizeof(float) + 1 * sizeof(UInt32) + 2 * sizeof(double)),
                PositW = 1,
            };
        }
        public Matrix GetMatrix(int index = 0)
        {
            return new Matrix()
            {
               RightX = GetSingle(index * 16 +  0),
               RightY = GetSingle(index * 16 +  1),
               RightZ = GetSingle(index * 16 +  2),
               RightW = GetSingle(index * 16 +  3),
               UpX    = GetSingle(index * 16 +  4),
               UpY    = GetSingle(index * 16 +  5),
               UpZ    = GetSingle(index * 16 +  6),
               UpW    = GetSingle(index * 16 +  7),
               FrontX = GetSingle(index * 16 +  8),
               FrontY = GetSingle(index * 16 +  9),
               FrontZ = GetSingle(index * 16 + 10),
               FrontW = GetSingle(index * 16 + 11),
               PositX = GetSingle(index * 16 + 12),
               PositY = GetSingle(index * 16 + 13),
               PositZ = GetSingle(index * 16 + 14),
               PositW = GetSingle(index * 16 + 15),
            };
        }

        public Euler GetEuler(int index = 0)
        {
            throw new NotImplementedException();
        }

        public byte[] GetBytes(int index = 0, int length = -1)
        {
            if (length == -1)
            {
                if (index >= data.Length) throw new ArgumentOutOfRangeException();
                return data.Skip(index).ToArray();
            }
            if (index + length > data.Length) throw new ArgumentOutOfRangeException();
            return data.Skip(index).Take(length).ToArray();
        }
        public byte[] GetRaw(int index = 0, int length = -1)
        {
            if (length == -1) return data.Skip(index).ToArray();
            return data.Skip(index).Take(length).ToArray();
        }

        public bool IsValidationOnly() { return false; }

        public override string ToString()
        {
            switch (type)
            {
                case BinaryFieldType.DATA_CHAR:
                    {
                        string str = GetString();
                        if (str.Any(dr => char.IsControl(dr)))
                            return $"BINARY\tType: {type.ToString().PadRight(13)}\tValue: {BitConverter.ToString(data.Take(20).ToArray())}{(data.Length > 20 ? "..." : string.Empty)}";
                        return $"BINARY\tType: {type.ToString().PadRight(13)}\tValue: \"{str}\"";
                    }
                case BinaryFieldType.DATA_SHORT: return $"BINARY\tType: {type.ToString().PadRight(13)}\tValue: {BitConverter.ToInt16(data, 0)}";
                case BinaryFieldType.DATA_LONG: return $"BINARY\tType: {type.ToString().PadRight(13)}\tValue: {BitConverter.ToInt32(data, 0)}";
                case BinaryFieldType.DATA_FLOAT: return $"BINARY\tType: {type.ToString().PadRight(13)}\tValue: {BitConverter.ToSingle(data, 0)}";
                case BinaryFieldType.DATA_DOUBLE: return $"BINARY\tType: {type.ToString().PadRight(13)}\tValue: {BitConverter.ToDouble(data, 0)}";
                case BinaryFieldType.DATA_ID: return $"BINARY\tType: {type.ToString().PadRight(13)}\tValue: {BitConverter.ToUInt32(data, 0):X8}";
                case BinaryFieldType.DATA_PTR: return $"BINARY\tType: {type.ToString().PadRight(13)}\tValue: {BitConverter.ToUInt32(data, 0):X8}";
                case BinaryFieldType.DATA_VEC2D:
                    {
                        Vector2D v = GetVector2D();
                        return $"BINARY\tType: {type.ToString().PadRight(13)}\tValue: {{ {v.X}, {v.Z} }}";
                    }
                case BinaryFieldType.DATA_VEC3D:
                    {
                        Vector3D v = GetVector3D();
                        return $"BINARY\tType: {type.ToString().PadRight(13)}\tValue: {{ {v.X}, {v.Y}, {v.Z} }}";
                    }
                case BinaryFieldType.DATA_MAT3DOLD:
                    {
                        Matrix m = GetMatrixOld();
                        return $"BINARY\tType: {type.ToString().PadRight(13)}\tValue: {{ {{ {m.RightX,10:0.00}, {m.RightY,10:0.00}, {m.RightZ,10:0.00} }},\r\n" +
                               $"      \t                   \t         {{ {m.UpX,10:0.00}, {m.UpY,10:0.00}, {m.UpZ,10:0.00} }},\r\n" +
                               $"      \t                   \t         {{ {m.FrontX,10:0.00}, {m.FrontY,10:0.00}, {m.FrontZ,10:0.00} }},\r\n" +
                               $"      \t                   \t         {{ {m.PositX,10:0.00}, {m.PositY,10:0.00}, {m.PositZ,10:0.00} }} }}";
                    }
                case BinaryFieldType.DATA_MAT3D:
                    {
                        Matrix m = GetMatrix();
                        return $"BINARY\tType: {type.ToString().PadRight(13)}\tValue: {{ {{ {m.RightX,10:0.00}, {m.RightY,10:0.00}, {m.RightZ,10:0.00}, {m.RightW,10:0.00} }},\r\n" +
                                $"      \t                   \t         {{ {m.UpX,10:0.00}, {m.UpY,10:0.00}, {m.UpZ,10:0.00}, {m.UpW,10:0.00} }},\r\n" +
                                $"      \t                   \t         {{ {m.FrontX,10:0.00}, {m.FrontY,10:0.00}, {m.FrontZ,10:0.00}, {m.FrontW,10:0.00} }},\r\n" +
                                $"      \t                   \t         {{ {m.PositX,10:0.00}, {m.PositY,10:0.00}, {m.PositZ,10:0.00}, {m.PositW,10:0.00} }} }}";
                    }
            }
            return $"BINARY\tType: {type.ToString().PadRight(13)}\tValue: {BitConverter.ToString(data.Take(20).ToArray())}{(data.Length > 20 ? "..." : string.Empty)}";
        }

        public string GetName()
        {
            throw new NotImplementedException();
        }

        public string GetRawName()
        {
            throw new NotImplementedException();
        }

        public bool Validate(string? name, BinaryFieldType type = BinaryFieldType.DATA_UNKNOWN)
        {
            if (this.type == BinaryFieldType.DATA_UNKNOWN) return true;
            return this.type == type;
        }
    }
}
