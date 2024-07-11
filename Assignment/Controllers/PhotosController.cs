using Microsoft.AspNetCore.Mvc;
using Assignment.Models;

[ApiController]
[Route("api/[controller]")]
public class PhotosController(IFetchService fetchService) : ControllerBase
{
    private readonly IFetchService _fetchService = fetchService;

    /// <summary>
    /// Endpoint to search photos from Flickr: api/photos/search
    /// </summary>
    /// <param name="searchTerm"></param>
    /// <param name="page"></param>
    /// <returns></returns>      
    [HttpGet("search")]
    public async Task<ActionResult<List<Photo>>> SearchPhotos([FromQuery] string searchTerm, [FromQuery] int page = 1)
    {
        var photos = await _fetchService.SearchPhotosAsync(searchTerm, page);
        return Ok(photos);
    }
}
