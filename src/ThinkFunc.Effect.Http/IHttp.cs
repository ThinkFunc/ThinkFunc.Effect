using LanguageExt;
using LanguageExt.Pipes;
using Microsoft.AspNetCore.Http;
using ThinkFunc.Effect.Abstractions;
using static LanguageExt.Prelude;

namespace ThinkFunc.Effect.Http;

public interface IHttp<RT> : IHas<RT, HttpContext>
    where RT : struct, IHasCancel<RT>, IHttp<RT>
{
    public static Aff<RT, T> GetRequestAff<T>() =>
        from http in IHas<RT, HttpContext>.Eff
        from _1 in Aff(() => http.Request.ReadFromJsonAsync<T>())
        select _1;

    public static Aff<RT, Unit> ResponseAff<T>(Aff<RT, T> aff) =>
        from http in IHas<RT, HttpContext>.Eff
        from _1 in aff.BiBind(
            x => Eff(() => Results.Ok(x)),
            e => Eff(() => e.Exception.Case switch
            {
                Exception ex => Results.Problem(ex.Message),
                _ => Results.StatusCode(500)
            }))
        from _2 in Aff(() => _1.ExecuteAsync(http).ToUnit().ToValue())
        select unit;
}
