using Microsoft.AspNetCore.Mvc.Testing;
using DotNetEnv;
using Microsoft.Extensions.Configuration;

public class IntegrationTest : IClassFixture<WebApplicationFactory<Program>> // Use Program as the entry point
{
    private readonly HttpClient _client;

    public IntegrationTest(WebApplicationFactory<Program> factory)
    {
        // _client = factory.CreateClient();

        // Specify the relative path to the .env file in the main project directory
        string envFilePath = "../Assignment/.env";
        
        // Load environment variables from the specified .env file
        Env.Load(envFilePath);

        _client = factory.WithWebHostBuilder(builder =>
        {
            _ = builder.ConfigureAppConfiguration((context, config) =>
            {
                _ = config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "FlickrApiSettings:ApiKey", Env.GetString("FLICKR_API_KEY") },
                    { "FlickrApiSettings:ApiSecret", Env.GetString("FLICKR_API_SECRET") }
                });
            });
        }).CreateClient();
    }

    [Fact]
    public async Task SearchPhotos_EndpointReturnsSuccessAndCorrectContentType()
    {
        try
        { 
            // Arrange
            var url = "/api/photos/search?searchTerm=test";

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode); // Ensure success status code

            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrEmpty(content), "Response content is empty");

            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }
        catch (HttpRequestException ex)
        {
            // Handle specific HTTP request exceptions
            Console.WriteLine($"HTTP request failed: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            // Handle any other unexpected exceptions
            Console.WriteLine($"Unexpected error: {ex.Message}");
            throw;
        }
    }
}