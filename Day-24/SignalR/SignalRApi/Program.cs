var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Crucial for SignalR, allows cookies/auth headers
        });

});

builder.Services.AddHostedService<TimeNotificationService>(); 

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
