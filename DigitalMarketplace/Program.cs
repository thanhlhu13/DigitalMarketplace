using DigitalMarketplace.API.Middleware;
using DigitalMarketplace.Application.Interfaces;
using DigitalMarketplace.Infrastructure.Data;
using DigitalMarketplace.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);
var jwtSettings = builder.Configuration.GetSection("Jwt");

var jwtKey = jwtSettings["Key"]
    ?? throw new InvalidOperationException("JWT Key is not configured.");

var jwtIssuer = jwtSettings["Issuer"]
    ?? throw new InvalidOperationException("JWT Issuer is not configured.");

var jwtAudience = jwtSettings["Audience"]
    ?? throw new InvalidOperationException("JWT Audience is not configured.");

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Validate the signature to ensure the token was created
            // by our application and has not been modified.
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),

            // Validate who issued the token.
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            // Validate which application the token is intended for.
            ValidateAudience = true,
            ValidAudience = jwtAudience,

            // Reject expired access tokens.
            ValidateLifetime = true,

            // Small clock tolerance between servers.
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();