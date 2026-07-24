using Common.DataConvrters;

namespace Tomouh.Domain;

[StoreEnumAsString(maxLength: 20, namingStrategy: EnumNamingStrategy.Default, caseInsensitive: false)]
public enum FundOrgType
{
    Country = 1,
    University,
    EuropeUnion
}
