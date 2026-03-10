using Microsoft.VisualBasic;
using System.Reflection.Metadata;
using static BZNParser.Tokenizer.BZNStreamReader;
using static BZNParser.Tokenizer.IMalformable;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace BZNParser.Tokenizer;


public enum Malformation
{
    UNKNOWN = 0,
    INCOMPAT,        // ?????????                           // Not loadable by game
    EXTRA_FIELD,     // "EXTRA_FIELD:CTX", <fields>         // Extra data
    MISINTERPRET,    // <fieldName>,       <interpretedAs>  // Misinterpreted by game but thus is loadable
    OVERCOUNT,       // <fieldName>                         // Too many objects of this type, maximum may have changed
    NOT_IMPLEMENTED, // <fieldName>                         // Field not implemented, but it probably won't break the BZN read
    INCORRECT,       // <fieldName>,       <incorrectValue> // Value is incorrect and has been corrected; incorrectValue is the correct type
    INCORRECT_TEXT,  // <fieldName>,       <incorrectValue> // Value is incorrect and has been corrected; incorrectValue is unparsed text
    LINE_ENDING,     // "ALL:LINE_ENDING", <incorrectValue> // Line ending is incorrect, "CR" for all "CR"s, "LF" for all "LF"s, "?" for other counts
    STRING_PAD,      // <fieldName>,       <length>         // String is padded by nuls to reach this length
    INCORRECT_NAME,  // <fieldName>,       <badFieldName>   // Field name is wrong, very rare since normally we just fail validation
    FLOAT_FORMAT,    // "ALL:FLOAT_TEXT",  <formatApplied>  // Float is in an unexpected text format
    RIGHT_TRIM,      // <fieldName>                         // Field is a 1-liner with an empty field that got Right-Trimmed
}

public static class MalformationExtensions
{
    /*public static string GetDescription(this Malformation malformation)
    {
        return malformation switch
        {
            Malformation.UNKNOWN => "Unknown malformation",
            Malformation.INCOMPAT => "Incompatible with game version",
            Malformation.MISINTERPRET => "Misinterpreted by game",
            Malformation.OVERCOUNT => "Too many objects of this type",
            Malformation.NOT_IMPLEMENTED => "Field not implemented",
            Malformation.INCORRECT => "Incorrect value, corrected during parsing",
            Malformation.LINE_ENDING => "Incorrect line ending count",
            _ => "Undefined malformation"
        };
    }*/

    /// <summary>
    /// Adds an extra field malformation entry to the specified <see cref="MalformationManager"/>.
    /// </summary>
    /// <remarks>
    /// This field has no rules that make it appear, it just doesn't belong and should never have been there.
    /// </remarks>
    /// <param name="manager">The <see cref="MalformationManager"/> to which the extra field malformation will be added.</param>
    /// <param name="context">A string providing context or additional information about the extra field. This value is included in the
    /// malformation entry for identification or debugging purposes.</param>
    /// <param name="tok">The <see cref="IBZNToken"/> associated with the extra field malformation. This token typically represents
    /// the source or location of the malformation.</param>
    public static void AddExtraField(this MalformationManager manager, string context, IBZNToken tok) =>
        manager.Add(Malformation.EXTRA_FIELD, $"EXTRA_FIELD:{context}", tok);

    /// <summary>
    /// Adds a misinterpretation entry for the specified field to the <see cref="MalformationManager"/>.
    /// </summary>
    /// <param name="manager">The <see cref="MalformationManager"/> to which the misinterpretation will be added. Cannot be <c>null</c>.</param>
    /// <param name="fieldName">The name of the field that was misinterpreted. Cannot be <c>null</c> or empty.</param>
    /// <param name="interpretedAs">The name of the field that received the value in error. Cannot be <c>null</c> or empty.</param>
    public static void AddMisinterpretation(this MalformationManager manager, string fieldName, string interpretedAs) =>
         manager.Add(Malformation.MISINTERPRET, fieldName, interpretedAs);

    /// <summary>
    /// Adds an overcount malformation entry for the specified field to the <see cref="MalformationManager"/>.
    /// </summary>
    /// <remarks>
    /// You should still store the overcount data in the appropriate field, but this malformation entry will indicate that the count exceeds what the game expects.
    /// </remarks>
    /// <param name="manager">The <see cref="MalformationManager"/> to which the overcount malformation will be added. Cannot be <c>null</c>.</param>
    /// <param name="fieldName">The name of the field associated with the overcount malformation. Cannot be <c>null</c> or empty.</param>
    public static void AddOvercount(this MalformationManager manager, string fieldName) =>
        manager.Add(Malformation.OVERCOUNT, fieldName);

