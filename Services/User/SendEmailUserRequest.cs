﻿using Org.BouncyCastle.Bcpg.OpenPgp;

namespace study4_be.Services.User
{
    public class SendEmailUserRequest
    {
        public string? userEmail { get; set; } = string.Empty;
    }
}
