using FluentAssertions;
using HotelSearch.Application.Hotels.Commands.CreateHotel;
using HotelSearch.Application.Interfaces;
using HotelSearch.Domain.Entities;
using Moq;

namespace HotelSearch.UnitTests.Application.Commands;

public class CreateHotelCommandHandlerTests
{
    private readonly Mock<IHotelRepository> _repositoryMock;
    private readonly CreateHotelCommandHandler _handler;

    public CreateHotelCommandHandlerTests()
    {
        _repositoryMock = new Mock<IHotelRepository>();
        _handler = new CreateHotelCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateHotelAndReturnDto()
    {
        // Arrange
        var command = new CreateHotelCommand("Grand Hotel", 150.00m, 45.815, 15.982);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Hotel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Hotel h, CancellationToken _) => h);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Grand Hotel");
        result.PricePerNight.Should().Be(150.00m);
        result.Latitude.Should().Be(45.815);
        result.Longitude.Should().Be(15.982);
        result.Id.Should().NotBeEmpty();

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Hotel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldTrimHotelName()
    {
        // Arrange
        var command = new CreateHotelCommand("  Grand Hotel  ", 150.00m, 45.815, 15.982);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Hotel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Hotel h, CancellationToken _) => h);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("Grand Hotel");
    }

    [Fact]
    public async Task Handle_WithRepositoryException_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateHotelCommand("Grand Hotel", 150.00m, 45.815, 15.982);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Hotel>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Repository error"));

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Repository error");
    }
}