    /// <summary>
    /// Adds a "not implemented" malformation entry for the specified field to the manager.
    /// </summary>
    /// <remarks>
    /// This means we didn't implement the field, but it's not a critical field so we can skip it.
    /// </remarks>
    /// <param name="manager">The <see cref="MalformationManager"/> to which the malformation entry will be added. Cannot be <c>null</c>.</param>
    /// <param name="fieldName">The name of the field that is not implemented. Cannot be <c>null</c> or empty.</param>
    public static void AddNotImplemented(this MalformationManager manager, string fieldName) =>
        manager.Add(Malformation.NOT_IMPLEMENTED, fieldName);

    /// <summary>
    /// Adds an entry indicating that the specified field contained an incorrect value but was corrected.
    /// </summary>
    /// <remarks>
    /// This should be used when a field's value is determined to be incorrect during parsing and was corrected.
    /// </remarks>
    /// <param name="manager">The <see cref="MalformationManager"/> instance to which the incorrect entry will be added. Cannot be <c>null</c>.</param>
    /// <param name="fieldName">The name of the field that contains the incorrect value. Cannot be <c>null</c> or empty.</param>
    /// <param name="incorrectValue">The value that is considered incorrect for the specified field. Can be <c>null</c> if the field's incorrect state is due to a missing or invalid value.</param>
    public static void AddIncorrect(this MalformationManager manager, string fieldName, object incorrectValue) =>
        manager.Add(Malformation.INCORRECT, fieldName, incorrectValue);



    /// <summary>
    /// Adds an entry indication that the specified field was padding with nulls to reach a specific length.
    /// </summary>
    /// <remarks>
    /// This mainly has to do with binary stored strings, it's mostly needed to replicate the original bytes.
    /// </remarks>
    /// <param name="manager">The <see cref="MalformationManager"/> instance to which the incorrect entry will be added. Cannot be <c>null</c>.</param>
    /// <param name="fieldName">The name of the field that contains the incorrect value. Cannot be <c>null</c> or empty.</param>
    /// <param name="length">The length of the string after padding.</param>
    public static void AddStringPad(this MalformationManager manager, string filedName, int length) =>
        manager.Add(Malformation.STRING_PAD, filedName, length);


    public static void AddFloatFormat(this MalformationManager manager, FloatTextFormat formatUsed) =>//, FloatTextFormat formatExpected) =>
        manager.Add(Malformation.FLOAT_FORMAT, "ALL:FLOAT_TEXT", formatUsed);

    public static void AddRightTrimmed(this MalformationManager manager, string filedName) =>
        manager.Add(Malformation.RIGHT_TRIM, filedName);
    public static FloatTextFormat? GetFloatTextFormat(this MalformationManager manager)
    {
        var mals = manager.GetMalformations(Malformation.FLOAT_FORMAT, "ALL:FLOAT_TEXT");
        if (mals.Length > 0)
            return (FloatTextFormat)mals[0].Fields[0];
        return null;
    }



    // might be able to unify with INCORRECT, or maybe INCORRECT applies in both modes and INCORRECT_TEXT only in text mode?
    public static void AddIncorrectTextParse<T, TProp>(this MalformationManager manager, Expression<Func<T, TProp>>? property, int? index, string originalText) where T : IMalformable =>
        manager.Add(property, index, Malformation.INCORRECT_TEXT, originalText);

    public static void AddIncorrectName<T, TProp>(this MalformationManager manager, Expression<Func<T, TProp>>? property, string badName) where T : IMalformable =>
        manager.Add<T, TProp>(property, null, Malformation.INCORRECT_NAME, badName);

    public static string? GetLineEnding(this MalformationManager manager)
    {
        var mals = manager.GetMalformations(Malformation.LINE_ENDING);
        if (mals.Length > 0)
            return (string)mals[0].Fields[0];
        return null;
    }

