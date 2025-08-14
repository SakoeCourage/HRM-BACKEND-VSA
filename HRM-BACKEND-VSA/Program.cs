using System.Reflection;
using AutoMapper;
using Carter;
using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Domains.Staffs.Services;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Providers;
using HRM_BACKEND_VSA.Serivices.ImageKit;
using HRM_BACKEND_VSA.Serivices.Mail_Service;
using HRM_BACKEND_VSA.Serivices.Notification_Service;
using HRM_BACKEND_VSA.Services.SMS_Service;
using HRM_BACKEND_VSA.Utilities;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using HRM_BACKEND_VSA.Middlewares;
using Microsoft.AspNetCore.RateLimiting;


var builder = WebApplication.CreateBuilder(args);
var assembly = typeof(Program).Assembly;
var AppKey = builder.Configuration.GetValue<string>("SiteSettings:AppKey");

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<HRMDBContext>(option =>
{
    option.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));

});

builder.Services.AddDbContext<HRMStaffDBContext>(option =>
{
    option.UseNpgsql(builder.Configuration.GetConnectionString("StaffApplicantConnection"));
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationDecisionType.Applicant.ToString(), policy =>
    {
        policy.RequireClaim(ClaimTypes.AuthorizationDecision, AuthorizationDecisionType.Applicant.ToString());
    });

    options.AddPolicy(AuthorizationDecisionType.Staff.ToString(), policy =>
    {
        policy.RequireClaim(ClaimTypes.AuthorizationDecision, AuthorizationDecisionType.Staff.ToString());
    });

    options.AddPolicy(AuthorizationDecisionType.HRMUser.ToString(), policy =>
    {
        policy.RequireClaim(ClaimTypes.AuthorizationDecision, AuthorizationDecisionType.HRMUser.ToString());
    });
});

builder.Services.AddScoped<ImageKit>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    return new ImageKit(configuration);
});

builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 4;
        options.Window = TimeSpan.FromSeconds(12);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    }));

builder.Services.AddTransient<SMSService>();
builder.Services.AddTransient<MailService>();
builder.Services.AddAntiforgery();

builder.Services.AddScoped<Authprovider>(services =>
{
    var scope = services.GetRequiredService<IServiceScopeFactory>();
    return new Authprovider(scope);
});

builder.Services.AddScoped<RequestService>(services =>
{
    var scope = services.GetRequiredService<IServiceScopeFactory>();
    var createdScope = scope.CreateScope();
    var dbContext = createdScope.ServiceProvider.GetRequiredService<HRMDBContext>();
    var staffDBContext = createdScope.ServiceProvider.GetRequiredService<HRMStaffDBContext>();
    var mapper = createdScope.ServiceProvider.GetRequiredService<IMapper>();
    var authProvider = createdScope.ServiceProvider.GetRequiredService<Authprovider>();

    return new RequestService(dbContext, staffDBContext, authProvider, mapper);
});

builder.Services.AddScoped(typeof(Notify<>));

JWTStartupConfig.ConfigureJWt(builder.Services, builder.Configuration);
builder.Services.AddSingleton<JWTProvider>(new JWTProvider(AppKey));
Paginator.SetHttpContextAccessor(builder.Services.BuildServiceProvider().GetService<IHttpContextAccessor>());

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

Paginator.SetHttpContextAccessor(builder.Services.BuildServiceProvider().GetService<IHttpContextAccessor>());

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddSwaggerGen(option => SwaggerDoc.OpenAuthentication(option));

builder.Services.AddMediatR(config => config.RegisterServicesFromAssemblies(assembly));

builder.Services.AddCarter();

builder.Services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var MyAllowSpecificOrigins = "_allowedOrgins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5181", "http://localhost:5180")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                      });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
}

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.DocumentTitle = "HRM-API Documentation";
    var endpointDefinitions = typeof(SwaggerDoc.SwaggerEndpointDefintions).GetFields(BindingFlags.Public | BindingFlags.Static);
    foreach (var definitions in endpointDefinitions)
    {
        c.SwaggerEndpoint($"/swagger/{definitions.GetValue(null)?.ToString()}/swagger.json",definitions.GetValue(null)?.ToString());
    }
});
app.UseAntiforgery();
app.UseCors(MyAllowSpecificOrigins);
app.UseMiddleware<JsonExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.MapCarter();
app.UseHangfireDashboard("/hangfire");

app.Run();


