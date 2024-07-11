using Assignment.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

/// <summary>
/// Primary constructor: class and constructor declaration in one go
/// Allowing HttpClientWrapper through the constructor to allow for dependency injection
/// </summary>
/// <param name="httpClientWrapper"></param>
/// <param name="flickrApiSettings"></param>
public class FetchService(IHttpClientWrapper httpClientWrapper, IOptions<FlickrApiSettings> flickrApiSettings) : IFetchService
{
    #region Fields (Private Fields)

    private readonly IHttpClientWrapper _httpClientWrapper = httpClientWrapper ?? throw new ArgumentNullException(nameof(httpClientWrapper));
    private readonly IOptions<FlickrApiSettings> _flickrApiSettings = flickrApiSettings ?? throw new ArgumentNullException(nameof(flickrApiSettings));

    // private readonly string _apiKey = flickrApiSettings.Value.ApiKey ?? throw new InvalidOperationException("API Key not found");
    // private readonly string _apiSecret = flickrApiSettings.Value.ApiSecret ?? throw new InvalidOperationException("API Secret not found");

    #endregion

    #region Methods (Public Methods)

    /// <summary>
    /// Search photos from Flickr API. The method is called from the PhotosController.
    /// </summary>
    /// <param name="searchTerm"></param>
    /// <param name="page"></param>
    /// <returns></returns>
    public async Task<List<Photo>> SearchPhotosAsync(string searchTerm, int page)
    {
        string flickrApiUrl = ConstructFlickrApiUrl(searchTerm, page);
        try
        {
            var response = await _httpClientWrapper.GetStringAsync(flickrApiUrl);
            var flickrResponse = JObject.Parse(response);
            var photos = new List<Photo>();

            if (flickrResponse.SelectToken("photos.photo") is not JArray photoArray)
            {
                // Handle the case where the JSON structure is not as expected
                return photos;
            }
            foreach (var photo in photoArray)
            {
                if (photo != null)
                {
    #pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                    photos.Add(new Photo(
                        PhotoId: 1,
                        Title: (string)photo["title"] ?? string.Empty,
                        Description: "",
                        DateFetched: new DateTime(),
                        Tags: searchTerm,
                        ImageUrl: $"https://live.staticflickr.com/{photo["server"] ?? "default"}/{photo["id"] ?? "default"}_{photo["secret"] ?? "default"}_m.jpg"
                    ));
    #pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                }
            }
            return photos;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Consgtructs the Flickr API URL from the values provided in the .env file and appsettings.json file.
    /// </summary>
    /// <param name="searchTerm"></param>
    /// <param name="page"></param>
    /// <returns></returns>
    public string ConstructFlickrApiUrl(string searchTerm, int page)
    {
        var flickr = _flickrApiSettings.Value;
        _ = flickr.ApiKey ?? throw new InvalidOperationException("API Key not found");
        _ = flickr.ApiSecret ?? throw new InvalidOperationException("API Secret not found");

        var request = new HttpRequestMessage(HttpMethod.Get, $"{flickr.BaseUrl}{flickr.EndpointPath}");
        var parameters = new Dictionary<string, string>
        {
            { "method", flickr.Method },
            { "api_key", flickr.ApiKey },
            { "text", searchTerm },
            { "format", flickr.Format },
            { "nojsoncallback", "1" },
            { "page", page.ToString() }
        };

        request.RequestUri = new Uri(QueryHelpers.AddQueryString(request.RequestUri!.ToString(), parameters));
        return request.RequestUri.ToString();
    }

    #endregion
}


