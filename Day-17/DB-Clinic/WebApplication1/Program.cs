using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers(); //added controller

builder.Services.AddDbContext<ClinicContext>(opts =>
{
    opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Register repositories
// builder.Services.AddSingleton<IPatientRepository, PatientRepository>(); // Using AddSingleton for in-memory (one is used throughout app)
// builder.Services.AddSingleton<IDoctorRepository, DoctorRepository>();
// builder.Services.AddSingleton<IAppointmentRepository, AppointmentRepository>();

// Register services
/*
why scoped?
- A new instance of the service is created once per HTTP request.
- The same instance is used throughout that request.
- On the next request, a new instance is created.
*/
// builder.Services.AddScoped<IPatientService, PatientService>();
// builder.Services.AddScoped<IDoctorService, DoctorService>();          
// builder.Services.AddScoped<IAppointmentService, AppointmentService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapControllers(); // mapping controllers

app.Run();
