using LibraryManagementApi.Data;
using LibraryManagementApi.Models;
using LibraryManagementApi.Repository;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using Hangfire;
using LibraryManagementApi.Services;
using LibraryManagementApi.Interfaces;
using log4net.Config;
using log4net;
using LibraryManagementApi.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext
builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LibraryManage")));

builder.Services.AddHangfire((sp, config) =>
{
    var connectionstring = sp.GetRequiredService<IConfiguration>().GetConnectionString("LibraryManage");
    config.UseSqlServerStorage(connectionstring);
});
builder.Services.AddHangfireServer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
        }
        );
});

// Add repositories and services
builder.Services.AddScoped<IBookRepository<Book>, BookRepository>();
builder.Services.AddScoped<IUserRepository<User>, UserRepository>();
builder.Services.AddScoped<IBorrowedBookRepository<BorrowedBook>, BorrowedBookRepository>();
builder.Services.AddScoped<IReservationRepository<Reservation>, ReservationRepository>();
builder.Services.AddScoped<IGenreRepository<Genre>, GenreRepository>();
builder.Services.AddScoped<IFineRepository<Fine>, FineRepository>();
builder.Services.AddScoped<IReviewRepository<Review>, ReviewRepository>();
builder.Services.AddScoped<IEventRepository<Event>, EventRepository>();

//builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddHostedService<EmailNotificationService>();
builder.Services.AddHostedService<ReservationEmailNotificationService>();
builder.Services.AddScoped<BorrowedBookEmailService>();
builder.Services.AddScoped<FineService>();
builder.Services.AddScoped<BackgroundJobService>();
builder.Services.AddScoped<PdfService>();

//builder.Services.AddScoped<BorrowBookService>();

builder.Services.AddScoped<IServiceScopeFactory>(provider => provider.GetRequiredService<IServiceScopeFactory>());
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new JsonDateOnlyConverter());
    });

builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAPI", Version = "v1" });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true, // Set to true to validate the token's expiration time
        ValidateIssuerSigningKey = true
    };
});

// Configure Log4Net
var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly());
XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

// Build the app
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHangfireDashboard("/hangfire");
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowReactApp");
app.UseAuthorization();
app.MapControllers();

// Schedule the recurring job for fine calculation
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var fineService = services.GetRequiredService<FineService>();
    var backgroundJobService = services.GetRequiredService<BackgroundJobService>();


    RecurringJob.AddOrUpdate(
        "generate-fines",
        () => fineService.GenerateFinesForOverdueBooksAsync(),
        Cron.Daily);
    RecurringJob.AddOrUpdate(
       "check-overdue-prebookings",
       () => backgroundJobService.CheckAndCancelOverduePreBookings(),
       Cron.Daily);
}

app.Run();
