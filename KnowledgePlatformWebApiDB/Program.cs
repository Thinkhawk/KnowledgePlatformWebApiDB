using KnowledgePlatformWebApiDB.Data.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsEnvironment("Testing"))
{
    const string CONNECTION_STRING_NAME = "DeafultConnectionString";
    const string MIGRATION_ASSEMBLY_NAME = "KnowledgePlatformWebApiDB";
    string connectionString = builder.Configuration.GetConnectionString(CONNECTION_STRING_NAME)
        ?? throw new InvalidOperationException($"Connection string {CONNECTION_STRING_NAME} is not specified in the appseetings.json");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseSqlServer(
            connectionString,
            builderOptions => builderOptions.MigrationsAssembly(MIGRATION_ASSEMBLY_NAME)
            );
    });
}

builder.Services
    .AddControllers(options => { 
        options.RespectBrowserAcceptHeader = true;
        options.ReturnHttpNotAcceptable = true;
        var jsonOutputFormatter = options.OutputFormatters.OfType<SystemTextJsonOutputFormatter>().FirstOrDefault();
        jsonOutputFormatter?.SupportedMediaTypes.Add("application/json; charset=utf-8");
    })
    .AddJsonOptions(options => { 
        options.JsonSerializerOptions.UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow;
    });

builder.Services.Configure<ApiBehaviorOptions>(options => {
    options.InvalidModelStateResponseFactory = context => {
        var problemDetails = new ValidationProblemDetails(context.ModelState) {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Failed",
            Detail = "One or more valiation errors have occured.",
            Instance = context.HttpContext.Request.Path
        };

        return new BadRequestObjectResult(problemDetails);
    };
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();




/***********************************************************************************/
/************************************ BUILDING *************************************/
/***********************************************************************************/




var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
