using System.Linq.Expressions;
using System.Reflection;
using static BZNParser.Tokenizer.IMalformable;

namespace BZNParser.Tokenizer;


// These functions should return the cleaned value and set the value on the property if the parent instance is set
public static class TokenExtensions
{
    /// <summary>
    /// Read a boolean from an <see cref="IBZNToken"/> and optionally set it on a property of a parent object,
    /// while also checking for common malformations.
    /// </summary>
    /// <typeparam name="T">Type that contains the target property and implements <see cref="IMalformable"/></typeparam>
    /// <typeparam name="TProp">Property type</typeparam>
    /// <param name="tok">Token</param>
    /// <param name="parent"><see cref="IMalformable"/> instance containing properties</param>
    /// <param name="property">Lambda to access the property to register malformations to and apply the value</param>
    /// <param name="index">Index of the value in the token</param>
    /// <param name="convert">Optional conversion function for the read boolean</param>
    /// <returns></returns>
    public static (TProp, bool) ReadBoolean<T, TProp>(this IBZNToken tok, T? parent, Expression<Func<T, TProp>>? property, int index = 0, Func<bool, TProp>? convert = null) where T : IMalformable
    {
        PropertyInfo? propInfo = null;
        if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo_)
            propInfo = propInfo_;

        bool valueInternal = tok.GetBoolean(index);
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
                if (string.Equals(valueInternal.ToString().ToLowerInvariant(), rawString, StringComparison.Ordinal))
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
