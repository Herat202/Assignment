namespace Assignment.Models;

// Notes to myself:

// Do not use positional record, because:
// They generate a primary constructor that takes parameters for all properties.
// They don't have a parameterless constructor by default, which is required for configuration binding with IOptions<T>.
// FX: This generates a constructor public FlickrApiSettings(string apiKey, string apiSecret) but no parameterless constructor.

// Regular classes can have both parameterless and parameterized constructors.
// The parameterless constructor is required for the configuration binding to work with IOptions<T>.


/// <summary>
/// The API key and secret for the Flickr API. 
/// The values are saved securely inside the .env file.
/// </summary>        
public class FlickrApiSettings
{
    public required string ApiKey { get; set; }
    public required string ApiSecret { get; set; }
}
