using LanguageExt.Effects.Traits;
using Microsoft.AspNetCore.Http;
using ThinkFunc.Effect.Abstractions;

namespace ThinkFunc.Effect.Http;

public interface IHttp<RT> : IHas<RT, HttpContext>
    where RT : struct, IHasCancel<RT>, IHttp<RT>
{
    
}
