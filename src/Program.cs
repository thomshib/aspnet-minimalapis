using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

//Cors

builder.Services.AddCors(options => {
    options.AddPolicy("AnyOrigin", x =>  x.AllowAnyOrigin());
});


//authentication

// pass "Authorization = VerySecret" in the header
builder.Services.AddAuthentication(ApiKeySchemeConstants.SchemeName)
    .AddScheme<ApiKeyAuthSchemeOptions,ApiKeyAuthHandler>(ApiKeySchemeConstants.SchemeName, _ => {});

builder.Services.AddAuthorization();
// Swagger

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddSingleton<IDbConnectionFactory>(
    _ => new SqliteConnectionFactory(builder.Configuration.GetValue<string>("Database:ConnectionString"))
);

builder.Services.AddSingleton<DatabaseInitializer>();


builder.Services.AddServices<Program>(builder.Configuration);

//Fluent validation registration
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

//Cors

app.UseCors();

// Swagger

app.UseSwagger();
app.UseSwaggerUI();

app.UseEndpoints<Program>();

//auth
app.UseAuthorization();




// Db init

var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

app.Run();
