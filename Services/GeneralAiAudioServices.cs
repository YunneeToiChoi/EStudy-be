using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using System.Diagnostics;

namespace study4_be.Services
{
    public class GeneralAiAudioServices
    {
    public async Task<string> UploadFileToFirebaseStorageAsync(byte[] fileBytes, string fileName, string bucketName)
        {
            // Assuming your service account file is named "serviceAccount.json"
            string serviceAccountPath = Path.Combine(Directory.GetCurrentDirectory(), "firebase_config.json");

            // Load the credential from the file
            var credential = GoogleCredential.FromFile(serviceAccountPath);

            // Create a StorageClient object
            var storage = StorageClient.Create(credential);
            string correctedBucketName = "estudy-426108.appspot.com"; // Assuming the correct name is 'estudy426108'
            // Create a MemoryStream object from the file bytes
            using (var memoryStream = new MemoryStream(fileBytes))
            {
                // Upload the file to Firebase Storage
                try
                {
                    var storageObject = await storage.UploadObjectAsync(correctedBucketName, fileName, null, memoryStream);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error uploading file: {ex.Message}");
                }
                return $"https://firebasestorage.googleapis.com/v0/b/{correctedBucketName}/o/{fileName}?alt=media";
            }
        }
        public void GenerateAudio(string text, string filePath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = @"C:\Program Files\eSpeak NG\espeak-ng.exe",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Use ArgumentList to safely pass arguments without string concatenation
            startInfo.ArgumentList.Add("-w");
            startInfo.ArgumentList.Add(filePath);  // Safe passing of file path
            startInfo.ArgumentList.Add(text);      // Safe passing of user-provided text

            using (var process = new Process { StartInfo = startInfo })
            {
                process.Start();
                process.WaitForExit();
            }
        }
    }
}
