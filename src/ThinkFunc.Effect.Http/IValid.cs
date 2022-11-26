using LanguageExt;
using LanguageExt.Pipes;
using ThinkFunc.Effect.Abstractions;
using static LanguageExt.Prelude;
using FluentValidation;

namespace ThinkFunc.Effect.Http;

public interface IValid<RT, T> : IHas<RT, IValidator<T>> where RT : struct, IValid<RT, T>
{
    public static Aff<RT, T> ValidateAff(T dto) =>
        from valid in IHas<RT, IValidator<T>>.Eff
        from _1 in Aff(() => valid.ValidateAndThrowAsync(dto).ToUnit().ToValue())
        select dto;
}
