using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Logging;
// Add at the top of Program.cs (before building the app)
Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
IdentityModelEventSource.LogCompleteSecurityArtifact = true;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://{builder.Configuration["Auth0:Domain"]}/"; // Auth0 Domain with trailing slash
        options.Audience = builder.Configuration["Auth0:Audience"]; // API Identifier (Audience)
        
        // Optional: Customize token validation parameters if needed
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Ensure the token has an audience claim
            ValidateAudience = true,
            // Ensure the token was issued by the correct authority
            ValidateIssuer = true,
            // Ensure the token is not expired
            ValidateLifetime = true,
            // Ensure the signing key is valid (handled by Auth0 SDK)
            ValidateIssuerSigningKey = true,
        };
        
        // Event for handling token validation failures or custom claims processing
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                // This event is called AFTER a token is successfully validated by Auth0.
                // Now, you can enrich the ClaimsPrincipal with data from your local database
                // or perform additional checks.

                var auth0UserId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value; // This is the 'sub' claim from Auth0, usually "auth0|userId"
                var email = context.Principal?.FindFirst(ClaimTypes.Email)?.Value; // Auth0 often provides email in the 'email' claim
                if (string.IsNullOrEmpty(auth0UserId))
                { 
                    context.Fail("Missing Auth0 user ID in token claims.");
                    return;
                }
                var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                var localUser = await userRepository.GetUserByAuth0UserIdAsync(auth0UserId);
                
                if (localUser == null)
                {
                    // This indicates a user logged in via Auth0 but doesn't exist in our local database.
                    context.Fail($"User with Auth0 ID '{auth0UserId}' not found in local database.");
                    return;
                }

                // Check if the local user is active
                if (!localUser.IsActive)
                {
                    context.Fail($"Local user account for '{email}' is inactive.");
                    return;
                }

                // Building new claims for the current user, incorporating local data
                var claims = new List<Claim>();

                // Add existing claims from Auth0 token (e.g., Auth0 sub, email, name, etc.)
                foreach (var claim in context.Principal.Claims)
                {
                    // Filter out claims you might replace or don't need from Auth0,
                    // or just add all of them.
                    // Example: If Auth0 sends a 'role' claim you don't trust, exclude it here.
                    claims.Add(claim);
                }

                // Add claims from your local user record
                claims.Add(new Claim(ClaimTypes.Role, localUser.Role));
                if (localUser.PatientId.HasValue)
                {
                    claims.Add(new Claim("patient_id", localUser.PatientId.Value.ToString()));
                }
                if (localUser.DoctorId.HasValue)
                {
                    claims.Add(new Claim("doctor_id", localUser.DoctorId.Value.ToString()));
                }

                // Replace the existing Principal with a new one that contains all desired claims
                var appIdentity = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
                context.Principal = new ClaimsPrincipal(appIdentity);
                System.Console.WriteLine("-------Auth success ------------");
                await Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                // Log authentication failures for debugging
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            }
        };
    });


// Register custom Authorization Handlers
builder.Services.AddScoped<IAuthorizationHandler, ResourceOwnerAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, MinimumDoctorExperienceHandler>();

// Configure Authorization Policies
builder.Services.AddAuthorization(options =>
{
    // Policy for checking if a Doctor has enough experience for cancellation
    options.AddPolicy("DoctorHasMinimumExperienceToCancel", policy =>
    {
        policy.Requirements.Add(new MinimumDoctorExperienceRequirement(5.0f)); // 5 years experience
    });

});

// Custom logging
// builder.Logging.AddLog4Net();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
// Configure Swagger to integrate with Auth0 for testing
builder.Services.AddSwaggerGen(options =>
  {
      options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
      {
          Title = "API Documentation",
          Version = "v1.0",
          Description = ""
      });
      options.ResolveConflictingActions(x => x.First());
      options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
      {
          Type = SecuritySchemeType.OAuth2,
          BearerFormat = "JWT",
          Flows = new OpenApiOAuthFlows
          {
              Implicit  = new OpenApiOAuthFlow
              {
                  TokenUrl = new Uri($"https://{builder.Configuration["Auth0:Domain"]}/oauth/token"),
                  AuthorizationUrl = new Uri($"https://{builder.Configuration["Auth0:Domain"]}/authorize?audience={builder.Configuration["Auth0:Audience"]}"),
                  Scopes = new Dictionary<string, string>
                  {
                      { "openid", "OpenId" },
                  
                  }
              }
          }
      });
      options.AddSecurityRequirement(new OpenApiSecurityRequirement
      {
          {
              new OpenApiSecurityScheme
              {
                  Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
              },
              new[] { "openid" }
          }
      });

  });


builder.Services.AddControllers(); //added controller

builder.Services.AddDbContext<ClinicContext>(opts =>
{
    opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddAutoMapper(typeof(MappingProfiles));

// Register repositories
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<ISpecialityRepository, SpecialityRepository>();
builder.Services.AddScoped<IDoctorSpecialityRepository, DoctorSpecialityRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IOtherContextFunctionalities, OtherContextFunctionalities>();
builder.Services.AddScoped<ExceptionFilterAttribute, CustomExceptionFilter>();

builder.Services.AddSingleton<IAuth0ManagementService, Auth0ManagementService>();

builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<ISpecialityService, SpecialityService>();
builder.Services.AddScoped<IDoctorSpecialityService, DoctorSpecialityService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
     app.UseSwagger();
  app.UseSwaggerUI(settings =>
  {
      settings.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1.0");
      settings.OAuthClientId(builder.Configuration["Auth0:ClientId"]);
      settings.OAuthClientSecret(builder.Configuration["Auth0:ClientSecret"]);
      settings.OAuthUsePkce();
  });
}

app.UseHttpsRedirection();

// Use authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers(); // mapping controllers

app.Run();

// Auth0 Identifier - https://my-clinic-api.com

