using Microsoft.AspNetCore.Mvc;
using ThinkFunc.Effect.Abstractions;
using static LanguageExt.Prelude;
using LanguageExt;

namespace ThinkFunc.Effect.Http.StubApi.Controllers;

public record RequestDto(string Hello);

[ApiController]
[Route("[controller]")]
public class EchoController : ControllerBase
{
    [HttpPost()]
    public async Task Post()
    {
        var q = IHttp<RT>.ResponseAff(
            from req in IHttp<RT>.GetRequestAff<RequestDto>()
            select req);

        var ret = await q.Run(new(HttpContext, default));
    }

    public readonly record struct RT
    (
        HttpContext HttpContext,
        CancellationTokenSource CancellationTokenSource
    ) : IHasCancel<RT>,
        IHttp<RT>
    {
        HttpContext IHas<RT, HttpContext>.It => HttpContext;
    }
}
