using System.Diagnostics;
using System.Dynamic;
using System.Text.Json;
using FluentValidation;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Pipes;
using Microsoft.AspNetCore.Http;
using ThinkFunc.Effect.Abstractions;
using static System.Net.WebRequestMethods;
using static LanguageExt.Prelude;

namespace ThinkFunc.Effect.Http;

public static class ExceptionExtensions
{
    internal static IResult ToResult(this ValidationException ex) =>
        Results.ValidationProblem(
            ex.Errors
              .GroupBy(x => x.PropertyName)
              .ToDictionary(x => x.Key, x => x.Select(e => e.ErrorMessage).ToArray()),
            extensions: new Dictionary<string, object?>
            {
                ["traceId"] = Activity.Current?.Id
            });

    internal static Eff<IResult> ToResultEff(this Error err) =>
        Eff(() => err.Exception.Case switch
        {
            ValidationException ex => ex.ToResult(),
            Exception ex => Results.Problem(ex.Message,
                extensions : new Dictionary<string, object?>
                {
                    ["traceId"] = Activity.Current?.Id
                }),
            _ => Results.StatusCode(500)
        });
}

public static class LiquidResultExtensions
{
    //public static IResult LiquidView(this IResultExtensions resultExtensions, string name)
    //{
    //    return new LiquidView(name);
    //}
}

public partial interface IHttp<RT> : IHas<RT, HttpContext> where RT : struct, IHasCancel<RT>, IHttp<RT>
{
    public static Aff<RT, T> GetRequestAff<T>() =>
        from http in IHas<RT, HttpContext>.Eff
        from _1 in Aff(() => http.Request.ReadFromJsonAsync<T>())
        select _1;

    public static Aff<RT, Unit> ResponseAff<T>(Aff<RT, T> aff, Uri uri = null) =>
        from http in IHas<RT, HttpContext>.Eff
        from _1 in aff.BiBind(
            x => Eff(() =>
            {
                var json = JsonSerializer.SerializeToNode(x, JsonSerializerOptions);
                json["traceId"] = Activity.Current?.Id;
                return uri is null ? Results.Ok(json) : Results.Created(uri, json);
            }),
            e => e.ToResultEff())
        from _2 in Aff(() => _1.ExecuteAsync(http).ToUnit().ToValue())
        select unit;
    internal static JsonSerializerOptions JsonSerializerOptions { get; } = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}
