using BZNParser.Tokenizer;
using System.Linq.Expressions;
using System.Reflection;

namespace BZNParser.Battlezone;

/// <summary>
/// Holds the size for a string if it's abberant, else determines it from the string value
/// </summary>
public class SizedString : IMalformable
{
    #region Malformable
    private readonly IMalformable.MalformationManager _malformationManager;
    public IMalformable.MalformationManager Malformations => _malformationManager;
    public void ClearMalformations()
    {
        length = null;
        Malformations.Clear();
    }
    #endregion Malformable

    public SizedString() : this(string.Empty, null) {}
    public SizedString(string value, uint? length = null)
    {
        _malformationManager = new IMalformable.MalformationManager(this);
        
        this.value = value;

        // don't bother setting the size if it's already correct,
        // we can only do this because we're setting both at once,
        // else we'd have to leave the size alone due to OoO issues
        if (value.Length != length)
            this.length = length;
    }

    public uint Length { get { return length ?? (uint)(value?.Length ?? 0); } set { length = value; } }
    public string Value {
        get { return value; }
        set {
            this.value = value;
            if (!blockAutoFixMalformations)
                length = null;
        }
    }

    private uint? length;
    private string value;
    private bool blockAutoFixMalformations = false;

    public override string ToString()
    {
        return Value;
    }
    public void DisableMalformationAutoFix()
    {
        blockAutoFixMalformations = true;
    }
    /// <summary>
    /// Allow automatic malformation and data corrections when data altered.
    /// This is blocked when constructed with no paramaters.
    /// </summary>
    public void EnableMalformationAutoFix()
    {
        blockAutoFixMalformations = false;
    }
}

static class SizedStringExtension
{
    /// <summary>
    /// Read a normal chars string unless BZ2 and version > 1128
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="reader"></param>
    /// <param name="name"></param>
    /// <param name="parent"></param>
    /// <param name="property"></param>
    /// <param name="destinationIndex">We already read index 0 of the Token, but this is what index we're writing to</param>
    /// <exception cref="Exception"></exception>
    public static (string? stored, string? raw) ReadSizedString<T, TProp>(this BZNStreamReader reader, string name, T? parent, Expression<Func<T, TProp?>> property, int destinationIndex = 0) where T : IMalformable
    {
        PropertyInfo? propInfo = null;
        if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo_)
            propInfo = propInfo_;

        //SizedString? value = null;
        //if (parent != null)
        //    value = (SizedString?)(propInfo?.GetValue(parent));
        //if (value == null && propInfo != null)
        //    value = new SizedString();
        //if (propInfo != null)
        //{
        //    value = new SizedString();
        //    if (parent != null)
        //        propInfo.SetValue(parent, value);
        //}

        SizedString? value = new SizedString();
        value.DisableMalformationAutoFix();

        try
        {
            TProp setVal = default!;
            bool did = false;
            if (typeof(TProp) == typeof(SizedString) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(SizedString))
            {
                setVal = (TProp)(object)value;
                did = true;
            }
            else if (typeof(TProp).IsArray && typeof(TProp).GetElementType() == typeof(SizedString))
            {
                SizedString[]? arr = (SizedString[]?)propInfo?.GetValue(parent);
                if (arr != null && destinationIndex >= 0 && destinationIndex < arr.Length)
                {
                    arr[destinationIndex] = value;
                    setVal = (TProp)(object)arr;
                    did = true;
                }
            }

            IBZNToken? tok;
            if (reader.InBinary)
            {
                // this only happens on BZ2/BZCC BZNs over version 1128s
                if (reader.Format == BZNFormat.Battlezone2 && reader.Version > 1128)
                {
                    // TODO this might be a compressed number so do figure that out
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_CHAR) || tok.GetCount() > 1)
                        throw new Exception($"Failed to parse {name}/CHAR");
                    (_, byte size) = tok.ApplyUInt8(value, x => x.Length);

                    if (size > 0) // descision based on raw value, not cleaned
                    {
                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate(name, BinaryFieldType.DATA_CHAR))
                            throw new Exception($"Failed to parse {name}/CHAR");
                        (string stored_, string raw_) = tok.ApplyChars(value, x => x.Value);

                        // this works when the property is a SizedString, but not a SizedString[] where we want to apply it instead to SizedString[destinationIndex]
                        if (propInfo != null && parent != null && did)
                            propInfo.SetValue(parent, setVal);

                        return (stored_, raw_);
                    }

                    // this works when the property is a SizedString, but not a SizedString[] where we want to apply it instead to SizedString[destinationIndex]
                    if (propInfo != null && parent != null && did)
                        propInfo.SetValue(parent, setVal);

