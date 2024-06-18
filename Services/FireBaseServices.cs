using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
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
    }
}