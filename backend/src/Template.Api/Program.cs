using Scalar.AspNetCore;
using Template.Api.ErrorHandling;
using Template.Application;
using Template.Infrastructure;
using Template.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfra(builder.Configuration);

builder.Services.AddControllers(MensagensDeBinding.EmPortugues)
    .ConfigureApiBehaviorOptions(o => o.InvalidModelStateResponseFactory = MensagensDeBinding.RespostaDeErro)
    // JSON malformado: não expõe a mensagem do parser (em inglês); o texto vem do MensagensDeBinding
    .AddJsonOptions(o => o.AllowInputFormatterExceptionMessages = false);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();

var app = builder.Build();

await app.Services.ApplyMigrationsAsync();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();
app.MapHealthChecks("/api/health");

app.Run();
