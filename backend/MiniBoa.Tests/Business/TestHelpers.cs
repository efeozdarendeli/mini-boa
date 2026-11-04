using MiniBoa.Framework;
using MiniBoa.Framework.Messaging;

namespace MiniBoa.Tests.Business;

internal static class TestHelpers
{
    public static IMessagingHelper Messaging() => new JsonMessagingHelper(new RequestContext());
}

/// <summary>Yaş gibi tarihe bağlı kuralları test edebilmek için "bugün"ü sabitleyen saat.</summary>
internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
