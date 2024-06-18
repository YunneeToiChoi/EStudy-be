using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using MediatR;
using study4_be.Interface;
using study4_be.Models;
using study4_be.Payment;
using study4_be.Payment.MomoPayment;
using study4_be.PaymentServices.Momo.Config;
using study4_be.Services;
//using study4_be.Services.EmailServices;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
//Đăng ký các dịch vụ
builder.Services.AddDbContext<STUDY4Context>(); // Đăng ký STUDY4Context vào DI container
builder.Services.AddScoped<UserCourseExpirationService>(); // Đăng ký dịch vụ UserCourseExpirationService với phạm vi Scoped
                                                           // Momo config payment
builder.Services.AddTransient<ICurrentUserServices, CurrentUserServices>(); // Đăng ký ICurrentUserServices
builder.Configuration.AddJsonFile("appsettings.json", optional: false);
builder.Services.AddTransient<IConnectionService, ConnectionService>();
builder.Services.AddTransient<ISqlService, SqlService>();
builder.Services.AddMediatR(typeof(Program).Assembly);
builder.Services.Configure<MomoConfig>(
    builder.Configuration.GetSection(MomoConfig.ConfigName));
//Add services to the container.
//builder.Services.Configure<EmailConfig>(builder.Configuration.GetSection(EmailConfig.ConfigName));
//builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddControllersWithViews();
// Thêm dịch vụ vào container
builder.Services.AddSingleton<FireBaseServices>();

// Register HttpContextAccessor if needed
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseCors(policy =>
{
    policy.AllowAnyOrigin();
    policy.AllowAnyHeader();
    policy.AllowAnyMethod();
});

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

// Middleware để đọc dữ liệu từ yêu cầu và chuyển đổi thành đối tượng C#
app.Use(async (context, next) =>
{
    // Đảm bảo rằng yêu cầu có thể đọc lại nếu cần thiết
    context.Request.EnableBuffering();

    using (var reader = new StreamReader(context.Request.Body))
    {
        var body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0; // Đặt lại vị trí của luồng để đọc lại nếu cần thiết
                                           // Lưu trữ dữ liệu trong context để các action sau có thể truy cập
        context.Items["RequestBody"] = body;
    }

    // Chuyển quyền kiểm soát sang middleware tiếp theo trong pipeline
    await next();
});

app.Run();
