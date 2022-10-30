using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using ThinkFunc.Effect.Abstractions;
using static LanguageExt.Prelude;

namespace ThinkFunc.Effect.Http.StubApi.Controllers;


[ApiController]
[Route("[controller]")]
public class PhoneController : ControllerBase
{
    [HttpPost()]
    [Consumes(typeof(PhoneDto), "application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task PostAsync(CancellationToken ct)
    {
        var q = IHttp<RT>.ResponseAff(
            from dto in IHttp<RT>.GetRequestAff<PhoneDto>()
            from _1 in IValid<RT, PhoneDto>.ValidateAff(dto)
            select new
            {
                dto.Phone,
            });

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _ = await q.Run(new(HttpContext, DtoValidator, cts));
    }

    internal PhoneDtoValidator DtoValidator { get; } = new PhoneDtoValidator();

    internal record PhoneDto(string Phone, string Region);


    internal class PhoneDtoValidator : AbstractValidator<PhoneDto>
    {
        public PhoneDtoValidator()
        {
            RuleFor(x => ValueTuple.Create(x.Phone, x.Region)).Phone();
            RuleFor(x => x.Phone).Phone("KR");
        }
    }

    internal readonly record struct RT
    (
        HttpContext HttpContext,
        IValidator<PhoneDto> Validator,
        CancellationTokenSource CancellationTokenSource
    ) : IHasCancel<RT>,
        IHttp<RT>,
        IValid<RT, PhoneDto>
    {
        HttpContext IHas<RT, HttpContext>.It => HttpContext;
        IValidator<PhoneDto> IHas<RT, IValidator<PhoneDto>>.It => Validator;
    }
}
