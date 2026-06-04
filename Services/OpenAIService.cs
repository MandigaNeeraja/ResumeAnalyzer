using System.Text;
using Newtonsoft.Json;
using ResumeAnalyzer.DTOs.AI;
using ResumeAnalyzer.Interfaces;

namespace ResumeAnalyzer.Services
{
    public class OpenAIService : IOpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public OpenAIService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<ResumeParseResponse>
            ParseResumeAsync(string resumeText)
        {
            var apiKey =
                _configuration["OpenAI:ApiKey"];

            var model =
                _configuration["OpenAI:Model"];

            var prompt = $@"
You are an ATS Resume Parser.

Extract:

1. Full Name
2. Email
3. Phone
4. Education
5. Current Designation
6. Total Experience Years
7. Skills

Return ONLY valid JSON.

Example:

{{
    ""fullName"": """",
    ""email"": """",
    ""phone"": """",
    ""education"": """",
    ""currentDesignation"": """",
    ""experienceYears"": 0,
    ""skills"": []
}}

Resume:

{resumeText}
";

            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },
                temperature = 0
            };

            var requestJson =
                JsonConvert.SerializeObject(requestBody);

            var request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://api.openai.com/v1/chat/completions");

            request.Headers.Add(
                "Authorization",
                $"Bearer {apiKey}");

            request.Content =
                new StringContent(
                    requestJson,
                    Encoding.UTF8,
                    "application/json");

            var response =
                await _httpClient.SendAsync(request);

            var responseContent =
                await response.Content.ReadAsStringAsync();
            Console.WriteLine("========== OPENAI RESPONSE ==========");
            Console.WriteLine(responseContent);
            Console.WriteLine("=====================================");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"OpenAI Error: {response.StatusCode}\n{responseContent}");
            }
            dynamic result =
                JsonConvert.DeserializeObject(responseContent)!;

            string jsonText =
                result.choices[0].message.content;

            return JsonConvert
                .DeserializeObject<ResumeParseResponse>(
                    jsonText)!;
        }
    }
}