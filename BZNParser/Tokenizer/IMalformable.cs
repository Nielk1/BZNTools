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
    INCOMPAT,         //X ?????????                           // Not loadable by game
    EXTRA_FIELD,      //X "EXTRA_FIELD:CTX", <fields>         // Extra data
    MISINTERPRET,     //X <fieldName>,       <interpretedAs>  // Misinterpreted by game but thus is loadable
    OVERCOUNT,        //X <fieldName>                         // Too many objects of this type, maximum may have changed
    NOT_IMPLEMENTED,  //X <fieldName>                         // Field not implemented, but it probably won't break the BZN read
    INCORRECT_RAW,    // <byte[] originalRaw> // value parsed improperly or otherwise differently than expected, preserved original raw bytes, ASCII and Binary modes (in text mode the bytes are dumped directly into the file, not converted)
    INCORRECT_TEXT,   // <string originalString> // value parsed improperly or otherwise differently than expected, preserved original text, ASCII only mode
    INCORRECT_CASE,   // <char 'U' or 'L'> // casing present instead of expected
    INCORRECT_LENGTH, // <int> // incorrect length in file
    LINE_ENDING,      //X "ALL:LINE_ENDING", <incorrectValue> // Line ending is incorrect, "CR" for all "CR"s, "LF" for all "LF"s, "?" for other counts
    STRING_PAD,       //X <fieldName>,       <length>         // String is padded by nuls to reach this length
    INCORRECT_NAME,   //X <fieldName>,       <badFieldName>   // Field name is wrong, very rare since normally we just fail validation
    FLOAT_FORMAT,     //X "ALL:FLOAT_TEXT",  <formatApplied>  // Float is in an unexpected text format
    RIGHT_TRIM,       //X <fieldName>                         // Field is a 1-liner with an empty field that got Right-Trimmed
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
    /// Adds a misinterpretation entry for the specified field to the <see cref="MalformationManager"/>.
    /// </summary>
    /// <param name="manager">The <see cref="MalformationManager"/> to which the misinterpretation will be added. Cannot be <c>null</c>.</param>
    /// <param name="fieldName">The name of the field that was misinterpreted. Cannot be <c>null</c> or empty.</param>
    /// <param name="interpretedAs">The name of the field that received the value in error. Cannot be <c>null</c> or empty.</param>
    [Obsolete]
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
    [Obsolete]
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
    [Obsolete]
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
    [Obsolete]
    public static void AddIncorrect(this MalformationManager manager, string fieldName, object incorrectValue) =>
        manager.Add(Malformation.INCORRECT_RAW, fieldName, incorrectValue);



    /// <summary>
    /// Adds an entry indication that the specified field was padding with nulls to reach a specific length.
    /// </summary>
    /// <remarks>
    /// This mainly has to do with binary stored strings, it's mostly needed to replicate the original bytes.
    /// </remarks>
    /// <param name="manager">The <see cref="MalformationManager"/> instance to which the incorrect entry will be added. Cannot be <c>null</c>.</param>
    /// <param name="fieldName">The name of the field that contains the incorrect value. Cannot be <c>null</c> or empty.</param>
    /// <param name="length">The length of the string after padding.</param>
    [Obsolete]
    public static void AddStringPad(this MalformationManager manager, string filedName, int length) =>
        manager.Add(Malformation.STRING_PAD, filedName, length);








    // This value is text incorrect, which means this value is the text representation not matching what it should be
    public static (bool, int?) GetIncorrectLength<T, TProp>(this MalformationManager manager, Expression<Func<T, TProp>> property) where T : IMalformable
    {
        if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo)
        {
            var mals = manager.GetMalformations(propInfo, 0, Malformation.INCORRECT_LENGTH);
            if (mals.Length > 0)
                return (true, (int)mals[0].Fields[0]);
            return (false, null);
        }
        throw new ArgumentException("Expression is not a property", nameof(property));
    }

    public static void SetIncorrectLength<T, TProp>(this MalformationManager manager, Expression<Func<T, TProp>>? property, int originalLength) where T : IMalformable =>
        manager.Add(property, 0, Malformation.INCORRECT_LENGTH, originalLength);





    public static bool IsRightTrimmed<T, TProp>(this MalformationManager manager, Expression<Func<T, TProp>> property, int index = 0) where T : IMalformable
    {
        if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo)
        {
            var mals = manager.GetMalformations(propInfo, index, Malformation.RIGHT_TRIM);
            if (mals.Length > 0)
                return true;
            return false;
        }
        throw new ArgumentException("Expression is not a property", nameof(property));
    }

    public static void AddRightTrimmed<T, TProp>(this MalformationManager manager, Expression<Func<T, TProp>> property, int index = 0) where T : IMalformable =>
        manager.Add(property, index, Malformation.RIGHT_TRIM);



    // This value is byte incorrect, which means the value here must be emitted as raw bytes
    public static (bool, byte[]?) GetIncorrectRaw<T, TProp>(this MalformationManager manager, Expression<Func<T, TProp>> property, int index = 0) where T : IMalformable
    {
        if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo)
        {
            var mals = manager.GetMalformations(propInfo, index, Malformation.INCORRECT_RAW);
            if (mals.Length > 0)
                return (true, (byte[])mals[0].Fields[0]);
            return (false, null);
        }
        throw new ArgumentException("Expression is not a property", nameof(property));
    }

    public static void AddIncorrectRaw<T, TProp>(this MalformationManager manager, Expression<Func<T, TProp>>? property, int index, byte[] originalBytes) where T : IMalformable =>
        manager.Add(property, index, Malformation.INCORRECT_RAW, originalBytes);



    // This value is text incorrect, which means this value is the text representation not matching what it should be
    public static (bool, string?) GetIncorrectTextParse<T, TProp>(this MalformationManager manager, Expression<Func<T, TProp>> property, int index = 0) where T : IMalformable
    {
        if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo)
        {
            var mals = manager.GetMalformations(propInfo, index, Malformation.INCORRECT_TEXT);
            if (mals.Length > 0)
                return (true, (string)mals[0].Fields[0]);
            return (false, null);
        }
        throw new ArgumentException("Expression is not a property", nameof(property));
    }

    public static void AddIncorrectTextParse<T, TProp>(this MalformationManager manager, Expression<Func<T, TProp>>? property, int index, string originalText) where T : IMalformable =>
        manager.Add(property, index, Malformation.INCORRECT_TEXT, originalText);




    public static (bool, char?) GetIncorrectCase<T, TProp>(this MalformationManager manager, Expression<Func<T, TProp>> property, int index = 0) where T : IMalformable
    {
        if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo)
        {
            var mals = manager.GetMalformations(propInfo, index, Malformation.INCORRECT_CASE);
            if (mals.Length > 0)
                return (true, (char)mals[0].Fields[0]);
            return (false, null);
        }
        throw new ArgumentException("Expression is not a property", nameof(property));
    }
    public static void AddIncorrectCase<T, TProp>(this MalformationManager manager, Expression<Func<T, TProp>>? property, int index, char casing) where T : IMalformable =>
    manager.Add(property, index, Malformation.INCORRECT_CASE, casing);



    #region INCORRECT_NAME
    public static (bool, string?) GetIncorrectName<T, TProp>(this MalformationManager manager, Expression<Func<T, TProp>>? property) where T : IMalformable
    {
        if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo)
        {
            var mals = manager.GetMalformations(propInfo, null, Malformation.INCORRECT_NAME);
            if (mals.Length > 0)
                return (true, (string)mals[0].Fields[0]);
        }
        return (false, null);
    }

    public static void AddIncorrectName<T, TProp>(this MalformationManager manager, Expression<Func<T, TProp>>? property, string badName) where T : IMalformable =>
        manager.Add<T, TProp>(property, null, Malformation.INCORRECT_NAME, badName);
    #endregion INCORRECT_NAME

    #region EXTRA_FIELD
    public static bool HasExtraField<T, TProp>(this MalformationManager manager, Expression<Func<T, TProp>> property) where T : IMalformable
    {
        if (property.Body is MemberExpression member && member.Member is PropertyInfo propInfo)
        {
            var mals = manager.GetMalformations(propInfo, null, Malformation.EXTRA_FIELD);
            if (mals.Length > 0)
                return true;
        }
        return false;
    }
    public static void SetExtraField<T, TProp>(this MalformationManager manager, Expression<Func<T, TProp>> property) where T : IMalformable =>
        manager.Add<T, TProp>(property, null, Malformation.EXTRA_FIELD);
    #endregion EXTRA_FIELD

    #region FLOAT_FORMAT
    public static FloatTextFormat? GetFloatTextFormat(this MalformationManager manager)
    {
        var mals = manager.GetMalformations(Malformation.FLOAT_FORMAT);
        if (mals.Length > 0)
            return (FloatTextFormat)mals[0].Fields[0];
        return null;
    }

    public static void SetFloatTextFormat(this MalformationManager manager, FloatTextFormat formatUsed) =>
        manager.Add(Malformation.FLOAT_FORMAT, formatUsed);
    #endregion FLOAT_FORMAT

    #region LINE_ENDING
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
    #endregion LINE_ENDING
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
                return;
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

        public void Clear()
        {
            while (malformations.Count > 0)
                malformations.Pop();
            malformations.Push((new List<MalformationData>(), new Dictionary<(PropertyInfo, int?), List<MalformationData>>()));
        }

        /// <summary>
        /// Clear all malformations from the specific property, or root if propertyLambda is null.
        /// Look at all possible indexes for the property.
        /// If the malformation parameter is provided, only clear malformations of that type, otherwise clear all malformations for the property or root.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="propertyLambda"></param>
        /// <param name="malformation"></param>
        public void Clear<T, TProp>(Expression<Func<T, TProp>>? propertyLambda, Malformation? malformation = null) where T : IMalformable
        {
            // loop all malformation layers without popping any
            foreach (var layer in malformations)
            {
                if (propertyLambda == null)
                {
                    if (malformation.HasValue)
                    {
                        layer.Root.RemoveAll(mal => mal.Property == null && mal.Type == malformation.Value);
                    }
                    else
                    {
                        layer.Root.RemoveAll(mal => mal.Property == null);
                    }
                }
                else if (propertyLambda.Body is MemberExpression member && member.Member is PropertyInfo propInfo)
                {
                    layer.Properties.Where(dr => dr.Key.Item1 == propInfo).ToList().ForEach(dr =>
                    {
                        if (malformation.HasValue)
                        {
                            dr.Value.RemoveAll(mal => mal.Type == malformation.Value);
                        }
                        else
                        {
                            dr.Value.Clear();
                        }
                    });
                }
            }
        }

        /// <summary>
        /// Clear all malformations from the specific property, or root if propertyLambda is null.
        /// Loop at specific property index, even null is a valid property index which is where there's a differnt function to ignore index.
        /// If the malformation parameter is provided, only clear malformations of that type, otherwise clear all malformations for the property or root.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="propertyLambda"></param>
        /// <param name="index"></param>
        /// <param name="malformation"></param>
        public void Clear<T, TProp>(Expression<Func<T, TProp>>? propertyLambda, int? index, Malformation? malformation = null) where T : IMalformable
        {
            // loop all malformation layers without popping any
            foreach (var layer in malformations)
            {
                if (propertyLambda == null)
                {
                    if (malformation.HasValue)
                    {
                        layer.Root.RemoveAll(mal => mal.Property == null && mal.Type == malformation.Value);
                    }
                    else
                    {
                        layer.Root.RemoveAll(mal => mal.Property == null);
                    }
                }
                else if (propertyLambda.Body is MemberExpression member && member.Member is PropertyInfo propInfo)
                {
                    var key = (propInfo, index);
                    if (layer.Properties.ContainsKey(key))
                    {
                        if (malformation.HasValue)
                        {
                            layer.Properties[key].RemoveAll(mal => mal.Type == malformation.Value);
                        }
                        else
                        {
                            layer.Properties[key].Clear();
                        }
                    }
                }
            }
        }
    }
 
    public MalformationManager Malformations { get; }
    public void ClearMalformations();

    /// <summary>
    /// Set object are currently building so it should not auto-alter properties
    /// </summary>
    public void DisableMalformationAutoFix();

    /// <summary>
    /// Set object as finalized so it can auto-alter properties as needed
    /// </summary>
    public void EnableMalformationAutoFix();
}
