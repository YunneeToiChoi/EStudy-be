﻿namespace study4_be.Services.Document
{
    public class UploadDocumentRequest
    {
        //, string userId, string description
        public IEnumerable<IFormFile>? files { get; set; }
        public string? userId { get; set; }
    }
}
