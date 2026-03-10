using System.Linq.Expressions;
using System.Reflection;
using static BZNParser.Tokenizer.IMalformable;

namespace BZNParser.Tokenizer;


// These functions should return the cleaned value and set the value on the property if the parent instance is set
public static class ITokenExtensions
{
    public static (TProp?, bool) ReadBoolean<T, TProp>(this IBZNToken tok, T? parent, Expression<Func<T, TProp>> property, int index = 0, Func<bool, TProp>? convert = null) where T : IMalformable
    {
        if (property.Body is MemberExpression member && member.Member is PropertyInfo propInfo)
        {
            bool valueInternal = tok.GetBoolean(index);
            if (tok.IsBinary)
            {

            }
            else
            {
                if (parent != null)
                {
                    // basic string issue like True vs true
                    string rawString = tok.GetString(index);
                    if (valueInternal.ToString().ToLowerInvariant() != rawString)
                        parent.Malformations.AddIncorrectTextParse(property, index, rawString);
                }
            }

            TProp? setVal = default;
            bool did = false;
            if (convert != null)
            {
                setVal = convert(valueInternal);
                did = true;
            }
            else if (typeof(TProp) == typeof(bool))
            {
                setVal = (TProp)(object)valueInternal;
                did = true;
            }

            if (parent != null)
            {
                if (did)
                    propInfo.SetValue(parent, setVal);
            }
            return (setVal, valueInternal);
        }
        throw new ArgumentException("Expression is not a property", nameof(property));
    }
}
