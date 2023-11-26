using MediatR;
using Microsoft.AspNetCore.Http.Extensions;
using UrlShortenerService.Api.Endpoints.Url.Requests;
using UrlShortenerService.Application.Url.Commands;
using IMapper = AutoMapper.IMapper;

namespace UrlShortenerService.Api.Endpoints.Url;

public class CreateShortUrlSummary : Summary<CreateShortUrlEndpoint>
{
    public CreateShortUrlSummary()
    {
        Summary = "Create short url from provided url";
        Description =
            "This endpoint will create a short url from provided original url.";
        Response(500, "Internal server error.");
    }
}

public class CreateShortUrlEndpoint : BaseEndpoint<CreateShortUrlRequest>
{
    public CreateShortUrlEndpoint(ISender mediator, IMapper mapper)
        : base(mediator, mapper) { }

    public override void Configure()
    {
        base.Configure();
        Post("u");
        AllowAnonymous();
        Description(
            d => d.WithTags("Url")
        );
        Summary(new CreateShortUrlSummary());
    }

    public override async Task HandleAsync(CreateShortUrlRequest req, CancellationToken ct)
    {
        // Not super happy with this way of accessing the host url
        IHttpContextAccessor httpContextAccessor = new HttpContextAccessor();
        string endpointUrl = "";
        if (httpContextAccessor.HttpContext != null)
        {
            endpointUrl = httpContextAccessor.HttpContext.Request.GetDisplayUrl();
        }
        var result = await Mediator.Send(
            new CreateShortUrlCommand
            {
                Url = req.Url,
                EndpointUrl = endpointUrl
            },
            ct
        );
        await SendOkAsync(result);
    }
}
