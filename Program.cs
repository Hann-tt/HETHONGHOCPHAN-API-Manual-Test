using HETHONGHOCPHAN.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Services
var connectionString = builder.Configuration.GetConnectionString("DULIEUHOCPHAN");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// JWT Auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// 2. Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Front-end static files
app.UseDefaultFiles(new DefaultFilesOptions
{
    DefaultFileNames = new List<string> { "INDEX.HTML", "index.html", "register.html" }
});
app.UseStaticFiles();

// CORS + Auth
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Exception Handler
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (error != null)
        {
            var inner = error.Error.InnerException?.Message;
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                success = false,
                message = error.Error.Message,
                inner = inner
            });
            await context.Response.WriteAsync(result);
        }
    });
});

// 3. Controllers và Routing
app.MapControllers();

// 4. User Secrets (DB + SMTP)
var smtpUser = builder.Configuration["SMTP:User"];
var smtpPassword = builder.Configuration["SMTP:Password"];
var userConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 5. Chạy ứng dụng
app.Run();
