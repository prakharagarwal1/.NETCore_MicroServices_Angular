using Microsoft.AspNetCore.Authentication;

namespace LearningShop.Gateway;

/// <summary>
/// Options for the API key authentication scheme.
/// </summary>
public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// The name of the header containing the API key.
    /// </summary>
    public string ApiKeyHeaderName { get; set; } = "X-Api-Key";

    /// <summary>
    /// The list of valid API keys. If empty, any non-empty API key header will be accepted.
    /// </summary>
    public IEnumerable<string> ValidApiKeys { get; set; } = new string[0];
}