using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotnetSpawn.Templating.Serialization
{
    internal class SpawnTemplateTypeConverter : JsonConverter<SpawnTemplate>
    {
        private readonly string _schemaPath;

        public SpawnTemplateTypeConverter(string schemaPath)
        {
            _schemaPath = schemaPath;
        }

        public override SpawnTemplate Read(
            ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public override void Write(
            Utf8JsonWriter writer, SpawnTemplate value, JsonSerializerOptions options)
        {
            var values = new Dictionary<string, object>
            {
                { "$schema", $"file:/{_schemaPath.Replace('\\', '/')}" }
            };

            var properties = typeof(SpawnTemplate).GetProperties(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.DeclaredOnly);

            foreach (var property in properties)
            {
                var propertyName = options.PropertyNamingPolicy.ConvertName(property.Name);
                var propertyValue = property.GetValue(value);

                values.Add(propertyName, propertyValue);
            }

            JsonSerializer.Serialize(writer, values, typeof(Dictionary<string, object>), options);
        }
    }
}