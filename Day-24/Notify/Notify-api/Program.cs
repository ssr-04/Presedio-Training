using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NotifyService.Data;
using NotifyService.Hubs;
using NotifyService.Repositories;
using NotifyService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://{builder.Configuration["Auth0:Domain"]}/"; // Auth0 Domain with trailing slash
        options.Audience = builder.Configuration["Auth0:Audience"]; // API Identifier (Audience)
        
        // Optional: Customize token validation parameters if needed
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = true,
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
                System.Console.WriteLine("hit");
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

                // Adding existing claims from Auth0 token (e.g., Auth0 sub, email, name, etc.)
                foreach (var claim in context.Principal!.Claims)
                {
                    claims.Add(claim);
                }
                
                claims.Add(new Claim(ClaimTypes.Role, localUser.Role));
                claims.Add(new Claim("Auth0UserId", localUser.Auth0UserId));

                var appIdentity = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
                context.Principal = new ClaimsPrincipal(appIdentity);
                System.Console.WriteLine("-------Auth success ------------");
                await Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            }
        };
    });

// --- Authorization Policies ---
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireHRAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireLoggedInUser", policy => policy.RequireAuthenticatedUser());
});

builder.Services.AddDbContext<FileManagement>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IAuth0ManagementService, Auth0ManagementService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDocumentService, DocumentService>(); 

builder.Services.AddAutoMapper(typeof(Program).Assembly); 

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.SetIsOriginAllowed(origin => true)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Crucial for SignalR, allows cookies/auth headers
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- CORS Middleware MUST be before UseRouting ---
app.UseCors("AllowSpecificOrigin"); 
app.UseRouting(); 

app.UseAuthorization();

app.MapControllers();

// --- SignalR Hub Endpoint ---
app.MapHub<NotificationHub>("/notificationhub"); // Maps hub to /notificationhub URL

app.Run();
