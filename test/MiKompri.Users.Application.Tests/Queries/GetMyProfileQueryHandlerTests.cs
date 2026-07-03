using FluentAssertions;
using MiKompri.Users.Application.Abstractions;
using MiKompri.Users.Application.Queries.GetMyProfile;
using MiKompri.Users.Domain.Users;
using NSubstitute;

namespace MiKompri.Users.Application.Tests.Queries;

public class GetMyProfileQueryHandlerTests
{
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();

    [Fact]
    public async Task Handle_UserExists_ReturnsUserProfileDto()
    {
        var userId = Guid.NewGuid();
        _currentUser.UserId.Returns(userId);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new User("Ana", "ana@demo.com", "entra", "sub-1"));

        var handler = new GetMyProfileQueryHandler(_currentUser, _userRepository);

        var result = await handler.Handle(new GetMyProfileQuery(), CancellationToken.None);

        result.DisplayName.Should().Be("Ana");
        result.IdentityProvider.Should().Be("entra");
    }

    [Fact]
    public async Task Handle_UserNotExists_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        _currentUser.UserId.Returns(userId);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var handler = new GetMyProfileQueryHandler(_currentUser, _userRepository);

        var act = () => handler.Handle(new GetMyProfileQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
