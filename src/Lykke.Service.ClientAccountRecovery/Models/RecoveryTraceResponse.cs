using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    public class RecoveryTraceResponse
    {
        [Required]
        public DateTime Time { get; internal set; }

        [Required]
        [JsonConverter(typeof(StringEnumConverter))]
        public State PreviousState { get; internal set; }

        [Required]
        [JsonConverter(typeof(StringEnumConverter))]
        public Trigger Action { get; internal set; }

        [Required]
        [JsonConverter(typeof(StringEnumConverter))]
        public State NewState { get; internal set; }

        [Required]
        public string Initiator { get; internal set; }

        public string Comment { get; internal set; }


        public static IEnumerable<RecoveryTraceResponse> Convert(RecoveryUnit unit)
        {
            var result = new List<RecoveryTraceResponse>();
            var first = new RecoveryTraceResponse { PreviousState = State.RecoveryStarted };
            result.Add(first);
            var counter = 0;
            foreach (var context in unit.Log)// Log is always sorted by SeqNo
            {
                var prev = result[counter];
                prev.Time = context.Time;
                prev.Action = context.Action;
                prev.Comment = context.Comment;
                prev.Initiator = context.Initiator;
                prev.NewState = context.State;
                var next = new RecoveryTraceResponse { PreviousState = context.State };
                result.Add(next);
                counter++;
            }

            return result.Take(result.Count - 1);
        }
    }


}
