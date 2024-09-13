using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using study4_be.Models;


namespace study4_be.Services
{
    public class FireBaseServices
    {
        private readonly IConfiguration _config;
        private readonly FirebaseServiceAccountKey _serviceAccountKey;
        private readonly string _firebaseBucketName;
        public FireBaseServices(IConfiguration config)
        {
            _config = config;
            _serviceAccountKey = _config.GetSection("Firebase:ServiceAccountKey").Get<FirebaseServiceAccountKey>();
            _firebaseBucketName = _config["Firebase:StorageBucket"];

            InitializeFirebase();
        }

        private void InitializeFirebase()
        {
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromJson(_serviceAccountKey.ToJson())
            });
        }

        public string GetFirebaseBucketName()
        {
            return _firebaseBucketName;
        }
        public async Task<string> UploadFileFromUrlToFirebaseStorageAsync(string imageUrl, string fileName, string bucketName)
        {
            // Đường dẫn tới tệp tin serviceAccount.json
            string serviceAccountPath = Path.Combine(Directory.GetCurrentDirectory(), "firebase_config.json");

            // Load thông tin xác thực từ file
            var credential = GoogleCredential.FromFile(serviceAccountPath);

            // Tạo đối tượng StorageClient
            var storage = StorageClient.Create(credential);

            try
            {
                // Download the image from the URL
                using (var httpClient = new HttpClient())
                {
                    var imageStream = await httpClient.GetStreamAsync(imageUrl);

                    // Create a MemoryStream to store the downloaded image
                    using (var memoryStream = new MemoryStream())
                    {
                        await imageStream.CopyToAsync(memoryStream);
                        memoryStream.Position = 0; // Reset stream position

                        // Upload the image to Firebase Storage
                        var storageObject = await storage.UploadObjectAsync(bucketName, fileName, null, memoryStream);

                        // Return the public URL of the uploaded file
                        return $"https://firebasestorage.googleapis.com/v0/b/{bucketName}/o/{fileName}?alt=media";
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any errors during upload
                Console.WriteLine($"Error uploading file: {ex.Message}");
                throw;
            }
        }

        public async Task<string> UploadFileToFirebaseStorageAsync(IFormFile file, string fileName, string bucketName)
        {
            // Đường dẫn tới tệp tin serviceAccount.json
            string serviceAccountPath = Path.Combine(Directory.GetCurrentDirectory(), "firebase_config.json");

            // Load thông tin xác thực từ file
            var credential = GoogleCredential.FromFile(serviceAccountPath);

            // Tạo đối tượng StorageClient
            var storage = StorageClient.Create(credential);

            try
            {
                // Tạo MemoryStream từ IFormFile
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);

                    // Upload file lên Firebase Storage
                    var storageObject = await storage.UploadObjectAsync(bucketName, fileName, null, memoryStream);
                    // Trả về URL công khai để truy cập tệp tin vừa tải lên
                    return $"https://firebasestorage.googleapis.com/v0/b/{bucketName}/o/{fileName}?alt=media";
                }
            }
            catch (Exception ex)
            {
                // Xử lý các lỗi tải lên
                Console.WriteLine($"Error uploading file: {ex.Message}");
                throw;
            }
        }
        public async Task DeleteFileFromFirebaseStorageAsync(string fileName, string bucketName)
        {
            // Đường dẫn tới tệp tin serviceAccount.json
            string serviceAccountPath = Path.Combine(Directory.GetCurrentDirectory(), "firebase_config.json");

            // Load thông tin xác thực từ file
            var credential = GoogleCredential.FromFile(serviceAccountPath);

            // Tạo đối tượng StorageClient
            var storage = StorageClient.Create(credential);

            try
            {
                // Xóa file khỏi Firebase Storage
                await storage.DeleteObjectAsync(bucketName, fileName);
            }
            catch (Google.GoogleApiException ex) when (ex.Error.Code == 404)
            {
                // Tệp không tồn tại
                Console.WriteLine($"File {fileName} does not exist in bucket {bucketName}");
            }
            catch (Exception ex)
            {
                // Xử lý các lỗi khác
                Console.WriteLine($"Error deleting file: {ex.Message}");
                throw;
            }
        }
     
    }
}