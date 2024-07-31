using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Configuracion de logs
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();


//Añadir servicios al contenedor.
builder.Configuration.AddJsonFile("appsettings.json");
var secretkey = builder.Configuration.GetSection("settings").GetSection("secretkey").ToString();
var keyBytes = Encoding.UTF8.GetBytes(secretkey);

builder.Services.AddAuthentication(config =>
{
    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(config =>
{
    config.RequireHttpsMetadata = false;
    config.SaveToken = true;
    config.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});
builder.Services.AddControllers();
builder.Services.AddHttpClient();

// Se agregan politicas de CORS para sitios seguros
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

// Configurar el registro
builder.Logging.ClearProviders();
// Agregar registro de consola
builder.Logging.AddConsole();
// Agregar registro de depuración
builder.Logging.AddDebug();

// Agregar servicios Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(doc =>
{
    doc.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Api Pokemon",
        Description = "Developed By: " +
        "Daniel Rodriguez Gil - 2024",
        Contact = new OpenApiContact
        {
            Name = "The Developer",
            Email = "darogi.08@gmail.com",
        },
        License = new OpenApiLicense
        {
            Name = "Contact The Company",
            Url = new Uri("https://github.com/darogi10/Api_Pokemon.git"),
        }
    });

    doc.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        In = ParameterLocation.Header,
        Description = "JWT Bearer Authorization",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    doc.AddSecurityRequirement(new OpenApiSecurityRequirement {
{
    new OpenApiSecurityScheme
    {
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }},
    Array.Empty<string>()
}});
});

var app = builder.Build();

// Configurar la canalización de solicitudes HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    // Habilite el middleware para servir Swagger generado como un punto final JSON.
    app.UseSwagger();

    // Habilitar middleware para servir swagger-ui (HTML, JS, CSS, etc.),
    // especificando el punto final JSON de Swagger.
    app.UseSwaggerUI(c =>
    {
        //c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        //c.RoutePrefix = string.Empty; // Set Swagger UI at the root
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("CorsPolicy");
app.UseMiddleware<ExceptionMiddleware>();
app.MapControllers();
app.Run();