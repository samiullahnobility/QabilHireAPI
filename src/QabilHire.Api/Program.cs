using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using QabilHire.Api.Authentication;
using QabilHire.Api.ErrorHandling;
using QabilHire.Api.Email;
using QabilHire.Api.Middleware;
using QabilHire.Api.Resumes;
using QabilHire.Api.Storage;
using QabilHire.Api.RateLimiting;
using QabilHire.Api.Logging;
using QabilHire.Infrastructure;
using QabilHire.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddProvider(new DailyFileLoggerProvider(builder.Environment.ContentRootPath, builder.Configuration));

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<RefreshTokenService>();
builder.Services.AddScoped<IPasswordResetEmailSender, SmtpPasswordResetEmailSender>();
builder.Services.AddScoped<IResumeTextExtractor, ResumeTextExtractor>();
builder.Services.AddScoped<ResumeStructuredExtractor>();
builder.Services.AddScoped<ResumeAnalysisService>();
builder.Services.Configure<GroqOptions>(builder.Configuration.GetSection("Groq"));
builder.Services.AddHttpClient<GroqResumeExtractor>();
builder.Services.Configure<SupabaseStorageOptions>(builder.Configuration.GetSection("Supabase"));
builder.Services.AddHttpClient<ISupabaseStorageService, SupabaseStorageService>();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT signing key is not configured.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(RateLimitPolicyNames.Login, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetClientPartitionKey(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = builder.Configuration.GetValue("RateLimiting:Login:PermitLimit", 10),
                Window = TimeSpan.FromMinutes(builder.Configuration.GetValue("RateLimiting:Login:WindowMinutes", 1)),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    options.AddPolicy(RateLimitPolicyNames.Registration, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetClientPartitionKey(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = builder.Configuration.GetValue("RateLimiting:Registration:PermitLimit", 5),
                Window = TimeSpan.FromMinutes(builder.Configuration.GetValue("RateLimiting:Registration:WindowMinutes", 5)),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    options.AddPolicy(RateLimitPolicyNames.Session, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetClientPartitionKey(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = builder.Configuration.GetValue("RateLimiting:Session:PermitLimit", 20),
                Window = TimeSpan.FromMinutes(builder.Configuration.GetValue("RateLimiting:Session:WindowMinutes", 1)),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    options.AddPolicy(RateLimitPolicyNames.PasswordRecovery, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetClientPartitionKey(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = builder.Configuration.GetValue("RateLimiting:PasswordRecovery:PermitLimit", 5),
                Window = TimeSpan.FromMinutes(builder.Configuration.GetValue("RateLimiting:PasswordRecovery:WindowMinutes", 15)),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    options.OnRejected = async (context, cancellationToken) =>
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Too many requests.",
            Detail = "Please wait before trying again.",
            Instance = context.HttpContext.Request.Path
        };
        problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

        context.HttpContext.Response.ContentType = "application/problem+json";
        await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
    };
});
builder.Services.AddCors(options => options.AddPolicy("Angular", policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

var app = builder.Build();

await IdentitySeeder.SeedAsync(
    app.Services,
    seedDemoUsers: app.Environment.IsDevelopment());

app.UseExceptionHandler();
app.UseMiddleware<RequestLoggingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("Angular");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();

static string GetClientPartitionKey(HttpContext context) =>
    context.Connection.RemoteIpAddress?.ToString() ?? "unknown-client";
