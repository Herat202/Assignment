using Newtonsoft.Json.Linq;
using Assignment.Models;
using Microsoft.Extensions.Options;

/// <summary>
/// Primary constructor: class and constructor declaration in one go
/// </summary>
/// <param name="httpClient"></param>
/// <param name="flickrApiSettings"></param>
/// <exception cref="InvalidOperationException"></exception>
public class FetchService(HttpClient httpClient, IOptions<FlickrApiSettings> flickrApiSettings)
{
    /// <summary>
    /// Private fields
    /// </summary>
    private readonly HttpClient _httpClient = httpClient;
    private readonly string _apiKey = flickrApiSettings.Value.ApiKey ?? throw new InvalidOperationException("API Key not found");
    private readonly string _apiSecret = flickrApiSettings.Value.ApiSecret ?? throw new InvalidOperationException("API Secret not found");


    /// <summary>
    /// Search photos from Flickr API. The method is called from the PhotosController.
    /// </summary>
    /// <param name="searchTerm"></param>
    /// <param name="page"></param>
    /// <returns></returns>
    public async Task<List<Photo>> SearchPhotosAsync(string searchTerm, int page)
    {
        string flickrApiUrl = $"https://api.flickr.com/services/rest/?method=flickr.photos.search&api_key={_apiKey}&text={searchTerm}&format=json&nojsoncallback=1&page={page}";

        var response = await _httpClient.GetStringAsync(flickrApiUrl);
        var flickrResponse = JObject.Parse(response);
        var photos = new List<Photo>();

        // Before pattern matching:
        // var photoArray = flickrResponse.SelectToken("photos.photo") as JArray;
        // if (photoArray == null)
        // {
        //     // Handle the case where the JSON structure is not as expected
        //     return photos;
        // }

        // With pattern matching:
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
}


