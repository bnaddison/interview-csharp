using UrlShortenerService.Domain.Common;

namespace UrlShortenerService.Domain.Entities;

/// <summary>
/// Url domain entity.
/// </summary>
public class Url : BaseAuditableEntity
{
    #region constructors and destructors

    /// <summary>
    /// Default constructor.
    /// </summary>
    public Url() { }

    #endregion

    #region properties

    /// <summary>
    /// The original url.
    /// </summary>
    public string OriginalUrl { get; set; } = default!;
    public string ShortUrl { get; set; } = default!;
    public string ShortCode { get; set; } = default!;

    #endregion
}
