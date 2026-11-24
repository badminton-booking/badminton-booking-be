using System.Globalization;
using System.Reflection;
using System.Resources;
using be_rallyhubv2.contract.abstractions.common;
using be_rallyhubv2.contract.abstractions.results;
using be_rallyhubv2.contract.resources;

namespace be_rallyhubv2.tests.unit.abstractions;

public class ErrorMessagesTests
{
    private static readonly ResourceManager Messages = new(typeof(ErrorMessages));

    public static IEnumerable<object[]> AllCodes() =>
        typeof(CommonErrors).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(Error))
            .Select(field => new object[] { ((Error)field.GetValue(null)!).Code })
            .Append(new object[] { "Common.NotFound" });
    
    [Theory]
    [MemberData(nameof(AllCodes))]
    public void Every_code_has_english_and_vietnamese(string code)
    {
        var en = Messages.GetString(code, CultureInfo.InvariantCulture);
        var vi = Messages.GetString(code, new CultureInfo("vi"));

        Assert.False(string.IsNullOrWhiteSpace(en));
        Assert.False(string.IsNullOrWhiteSpace(vi));
        Assert.NotEqual(en, vi);
    }
}