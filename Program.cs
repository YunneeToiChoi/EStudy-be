using FirebaseAdmin;
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
using Hangfire;
using Hangfire.SqlServer;
using study4_be.Interface.Rating;
using study4_be.Services.Rating;
using Google.Api;
using study4_be.Services.User;
using study4_be.Interface.User;
using study4_be.Repositories;
using study4_be.Validation;
using PusherServer;
using study4_be.PaymentServices.Momo.Request;
using Azure.Storage.Blobs;
using study4_be.Services.Backup;
using study4_be.Services.Category;
using study4_be.Services.Exam;
using study4_be.Services.Tingee;

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
    var connectString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectString);
});
builder.Services.AddSingleton(x =>
    new BlobServiceClient(builder.Configuration.GetValue<string>("AzureBlobStorage:ConnectionStringAzure")));
// Đăng ký các dịch vụ
builder.Services.AddScoped<UserCourseExpirationService>();
builder.Services.AddTransient<ICurrentUserServices, CurrentUserServices>();
builder.Services.AddTransient<UserRegistrationValidator>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IContainerService, ContainerService>();
builder.Services.AddTransient<UserRepository>();
builder.Services.AddTransient<ISqlService, SqlService>();
builder.Services.AddTransient<IRatingService, RatingService>();
builder.Services.AddTransient<IReplyService, ReplyService>();
builder.Services.AddTransient<IDocumentService, DocumentService>();
builder.Services.AddTransient<ICategoryService, CategoryService>();
builder.Services.AddTransient<ICourseService, CourseService>();
builder.Services.AddTransient<IWritingService, WritingService>();
// Register BlobServiceClient for Azure Blob Storage
builder.Services.Configure<MomoConfig>(builder.Configuration.GetSection(MomoConfig.ConfigName));
builder.Services.Configure<MomoTestConfig>(builder.Configuration.GetSection(MomoTestConfig.ConfigName));
builder.Services.AddScoped<ContractPOServices>();
builder.Services.AddSingleton<TingeeApi>(); // Đăng ký TingeeApi là dịch vụ
// Dịch vụ Firebase và SMTP
builder.Services.AddSingleton<FireBaseServices>();
builder.Services.AddSingleton<SMTPServices>();
builder.Services.AddSingleton<AzureOpenAiService>();

// Configure Pusher settings
builder.Services.Configure<study4_be.Services.PusherOptions>(builder.Configuration.GetSection("Pusher"));

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

builder.Services.AddHangfire(config => {
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddHangfireServer();

builder.Services.AddTransient<study4_be.Services.DateTimeService>(); // or AddTransient if the service needs a shorter lifespan

builder.Services.AddScoped<BackupService>();

// Register BackupSchedulerService as a hosted service
builder.Services.AddHostedService<BackupSchedulerService>();
var app = builder.Build();
// Cấu hình sử dụng Hangfire dashboard
app.UseHangfireDashboard();
// Khởi động server cho Hangfire
app.UseHangfireServer();

// Thiết lập RecurringJob để chạy job xóa đơn hàng hết hạn sau 15 phút mỗi 5 phút
RecurringJob.AddOrUpdate<DateTimeService>(
    "CheckAndDeleteExpiredOrders",
    service => service.CheckAndDeleteExpiredOrders(),
    Cron.MinuteInterval(5)
);

//Thiết lập RecurringJob để chạy job set các user's course và user's plan có state = false
RecurringJob.AddOrUpdate<DateTimeService>(
    "CheckAndExpireSubscriptions",
    service => service.CheckAndExpireSubscriptions(),
    Cron.Hourly
);

RecurringJob.AddOrUpdate<DateTimeService>(
    "CheckAndExpireUserCourse",
    service => service.CheckAndExpireUserCourse(),
    Cron.Hourly
    );
// Cấu hình HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll"); //remember fix this problem

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

app.MapControllers();

await app.RunAsync();
