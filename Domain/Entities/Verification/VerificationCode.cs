﻿#nullable disable

using ESMART.Domain.Entities.Data;

namespace ESMART.Domain.Entities.Verification
{
    public class VerificationCode
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Code { get; set; }

        public string ServiceId { get; set; }
        public string ApplicationUserId { get; set; }
        public DateTime IssuedAt { get; set; } = DateTime.Now;
        public DateTime ExpiresAt { get; set; } = DateTime.Now.AddMinutes(20);
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}