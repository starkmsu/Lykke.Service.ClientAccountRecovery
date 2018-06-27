﻿using System;

namespace Lykke.Service.ClientAccountRecovery.Core.Domain
{
    public class RecoveryContext
    {
        public string RecoveryId { get; set; }
        public string ClientId { get; set; }
        public int SeqNo { get; set; }
        public DateTime Time { get; set; }
        public State State { get; set; }
        public Trigger Action { get; set; }
        public bool HasSecretPhrases { get; set; }
        public bool DeviceVerified { get; set; }
        public bool DeviceVerificationRequested { get; set; }
        public bool SmsVerified { get; set; }
        public bool EmailVerified { get; set; }
        public bool KycPassed { get; set; }
        public bool HasPin { get; set; }
        public int SmsRecoveryAttempts { get; set; }
        public int EmailRecoveryAttempts { get; set; }
        public string Initiator { get; set; }
        public string Comment { get; set; }
        public string Ip { get; set; }
        public string UserAgent { get; set; }
        public DateTime? FrozenDate { get; set; }
    }
}
