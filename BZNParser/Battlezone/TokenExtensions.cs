using BZNParser.Battlezone;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using static BZNParser.Tokenizer.BZNStreamReader;
using static BZNParser.Tokenizer.IMalformable;
using Int8 = sbyte;
using UInt8 = byte;

namespace BZNParser.Tokenizer;


// These functions should return the cleaned value and set the value on the property if the parent instance is set
public static class TokenExtensions
{
    public static (TProp stored, Vector3D raw) ApplyVector3D<T, TProp>(this IBZNToken tok, T? parent, Expression<Func<T, TProp>>? property, int index = 0, FloatTextFormat format = FloatTextFormat.G) where T : IMalformable
    {
        PropertyInfo? propInfo = null;
        if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo_)
            propInfo = propInfo_;

        Vector3D value = tok.GetVector3D(index);

        TProp setVal = default!;
        bool did = false;
        if (typeof(TProp) == typeof(Vector3D) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(Vector3D))
        {
            setVal = (TProp)(object)value;
            did = true;
        }
        else if (typeof(TProp).IsArray && typeof(TProp).GetElementType() == typeof(Vector3D))
        {
            Vector3D[]? arr = (Vector3D[]?)propInfo?.GetValue(parent);
            if (arr != null && index >= 0 && index < arr.Length)
            {
                arr[index] = value;
                setVal = (TProp)(object)arr;
                did = true;
            }
        }

        // store the value into the property if possible
        if (parent != null && propInfo != null && did)
            propInfo.SetValue(parent, setVal);

        // we can't process anything, so just serve the vector as is
        if (parent == null || propInfo == null)
            return (setVal, value);

        // binary doesn't have subtokens, it's just a blob of data
        if (tok.IsBinary)
            return (setVal, value);

        // re-load the values again from the reader to let them pass through our malformation handlers
        IBZNToken subTok;
        subTok = tok.GetSubToken(index, 0); subTok.ApplySingle(value, x => x.X, format: format); if (subTok.GetRawName() != @"  x") { value.Malformations.AddIncorrectName<Vector3D, float>(x => x.X, subTok.GetRawName()); }
        subTok = tok.GetSubToken(index, 1); subTok.ApplySingle(value, x => x.Y, format: format); if (subTok.GetRawName() != @"  y") { value.Malformations.AddIncorrectName<Vector3D, float>(x => x.Y, subTok.GetRawName()); }
        subTok = tok.GetSubToken(index, 2); subTok.ApplySingle(value, x => x.Z, format: format); if (subTok.GetRawName() != @"  z") { value.Malformations.AddIncorrectName<Vector3D, float>(x => x.Z, subTok.GetRawName()); }

