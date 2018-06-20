using System;
using Lykke.AzureStorage.Tables;
using Lykke.Service.ClientAccountRecovery.Core.Domain;

namespace Lykke.Service.ClientAccountRecovery.AzureRepositories
{
    public class LogTableEntity : AzureTableEntity
    {
        public string RecoveryId => PartitionKey;
        public int SeqNo => int.Parse(RowKey);
        public string ClientId { get; set; }

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
        
        public static LogTableEntity CreateNew(RecoveryContext context)
        {
            return new LogTableEntity
            {
                ClientId = context.ClientId,
                PartitionKey = GetPartitionKey(context.RecoveryId),
                RowKey = context.SeqNo.ToString(),
                Time = context.Time,
                Action = context.Action,
                State = context.State,
                Initiator = context.Initiator,
                Comment = context.Comment,
                HasSecretPhrases = context.HasSecretPhrases,
                DeviceVerified = context.DeviceVerified,
                DeviceVerificationRequested = context.DeviceVerificationRequested,
                SmsVerified = context.SmsVerified,
                EmailVerified = context.EmailVerified,
                KycPassed = context.KycPassed,
                HasPin = context.HasPin,
                SmsRecoveryAttempts = context.SmsRecoveryAttempts,
                EmailRecoveryAttempts = context.EmailRecoveryAttempts,
                Ip = context.Ip,
                UserAgent = context.UserAgent
            };
        }

        public RecoveryContext Convert()
        {
            var context = new RecoveryContext
            {
                ClientId = ClientId,
                RecoveryId = RecoveryId,
                SeqNo = SeqNo,
                Time = Time,
                Action = Action,
                State = State,
                Initiator = Initiator,
                Comment = Comment,
                HasSecretPhrases = HasSecretPhrases,
                DeviceVerified = DeviceVerified,
                DeviceVerificationRequested = DeviceVerificationRequested,
                SmsVerified = SmsVerified,
                EmailVerified = EmailVerified,
                KycPassed = KycPassed,
                HasPin = HasPin,
                SmsRecoveryAttempts = SmsRecoveryAttempts,
                EmailRecoveryAttempts = EmailRecoveryAttempts,
                Ip = Ip,
                UserAgent = UserAgent
            };
            return context;
        }

        public static string GetPartitionKey(string recoveryId)
        {
            return recoveryId;
        }

        public static string GetRowKey(DateTime time)
        {
            return time.ToString("O");
        }
    }
}
