using BZNParser.Battlezone;
using BZNParser.Battlezone.GameObject;
using BZNParser.Tokenizer;
using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

public class BZNFileBattlezoneJsonConverter : JsonConverter<BZNFileBattlezone>
{
    public override BZNFileBattlezone? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Deserialization is not implemented in this initial version.
        throw new NotImplementedException("Deserialization is not implemented.");
    }

    public override void Write(Utf8JsonWriter writer, BZNFileBattlezone value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();

        var type = value.GetType();
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            switch (prop.Name)
            {
                case "ClassLabelMap":
                    continue;
            }

            if (prop.PropertyType == typeof(IMalformable.MalformationManager))
            {
                var malformations = (value as IMalformable)?.Malformations;
                if (malformations != null && malformations.Keys.Length > 0)
                {
                    writer.WritePropertyName(prop.Name);
                    JsonSerializer.Serialize(writer, malformations, malformations?.GetType() ?? typeof(object), options);
                }
                continue;
            }

            // skip indexers and non-readable
            if (prop.GetIndexParameters().Length > 0 || !prop.CanRead)
                continue;

            var propValue = prop.GetValue(value);

            // skip delegates
            if (propValue is Delegate)
                continue;

            writer.WritePropertyName(prop.Name);
            JsonSerializer.Serialize(writer, propValue, propValue?.GetType() ?? typeof(object), options);
        }

        writer.WriteEndObject();
    }
}

public class EntityJsonConverter : JsonConverter<Entity>
{
    public override Entity? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Deserialization is not implemented in this initial version.
        throw new NotImplementedException("Deserialization is not implemented.");
    }

    public override void Write(Utf8JsonWriter writer, Entity value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        if (value.GetType() != typeof(Entity))
        {
            JsonSerializer.Serialize(writer, value, value.GetType() ?? typeof(object), options);
            return;
        }

        writer.WriteStartObject();

        var type = value.GetType();
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // skip indexers and non-readable
            if (prop.GetIndexParameters().Length > 0 || !prop.CanRead)
                continue;

            var propValue = prop.GetValue(value);

            // skip delegates
            if (propValue is Delegate)
                continue;

            writer.WritePropertyName(prop.Name);
            JsonSerializer.Serialize(writer, propValue, propValue?.GetType() ?? typeof(object), options);
        }

        writer.WriteEndObject();
    }
}

public class ByteArrayHexJsonConverter : JsonConverter<byte[]>
{
    public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(Convert.ToHexString(value));
    }
}

public class SizedStringJsonConverter : JsonConverter<SizedString>
{
    public override SizedString? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, SizedString value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}

public class MalformationJsonConverter : JsonConverter<IMalformable.MalformationManager>
{
    public override IMalformable.MalformationManager? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, IMalformable.MalformationManager value, JsonSerializerOptions options)
    {
        if (value.Keys.Length == 0)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();

        if (value.Keys.Length > 0)
        {
            foreach (var key in value.Keys)
            {
                if (key == null)
                {
                    // root level malformations
                    var propValueInR = value.GetMalformations(null);
                    if (propValueInR.Length > 0)
                    {
                        writer.WritePropertyName("*");
                        writer.WriteStartArray();
                        foreach (var propValInR in propValueInR)
                        {
                            writer.WriteStartArray();
                            writer.WriteStringValue(Enum.GetName(typeof(Malformation), propValInR.Type));
                            foreach (var field in propValInR.Fields)
                                JsonSerializer.Serialize(writer, field, field?.GetType() ?? typeof(object), options);
                            writer.WriteEndArray();
                        }
                        writer.WriteEndArray();
                    }
                    continue;
                }
                if (key.Value.Item2.HasValue)
                    writer.WritePropertyName($"{key.Value.Item1.Name}#{key.Value.Item2.Value}");
                else
                    writer.WritePropertyName(key.Value.Item1.Name);

                writer.WriteStartArray();
                var propValueIn = value.GetMalformations(key.Value.Item1, key.Value.Item2, null);
                foreach (var propValIn in propValueIn)
                {
                    writer.WriteStartArray();
                    writer.WriteStringValue(Enum.GetName(typeof(Malformation), propValIn.Type));
                    foreach (var field in propValIn.Fields)
                        JsonSerializer.Serialize(writer, field, field?.GetType() ?? typeof(object), options);
                    writer.WriteEndArray();
                }
                writer.WriteEndArray();

            }
        }
        writer.WriteEndObject();
    }
}
public class StreamDefectJsonConverter : JsonConverter<StreamDefect>
{
    public override StreamDefect? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Deserialization is not implemented in this initial version.
        throw new NotImplementedException("Deserialization is not implemented.");
    }

    public override void Write(Utf8JsonWriter writer, StreamDefect value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();

        var type = value.GetType();
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // skip indexers and non-readable
            if (prop.GetIndexParameters().Length > 0 || !prop.CanRead)
                continue;

            var propValue = prop.GetValue(value);

            // skip delegates
            if (propValue is Delegate)
                continue;

            // only write out non-null values
            if (propValue is null)
                continue;

            // only write out non-default values
            if (propValue != null && prop.PropertyType.IsValueType && propValue.Equals(Activator.CreateInstance(prop.PropertyType)))
                continue;

            writer.WritePropertyName(prop.Name);
            JsonSerializer.Serialize(writer, propValue, propValue.GetType(), options);
        }

        writer.WriteEndObject();
    }
}

public class MultiClassJsonConverter : JsonConverter<MultiClass>
{
    public override MultiClass? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, MultiClass value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartArray();
        foreach(var candidate in value.Candidates.OrderBy(dr => dr.Expected ? 0 : 1))
        {
            //writer.WriteStartObject();
            //writer.WritePropertyName("Name");
            //writer.WriteStringValue(candidate.Name);
            //writer.WritePropertyName("Expected");
            //writer.WriteBooleanValue(candidate.Expected);
            //writer.WritePropertyName("Object");
            JsonSerializer.Serialize(writer, candidate.Object, candidate.Object.GetType() ?? typeof(object), options);
            //writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }
}

public class MalformationCleanerJsonConverter<T> : JsonConverter<T>
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Deserialization is not implemented in this initial version.
        throw new NotImplementedException("Deserialization is not implemented.");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();

        var type = value.GetType();
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // skip indexers and non-readable
            if (prop.GetIndexParameters().Length > 0 || !prop.CanRead)
                continue;

            var propValue = prop.GetValue(value);

            if (prop.PropertyType == typeof(IMalformable.MalformationManager))
            {
                var malformations = (value as IMalformable)?.Malformations;
                if (malformations != null && malformations.Keys.Length > 0)
                {
                    writer.WritePropertyName(prop.Name);
                    JsonSerializer.Serialize(writer, malformations, malformations?.GetType() ?? typeof(object), options);
                }
                continue;
            }

            // skip delegates
            if (propValue is Delegate)
                continue;

            writer.WritePropertyName(prop.Name);
            JsonSerializer.Serialize(writer, propValue, propValue?.GetType() ?? typeof(object), options);
        }

        writer.WriteEndObject();
    }
}