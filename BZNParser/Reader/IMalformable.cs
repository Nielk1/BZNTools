using static BZNParser.Reader.IMalformable;

namespace BZNParser.Reader
{
    public enum Malformation
    {
        UNKNOWN = 0,
        INCOMPAT,        // ?????????                           // Not loadable by game
        EXTRA_FIELD,     // "EXTRA_FIELD:CTX", <fields>         // Extra data
        MISINTERPRET,    // <fieldName>,       <interpretedAs>  // Misinterpreted by game but thus is loadable
        OVERCOUNT,       // <fieldName>                         // Too many objects of this type, maximum may have changed
        NOT_IMPLEMENTED, // <fieldName>                         // Field not implemented, but it probably won't break the BZN read
        INCORRECT,       // <fieldName>,       <incorrectValue> // Value is incorrect and has been corrected
        LINE_ENDING,     // "ALL:LINE_ENDING", <incorrectValue> // Line ending is incorrect, "CR" for all "CR"s, "LF" for all "LF"s, "?" for other counts
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
        /// Add an entry indicating that the line ending count is incorrect and has been corrected.
        /// </summary>
        /// <remarks>
        /// This is a whole file issue. This may or may not be reversible when trying to output a BZN with the malformations intact.
        /// </remarks>
        /// <param name="manager"></param>
        /// <param name="incorrectValue"></param>
        public static void AddLineEnding(this MalformationManager manager, string incorrectValue) =>
                manager.Add(Malformation.LINE_ENDING, "ALL:LINE_ENDING", incorrectValue);
    }
    public interface IMalformable
    {
        public struct MalformationData
        {
            public Malformation Type { get; }
            public string Property { get; }
            public object[] Fields { get; }
            public MalformationData(Malformation type, string property, object[] fields)
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
            private readonly Stack<Dictionary<string, List<MalformationData>>> malformations;

            public MalformationData[] this[string property]
            {
                get
                {
                    return malformations
                        .SelectMany(stack => stack)
                        .Where(kvp => kvp.Key == property)
                        .SelectMany(kvp => kvp.Value)
                        .ToArray();
                }
            }
            public string[] Keys => malformations.SelectMany(stack => stack.Keys).ToArray();
            public MalformationManager(IMalformable parent)
            {
                _parent = parent;
                malformations = new Stack<Dictionary<string, List<MalformationData>>>();
                malformations.Push(new Dictionary<string, List<MalformationData>>());
            }

            public void Add(Malformation malformation, string property, params object[] fields)
            {
                if (!malformations.Peek().ContainsKey(property))
                {
                    malformations.Peek()[property] = new List<MalformationData>();
                }
                malformations.Peek()[property].Add(new MalformationData(malformation, property, fields));
            }

            /// <summary>
            /// Create a new temporary malformation context.
            /// </summary>
            public void Push()
            {
                 malformations.Push(new Dictionary<string, List<MalformationData>>());
            }

            /// <summary>
            /// Pop the current malformation context and merge it into the previous context.
            /// This should be used when exiting a temporary context to *preserve* any malformations that were added within it.
            /// </summary>
            public void Pop()
            {
                if (malformations.Count > 1)
                {
                    Dictionary<string, List<MalformationData>> old = malformations.Pop();
                    foreach (var kvp in old)
                    {
                        if (!malformations.Peek().ContainsKey(kvp.Key))
                        {
                            malformations.Peek()[kvp.Key] = new List<MalformationData>();
                        }
                        malformations.Peek()[kvp.Key].AddRange(kvp.Value);
                    }
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
}
