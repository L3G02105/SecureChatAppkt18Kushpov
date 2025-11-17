using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SecureChatApp.Hubs;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Добавление контроллеров и SignalR
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Добавление CORS с конкретным источником
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:7078") // ✅ замените на нужный адрес, если используете другой порт
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Ключ для JWT
var key = Encoding.ASCII.GetBytes("your_super_secret_key_here");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };

        // JWT из query строки для SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Добавление Swagger (если нужно)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Строим приложение
var app = builder.Build();

// Swagger в режиме разработки
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Использование CORS, статики, маршрутизации и авторизации
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors(); // Включаем CORS

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

app.Run();
