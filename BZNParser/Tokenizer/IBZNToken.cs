using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BZNParser.Tokenizer
{
    public interface IBZNToken
    {
        [Obsolete("Remove this flag later")]
        bool IsBinary { get; }
        int GetCount();
        [Obsolete("Remove this flag later")]
        int GetSubCount(int index = 0);
        [Obsolete("Remove this flag later")]
        IBZNToken GetSubToken(int index = 0, int subIndex = 0);
        [Obsolete("Remove this flag later")]
        bool GetBoolean(int index = 0);
        [Obsolete("Remove this flag later")]
        UInt64 GetUInt64(int index = 0);
        [Obsolete("Remove this flag later")]
        UInt64 GetUInt64H(int index = 0);
        [Obsolete("Remove this flag later")]
        Int32 GetInt32(int index = 0);
        [Obsolete("Remove this flag later")]
        Int32 GetInt32H(int index = 0);
        [Obsolete("Remove this flag later")]
        UInt32 GetUInt32(int index = 0);
        [Obsolete("Remove this flag later")]
        UInt32 GetUInt32H(int index = 0);
        
        /// <summary>
        /// For UInt32s stored as raw bytes or hex strings
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        [Obsolete("Remove this flag later")]
        UInt32 GetUInt32HR(int index = 0);
        [Obsolete("Remove this flag later")]
        UInt32 GetUInt32Raw(int index = 0);
        [Obsolete("Remove this flag later")]
        Int16 GetInt16(int index = 0);
        [Obsolete("Remove this flag later")]
        UInt16 GetUInt16(int index = 0);
        [Obsolete("Remove this flag later")]
        UInt16 GetUInt16H(int index = 0);
        [Obsolete("Remove this flag later")]
        SByte GetInt8(int index = 0);
        [Obsolete("Remove this flag later")]
        byte GetUInt8(int index = 0);
        [Obsolete("Remove this flag later")]
        string GetString(int index = 0);
        [Obsolete("Remove this flag later")]
        float GetSingle(int index = 0);
        [Obsolete("Remove this flag later")]
        Vector3D GetVector3D(int index = 0);
        [Obsolete("Remove this flag later")]
        Vector2D GetVector2D(int index = 0);
        [Obsolete("Remove this flag later")]
        Matrix GetMatrixOld(int index = 0);
        [Obsolete("Remove this flag later")]
        Matrix GetMatrix(int index = 0);
        [Obsolete("Remove this flag later")]
        Euler GetEuler(int index = 0);
        [Obsolete("Remove this flag later")]
        byte[] GetBytes(int index = 0, int length = -1);
        [Obsolete("Remove this flag later")]
        byte[] GetRaw(int index = 0, int length = -1);

        [Obsolete("Remove this flag later")]
        string GetName();
        [Obsolete("Remove this flag later")]
        string GetRawName();

        bool IsValidationOnly();
        bool Validate(string? name, BinaryFieldType type = BinaryFieldType.DATA_UNKNOWN);
    }
}
