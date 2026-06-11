using System.Text.Json;
using ResumeAnalyzer.DTOs.Candidate;

namespace ResumeAnalyzer.Helpers
{
    public static class ManagerAvailabilityHelper
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static List<AvailabilitySlotDto> Deserialize(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return [];

            try
            {
                return JsonSerializer.Deserialize<List<AvailabilitySlotDto>>(json, JsonOptions) ?? [];
            }
            catch
            {
                return [];
            }
        }

        public static string Serialize(IEnumerable<AvailabilitySlotDto> slots) =>
            JsonSerializer.Serialize(slots, JsonOptions);

        public static bool HasAvailability(string? json) =>
            Deserialize(json).Count > 0;

        public static bool IsScheduledWithinAvailability(
            string? json,
            DateTime interviewDate,
            TimeSpan interviewTime)
        {
            var slots = Deserialize(json);
            if (slots.Count == 0)
                return true;

            var date = interviewDate.Date;

            foreach (var slot in slots)
            {
                if (!DateTime.TryParse(slot.Date, out var slotDate))
                    continue;

                if (slotDate.Date != date)
                    continue;

                if (!TimeSpan.TryParse(NormalizeTime(slot.StartTime), out var start))
                    continue;

                if (!TimeSpan.TryParse(NormalizeTime(slot.EndTime), out var end))
                    continue;

                if (interviewTime >= start && interviewTime <= end)
                    return true;
            }

            return false;
        }

        private static string NormalizeTime(string time)
        {
            if (string.IsNullOrWhiteSpace(time))
                return "00:00";

            return time.Length == 5 ? time : time[..5];
        }
    }
}
