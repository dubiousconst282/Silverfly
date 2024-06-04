using Argon;
using Furesoft.PrattParser.Testing.Converter;
using VerifyTests;
using static VerifyTests.VerifierSettings;

namespace Furesoft.PrattParser.Testing;

public class SnapshotParserTestBase
{
    public static VerifySettings settings = new VerifySettings();

    public static void Init()
    {
        AddExtraSettings(_ =>
        {
            _.Converters.Add(new SymbolConverter());
            _.Converters.Add(new DocumentConverter());
            _.Converters.Add(new RangeConverter());
            _.Converters.Add(new ReadonlyMemoryConverter());

            _.TypeNameHandling = TypeNameHandling.All;
        });

        settings.UseDirectory("TestResults");
    }
}
