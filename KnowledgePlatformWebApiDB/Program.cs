using KnowledgePlatformWebApiDB.Data.Data;
using KnowledgePlatformWebApiDB.Data.Entities;
using KnowledgePlatformWebApiDB.Auth.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using KnowledgePlatformWebApiDB.Services.Notes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using KnowledgePlatformWebApiDB.Services.Projects;
using KnowledgePlatformWebApiDB.Services.Teams;
using KnowledgePlatformWebApiDB.Services.TeamAccesses;
using KnowledgePlatformWebApiDB.Services.UserAccess;

var builder = WebApplication.CreateBuilder(args);

// 1. DATABASE CONFIGURATION
if (!builder.Environment.IsEnvironment("Testing"))
{
    const string CONNECTION_STRING_NAME = "DefaultConnectionString";
    const string MIGRATION_ASSEMBLY_NAME = "KnowledgePlatformWebApiDB";

    string connectionString = builder.Configuration.GetConnectionString(CONNECTION_STRING_NAME)
        ?? throw new InvalidOperationException($"Connection string {CONNECTION_STRING_NAME} is not specified in the appsettings.json");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseSqlServer(
            connectionString,
            builderOptions => builderOptions.MigrationsAssembly(MIGRATION_ASSEMBLY_NAME)
        );
    });
}

// 2. IDENTITY CONFIGURATION
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// 3. REGISTER AUTH SERVICES
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TokenService>();

// 4. JWT AUTHENTICATION CONFIGURATION
var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSection["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is missing.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// 5. CONTROLLERS & API BEHAVIOR
builder.Services
    .AddControllers(options =>
    {
        options.RespectBrowserAcceptHeader = true;
        options.ReturnHttpNotAcceptable = true;

        var jsonOutputFormatter = options.OutputFormatters
            .OfType<SystemTextJsonOutputFormatter>()
            .FirstOrDefault();

        jsonOutputFormatter?.SupportedMediaTypes.Add("application/json; charset=utf-8");
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.UnmappedMemberHandling =
            System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow;
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var problemDetails = new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Failed",
            Detail = "One or more validation errors have occurred.",
            Instance = context.HttpContext.Request.Path
        };

        return new BadRequestObjectResult(problemDetails);
    };
});

builder.Services.AddAuthorization();

// 6. CORS CONFIGURATION
var angularCorsPolicy = builder.Configuration["MyAppSettings:AngularCORSPolicyName"] ?? "AngularPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(angularCorsPolicy, policy =>
    {
        policy.WithOrigins(
            "http://localhost:4200",
            "http://localhost:53438"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<TeamService>();
builder.Services.AddScoped<TeamAccessService>();
builder.Services.AddScoped<NoteService>();
builder.Services.AddScoped<UserAccessService>();


var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// 6. SEEDING LOGIC (Runs on startup)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        await IdentitySeeder.SeedAsync(context, roleManager, userManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database seeding.");
    }
}

app.UseHttpsRedirection();

app.UseRouting();

// Apply CORS policy
var corsPolicyName = builder.Configuration["MyAppSettings:AngularCORSPolicyName"] ?? "AngularPolicy";
app.UseCors(corsPolicyName);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();