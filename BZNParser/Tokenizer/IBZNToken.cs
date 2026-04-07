using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BZNParser.Tokenizer
{
    public interface IBZNToken
    {
        bool IsBinary { get; }
        int GetCount();
        int GetCount(BinaryFieldType type);
        int GetSubCount(int index = 0);
        IBZNToken GetSubToken(int index = 0, int subIndex = 0);
        bool GetBoolean(int index = 0);
        UInt64 GetUInt64(int index = 0);
        UInt64 GetUInt64H(int index = 0);
        Int32 GetInt32(int index = 0);
        Int32 GetInt32H(int index = 0);
        UInt32 GetUInt32(int index = 0);
        UInt32 GetUInt32H(int index = 0);
        
        /// <summary>
        /// For UInt32s stored as raw bytes or hex strings
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        UInt32 GetUInt32HR(int index = 0);
        UInt32 GetUInt32Raw(int index = 0);
        Int16 GetInt16(int index = 0);
        UInt16 GetUInt16(int index = 0);
        UInt16 GetUInt16H(int index = 0);
        SByte GetInt8(int index = 0);
        byte GetUInt8(int index = 0);
        string GetString(int index = 0);
        float GetSingle(int index = 0);
        Vector3D GetVector3D(int index = 0);
        Vector2D GetVector2D(int index = 0);
        Matrix GetMatrixOld(int index = 0);
        Matrix GetMatrix(int index = 0);
        Euler GetEuler(int index = 0);
        byte[] GetBytes(int index = 0, int length = -1);
        byte[] GetRaw(int index = 0, int length = -1);

        string GetName();
        string GetRawName();

        bool IsValidationOnly();
        bool Validate(string? name, BinaryFieldType type = BinaryFieldType.DATA_UNKNOWN);
    }
}
