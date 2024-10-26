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

                var users = await _context.Users.Include(u => u.Orders).ToListAsync();
                var orders = await _context.Orders.ToListAsync();

                // Tạo file Excel cho Users
                var userExcelFile = GenerateExcel(users, "Users");
                var orderExcelFile = GenerateExcel(orders, "Orders");

                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                await containerClient.CreateIfNotExistsAsync();

                // Tải file users.xlsx lên Blob
                await UploadToBlobAsync("users.xlsx", userExcelFile);
                // Tải file orders.xlsx lên Blob
                await UploadToBlobAsync("orders.xlsx", orderExcelFile);

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
