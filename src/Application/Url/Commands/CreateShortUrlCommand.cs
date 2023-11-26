using FluentValidation;
using HashidsNet;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UrlShortenerService.Application.Common.Interfaces;

namespace UrlShortenerService.Application.Url.Commands;

public record CreateShortUrlCommand : IRequest<string>
{
    public string Url { get; init; } = default!;
    public string? HostUrl {  get; init; } = default!;
}

public class CreateShortUrlCommandValidator : AbstractValidator<CreateShortUrlCommand>
{
    public CreateShortUrlCommandValidator()
    {
        _ = RuleFor(v => v.Url)
          .NotEmpty()
          .WithMessage("Url is required.");
        _ = RuleFor(v => v.Url)
            .Custom((url, context) =>
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out _))
                {
                    context.AddFailure("Bad URL Provided");
                }
            });
        _ = RuleFor(v => v.HostUrl)
            .Custom((url, context) =>
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out _))
                {
                    context.AddFailure("Error Handling Host URL");
                }
            });
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

        while (true)
        {
            string shortCode = CreateShortCode(5);
            // Fetching information from Db in a while-loop seems expensive, investigate something more efficient.
            if (!await _context.Urls.AnyAsync(s => s.ShortCode == shortCode))
            {
                string shortUrl = request.HostUrl + "/" + shortCode;
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
