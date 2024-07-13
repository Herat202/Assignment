using Assignment.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

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
        var response = await _httpClientWrapper.GetStringAsync(flickrApiUrl).ConfigureAwait(false);
        var flickrApiResponse = JsonConvert.DeserializeObject<FlickrApiResponse>(response);
        
        if (flickrApiResponse?.Photos?.Photo == null)
        {
            var message = $"Response from Flickr: No photos found!";
            throw new Exception(message);
        }

        var photoList = flickrApiResponse.Photos.Photo.ToList();
        return photoList;
    }
    catch (HttpRequestException httpEx)
    {
        var message = $"Start An error occurred while processing your request. {Environment.NewLine} {httpEx.Message} End";
        throw new Exception(message.Replace(Environment.NewLine, "<br>"));

    }
    catch (Exception ex)
    {
        var message = $"Start An error occurred while processing your request. {Environment.NewLine} {ex.Message} End";
        throw new Exception(message.Replace(Environment.NewLine, "<br>"));
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

        if (string.IsNullOrEmpty(flickr.ApiKey) || string.IsNullOrEmpty(flickr.ApiSecret))
        {
            throw new InvalidOperationException("Flickr API Key or Secret not found");;
        }

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


