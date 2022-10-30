using FluentValidation;
using ThinkFunc.Effect.Http.StubApi;
ValidatorOptions.Global.LanguageManager.Enabled = false;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(config =>
{
    config.ModelBinderProviders.Clear();
    config.ModelValidatorProviders.Clear();
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

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
