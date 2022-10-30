using FluentValidation;
using PhoneNumbers;

namespace ThinkFunc.Effect.Http.StubApi;

public static class MyCustomValidators
{
    public static IRuleBuilderOptionsConditions<T, (string, string)> Phone<T>(this IRuleBuilder<T, (string Phone, string Region)> ruleBuilder) =>
        ruleBuilder.Custom((v, context) =>
        {
            try
            {
                var util = PhoneNumberUtil.GetInstance();
                var x = util.Parse(v.Phone, v.Region);
                if (!util.IsValidNumberForRegion(x, v.Region))
                {
                    context.AddFailure($"Wrong number ({x.CountryCode}{x.NationalNumber})");
                }
            }
            catch (Exception ex)
            {
                context.AddFailure(ex.Message);
            }
        });


    public static IRuleBuilderOptionsConditions<T, string> Phone<T>(this IRuleBuilder<T, string> ruleBuilder, string region) =>
        ruleBuilder.Custom((phone, context) =>
        {
            try
            {
                var util = PhoneNumberUtil.GetInstance();
                var x = util.Parse(phone, region);
                if (!util.IsValidNumberForRegion(x, region))
                {
                    context.AddFailure($"Wrong number ({x.CountryCode}{x.NationalNumber})");
                }
            }
            catch (Exception ex)
            {
                context.AddFailure(ex.Message);
            }
        });
}