                    return (null, null);
                }
            }
            tok = reader.ReadToken();
            if (tok == null || !tok.Validate(name, BinaryFieldType.DATA_CHAR))
                throw new Exception($"Failed to parse {name}/CHAR");
            (string stored, string raw) = tok.ApplyChars(value, x => x.Value);

            if (propInfo != null && parent != null && did)
                propInfo.SetValue(parent, setVal);

            return (stored, raw);
        }
        finally
        {
            value?.EnableMalformationAutoFix();
        }
    }

    public static (string? stored, string? raw) ReadGameObjectClass_BZ2<T, TProp>(this BZNStreamReader reader, SaveType saveType, string name, T? parent, Expression<Func<T, TProp>> property, int index = 0) where T : IMalformable
    {
        if (reader.Version < 1145)
        {
            //return reader.ReadSizedString_BZ2_1145(name, 16, malformations);
            return reader.ReadSizedString(name, parent, property!, index);
        }
        else
        {
            if (saveType == SaveType.LOCKSTEP)
            {
                throw new NotImplementedException();
            }
            else
            {
                return reader.ReadSizedString(name, parent, property!, index);
            }
        }
    }

    // TODO fix index handling
    public static void WriteSizedString<T, TProp>(this BZNStreamWriter writer, string name, T parent, Expression<Func<T, TProp>> property, Func<TProp, SizedString>? convert = null)
    {
        TProp wrappedValue = BZNStreamWriter.ExtractPropertyValue(parent, property);
        SizedString value;

        if (convert != null)
        {
            value = convert(wrappedValue);
        }
        //else if (typeof(TProp).IsArray && typeof(TProp).GetElementType() == typeof(SizedString))
        //{
        //    values = (SizedString[])(object)propValue!;
        //}
        else if (typeof(TProp) == typeof(SizedString))
        {
            value = (SizedString)(object)wrappedValue!;
        }
        else
        {
            throw new NotImplementedException("Property type not handled");
        }

        if (writer.InBinary)
        {
            if (writer.Format == BZNFormat.Battlezone2 && writer.Version > 1128)
            {
                (byte size, _) = writer.WriteUInt8(null, value, x => x.Length);
                if (size > 0)
                    writer.WriteChars(name, value, x => x.Value);
                return;
            }
        }
        writer.WriteChars(name, value, x => x.Value);
    }

    /// <summary>
    /// Sized path name string
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="reader"></param>
    /// <param name="name"></param>
    /// <param name="parent"></param>
    /// <param name="property"></param>
    /// <param name="index"></param>
    /// <exception cref="Exception"></exception>
    public static (string stored, string raw) ReadSizedStringType2<T>(this BZNStreamReader reader, string name, T? parent, Expression<Func<T, SizedString>>? property, int index = 0) where T : IMalformable
    {
        PropertyInfo? propInfo = null;
        if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo_)
            propInfo = propInfo_;

        SizedString? value = null;
        if (parent != null)
            value = (SizedString?)(propInfo?.GetValue(parent));
        if (value == null && propInfo != null)
        {
            value = new SizedString();
            value.DisableMalformationAutoFix();
        }
        if (propInfo != null)
        {
            value = new SizedString();
            value.DisableMalformationAutoFix();
            if (parent != null)
                propInfo.SetValue(parent, value);
        }

        try
        {
            IBZNToken? tok;
            // TODO this might be a compressed number so do figure that out
            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("size", BinaryFieldType.DATA_LONG) || tok.GetCount() > 1)
                throw new Exception($"Failed to parse size/LONG");
            (_, uint size) = tok.ApplyUInt32(value, x => x.Length);

            if (size > 0) // descision based on raw value, not cleanedsssss
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate(name, BinaryFieldType.DATA_CHAR))
                    throw new Exception($"Failed to parse {name}/CHAR");
                return tok.ApplyChars(value, x => x.Value);
            }
            return (null, null)!;
        }
        finally
        {
            // unlock the SizedString so editing its value wipes the length override
            value?.EnableMalformationAutoFix();
        }
    }
    public static void WriteSizedStringType2<T>(this BZNStreamWriter writer, string name, T parent, Expression<Func<T, SizedString>> property)
    {
        SizedString wrappedValue = BZNStreamWriter.ExtractPropertyValue(parent, property);

        (uint size, _) = writer.WriteUInt32("size", wrappedValue, x => x.Length);
        if (size > 0)
            writer.WriteChars(name, wrappedValue, x => x.Value);
    }
}
