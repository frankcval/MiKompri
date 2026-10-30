using Microsoft.Extensions.Logging;
using MiKompri.Users.Application.Behavior;
using MiKompri.Users.Application.Commands.SyncProfile;
using NSubstitute;
namespace MiKompri.Users.Application.Tests;
public class LoggingPrivacyTests
{
    [Fact]
    public async Task AuthenticationAndProfilePayloads_AreNotDestructuredIntoLogs()
    {
        var logger = Substitute.For<ILogger<LoggingBehavior<SyncProfileCommand, int>>>();
        var behavior = new LoggingBehavior<SyncProfileCommand, int>(logger);
        var request = new SyncProfileCommand("ciam", "tenant-private", "object-private", null, "Private name", "private@example.invalid");
        await behavior.Handle(request, () => Task.FromResult(1), default);
        var log = string.Join("\n", logger.ReceivedCalls().Where(c => c.GetMethodInfo().Name == "Log").Select(c => c.GetArguments()[2]?.ToString()));
        Assert.DoesNotContain("private@example.invalid", log);
        Assert.DoesNotContain("Private name", log);
        Assert.DoesNotContain("object-private", log);
        Assert.Contains(nameof(SyncProfileCommand), log);
    }
}
