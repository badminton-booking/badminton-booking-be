using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

/// <summary>Preserves the UPPER_SNAKE_CASE database values with PascalCase C# members.</summary>
internal sealed class EnumStorageConverter<TEnum> : ValueConverter<TEnum, string>
    where TEnum : struct, Enum
{
    public EnumStorageConverter()
        : base(value => ToDatabase(value), value => FromDatabase(value))
    {
    }

    private static readonly IReadOnlyDictionary<TEnum, string> ToStore =
        Enum.GetValues<TEnum>().ToDictionary(
            value => value,
            value => Regex.Replace(value.ToString(), "([a-z0-9])([A-Z])", "$1_$2").ToUpperInvariant());

    private static readonly IReadOnlyDictionary<string, TEnum> FromStore =
        ToStore.ToDictionary(pair => pair.Value, pair => pair.Key, StringComparer.Ordinal);

    private static string ToDatabase(TEnum value) => ToStore[value];

    private static TEnum FromDatabase(string value) => FromStore[value];
}
