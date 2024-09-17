﻿using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using MediatR;
using study4_be.Interface;
using study4_be.Models;
using study4_be.Payment;
using study4_be.Payment.MomoPayment;
using study4_be.PaymentServices.Momo.Config;
using study4_be.Services;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Thêm dịch vụ MVC và các view
builder.Services.AddControllersWithViews();

// Thêm dịch vụ Controllers với tùy chọn JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Đăng ký DbContext
builder.Services.AddDbContext<Study4Context>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Đăng ký các dịch vụ
builder.Services.AddScoped<UserCourseExpirationService>();
builder.Services.AddTransient<ICurrentUserServices, CurrentUserServices>();
builder.Services.AddTransient<IConnectionService, ConnectionService>();
builder.Services.AddTransient<ISqlService, SqlService>();
builder.Services.Configure<MomoConfig>(builder.Configuration.GetSection(MomoConfig.ConfigName));

// Dịch vụ Firebase và SMTP
builder.Services.AddSingleton<FireBaseServices>();
builder.Services.AddSingleton<SMTPServices>();

// Thêm IHttpContextAccessor
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Cấu hình logging
builder.Logging.ClearProviders(); // Xóa các provider logging mặc định
builder.Logging.AddConsole(); // Thêm provider logging Console
builder.Logging.AddDebug(); // Thêm provider logging Debug
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.SaveTokens = true; // Ensure tokens are saved in the authentication properties
    options.Scope.Add("openid"); // Add openid scope
    options.Scope.Add("profile");
    options.Scope.Add("email");
})
.AddFacebook(options =>
{
    options.AppId = builder.Configuration["Authentication:Facebook:AppId"];
    options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    var secretKey = jwtSettings["SecretKey"];
    var key = Encoding.ASCII.GetBytes(secretKey);

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"]
    };
}); 
builder.Services.AddControllers();
// Đăng ký JwtTokenGenerator
builder.Services.AddSingleton<JwtTokenGenerator>();

// Thêm HttpClient
builder.Services.AddHttpClient();

// Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Cấu hình HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll");

// Thêm middleware xác thực và phân quyền
app.UseAuthentication();
app.UseAuthorization();

// Đăng ký các route
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

// Middleware để đọc body của request và lưu vào context
app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();

    using (var reader = new StreamReader(context.Request.Body))
    {
        var body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;
        context.Items["RequestBody"] = body;
    }

    await next();
});

app.Run();
