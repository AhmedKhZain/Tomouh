using Tomouh.Auth.Domain.Enums;
using Tomouh.Shared.Kernel.DataConvrters;

namespace Tomouh.Auth.Infrastructure.JsonConverters
{
    internal class InfrastructureJsonRegistry
    {
        public static void RegisterConverters()
        {
            JsonSerializationHelper.AddConverter(new UserJsonConverter());
            JsonSerializationHelper.AddConverter(new Role.RoleJsonConverter());
            JsonSerializationHelper.AddConverter(new UserProfileJsonConverter());
            JsonSerializationHelper.AddConverter(new NonFlagsEnumConverterFactory());
        }
    }
}
