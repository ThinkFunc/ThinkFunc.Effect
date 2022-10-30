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
    public async Task Post()
    {
        var q = IHttp<RT>.ResponseAff(
            from dto in IHttp<RT>.GetRequestAff<RequestDto>()
            from _1 in IValid<RT, RequestDto>.ValidateAff(dto)
            select new
            {
                dto.Hello,
            });

        var ret = await q.Run(new(HttpContext, new RequestDtoValidator(), default));
    }

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
