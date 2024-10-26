using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using study4_be.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OfficeOpenXml; // Đừng quên thêm using này

namespace study4_be.Services.Backup
{
    public class BackupService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly Study4Context _context;     
        private readonly string _containerName;

        public BackupService(BlobServiceClient blobServiceClient, Study4Context dbContext, IConfiguration configuration)
        {
            _blobServiceClient = blobServiceClient;
            _context = dbContext;
            _containerName = configuration["AzureBlobStorage:ContainerName"];
        }

     public async Task BackupDataAsync()
{
    try
    {
        Console.WriteLine("Bắt đầu sao lưu dữ liệu...");

        // Thiết lập ngữ cảnh giấy phép cho EPPlus
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        // Lấy dữ liệu từ các bảng
        var users = await _context.Users.Include(u => u.Orders).ToListAsync();
        var orders = await _context.Orders.ToListAsync();
        var categories = await _context.Categories.ToListAsync();
        var containers = await _context.Containers.ToListAsync();
        var counters = await _context.Counters.ToListAsync();
        var courses = await _context.Courses.Include(c => c.Documents).ToListAsync();
        var departments = await _context.Departments.ToListAsync();
        var documents = await _context.Documents.ToListAsync();
        var exams = await _context.Exams.ToListAsync();
        var lessons = await _context.Lessons.ToListAsync();
        var plans = await _context.PlanCourses.ToListAsync();
        var questions = await _context.Questions.ToListAsync();
        var ratings = await _context.Ratings.ToListAsync();
        var roles = await _context.Roles.ToListAsync();
        var staffs = await _context.Staff.ToListAsync();
        var states = await _context.States.ToListAsync();
        var subscriptionPlans = await _context.Subscriptionplans.ToListAsync();
        var tags = await _context.Tags.ToListAsync();
        var units = await _context.Units.ToListAsync();
        var userDocuments = await _context.UserDocuments.ToListAsync();
        var userCourses = await _context.UserCourses.ToListAsync();
        var usersExams = await _context.UsersExams.ToListAsync();

        // Tạo Excel files cho các bảng
        var userExcelFile = GenerateExcel(users, "Users");
        var orderExcelFile = GenerateExcel(orders, "Orders");
        var categoryExcelFile = GenerateExcel(categories, "Categories");
        var containerExcelFile = GenerateExcel(containers, "Containers");
        var counterExcelFile = GenerateExcel(counters, "Counters");
        var courseExcelFile = GenerateExcel(courses, "Courses");
        var departmentExcelFile = GenerateExcel(departments, "Departments");
        var documentExcelFile = GenerateExcel(documents, "Documents");
        var examExcelFile = GenerateExcel(exams, "Exams");
        var lessonExcelFile = GenerateExcel(lessons, "Lessons");
        var planExcelFile = GenerateExcel(plans, "Plans");
        var questionExcelFile = GenerateExcel(questions, "Questions");
        var ratingExcelFile = GenerateExcel(ratings, "Ratings");
        var roleExcelFile = GenerateExcel(roles, "Roles");
        var staffExcelFile = GenerateExcel(staffs, "Staffs");
        var stateExcelFile = GenerateExcel(states, "States");
        var subscriptionPlanExcelFile = GenerateExcel(subscriptionPlans, "SubscriptionPlans");
        var tagExcelFile = GenerateExcel(tags, "Tags");
        var unitExcelFile = GenerateExcel(units, "Units");
        var userDocumentExcelFile = GenerateExcel(userDocuments, "UserDocuments");
        var userCourseExcelFile = GenerateExcel(userCourses, "UserCourses");
        var usersExamExcelFile = GenerateExcel(usersExams, "UsersExams");

        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync();

        // Tạo folder path với timestamp
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

        // Upload tất cả các file Excel vào blob storage
        await UploadToBlobAsync($"Users/{timestamp}_users.xlsx", userExcelFile);
        await UploadToBlobAsync($"Orders/{timestamp}_orders.xlsx", orderExcelFile);
        await UploadToBlobAsync($"Categories/{timestamp}_categories.xlsx", categoryExcelFile);
        await UploadToBlobAsync($"Containers/{timestamp}_containers.xlsx", containerExcelFile);
        await UploadToBlobAsync($"Counters/{timestamp}_counters.xlsx", counterExcelFile);
        await UploadToBlobAsync($"Courses/{timestamp}_courses.xlsx", courseExcelFile);
        await UploadToBlobAsync($"Departments/{timestamp}_departments.xlsx", departmentExcelFile);
        await UploadToBlobAsync($"Documents/{timestamp}_documents.xlsx", documentExcelFile);
        await UploadToBlobAsync($"Exams/{timestamp}_exams.xlsx", examExcelFile);
        await UploadToBlobAsync($"Lessons/{timestamp}_lessons.xlsx", lessonExcelFile);
        await UploadToBlobAsync($"Plans/{timestamp}_plans.xlsx", planExcelFile);
        await UploadToBlobAsync($"Questions/{timestamp}_questions.xlsx", questionExcelFile);
        await UploadToBlobAsync($"Ratings/{timestamp}_ratings.xlsx", ratingExcelFile);
        await UploadToBlobAsync($"Roles/{timestamp}_roles.xlsx", roleExcelFile);
        await UploadToBlobAsync($"Staffs/{timestamp}_staffs.xlsx", staffExcelFile);
        await UploadToBlobAsync($"States/{timestamp}_states.xlsx", stateExcelFile);
        await UploadToBlobAsync($"SubscriptionPlans/{timestamp}_subscriptionPlans.xlsx", subscriptionPlanExcelFile);
        await UploadToBlobAsync($"Tags/{timestamp}_tags.xlsx", tagExcelFile);
        await UploadToBlobAsync($"Units/{timestamp}_units.xlsx", unitExcelFile);
        await UploadToBlobAsync($"UserDocuments/{timestamp}_userDocuments.xlsx", userDocumentExcelFile);
        await UploadToBlobAsync($"UserCourses/{timestamp}_userCourses.xlsx", userCourseExcelFile);
        await UploadToBlobAsync($"UsersExams/{timestamp}_usersExams.xlsx", usersExamExcelFile);

        Console.WriteLine("Sao lưu thành công!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Lỗi trong quá trình sao lưu: {ex.Message}");
    }
}


        private byte[] GenerateExcel<T>(List<T> items, string sheetName)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(sheetName);

            var properties = typeof(T).GetProperties();

            // Thêm tiêu đề
            for (int i = 0; i < properties.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = properties[i].Name;
            }

            // Thêm dữ liệu
            for (int i = 0; i < items.Count; i++)
            {
                for (int j = 0; j < properties.Length; j++)
                {
                    worksheet.Cells[i + 2, j + 1].Value = properties[j].GetValue(items[i]);
                }
            }

            return package.GetAsByteArray();
        }

        private async Task UploadToBlobAsync(string fileName, byte[] content)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            using var memoryStream = new MemoryStream(content);
            await blobClient.UploadAsync(memoryStream, overwrite: true);

            Console.WriteLine($"Đã lưu file: {fileName}");
        }
    } 
}
