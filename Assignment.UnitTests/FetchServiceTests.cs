using Assignment.Models;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json.Linq;

public class FetchServiceTests
{
    [Fact]
    public async Task SearchPhotosAsync_ReturnsPhotos()
    {
        // Arrange
        var mockHttpClientWrapper = new Mock<IHttpClientWrapper>();
        var flickrApiSettings = Options.Create(new FlickrApiSettings { ApiKey = "test_key", ApiSecret = "test_secret" });
        var fetchService = new FetchService(mockHttpClientWrapper.Object, flickrApiSettings);

        var searchTerm = "cat";
        var page = 1;
        var flickrApiUrl = $"https://api.flickr.com/services/rest?method=flickr.photos.search&api_key=test_key&text={searchTerm}&format=json&nojsoncallback=1&page={page}";

        var mockResponseContent = new JObject(
            new JProperty("photos", new JObject(
                new JProperty("photo", new JArray(
                    new JObject(
                        new JProperty("id", "1"),
                        new JProperty("title", "Test Photo"),
                        new JProperty("server", "server1"),
                        new JProperty("secret", "secret1")
                    )
                ))
            ))
        ).ToString();

        mockHttpClientWrapper.Setup(wrapper => wrapper.GetStringAsync(It.Is<string>(url => url == flickrApiUrl)))
                             .ReturnsAsync(mockResponseContent);

        // Act
        var result = await fetchService.SearchPhotosAsync(searchTerm, page);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Test Photo", result[0].Title);
        Assert.Equal($"https://live.staticflickr.com/server1/1_secret1_m.jpg", result[0].ImageUrl);
    }
}