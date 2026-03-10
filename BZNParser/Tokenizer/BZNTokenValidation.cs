using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BZNParser.Tokenizer
{
    public class BZNTokenValidation : IBZNToken
    {
        private string name;

        public BZNTokenValidation(string name)
        {
            this.name = name;
        }

        public bool IsBinary => false;

        public int GetCount(int PtrSize) => 0;
        public int GetSubCount(int index = 0) => 0;
        public IBZNToken GetSubToken(int index = 0, int subIndex = 0) { throw new InvalidOperationException("Validation Tokens have no data, check IsValidationOnly() before processing"); }
        public bool GetBoolean(int index = 0) { throw new InvalidOperationException("Validation Tokens have no data, check IsValidationOnly() before processing"); }
        public UInt64 GetUInt64(int index = 0) { throw new InvalidOperationException(); }
        public UInt64 GetUInt64H(int index = 0) => GetUInt64(index);
        public Int32 GetInt32(int index = 0) { throw new InvalidOperationException("Validation Tokens have no data, check IsValidationOnly() before processing"); }
        public Int32 GetInt32H(int index = 0) => GetInt32(index);
        public UInt32 GetUInt32(int index = 0) { throw new InvalidOperationException("Validation Tokens have no data, check IsValidationOnly() before processing"); }
        public UInt32 GetUInt32H(int index = 0) => GetUInt32(index);
        public UInt32 GetUInt32HR(int index = 0) => GetUInt32(index);
        public UInt32 GetUInt32Raw(int index = 0) => GetUInt32(index);
        public Int16 GetInt16(int index = 0) { throw new InvalidOperationException("Validation Tokens have no data, check IsValidationOnly() before processing"); }
        public UInt16 GetUInt16(int index = 0) { throw new InvalidOperationException("Validation Tokens have no data, check IsValidationOnly() before processing"); }
        public UInt16 GetUInt16H(int index = 0) { throw new InvalidOperationException("Validation Tokens have no data, check IsValidationOnly() before processing"); }
        public SByte GetInt8(int index = 0) { throw new InvalidOperationException("Validation Tokens have no data, check IsValidationOnly() before processing"); }
        public byte GetUInt8(int index = 0) { throw new InvalidOperationException("Validation Tokens have no data, check IsValidationOnly() before processing"); }
        public float GetSingle(int index = 0) { throw new InvalidOperationException("Validation Tokens have no data, check IsValidationOnly() before processing"); }
        public string GetString(int index = 0) { throw new InvalidOperationException("Validation Tokens have no data, check IsValidationOnly() before processing"); }
        public Vector3D GetVector3D(int index = 0) { throw new InvalidOperationException("Validation Tokens have no data, check IsValidationOnly() before processing"); }
        public Vector2D GetVector2D(int index = 0) { throw new InvalidOperationException("Validation Tokens have no data, check IsValidationOnly() before processing"); }
        public Matrix GetMatrixOld(int index = 0) { throw new InvalidOperationException("Validation Tokens have no data, check IsValidationOnly() before processing"); }
        public Matrix GetMatrix(int index = 0) { throw new InvalidOperationException("Validation Tokens have no data, check IsValidationOnly() before processing"); }
        public Euler GetEuler(int index = 0) { throw new InvalidOperationException("Validation Tokens have no data, check IsValidationOnly() before processing"); }
        public byte[] GetBytes(int index = 0, int length = -1) { throw new InvalidOperationException("Validation Tokens have no data, check IsValidationOnly() before processing"); }
        public byte[] GetRaw(int index = 0, int length = -1) { throw new InvalidOperationException("Validation Tokens have no data, check IsValidationOnly() before processing"); }

        public string GetName()
        {
            return name.Trim();
        }

        public string GetRawName()
        {
            return name;
        }

        public bool IsValidationOnly() { return true; }

        public override string ToString()
        {
            return "VALDT\t[" + name + "]";
        }

        public bool Validate(string? name, BinaryFieldType type = BinaryFieldType.DATA_UNKNOWN)
        {
            return this.name == name;
        }
    }
}
