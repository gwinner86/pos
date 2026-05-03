using System.Text.Json;
using System.Text.Json.Serialization;

namespace POS.API.Converters
{
    public class NullableGuidJsonConverter : JsonConverter<Guid?>
    {
        public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (string.IsNullOrEmpty(stringValue))
                {
                    return null;
                }
                
                if (Guid.TryParse(stringValue, out var guid))
                {
                    return guid;
                }
            }
            
            // Fallback to default behavior (will throw if not a string or invalid guid)
            // But usually for nullable, if it's null token, it's handled automatically.
            // If it is a string but not a guid, we let it fail or handle it? 
            // The error said "could not be converted", so standard parsing is strict.
            // Let's rely on Guid.Parse for non-empty strings to get standard error behavior if invalid format.
            return Guid.Parse(reader.GetString()!);
        }

        public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
