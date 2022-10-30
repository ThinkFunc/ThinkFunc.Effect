using Microsoft.AspNetCore.Mvc;
using ThinkFunc.Effect.Abstractions;
using static LanguageExt.Prelude;
using LanguageExt;
using FluentValidation;

namespace ThinkFunc.Effect.Http.StubApi.Controllers;

public record RequestDto(string Hello);

public class RequestDtoValidator : AbstractValidator<RequestDto>
{
    public RequestDtoValidator()
    {
        RuleFor(x => x.Hello).NotEmpty();
    }
}

[ApiController]
[Route("[controller]")]
public class EchoController : ControllerBase
{
    [HttpPost()]
    [Consumes(typeof(RequestDto), "application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task Post(CancellationToken ct)
    {
        var q = IHttp<RT>.ResponseAff(
            from dto in IHttp<RT>.GetRequestAff<RequestDto>()
            from _1 in IValid<RT, RequestDto>.ValidateAff(dto)
            select new
            {
                dto.Hello,
            });

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        await q.Run(new(HttpContext, RequestDtoValidator, cts));
    }

    internal RequestDtoValidator RequestDtoValidator { get; } = new RequestDtoValidator();

    public readonly record struct RT
    (
        HttpContext HttpContext,
        IValidator<RequestDto> Validator,
        CancellationTokenSource CancellationTokenSource
    ) : IHasCancel<RT>,
        IHttp<RT>,
        IValid<RT, RequestDto>
    {
        HttpContext IHas<RT, HttpContext>.It => HttpContext;
        IValidator<RequestDto> IHas<RT, IValidator<RequestDto>>.It => Validator;
    }
}
