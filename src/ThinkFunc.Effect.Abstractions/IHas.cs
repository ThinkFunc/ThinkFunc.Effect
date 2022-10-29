using LanguageExt;
using LanguageExt.Effects.Traits;
using static LanguageExt.Prelude;

namespace ThinkFunc.Effect.Abstractions;

public interface IHas<RT, T> : HasCancel<RT>
    where RT : struct, IHas<RT, T>
{
    protected T It { get; }

    public Eff<RT, T> Eff => Eff<RT, T>(rt => rt.It);
}
