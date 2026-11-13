using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;
using Microsoft.IdentityModel.Tokens;
using SmartPalm.IngestionService.Api;
using SmartPalm.IngestionService.Application.Exceptions;
using SmartPalm.IngestionService.Infrastructure;
using SmartPalm.IngestionService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers(); builder.Services.AddEndpointsApiExplorer(); builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization", Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT",
        In = ParameterLocation.Header, Description = "Pega solo el token JWT; Swagger agregará Bearer automáticamente."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});
builder.Services.AddIngestionInfrastructure(builder.Configuration);
var jwtKey = builder.Configuration["Jwt:Key"] ?? string.Empty;
if (Encoding.UTF8.GetByteCount(jwtKey) < 32) throw new InvalidOperationException("Jwt:Key must be configured with at least 32 bytes.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.MapInboundClaims = false; options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidIssuer = builder.Configuration["Jwt:Issuer"], ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"], ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)), ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero, NameClaimType = "name", RoleClaimType = "role"
    };
});
builder.Services.AddAuthorization(); builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddHealthChecks().AddCheck<IngestionDatabaseHealthCheck>("ingestion-database");
var app = builder.Build();
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var ex = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    context.Response.StatusCode = ex switch
    {
        UnauthorizedAccessException => 401,
        ForbiddenAccessException => 403,
        KeyNotFoundException => 404,
        ArgumentException => 400,
        InvalidOperationException => 409,
        _ => 500
    };
    await context.Response.WriteAsJsonAsync(new { message = context.Response.StatusCode == 500 ? "An unexpected error occurred." : ex?.Message });
}));
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseCors(); app.UseAuthentication(); app.UseAuthorization(); app.MapControllers();
app.MapHealthChecks("/health/live", new() { Predicate = _ => false }); app.MapHealthChecks("/health/ready");
await IngestionDatabaseInitializer.InitializeAsync(app.Services); await app.RunAsync();
public partial class Program;