        return (setVal, value);
    }

    public static (TProp stored, Vector2D raw) ReadVector2D<T, TProp>(this IBZNToken tok, T? parent, Expression<Func<T, TProp>>? property, int index = 0, FloatTextFormat format = FloatTextFormat.G) where T : IMalformable
    {
        PropertyInfo? propInfo = null;
        if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo_)
            propInfo = propInfo_;

        Vector2D value = tok.GetVector2D(index);

        TProp setVal = default!;
        bool did = false;
        if (typeof(TProp) == typeof(Vector2D) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(Vector2D))
        {
            setVal = (TProp)(object)value;
            did = true;
        }
        else if (typeof(TProp).IsArray && typeof(TProp).GetElementType() == typeof(Vector2D))
        {
            Vector2D[]? arr = (Vector2D[]?)propInfo?.GetValue(parent);
            if (arr != null && index >= 0 && index < arr.Length)
            {
                arr[index] = value;
                setVal = (TProp)(object)arr;
                did = true;
            }
        }

        // store the value into the property if possible
        if (parent != null && propInfo != null && did)
            propInfo.SetValue(parent, setVal);

        // we can't process anything, so just serve the vector as is
        if (parent == null || propInfo == null)
            return (setVal, value);

        // binary doesn't have subtokens, it's just a blob of data
        if (tok.IsBinary)
            return (setVal, value);

        // re-load the values again from the reader to let them pass through our malformation handlers
        IBZNToken subTok;
        subTok = tok.GetSubToken(index, 0); subTok.ApplySingle(value, x => x.X, format: format); if (subTok.GetRawName() != @"  x") { value.Malformations.AddIncorrectName<Vector2D, float>(x => x.X, subTok.GetRawName()); }
        subTok = tok.GetSubToken(index, 1); subTok.ApplySingle(value, x => x.Z, format: format); if (subTok.GetRawName() != @"  z") { value.Malformations.AddIncorrectName<Vector2D, float>(x => x.Z, subTok.GetRawName()); }

        return (setVal, value);
    }

    /// <summary>
    /// Read a Single from an <see cref="IBZNToken"/> and optionally set it on a property of a parent object,
    /// while also checking for common malformations.
    /// </summary>
    /// <remarks>
    /// Handles the following malformations: <see cref="Malformation.INCORRECT_TEXT"/>
    /// </remarks>
    /// <typeparam name="T">Type that contains the target property and implements <see cref="IMalformable"/></typeparam>
    /// <typeparam name="TProp">Property type</typeparam>
    /// <param name="tok">Token</param>
    /// <param name="parent"><see cref="IMalformable"/> instance containing properties</param>
    /// <param name="property">Lambda to access the property to register malformations to and apply the value</param>
    /// <param name="index">Index of the value in the token</param>
    /// <param name="convert">Optional conversion function for the read boolean</param>
    /// <returns></returns>
    public static (TProp stored, Single raw) ApplySingle<T, TProp>(this IBZNToken tok, T? parent, Expression<Func<T, TProp>>? property, int index = 0, Func<Single, TProp>? convert = null, FloatTextFormat format = FloatTextFormat.G) where T : IMalformable
    {
        PropertyInfo? propInfo = null;
        if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo_)
            propInfo = propInfo_;

        Single valueInternal = tok.GetSingle(index);
        if (tok.IsBinary)
        {
            // no binary exclusive paths yet
        }
        else
        {
            if (propInfo != null && parent != null)
            {
                string textValue = valueInternal.ToBZNString(format);
                // basic string issue like True vs true
                string rawString = tok.GetString(index);
                if (!string.Equals(textValue, rawString, StringComparison.Ordinal))
                    parent.Malformations.AddIncorrectTextParse(property, index, rawString);
            }
        }

        TProp setVal = default!;
        bool did = false;
        if (convert != null)
        {
            setVal = convert(valueInternal);
            did = true;
        }
        else if (typeof(TProp) == typeof(Single) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(Single))
        {
            setVal = (TProp)(object)(Single)valueInternal;
            did = true;
        }
        else if (typeof(TProp).IsGenericType && typeof(TProp).GetGenericTypeDefinition() == typeof(DualModeValue<,>))
        {
            var genericArgs = typeof(TProp).GetGenericArguments();
            if (genericArgs[0] == typeof(Single))
            {
                setVal = (TProp)Activator.CreateInstance(typeof(TProp), valueInternal)!;
                did = true;
            }
            else if (genericArgs[1] == typeof(Single))
            {
                setVal = (TProp)Activator.CreateInstance(typeof(TProp), valueInternal)!;
                did = true;
            }
        }

        if (propInfo != null && parent != null && did)
            propInfo.SetValue(parent, setVal);

        return (setVal, valueInternal);
    }

    /// <summary>
    /// Read a Int32 from an <see cref="IBZNToken"/> and optionally set it on a property of a parent object,
    /// while also checking for common malformations.
    /// </summary>
    /// <remarks>
    /// Handles the following malformations: <see cref="Malformation.INCORRECT_TEXT"/>
    /// </remarks>
    /// <typeparam name="T">Type that contains the target property and implements <see cref="IMalformable"/></typeparam>
    /// <typeparam name="TProp">Property type</typeparam>
    /// <param name="tok">Token</param>
    /// <param name="parent"><see cref="IMalformable"/> instance containing properties</param>
    /// <param name="property">Lambda to access the property to register malformations to and apply the value</param>
    /// <param name="index">Index of the value in the token</param>
    /// <param name="convert">Optional conversion function for the read boolean</param>
    /// <returns></returns>
    public static (TProp stored, Int32 raw) ApplyInt32<T, TProp>(this IBZNToken tok, T? parent, Expression<Func<T, TProp>>? property, int index = 0, Func<Int32, TProp>? convert = null) where T : IMalformable
    {
        PropertyInfo? propInfo = null;
        if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo_)
            propInfo = propInfo_;

        Int32 valueInternal = tok.GetInt32(index);
        string textValue = valueInternal.ToString();
        if (tok.IsBinary)
        {
            // no binary exclusive paths yet
        }
        else
        {
            if (propInfo != null && parent != null)
            {
                // basic string issue like True vs true
                string rawString = tok.GetString(index);
                if (!string.Equals(textValue, rawString, StringComparison.Ordinal))
                    parent.Malformations.AddIncorrectTextParse(property, index, rawString);
            }
        }

        TProp setVal = default!;
        bool did = false;
        if (convert != null)
        {
            setVal = convert(valueInternal);
            did = true;
        }
        else if (typeof(TProp) == typeof(Int8) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(Int8))
        {
            setVal = (TProp)(object)(Int8)valueInternal;
            did = true;
        }
        else if (typeof(TProp) == typeof(Int16) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(Int16))
        {
            setVal = (TProp)(object)(Int16)valueInternal;
            did = true;
        }
        else if (typeof(TProp) == typeof(Int32) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(Int32))
        {
            setVal = (TProp)(object)(Int32)valueInternal;
            did = true;
        }
        else if (typeof(TProp) == typeof(Int64) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(Int64))
        {
            setVal = (TProp)(object)(Int64)valueInternal;
            did = true;
        }
        else if (typeof(TProp).IsGenericType && typeof(TProp).GetGenericTypeDefinition() == typeof(DualModeValue<,>))
        {
            var genericArgs = typeof(TProp).GetGenericArguments();
            if (genericArgs[0] == typeof(Int32))
            {
                setVal = (TProp)Activator.CreateInstance(typeof(TProp), valueInternal)!;
                did = true;
            }
            else if (genericArgs[1] == typeof(Int32))
            {
                setVal = (TProp)Activator.CreateInstance(typeof(TProp), valueInternal)!;
                did = true;
            }
        }

        if (propInfo != null && parent != null && did)
            propInfo.SetValue(parent, setVal);

        return (setVal, valueInternal);
    }

    public static (TProp stored, UInt32 raw) ApplyUInt32H8<T, TProp>(this IBZNToken tok, T? parent, Expression<Func<T, TProp>>? property, int index = 0, Func<UInt32, TProp>? convert = null) where T : IMalformable
    {
        PropertyInfo? propInfo = null;
        if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo_)
            propInfo = propInfo_;

        UInt32 valueInternal = tok.GetUInt32H(index);
        string textValue = valueInternal.ToString("X8");
        if (tok.IsBinary)
        {
            // no binary exclusive paths yet
        }
        else
        {
            if (propInfo != null && parent != null)
            {
                // basic string issue like True vs true
                string rawString = tok.GetString(index);
                if (!string.Equals(textValue, rawString, StringComparison.Ordinal))
                    parent.Malformations.AddIncorrectTextParse(property, index, rawString);
            }
        }

        TProp setVal = default!;
        bool did = false;
        if (convert != null)
        {
            setVal = convert(valueInternal);
            did = true;
        }
        else if (typeof(TProp) == typeof(UInt8) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt8))
        {
            setVal = (TProp)(object)(UInt8)valueInternal;
            did = true;
        }
        else if (typeof(TProp) == typeof(UInt16) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt16))
        {
            setVal = (TProp)(object)(UInt16)valueInternal;
            did = true;
        }
        else if (typeof(TProp) == typeof(UInt32) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt32))
        {
            setVal = (TProp)(object)(UInt32)valueInternal;
            did = true;
        }
        else if (typeof(TProp) == typeof(UInt64) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt64))
        {
            setVal = (TProp)(object)(UInt64)valueInternal;
            did = true;
        }

        if (propInfo != null && parent != null && did)
            propInfo.SetValue(parent, setVal);

        return (setVal, valueInternal);
    }
    public static (TProp stored, UInt32 raw) ApplyUInt32h<T, TProp>(
        this IBZNToken tok,
        T? parent,
        Expression<Func<T, TProp>>? property,
        int index = 0,
        Func<UInt32, TProp>? convert = null
    ) where T : IMalformable
    {
        PropertyInfo? propInfo = null;
        if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo_)
            propInfo = propInfo_;

        UInt32 valueInternal = tok.GetUInt32H(index);
        string textValue = valueInternal.ToString("x");
        if (tok.IsBinary)
        {
            // no binary exclusive paths yet
        }
        else
        {
            if (propInfo != null && parent != null)
            {
                // basic string issue like True vs true
                string rawString = tok.GetString(index);
                if (!string.Equals(textValue, rawString, StringComparison.Ordinal))
                    parent.Malformations.AddIncorrectTextParse(property, index, rawString);
            }
        }

        TProp setVal = default!;
        bool did = false;

        // Array handling (including nullable arrays)
        if (typeof(TProp).IsArray && typeof(TProp).GetElementType() == typeof(UInt32))
        {
            var arr = (UInt32[]?)propInfo?.GetValue(parent);
            if (arr != null && index >= 0 && index < arr.Length)
            {
                arr[index] = valueInternal;
                setVal = (TProp)(object)arr;
                did = true;
            }
        }
        else if (convert != null)
        {
            setVal = convert(valueInternal);
            did = true;
        }
        else if (typeof(TProp) == typeof(UInt8) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt8))
        {
            setVal = (TProp)(object)(UInt8)valueInternal;
            did = true;
        }
        else if (typeof(TProp) == typeof(UInt16) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt16))
        {
            setVal = (TProp)(object)(UInt16)valueInternal;
            did = true;
        }
        else if (typeof(TProp) == typeof(UInt32) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt32))
        {
            setVal = (TProp)(object)(UInt32)valueInternal;
            did = true;
        }
        else if (typeof(TProp) == typeof(UInt64) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt64))
        {
            setVal = (TProp)(object)(UInt64)valueInternal;
            did = true;
        }

        if (propInfo != null && parent != null && did)
            propInfo.SetValue(parent, setVal);

        return (setVal, valueInternal);
    }

    /// <summary>
    /// Read a UInt32 from an <see cref="IBZNToken"/> and optionally set it on a property of a parent object,
    /// while also checking for common malformations.
    /// </summary>
    /// <remarks>
    /// Handles the following malformations: <see cref="Malformation.INCORRECT_TEXT"/>
    /// </remarks>
    /// <typeparam name="T">Type that contains the target property and implements <see cref="IMalformable"/></typeparam>
    /// <typeparam name="TProp">Property type</typeparam>
    /// <param name="tok">Token</param>
    /// <param name="parent"><see cref="IMalformable"/> instance containing properties</param>
    /// <param name="property">Lambda to access the property to register malformations to and apply the value</param>
    /// <param name="index">Index of the value in the token</param>
    /// <param name="convert">Optional conversion function for the read boolean</param>
    /// <returns></returns>
    public static (TProp stored, UInt32 raw) ApplyUInt32<T, TProp>(this IBZNToken tok, T? parent, Expression<Func<T, TProp>>? property, int index = 0, Func<UInt32, TProp>? convert = null) where T : IMalformable
    {
        PropertyInfo? propInfo = null;
        if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo_)
            propInfo = propInfo_;

        UInt32 valueInternal = tok.GetUInt32(index);
        string textValue = valueInternal.ToString();
        if (tok.IsBinary)
        {
            // no binary exclusive paths yet
        }
        else
        {
            if (propInfo != null && parent != null)
            {
                // basic string issue like True vs true
                string rawString = tok.GetString(index);
                if (!string.Equals(textValue, rawString, StringComparison.Ordinal))
                    parent.Malformations.AddIncorrectTextParse(property, index, rawString);
            }
        }

        TProp setVal = default!;
        bool did = false;
        if (convert != null)
        {
            setVal = convert(valueInternal);
            did = true;
        }
        else if (typeof(TProp) == typeof(UInt8) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt8))
        {
            setVal = (TProp)(object)(UInt8)valueInternal;
            did = true;
        }
        else if (typeof(TProp) == typeof(UInt16) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt16))
        {
            setVal = (TProp)(object)(UInt16)valueInternal;
            did = true;
        }
        else if (typeof(TProp) == typeof(UInt32) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt32))
        {
            setVal = (TProp)(object)(UInt32)valueInternal;
            did = true;
        }
        else if (typeof(TProp) == typeof(UInt64) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt64))
        {
            setVal = (TProp)(object)(UInt64)valueInternal;
            did = true;
        }

        if (propInfo != null && parent != null && did)
            propInfo.SetValue(parent, setVal);

        return (setVal, valueInternal);
    }

    public static (TProp stored, UInt16 raw) ApplyUInt16h<T, TProp>(this IBZNToken tok, T? parent, Expression<Func<T, TProp>>? property, int index = 0, Func<UInt16, TProp>? convert = null) where T : IMalformable
    {
        PropertyInfo? propInfo = null;
        if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo_)
            propInfo = propInfo_;

        UInt16 valueInternal = tok.GetUInt16H(index);
        string textValue = valueInternal.ToString("x");
        if (tok.IsBinary)
        {
            // no binary exclusive paths yet
        }
        else
        {
            if (propInfo != null && parent != null)
            {
                // basic string issue like True vs true
                string rawString = tok.GetString(index);
                if (!string.Equals(textValue, rawString, StringComparison.Ordinal))
                    parent.Malformations.AddIncorrectTextParse(property, index, rawString);
            }
        }

        TProp setVal = default!;
        bool did = false;
        if (convert != null)
        {
            setVal = convert(valueInternal);
            did = true;
        }
        else if (typeof(TProp) == typeof(UInt8) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt8))
        {
            setVal = (TProp)(object)(UInt8)valueInternal;
            did = true;
        }
        else if (typeof(TProp) == typeof(UInt16) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt16))
        {
            setVal = (TProp)(object)(UInt16)valueInternal;
            did = true;
        }
        else if (typeof(TProp) == typeof(UInt32) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt32))
        {
            setVal = (TProp)(object)(UInt32)valueInternal;
            did = true;
        }
        else if (typeof(TProp) == typeof(UInt64) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt64))
        {
            setVal = (TProp)(object)(UInt64)valueInternal;
            did = true;
        }

        if (propInfo != null && parent != null && did)
            propInfo.SetValue(parent, setVal);

        return (setVal, valueInternal);
    }
    public static (TProp stored, UInt16 raw) ApplyUInt16<T, TProp>(this IBZNToken tok, T? parent, Expression<Func<T, TProp>>? property, int index = 0, Func<UInt16, TProp>? convert = null) where T : IMalformable
    {
        PropertyInfo? propInfo = null;
        if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo_)
            propInfo = propInfo_;

        UInt16 valueInternal = tok.GetUInt16(index);
        string textValue = valueInternal.ToString();
        if (tok.IsBinary)
        {
            // no binary exclusive paths yet
        }
        else
        {
            if (propInfo != null && parent != null)
            {
                // basic string issue like True vs true
                string rawString = tok.GetString(index);
                if (!string.Equals(textValue, rawString, StringComparison.Ordinal))
                    parent.Malformations.AddIncorrectTextParse(property, index, rawString);
            }
        }

        TProp setVal = default!;
        bool did = false;
        if (convert != null)
        {
            setVal = convert(valueInternal);
            did = true;
        }
        else if (typeof(TProp) == typeof(UInt8) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt8))
        {
            setVal = (TProp)(object)(UInt8)valueInternal;
            did = true;
        }
        else if (typeof(TProp) == typeof(UInt16) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt16))
        {
            setVal = (TProp)(object)(UInt16)valueInternal;
            did = true;
        }
        else if (typeof(TProp) == typeof(UInt32) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt32))
        {
            setVal = (TProp)(object)(UInt32)valueInternal;
            did = true;
        }
        else if (typeof(TProp) == typeof(UInt64) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt64))
        {
            setVal = (TProp)(object)(UInt64)valueInternal;
            did = true;
        }

        if (propInfo != null && parent != null && did)
            propInfo.SetValue(parent, setVal);

        return (setVal, valueInternal);
    }
    public static (TProp stored, Int8 raw) ApplyInt8<T, TProp>(this IBZNToken tok, T? parent, Expression<Func<T, TProp>>? property, int index = 0, Func<Int8, TProp>? convert = null) where T : IMalformable
    {
        PropertyInfo? propInfo = null;
        if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo_)
            propInfo = propInfo_;

        Int8 valueInternal = tok.GetInt8(index);
        string textValue = valueInternal.ToString();
        if (tok.IsBinary)
        {
            // no binary exclusive paths yet
        }
        else
        {
            if (propInfo != null && parent != null)
            {
                // basic string issue like True vs true
                string rawString = tok.GetString(index);
                if (!string.Equals(textValue, rawString, StringComparison.Ordinal))
                    parent.Malformations.AddIncorrectTextParse(property, index, rawString);
            }
        }

        TProp setVal = default!;
        bool did = false;
        if (convert != null)
        {
            setVal = convert(valueInternal);
            did = true;
        }
        else if (typeof(TProp) == typeof(Int8) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(Int8))
        {
            setVal = (TProp)(object)(Int8)valueInternal;
            did = true;
        }
        else if (typeof(TProp) == typeof(Int16) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(Int16))
        {
            setVal = (TProp)(object)(Int16)valueInternal;
            did = true;
        }
        else if (typeof(TProp) == typeof(Int32) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(Int32))
        {
            setVal = (TProp)(object)(Int32)valueInternal;
            did = true;
        }
        else if (typeof(TProp) == typeof(Int64) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(Int64))
        {
            setVal = (TProp)(object)(Int64)valueInternal;
            did = true;
        }

        if (propInfo != null && parent != null && did)
            propInfo.SetValue(parent, setVal);

        return (setVal, valueInternal);
    }

    /// <summary>
    /// Read a UInt8 from an <see cref="IBZNToken"/> and optionally set it on a property of a parent object,
    /// while also checking for common malformations.
    /// </summary>
    /// <remarks>
    /// Handles the following malformations: <see cref="Malformation.INCORRECT_TEXT"/>
    /// </remarks>
    /// <typeparam name="T">Type that contains the target property and implements <see cref="IMalformable"/></typeparam>
    /// <typeparam name="TProp">Property type</typeparam>
    /// <param name="tok">Token</param>
    /// <param name="parent"><see cref="IMalformable"/> instance containing properties</param>
    /// <param name="property">Lambda to access the property to register malformations to and apply the value</param>
    /// <param name="index">Index of the value in the token</param>
    /// <param name="convert">Optional conversion function for the read boolean</param>
    /// <returns></returns>
    public static (TProp stored, UInt8 raw) ApplyUInt8<T, TProp>(this IBZNToken tok, T? parent, Expression<Func<T, TProp>>? property, int index = 0, Func<UInt8, TProp>? convert = null) where T : IMalformable
    {
        PropertyInfo? propInfo = null;
        if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo_)
            propInfo = propInfo_;

        UInt8 valueInternal = tok.GetUInt8(index);
        string textValue = valueInternal.ToString();
        if (tok.IsBinary)
        {
            // no binary exclusive paths yet
        }
        else
        {
            if (propInfo != null && parent != null)
            {
                // basic string issue like True vs true
                string rawString = tok.GetString(index);
                if (!string.Equals(textValue, rawString, StringComparison.Ordinal))
                    parent.Malformations.AddIncorrectTextParse(property, index, rawString);
            }
        }

        TProp setVal = default!;
        bool did = false;
        if (convert != null)
        {
            setVal = convert(valueInternal);
            did = true;
        }
        else if (typeof(TProp) == typeof(UInt8) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt8))
        {
            setVal = (TProp)(object)(UInt8)valueInternal;
            did = true;
        }
        else if (typeof(TProp) == typeof(UInt16) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt16))
        {
            setVal = (TProp)(object)(UInt16)valueInternal;
            did = true;
        }
        else if (typeof(TProp) == typeof(UInt32) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt32))
        {
            setVal = (TProp)(object)(UInt32)valueInternal;
            did = true;
        }
        else if (typeof(TProp) == typeof(UInt64) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt64))
        {
            setVal = (TProp)(object)(UInt64)valueInternal;
            did = true;
        }

        if (propInfo != null && parent != null && did)
            propInfo.SetValue(parent, setVal);

        return (setVal, valueInternal);
    }
    public static (TProp stored, UInt64 raw) ApplyID<T, TProp>(this IBZNToken tok, T? parent, Expression<Func<T, TProp>>? property, int index = 0, Func<UInt64, TProp>? convert = null) where T : IMalformable
    {
        PropertyInfo? propInfo = null;
        if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo_)
            propInfo = propInfo_;

        // Read the raw bytes (always 8 bytes for ID)
        byte[] rawBytes;
        if (tok.IsBinary)
        {
            rawBytes = tok.GetBytes(index, -1);
        }
        else
        {
            rawBytes = tok.GetRaw(0, -1);
            if (rawBytes.Length > 8)
            {
                // bugged path!
                // Probably not converting these properly
                if (parent != null)
                    parent.Malformations.AddIncorrectRaw(property, index, rawBytes);
            
                string utf8Str = Encoding.UTF8.GetString(rawBytes);
                byte[] newRawBytes = BZNEncoding.win1252.GetBytes(utf8Str);
                rawBytes = newRawBytes;
            }
            else
            {

            }

            byte[] strBytes = rawBytes;
            rawBytes = new byte[8];
            int len = Math.Min(strBytes.Length, 8);
            Array.Copy(strBytes, rawBytes, len);
        }

        // Interpret as UInt64
        UInt64 valueUInt64 = BitConverter.ToUInt64(rawBytes, 0);

        // Interpret as string (up to first null)
        string valueStringRaw = BZNEncoding.win1252.GetString(rawBytes);
        int nullIdx = valueStringRaw.IndexOf('\0');
        string valueString = nullIdx >= 0 ? valueStringRaw.Substring(0, nullIdx) : valueStringRaw;

        // Malformation: garbage after first null in string mode
        bool hasGarbageAfterNull = false;
        if ((typeof(TProp) == typeof(string) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(string) ||
             typeof(TProp) == typeof(SizedString) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(SizedString))
            && nullIdx >= 0 && rawBytes.Skip(nullIdx + 1).Any(b => b != 0))
        {
            hasGarbageAfterNull = true;
        }

        // Malformation: empty string in BZNTokenString means all 0x00 in UInt64 mode
        bool isEmptyString = false;
        if (tok is BZNTokenString)
        {
            if (typeof(TProp) == typeof(string) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(string) ||
                typeof(TProp) == typeof(SizedString) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(SizedString))
                isEmptyString = valueString.Length == 0;
            else
                isEmptyString = rawBytes.All(b => b == 0);
        }

        // Register malformations
        if (propInfo != null && parent != null)
        {
            if (hasGarbageAfterNull)
                parent.Malformations.AddIncorrectRaw<T, TProp>(property, index, rawBytes);
        }

        if (parent != null && property != null)
        {
            var tmp = tok as BZNTokenString;
            if (tmp != null && tmp.RightTrimmedOneLiner)
                parent.Malformations.AddRightTrimmed(property, index);
        }

        // Prepare the return value
        TProp finalValue = default!;
        bool finalValueReady = false;
        if (convert != null)
        {
            finalValue = convert(valueUInt64);
            finalValueReady = true;
        }
        else
        {
            if (typeof(TProp) == typeof(string) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(string))
            {
                finalValue = (TProp)(object)valueString;
                finalValueReady = true;
            }
            else if (typeof(TProp) == typeof(SizedString) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(SizedString))
            {
                finalValue = (TProp)(object)new SizedString(valueString);
                finalValueReady = true;
            }
            else if (typeof(TProp) == typeof(UInt64) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(UInt64))
            {
                finalValue = (TProp)(object)valueUInt64;
                finalValueReady = true;
            }
        }

        // Set the property if possible
        if (propInfo != null && parent != null && finalValueReady)
            propInfo.SetValue(parent, finalValue);

        // Always return the UInt64 raw value for consistency with the new function signature
        return (finalValue, valueUInt64);
    }

    /// <summary>
    /// Read a chars string from an <see cref="IBZNToken"/> and optionally set it on a property of a parent object,
    /// while also checking for common malformations.
    /// </summary>
    /// <remarks>
    /// Handles the following malformations: <see cref="Malformation.INCORRECT_RAW"/>, <see cref="Malformation.RIGHT_TRIM"/>
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProp"></typeparam>
    /// <param name="tok"></param>
    /// <param name="parent"></param>
    /// <param name="property"></param>
    /// <param name="index"></param>
    /// <param name="convert"></param>
    /// <returns></returns>
    public static (TProp stored, string raw) ApplyChars<T, TProp>(this IBZNToken tok, T? parent, Expression<Func<T, TProp>>? property, int index = 0, Func<string, TProp>? convert = null) where T : IMalformable
    {
        PropertyInfo? propInfo = null;
        if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo_)
            propInfo = propInfo_;

        string valueInternal = tok.GetString(index);
        string valueProcessed = valueInternal;

        // clean up intake data
        int idx = valueProcessed.IndexOf('\0');
        if (idx > -1)
            valueProcessed = valueProcessed.Substring(0, idx);

        // register malformations if possible
        if (propInfo != null && parent != null)
        {
            // the processed value doesn't match the internal value, log the malformation
            if (!string.Equals(valueInternal, valueProcessed, StringComparison.Ordinal))
                parent.Malformations.AddIncorrectRaw<T, TProp>(property, index, BZNEncoding.win1252.GetBytes(valueInternal));
        }

        if (parent != null && property != null)
        {
            var tmp = tok as BZNTokenString;
            if (tmp != null)
            {
                if (tmp.RightTrimmedOneLiner)
                    parent.Malformations.AddRightTrimmed(property, index);
            }
        }

        TProp finalValue = default!;
        bool finalValueReady = false;
        if (convert != null)
        {
            // if a converter is given, use it on the raw value
            finalValue = convert(valueProcessed);
            finalValueReady = true;
        }
        else
        {
            // no converter so try to cast
            if (typeof(TProp) == typeof(string) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(string))
            {
                finalValue = (TProp)(object)valueProcessed;
                finalValueReady = true;
            }
            else if (typeof(TProp) == typeof(SizedString) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(SizedString))
            {
                finalValue = (TProp)(object)new SizedString(valueProcessed);
                finalValueReady = true;
            }
        }

        // apply the value if possible
        if (propInfo != null && parent != null && finalValueReady)
            propInfo.SetValue(parent, finalValue);

        // always return processed data, even if we didn't attach it to the property or store malformations, we still read the value
        return (finalValue, valueProcessed);
    }

    public static (TProp stored, byte[] raw) ApplyVoidBytes<T, TProp>(
        this IBZNToken tok,
        T? parent,
        Expression<Func<T, TProp>>? property,
        int index = 0,
        Func<byte[], TProp>? convert = null,
        char? expectedCase = null
    ) where T : IMalformable
    {
        PropertyInfo? propInfo = null;
        if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo_)
            propInfo = propInfo_;

        // Read the raw bytes from the token
        byte[] valueInternal = tok.GetBytes(index, -1);

        TProp setVal = default!;
        bool did = false;
        if (convert != null)
        {
            setVal = convert(valueInternal);
            did = true;
        }
        else if (typeof(TProp) == typeof(byte[]) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(byte[]))
        {
            setVal = (TProp)(object)valueInternal;
            did = true;
        }

        if (tok.IsBinary)
        {
            // no binary exclusive paths yet
        }
        else
        {
            if (propInfo != null && parent != null)
            {
                bool textMalformationHandled = false;
                if (expectedCase.HasValue)
                {
                    string rawString = tok.GetString(index);

                    string upperString = rawString.ToUpperInvariant();
                    string lowerString = rawString.ToLowerInvariant();

                    if (expectedCase == 'L')
                    {
                        if (rawString != lowerString)
                        {
                            if (rawString == upperString)
                            {
                                parent.Malformations.AddIncorrectCase(property, index, 'U');
                            }
                            else
                            {
                                parent.Malformations.AddIncorrectTextParse(property, index, rawString);
                            }
                            textMalformationHandled = true;
                        }
                    }
                    else if (expectedCase == 'U')
                    {
                        if (rawString != upperString)
                        {
                            if (rawString == lowerString)
                            {
                                parent.Malformations.AddIncorrectCase(property, index, 'L');
                            }
                            else
                            {
                                parent.Malformations.AddIncorrectTextParse(property, index, rawString);
                            }
                            textMalformationHandled = true;
                        }
                    }
                }

                if (!textMalformationHandled)
                {
                    // assume uppercase
                    string textValue = BitConverter.ToString(valueInternal).Replace("-", string.Empty);
                    // basic string issue like True vs true
                    string rawString = tok.GetString(index);
                    if (!string.Equals(textValue, rawString, StringComparison.Ordinal))
                        parent.Malformations.AddIncorrectTextParse(property, index, rawString);
                }
            }
        }

        if (propInfo != null && parent != null && did)
            propInfo.SetValue(parent, setVal);

        return (setVal, valueInternal);
    }

    public static (TProp stored, byte[] raw) ApplyVoidBytesRaw<T, TProp>(
        this IBZNToken tok,
        T? parent,
        Expression<Func<T, TProp>>? property,
        int index = 0,
        Func<byte[], TProp>? convert = null
    ) where T : IMalformable
    {
        PropertyInfo? propInfo = null;
        if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo_)
            propInfo = propInfo_;

        // Read the raw bytes from the token
        byte[] valueInternal = tok.GetRaw(index, -1);

        TProp setVal = default!;
        bool did = false;
        if (convert != null)
        {
            setVal = convert(valueInternal);
            did = true;
        }
        else if (typeof(TProp) == typeof(byte[]) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(byte[]))
        {
            setVal = (TProp)(object)valueInternal;
            did = true;
        }

        if (propInfo != null && parent != null && did)
            propInfo.SetValue(parent, setVal);

        return (setVal, valueInternal);
    }

    /// <summary>
    /// Read a boolean from an <see cref="IBZNToken"/> and optionally set it on a property of a parent object,
    /// while also checking for common malformations.
    /// </summary>
    /// <remarks>
    /// Handles the following malformations: <see cref="Malformation.INCORRECT_TEXT"/>
    /// </remarks>
    /// <typeparam name="T">Type that contains the target property and implements <see cref="IMalformable"/></typeparam>
    /// <typeparam name="TProp">Property type</typeparam>
    /// <param name="tok">Token</param>
    /// <param name="parent"><see cref="IMalformable"/> instance containing properties</param>
    /// <param name="property">Lambda to access the property to register malformations to and apply the value</param>
    /// <param name="index">Index of the value in the token</param>
    /// <param name="convert">Optional conversion function for the read boolean</param>
    /// <returns></returns>
    public static (TProp stored, bool raw) ApplyBoolean<T, TProp>(this IBZNToken tok, T? parent, Expression<Func<T, TProp>>? property, int index = 0, Func<bool, TProp>? convert = null) where T : IMalformable
    {
        PropertyInfo? propInfo = null;
        if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo_)
            propInfo = propInfo_;

        bool valueInternal = tok.GetBoolean(index);
        string textValue = valueInternal ? "true" : "false";
        if (tok.IsBinary)
        {
            // no binary exclusive paths yet
        }
        else
        {
            if (propInfo != null && parent != null)
            {
                // basic string issue like True vs true
                string rawString = tok.GetString(index);
                if (!string.Equals(textValue, rawString, StringComparison.Ordinal))
                    parent.Malformations.AddIncorrectTextParse(property, index, rawString);
            }
        }

        TProp setVal = default!;
        bool did = false;
        if (convert != null)
        {
            setVal = convert(valueInternal);
            did = true;
        }
        else if (typeof(TProp) == typeof(bool) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(bool))
        {
            setVal = (TProp)(object)valueInternal;
            did = true;
        }

        if (propInfo != null && parent != null && did)
            propInfo.SetValue(parent, setVal);

        return (setVal, valueInternal);
    }
}