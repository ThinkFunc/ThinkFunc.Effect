using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using FluentValidation;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Pipes;
using Microsoft.AspNetCore.Http;
using ThinkFunc.Effect.Abstractions;
using static LanguageExt.Prelude;

namespace ThinkFunc.Effect.Http;

public interface IHttp<RT> : IHas<RT, HttpContext> where RT : struct, IHttp<RT>
{
    public static Aff<RT, T> GetRequestAff<T>() =>
        from http in Eff
        from _1 in Aff(() => http.Request.ReadFromJsonAsync<T>())
        select _1;

    public static Aff<RT, Unit> ResponseAff<T>(Aff<RT, T> aff, Func<JsonNode, IResult>? resultFactory = null) =>
        from http in Eff
        from _1 in aff.BiBind(
            x => Eff(() =>
            {
                var json = JsonSerializer.SerializeToNode(x, JsonSerializerOptions);
                json["traceId"] = Activity.Current?.Id;
                return resultFactory is null ? Results.Ok(json) : resultFactory(json);
            }),
            e => e.ToResultEff())
        from _2 in Aff(() => _1.ExecuteAsync(http).ToUnit().ToValue())
        select unit;
    internal static JsonSerializerOptions JsonSerializerOptions { get; } = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}

static file class ExceptionExtensions
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
                extensions: new Dictionary<string, object?>
                {
                    ["traceId"] = Activity.Current?.Id
                }),
            _ => Results.StatusCode(500)
        });
}
