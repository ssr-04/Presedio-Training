using FreelanceProjectBoardApi.Context;
using FreelanceProjectBoardApi.Interfaces.Repositories;
using FreelanceProjectBoardApi.Repositories;
using FreelanceProjectBoardApi.Services.Implementations;
using FreelanceProjectBoardApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Reflection; 
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using FreelanceProjectBoardApi.ErrorHandling;
using Serilog;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Security.Claims;
using System.Security.Authentication;
using Asp.Versioning;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Asp.Versioning.ApiExplorer;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using FreelanceProjectBoardApi.Hubs;

// Serilog is configured outside the main try-catch to log any errors during startup.
Log.Logger = new LoggerConfiguration()
                .WriteTo.Console() // A fallback logger in case configuration is not read.
                .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // // KESTREL HTTPS CONFIGURATION
    // builder.WebHost.ConfigureKestrel(serverOptions =>
    // {
    //     // Listens on the port specified in launchSettings for HTTPS
    //     serverOptions.ListenAnyIP(7247, listenOptions =>
    //     {
    //         // Use HTTPS
    //         listenOptions.UseHttps(httpsOptions =>
    //         {
    //             // Enforces TLS 1.2 or 1.3
    //             httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
    //         });
    //     });

    //     // Also listens to the HTTP port for redirection to work
    //     serverOptions.ListenAnyIP(5071); 
    // });

    // Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration) // Reads configuration from appsettings.json
        .ReadFrom.Services(services)                   // Allows DI services to be used in sinks/enrichers
        .Enrich.FromLogContext());                     // For contextual logging

    // Add HttpContextAccessor to services
    builder.Services.AddHttpContextAccessor(); // Acccessed in Db context

    // Add Open telemetry configuration for APM - jaeger running in docker
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource
            .AddService(serviceName: builder.Environment.ApplicationName))
        .WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation() // Automatic traces for ASP.NET Core
            .AddHttpClientInstrumentation()   // Automatic traces for HttpClient
            .AddEntityFrameworkCoreInstrumentation(options => // Automatic traces for EF Core
            {
                options.SetDbStatementForText = true; // Records the text of the SQL query
            })
            .AddConsoleExporter() // Exports traces to the console
            .AddOtlpExporter(otlpOptions =>
{
    otlpOptions.Endpoint = new Uri(builder.Configuration.GetValue<string>("Otlp:Endpoint")!);
}));

    // DBContext
    builder.Services.AddDbContext<FreelanceContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    // AutoMapper
    builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

    // Register Repositories
    builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
    builder.Services.AddScoped<IFreelancerProfileRepository, FreelancerProfileRepository>();
    builder.Services.AddScoped<IClientProfileRepository, ClientProfileRepository>();
    builder.Services.AddScoped<IProposalRepository, ProposalRepository>();
    builder.Services.AddScoped<IFileRepository, FileRepository>();
    builder.Services.AddScoped<IRatingRepository, RatingRepository>();
    builder.Services.AddScoped<ISkillRepository, SkillRepository>();
    builder.Services.AddScoped<IFreelancerSkillRepository, FreelancerSkillRepository>();
    builder.Services.AddScoped<IProjectSkillRepository, ProjectSkillRepository>();
    builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

    // Register Services
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IClientProfileService, ClientProfileService>();
    builder.Services.AddScoped<IFreelancerProfileService, FreelancerProfileService>();
    builder.Services.AddScoped<IProjectService, ProjectService>();
    builder.Services.AddScoped<IProposalService, ProposalService>();
    builder.Services.AddScoped<IFileService, FileService>();
    builder.Services.AddScoped<IRatingService, RatingService>();
    builder.Services.AddScoped<ISkillService, SkillService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();


    // Add SignalR services
    builder.Services.AddSignalR();

    // API Versioning
    builder.Services.AddApiVersioning(options =>
    {
        // shows the supported versions in the 'api-supported-versions' response header.
        options.ReportApiVersions = true;
        // Automatically uses the default version when a client doesn't specify one.
        options.AssumeDefaultVersionWhenUnspecified = true;
        // Specifing the default version.
        options.DefaultApiVersion = new ApiVersion(1, 0);
        // Reads the version from the URL segment (/api/v1/...).
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    }).AddApiExplorer(options =>
    {
        // format for the version in the URL.
        // 'v' is a prefix, followed by the major version number.
        // like "v1", "v2", etc.
        options.GroupNameFormat = "'v'VVV";
        // When a version is specified in the URL, we don't need it as a parameter in the action method.
        options.SubstituteApiVersionInUrl = true;
    });

    // CORS
    const string corsPolicyName = "AllowFrontend";

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: corsPolicyName, policy =>
        {
            var origins = builder.Configuration["Cors:AllowedOrigins"]?
                                .Split(',', StringSplitOptions.RemoveEmptyEntries) 
                                ?? Array.Empty<string>();

            policy.WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
            
        });
    });


    //  RATE LIMITING CONFIGURATION 
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        // The partition key will be the User ID or, as a fallback, the IP address.
        options.AddPolicy("user-policy", httpContext =>
        {
            // User ID from the claims.
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var partitionKey = !string.IsNullOrEmpty(userId) 
                ? userId 
                : httpContext.Connection.RemoteIpAddress?.ToString(); // IP fallback
            
            // Read settings from appsettings.json
            var settings = builder.Configuration.GetSection("RateLimiting");
            int permitLimit = settings.GetValue<int>("PermitLimit");
            int windowInHours = settings.GetValue<int>("WindowInHours");

            return RateLimitPartition.GetFixedWindowLimiter(partitionKey,
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = TimeSpan.FromHours(windowInHours),
                    // QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    // QueueLimit = 2 -- Queing doesnt immediately rejects and sends 429 instead queues and stuck loading and not suitable for 1 hour window
                });
        });
    });

    builder.Services.AddControllers();

    // JWT Authentication
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/api/notificationHub"))
                        {
                            // Read the token from the query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),

                RoleClaimType = System.Security.Claims.ClaimTypes.Role
            };
        });

    builder.Services.AddEndpointsApiExplorer();

    // Loads versioning from the ConfigureSwaggerOptions.cs file
    builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme.\r\n\r\nEnter your token without quotes."
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id   = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });


    var app = builder.Build();

    // Should be the first middleware
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("UserId", httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "Anonymous");
            diagnosticContext.Set("UserEmail", httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? string.Empty);
        };
    });

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
            // Builds a swagger endpoint for each discovered API version
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
            }
        });

    }

    // app.UseHttpsRedirection();

    app.UseCors("AllowFrontend");
    
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseRateLimiter();

    app.MapHub<NotificationHub>("/api/notificationHub");

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Startup failed unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// dotnet dev-certs https --trust