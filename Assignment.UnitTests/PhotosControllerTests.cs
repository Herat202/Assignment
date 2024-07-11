using Microsoft.AspNetCore.Mvc;
using Moq;
using Assignment.Models;

public class PhotosControllerTests
{
    [Fact]
    public async Task SearchPhotos_ReturnsOkObjectResult_WithListOfPhotos()
    {
        // Arrange
        var mockFetchService = new Mock<IFetchService>();
        var photosController = new PhotosController(mockFetchService.Object);
        var searchTerm = "test";
        var page = 1;
        var mockPhotos = new List<Photo>
        {
            new Photo(1, "Test Photo", "", "http://example.com/photo.jpg", new DateTime(), searchTerm)
        };

        mockFetchService.Setup(service => service.SearchPhotosAsync(searchTerm, page)).ReturnsAsync(mockPhotos);

        // Act
        var result = await photosController.SearchPhotos(searchTerm, page);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedPhotos = Assert.IsType<List<Photo>>(okResult.Value);
        Assert.Single(returnedPhotos);
        Assert.Equal("Test Photo", returnedPhotos[0].Title);
    }
}