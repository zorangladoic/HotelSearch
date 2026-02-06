using FluentAssertions;
using HotelSearch.Application.Hotels.Queries.SearchHotels;
using HotelSearch.Application.Interfaces;
using HotelSearch.Domain.Entities;
using HotelSearch.Infrastructure.Services;
using Moq;

namespace HotelSearch.UnitTests.Application.Queries;

public class SearchHotelsQueryHandlerTests
{
    private readonly Mock<IHotelRepository> _repositoryMock;
    private readonly IHotelSearchService _searchService;
    private readonly SearchHotelsQueryHandler _handler;

    public SearchHotelsQueryHandlerTests()
    {
        _repositoryMock = new Mock<IHotelRepository>();
        // Use real search service - it contains the business logic we want to test
        _searchService = new HotelSearchService();
        _handler = new SearchHotelsQueryHandler(_repositoryMock.Object, _searchService);
    }

    [Fact]
    public async Task Handle_WithNoHotels_ShouldReturnEmptyResult()
    {
        // Arrange
        var query = new SearchHotelsQuery(45.815, 15.982, 1, 10);

        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Hotel>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_WithHotels_ShouldReturnSortedByPriceAndDistance()
    {
        // Arrange
        var userLatitude = 45.815;
        var userLongitude = 15.982;

        // Create hotels with different prices and distances
        // Hotel close and cheap (should rank high)
        var cheapClose = Hotel.Create("Cheap & Close", 50m, 45.82, 15.99);
        // Hotel far and expensive (should rank low)
        var expensiveFar = Hotel.Create("Expensive & Far", 500m, 48.20, 16.37);
        // Hotel medium price, medium distance
        var medium = Hotel.Create("Medium", 150m, 46.5, 16.0);

        var hotels = new List<Hotel> { expensiveFar, medium, cheapClose };

        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotels);

        var query = new SearchHotelsQuery(userLatitude, userLongitude, 1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);

        // Cheap & Close should be first (lowest combined score)
        result.Items[0].Name.Should().Be("Cheap & Close");
        // Expensive & Far should be last (highest combined score)
        result.Items[2].Name.Should().Be("Expensive & Far");
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var hotels = Enumerable.Range(1, 25)
            .Select(i => Hotel.Create($"Hotel {i}", 100m + i, 45.0 + i * 0.01, 15.0))
            .ToList();

        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotels);

        var query = new SearchHotelsQuery(45.0, 15.0, 2, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(3);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_WithLastPage_ShouldReturnRemainingItems()
    {
        // Arrange
        var hotels = Enumerable.Range(1, 25)
            .Select(i => Hotel.Create($"Hotel {i}", 100m + i, 45.0 + i * 0.01, 15.0))
            .ToList();

        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotels);

        var query = new SearchHotelsQuery(45.0, 15.0, 3, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(5); // Last page has only 5 items
        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WithSingleHotel_ShouldReturnSingleResult()
    {
        // Arrange
        var hotel = Hotel.Create("Only Hotel", 100m, 45.815, 15.982);

        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Hotel> { hotel });

        var query = new SearchHotelsQuery(45.815, 15.982, 1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be("Only Hotel");
        result.Items[0].DistanceKm.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldIncludeDistanceInResults()
    {
        // Arrange
        var hotel = Hotel.Create("Test Hotel", 100m, 48.208, 16.373); // Vienna

        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Hotel> { hotel });

        // User in Zagreb
        var query = new SearchHotelsQuery(45.815, 15.982, 1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].DistanceKm.Should().BeApproximately(268, 15); // ~268 km Zagreb to Vienna
    }

    [Fact]
    public async Task Handle_WithSamePriceHotels_ShouldSortByDistance()
    {
        // Arrange
        var closeHotel = Hotel.Create("Close Hotel", 100m, 45.82, 15.99);
        var farHotel = Hotel.Create("Far Hotel", 100m, 48.20, 16.37);

        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Hotel> { farHotel, closeHotel });

        var query = new SearchHotelsQuery(45.815, 15.982, 1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items[0].Name.Should().Be("Close Hotel");
        result.Items[1].Name.Should().Be("Far Hotel");
    }

    [Fact]
    public async Task Handle_WithSameDistanceHotels_ShouldSortByPrice()
    {
        // Arrange - Hotels at same location, different prices
        var cheapHotel = Hotel.Create("Cheap Hotel", 50m, 45.815, 15.982);
        var expensiveHotel = Hotel.Create("Expensive Hotel", 500m, 45.815, 15.982);

        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Hotel> { expensiveHotel, cheapHotel });

        var query = new SearchHotelsQuery(45.815, 15.982, 1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items[0].Name.Should().Be("Cheap Hotel");
        result.Items[1].Name.Should().Be("Expensive Hotel");
    }
}
