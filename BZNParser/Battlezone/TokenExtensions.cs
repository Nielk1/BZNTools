using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;
using static BZNParser.Tokenizer.IMalformable;
using UInt8 = byte;

namespace BZNParser.Tokenizer;


// These functions should return the cleaned value and set the value on the property if the parent instance is set
public static class TokenExtensions
{
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
    public static (TProp stored, UInt32 raw) ReadUInt32<T, TProp>(this IBZNToken tok, T? parent, Expression<Func<T, TProp>>? property, int index = 0, Func<UInt32, TProp>? convert = null) where T : IMalformable
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
    public static (TProp stored, UInt8 raw) ReadUInt8<T, TProp>(this IBZNToken tok, T? parent, Expression<Func<T, TProp>>? property, int index = 0, Func<UInt8, TProp>? convert = null) where T : IMalformable
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
    public static (TProp stored, string raw) ReadChars<T, TProp>(this IBZNToken tok, T? parent, Expression<Func<T, TProp>>? property, int index = 0, Func<string, TProp>? convert = null) where T : IMalformable
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
            else if(typeof(TProp) == typeof(SizedString) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(SizedString))
            {
                finalValue = (TProp)(object)new SizedString() { Value = valueProcessed };
                finalValueReady = true;
            }
        }

        // apply the value if possible
        if (propInfo != null && parent != null && finalValueReady)
            propInfo.SetValue(parent, finalValue);

        // always return processed data, even if we didn't attach it to the property or store malformations, we still read the value
        return (finalValue, valueProcessed);
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
    public static (TProp stored, bool raw) ReadBoolean<T, TProp>(this IBZNToken tok, T? parent, Expression<Func<T, TProp>>? property, int index = 0, Func<bool, TProp>? convert = null) where T : IMalformable
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

    // Only used by BZ1, never BZ2, others unknown
    /// <summary>
    /// Read an ID from an <see cref="IBZNToken"/> and optionally set it on a property of a parent object,
    /// while also checking for common malformations.
    /// </summary>
    /// <remarks>
    /// Handles the following malformations: <see cref="Malformation.INCORRECT_RAW"/>
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProp"></typeparam>
    /// <param name="tok"></param>
    /// <param name="parent"></param>
    /// <param name="property"></param>
    /// <param name="index"></param>
    /// <param name="convert"></param>
    /// <returns></returns>
    public static (TProp stored, string cleaned, string raw) ReadID<T, TProp>(this IBZNToken tok, T? parent, Expression<Func<T, TProp>>? property, int index = 0, Func<string, TProp>? convert = null) where T : IMalformable
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
        }

        // apply the value if possible
        if (propInfo != null && parent != null && finalValueReady)
            propInfo.SetValue(parent, finalValue);

        // always return processed data, even if we didn't attach it to the property or store malformations, we still read the value
        return (finalValue, valueProcessed, valueInternal);
    }
}
