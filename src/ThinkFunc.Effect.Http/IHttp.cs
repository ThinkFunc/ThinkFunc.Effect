using LanguageExt;
using LanguageExt.Effects.Traits;
using LanguageExt.Pipes;
using Microsoft.AspNetCore.Http;
using ThinkFunc.Effect.Abstractions;
using static LanguageExt.Prelude;
using static LanguageExt.Pipes.Proxy;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using LanguageExt.ClassInstances;
using System.ComponentModel.DataAnnotations;

namespace ThinkFunc.Effect.Http;

public interface IValid<RT, T> : IHas<RT, IValidator<T>>
    where RT : struct, IHasCancel<RT>, IValid<RT, T>
{

}

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


    public static Producer<RT, T, Unit> RequestTo<T>() =>
       from http in IHas<RT, HttpContext>.Eff
       from _1 in Aff(() => http.Request.ReadFromJsonAsync<T>())
       from _2 in yield(_1)
       select unit;

    public static Pipe<RT, T, T, Unit> Validation<T>(IValidator<T> validator) =>
        from v in awaiting<T>()
        from _1 in Aff<RT, Unit>(rt => validator.ValidateAndThrowAsync(v).ToUnit().ToValue())
        from _2 in yield(v)
        select unit;
}
