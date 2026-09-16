using Tomouh.Shared.Kernel.DataConvrters;

namespace Tomouh.Auth.Infrastructure.JsonConverters
{
    internal class InfrastructureJsonRegistry
    {
        public static void RegisterConverters()
        {
            JsonSerializationHelper.AddConverter(new NonFlagsEnumConverterFactory());
        }
    }
}
