using System.Net.Http;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Web;
using FluentValidation;
using HashidsNet;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UrlShortenerService.Application.Common.Exceptions;
using UrlShortenerService.Application.Common.Interfaces;

namespace UrlShortenerService.Application.Url.Commands;

public record CreateShortUrlCommand : IRequest<string>
{
    public string Url { get; init; } = default!;
    public string? EndpointUrl {  get; init; } = default!;
}

public class CreateShortUrlCommandValidator : AbstractValidator<CreateShortUrlCommand>
{
    public CreateShortUrlCommandValidator()
    {
        _ = RuleFor(v => v.Url)
          .NotEmpty()
          .WithMessage("Url is required.");
        // Should possible do Url validity check here?
    }
}

public class CreateShortUrlCommandHandler : IRequestHandler<CreateShortUrlCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly IHashids _hashids;
    private readonly Random _random;

    public CreateShortUrlCommandHandler(IApplicationDbContext context, IHashids hashids)
    {
        _context = context;
        _hashids = hashids;
        _random = new Random();
    }

    public async Task<string> Handle(CreateShortUrlCommand request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
        {
            throw new NotFoundException("Bad URL provided");
        }
        if (!Uri.TryCreate(request.EndpointUrl, UriKind.Absolute, out _))
        {
            throw new NotFoundException("Error Handling Host URL");
        }

        while (true)
        {
            string shortCode = CreateShortCode(5);
            // Fetching information from Db in a while-loop seems expensive, investigate something more efficient.
            if (!await _context.Urls.AnyAsync(s => s.ShortCode == shortCode))
            {
                string shortUrl = request.EndpointUrl + "/" + shortCode;
                _ = await _context.Urls.AddAsync(new Domain.Entities.Url()
                {
                    OriginalUrl = request.Url,
                    ShortCode = shortCode,
                    ShortUrl = shortUrl
                });
                _ = _context.SaveChangesAsync(cancellationToken);
                return shortUrl;
            }
        }
    }

    private string CreateShortCode(int shortStringLength)
    {
        string urlSafeChars = "abcdefghijklmnopqrstuvwxwzABCDEFGHIJKLMNOPQRSTUVWXWZ0123456789-_.~";
        string shortCodeResult = "";

        int i = 0;
        while (i < shortStringLength)
        {
            shortCodeResult += urlSafeChars[_random.Next(urlSafeChars.Length) - 1];
            i++;
        }
        return shortCodeResult;
    }
}