    /// <summary>
    /// Set the whole file's non-standard newline characters.
    /// </summary>
    /// <remarks>
    /// This is a whole file issue. This may or may not be reversible when trying to output a BZN with the malformations intact.
    /// </remarks>
    public static void SetLineEnding(this MalformationManager manager, string characters) =>
        manager.Add(Malformation.LINE_ENDING, characters);
}
public interface IMalformable
{
    public struct MalformationData
    {
        public Malformation Type { get; }
        //public string Property { get; }
        public (PropertyInfo, int?)? Property { get; }
        public object[] Fields { get; }
        public MalformationData(Malformation type, (PropertyInfo, int?)? property, object[] fields)
        {
            Type = type;
            Property = property;
            Fields = fields;
        }
    }

    public MalformationManager Malformations { get; }
    public class MalformationManager
    {
        private readonly IMalformable _parent;
        private readonly Stack<(List<MalformationData> Root, Dictionary<(PropertyInfo, int?), List<MalformationData>> Properties)> malformations;

        public (PropertyInfo, int?)?[] Keys => malformations
            .SelectMany(stackItem =>
                stackItem.Properties.Keys.Cast<(PropertyInfo, int?)?>()
                .Concat(stackItem.Root.Any() ? new (PropertyInfo, int?)?[] { null } : Array.Empty<(PropertyInfo, int?)?>()))
            .ToArray();

        public MalformationManager(IMalformable parent)
        {
            _parent = parent;
            malformations = new Stack<(List<MalformationData>, Dictionary<(PropertyInfo, int?), List<MalformationData>>)>();
            malformations.Push((new List<MalformationData>(), new Dictionary<(PropertyInfo, int?), List<MalformationData>>()));
        }

        public MalformationData[] GetMalformations(Malformation? type)
        {
            return malformations.SelectMany(dr => dr.Root).Where(mal => !type.HasValue || mal.Type == type.Value).ToArray();
        }
        public MalformationData[] GetMalformations(PropertyInfo? property, int? index, Malformation? type)
        {
            if (property == null)
                return GetMalformations(type);

            return malformations
                .Where(dr => dr.Properties.ContainsKey((property, index)))
                .SelectMany(dr => dr.Properties[(property, index)].Where(mal => !type.HasValue || mal.Type == type.Value))
                .ToArray();
        }

        public void Add(Malformation malformation, params object[] fields)
        {
            malformations.Peek().Root.Add(new MalformationData(malformation, null, fields));
            return;
        }
        public void Add<T, TProp>(Expression<Func<T, TProp>>? propertyLambda, int? index, Malformation malformation, params object[] fields) where T : IMalformable
        {
            if (propertyLambda == null)
            {
                Add(malformation, fields);
                return;
            }
            if (propertyLambda.Body is MemberExpression member && member.Member is PropertyInfo propInfo)
            {
                var key = (propInfo, index);
                var head = malformations.Peek();
                if (!head.Properties.ContainsKey(key))
                    head.Properties[key] = new List<MalformationData>();
                head.Properties[key].Add(new MalformationData(malformation, key, fields));
            }
            throw new ArgumentException("Expression is not a property", nameof(propertyLambda));
        }

        /// <summary>
        /// Create a new temporary malformation context.
        /// </summary>
        public void Push()
        {
             malformations.Push((new List<MalformationData>(), new Dictionary<(PropertyInfo, int?), List<MalformationData>>()));
        }

        /// <summary>
        /// Pop the current malformation context and merge it into the previous context.
        /// This should be used when exiting a temporary context to *preserve* any malformations that were added within it.
        /// </summary>
        public void Pop()
        {
            if (malformations.Count > 1)
            {
                (List<MalformationData> oldRoot, Dictionary<(PropertyInfo, int?), List<MalformationData>> oldProperties) = malformations.Pop();
                var head = malformations.Peek();
                foreach (var kvp in oldProperties)
                {
                    if (!head.Properties.ContainsKey(kvp.Key))
                    {
                        head.Properties[kvp.Key] = new List<MalformationData>();
                    }
                    head.Properties[kvp.Key].AddRange(kvp.Value);
                }
                head.Root.AddRange(oldRoot);
            }
        }

        /// <summary>
        /// Removes the most recently added malformation from the collection.
        /// This should be used when exiting a temporary context to *discard* any malformations that were added within it.
        /// </summary>
        public void Discard()
        {
            if (malformations.Count > 1)
            {
                malformations.Pop();
            }
        }
    }
}
