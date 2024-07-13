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

    #endregion

    #region Services (Methods) called directly from the controller

        /// <summary>
        /// Search photos from Flickr API.
        /// Used when users DO provide the searchTerm in the search box.
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="page"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<List<Photo>> SearchPhotosAsync(string searchTerm, int page, string filter)
        {
            string flickrApiUrl = ConstructFlickrSearchApiUrl(searchTerm, page, filter);
            try
            {
                var response = await _httpClientWrapper.GetStringAsync(flickrApiUrl).ConfigureAwait(false);
                var flickrApiResponse = JsonConvert.DeserializeObject<FlickrApiResponse>(response);
                
                var photos = flickrApiResponse?.Photos?.Photo;
                if (photos == null || !(photos?.ToList()?.Any() ?? false))
                {
                    var message = $"Flickr could not find any matches for {Environment.NewLine} \"{searchTerm}\" ";
                    throw new Exception(message);
                }
                var photoList = photos.ToList();
                return photoList;
            }
            catch (Exception ex)
            {
                var message = $"Start {Environment.NewLine} Oops! {Environment.NewLine} {ex.Message} {Environment.NewLine} End";
                throw new Exception(message.Replace(Environment.NewLine, "<br>"));
            }
        }
        

        /// <summary>
        /// Get the most recent photos from Flickr GetRecent API. 
        /// Used when users DO NOT provide the searchTerm in the search box. 
        /// When users do not specify what photos to search, Flickr will just return the most recent photos.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<Photo>> GetRecentPhotosAsync(int page)
        {
            try
            {
                string flickrApiUrl = ConstructFlickrGetRecentApiUrl(page);
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
            catch (Exception ex)
            {
                var message = $"Start {Environment.NewLine} Oops! {Environment.NewLine} {ex.Message} {Environment.NewLine} End";
                throw new Exception(message.Replace(Environment.NewLine, "<br>"));
            }
        }

    #endregion

    #region Helper Methods

        /// <summary>
        /// Constructs the Flickr search API URL from the values provided in the .env file and appsettings.json file.
        /// The method is used when the searchTerm is not empty or null.
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="page"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public string ConstructFlickrSearchApiUrl(string searchTerm, int page, string filter)
        {
            var flickr = _flickrApiSettings.Value;

            if (string.IsNullOrEmpty(flickr.ApiKey) || string.IsNullOrEmpty(flickr.ApiSecret))
            {
                throw new Exception("Flickr API Key or Secret not found");
            }

            var category = filter switch
            {
                "Relevant" => "relevance",
                "Interesting" => "interestingness-desc",
                "Data uploaded" => "date-posted-desc",
                "Date taken" => "date-taken-desc",
                _ => "relevance"
            };

            var request = new HttpRequestMessage(HttpMethod.Get, $"{flickr.BaseUrl}{flickr.EndpointPath}");
            var parameters = new Dictionary<string, string>
            {
                { "method", flickr.Method },
                { "api_key", flickr.ApiKey },
                { "text", searchTerm },
                { "sort", category},
                { "format", flickr.Format },
                { "nojsoncallback", "1" },
                { "page", page.ToString() }
            };

            // https://www.flickr.com/services/rest/?method=flickr.photos.search&api_key=f41fc432b306fe4659037538b095c884&text=dogs&sort=relevance&format=json&nojsoncallback=1
            request.RequestUri = new Uri(QueryHelpers.AddQueryString(request.RequestUri!.ToString(), parameters));
            return request.RequestUri.ToString();
        }


        /// <summary>
        /// Constructs the Flickr API URL from the values provided in the .env file and appsettings.json file.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public string ConstructFlickrGetRecentApiUrl(int page)
        {
            var flickr = _flickrApiSettings.Value;

            if (string.IsNullOrEmpty(flickr.ApiKey) || string.IsNullOrEmpty(flickr.ApiSecret))
            {
                throw new Exception("Flickr API Key or Secret not found");;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"{flickr.BaseUrl}{flickr.EndpointPath}");
            var parameters = new Dictionary<string, string>
            {
                { "method", "flickr.photos.getRecent" },
                { "api_key", flickr.ApiKey },
                { "format", flickr.Format },
                { "nojsoncallback", "1" },
                { "page", page.ToString() }
            };

            // https://www.flickr.com/services/rest/?method=flickr.photos.getRecent&api_key=f41fc432b306fe4659037538b095c884&format=json&nojsoncallback=1
            request.RequestUri = new Uri(QueryHelpers.AddQueryString(request.RequestUri!.ToString(), parameters));
            return request.RequestUri.ToString();
        }
        
    #endregion 
}


