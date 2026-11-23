using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace be_rallyhubv2.persistence.configurations;

public sealed class EnumStringConverter<TEnum> :ValueConverter<TEnum, String> where TEnum:  struct ,Enum
{
    public EnumStringConverter() 
        : base(value => EnumNaming<TEnum>.ToDb(value), text => EnumNaming<TEnum>.FromDb(text))
    {
        
    }
}

public static class EnumNaming<TEnum> where TEnum : struct ,Enum
{
    private static readonly Dictionary<TEnum, string> ToDbMap =
        Enum.GetValues<TEnum>().ToDictionary(x => x, x => ToUpperSnake(x.ToString()));
    
    private static readonly Dictionary<string, TEnum> FromDbMap =
        ToDbMap.ToDictionary(x => x.Value, x => x.Key);
    
    public static string ToDb(TEnum value) => ToDbMap[value];
    
    public static TEnum FromDb(string text) => FromDbMap[text];

    private static string ToUpperSnake(string name)
    {
        var builder = new System.Text.StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            var current = name[i];
            if (i > 0 && char.IsUpper(current) && !char.IsUpper(name[i - 1]))
            {
                builder.Append('_');
            }
            
            builder.Append(char.ToUpper(current));
        }
        
        return builder.ToString();
    }
}