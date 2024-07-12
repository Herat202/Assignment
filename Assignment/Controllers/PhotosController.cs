using Microsoft.AspNetCore.Mvc;
using Assignment.Models;

[ApiController]
[Route("api/[controller]")]
public class PhotosController(IFetchService fetchService, ILogger<PhotosController> logger) : ControllerBase
{
    private readonly IFetchService _fetchService = fetchService;
    private readonly ILogger<PhotosController> _logger = logger;

    /// <summary>
    /// Endpoint to search photos from Flickr: api/photos/search
    /// </summary>
    /// <param name="searchTerm"></param>
    /// <param name="page"></param>
    /// <returns></returns>      
    [HttpGet("search")]
    public async Task<ActionResult<List<Photo>>> SearchPhotos([FromQuery] string searchTerm, [FromQuery] int page = 1)
    {
        try
        {
            var photos = await _fetchService.SearchPhotosAsync(searchTerm, page);
            return Ok(photos);
        }
        catch (Exception ex)
        {
            // Log the exception details
            _logger.LogError(ex, "Failed to search photos with term {SearchTerm} and page {Page}", searchTerm, page);

            // Return an appropriate error response
            var message = $"Start An error occurred while processing your request. {Environment.NewLine} {ex.Message} End";
            throw new Exception(message.Replace(Environment.NewLine, "<br>"));
        }
    }
}
