using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ThinkFunc.Effect.Abstractions;
using ThinkFunc.Effect.Http;
using ThinkFunc.Effect.Http.StubApi;
using ThinkFunc.Effect.Http.StubApi.Controllers;
using LanguageExt;

ValidatorOptions.Global.LanguageManager.Enabled = false;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
    options.SuppressMapClientErrors = true;
});

builder.Services.AddSingleton<IValidator<RequestDto>, RequestDtoValidator>();
builder.Services.AddControllers(options =>
{
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.OperationFilter<RequestBodyTypeFilter>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/echo2", static async (HttpContext context, IValidator<RequestDto> validator, CancellationToken ct) =>
{
    var q = IHttp<RT>.ResponseAff(
        from dto in IHttp<RT>.GetRequestAff<RequestDto>()
        from _1 in IValid<RT, RequestDto>.ValidateAff(dto)
        select new
        {
            dto.Hello,
        });

    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    await q.Run(new(context, validator, cts));
}).Accepts<RequestDto>("application/json");


app.MapControllers();
app.Run();

public partial class Program { }

public readonly record struct RT
   (
       HttpContext HttpContext,
       IValidator<RequestDto> Validator,
       CancellationTokenSource CancellationTokenSource
   ) : IHasCancel<RT>,
       IHttp<RT>,
       IValid<RT, RequestDto>
{
    HttpContext IHas<RT, HttpContext>.It => HttpContext;
    IValidator<RequestDto> IHas<RT, IValidator<RequestDto>>.It => Validator;
}
