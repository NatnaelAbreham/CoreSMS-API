using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using ZenerpSMS.Models;
using System.Xml;
using ZenerpSMS.Utils;
using Newtonsoft.Json;
using System.Text.Json;

namespace ZenerpSMS.Controllers
{
    [ApiController]
    [Route("sms")]
    public class HomeController : Controller
    {
        private readonly ILogger _smsLogger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly LogWriter _logWriter;

        public HomeController(ILoggerFactory loggerFactory, IConfiguration configuration, IHttpClientFactory httpClientFactory, LogWriter logWriter)
        {
            _smsLogger = loggerFactory.CreateLogger("SMSservice");
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logWriter = logWriter;
        }

        [HttpPost("onesms")]
        public async Task<IActionResult> ShareSms([FromBody] ApiSmsRequest request)
        {
            var traceId = Guid.NewGuid().ToString();
            StringBuilder logBuilder = new StringBuilder();

            string zenuserName = _configuration["SmsSettings:ZenUsername"];
            string zenpassword = _configuration["SmsSettings:ZenPassword"];

            if (request.UserName != zenuserName || request.Password != zenpassword)
            {
                _smsLogger.LogWarning("TraceId: {TraceId} - Unauthorized access attempt with Username: {Username}", traceId, request.UserName);
                logBuilder.AppendLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\tTraceId: {traceId} - Unauthorized access attempt with Username: {request.UserName}");

                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = $"Unauthorized access attempt with Username: {request.UserName}"
                });
            }

            string apiUrl = _configuration["SmsSettings:ApiUrl"];
            var httpClient = _httpClientFactory.CreateClient();
            string userName = _configuration["SmsSettings:UserName"];
            string password = _configuration["SmsSettings:Password"];
            string responseContent1;
            int successCount = 0, failCount = 0;

            string phoneNumber = request.PhoneNumber;
            if (phoneNumber.StartsWith("+251"))
            {
                phoneNumber = phoneNumber.Replace("+251", "251");
            }
            else if (phoneNumber.Length == 9 && phoneNumber[0] == '9')
            {
                phoneNumber = "0" + phoneNumber;
            }


            LogWriter.Service_Name = phoneNumber + "_" + traceId;

            var payload = new
            {
                timestamp = request.Timestamp,
                phoneNumber = phoneNumber,
                userName = userName,
                password = password,
                message = request.Message,
                language = "EN"
            };

            var payloadLog = new
            {
                timestamp = request.Timestamp,
                phoneNumber = phoneNumber,
                userName = userName,
                password = "*************************",
                message = request.Message,
                language = "EN"
            };
            var jsonContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                logBuilder.AppendLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                    .Append("TraceId: ")

          .Append(traceId)
          .Append(" - Sending SMS to ")
          .Append(phoneNumber)
          .Append(". Payload: ")
          .Append(JsonConvert.SerializeObject(payloadLog, Newtonsoft.Json.Formatting.Indented));


                _smsLogger.LogInformation("TraceId: {TraceId} - Sending SMS to {PhoneNumber}. Payload: {Payload}",
                    traceId, phoneNumber, System.Text.Json.JsonSerializer.Serialize(payloadLog));

                var response = await httpClient.PostAsync(apiUrl, jsonContent);

                string responseContent = await response.Content.ReadAsStringAsync();

                responseContent1 = responseContent;

                logBuilder.AppendLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\tINFO\tTraceId: {traceId}")
                          .AppendLine($"Request URL: {apiUrl}")
                          .AppendLine($"Status Code: {response.StatusCode}")
                          .AppendLine("Response Content:")
                          .AppendLine(responseContent);

                _smsLogger.LogInformation(logBuilder.ToString());

                // Optionally, also log as structured message
                _smsLogger.LogInformation("TraceId: {TraceId} - Status Code: {StatusCode}. Response: {Response}",
                    traceId, response.StatusCode, responseContent);

                if (response.IsSuccessStatusCode)
                {

                    using var doc = JsonDocument.Parse(responseContent);
                    var root = doc.RootElement;

                    string responseCode = root.GetProperty("responseCode").GetString();
                    string message = root.GetProperty("message").GetString();

                    if (responseCode == "0")
                    {
                        successCount++;
                        logBuilder.AppendLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\tTraceId: {traceId} - SMS sent successfully to {phoneNumber}");
                        _smsLogger.LogInformation("TraceId: {TraceId} - SMS sent successfully to {PhoneNumber}", traceId, phoneNumber);
                    }
                    else
                    {
                        failCount++;
                        logBuilder.AppendLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\tTraceId: {traceId} - SMS not sent  -Error: {message}");
                        _smsLogger.LogInformation("TraceId: {TraceId} - SMS not sent   {Error}", traceId, message);

                    }

                }
                else
                {
                    failCount++;
                    responseContent = await response.Content.ReadAsStringAsync();

                    logBuilder.AppendLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\tWARNING\tTraceId: {traceId}")
              .AppendLine($"Failed to send SMS to {phoneNumber}")
              .AppendLine($"Status Code: {response.StatusCode}")
              .AppendLine("Response:")
              .AppendLine(responseContent);

                    _smsLogger.LogWarning("TraceId: {TraceId} - Failed to send SMS to {PhoneNumber}. Status Code: {StatusCode}. Response: {Response}",
                        traceId, phoneNumber, response.StatusCode, responseContent);
                }
            }
            catch (Exception ex)
            {
                failCount++;
                logBuilder.AppendLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\tERROR\tTraceId: {traceId}")
          .AppendLine($"Error sending SMS to {phoneNumber}")
          .AppendLine("Exception Details:")
          .AppendLine(ex.ToString());
                _smsLogger.LogError(ex, "TraceId: {TraceId} - Error sending SMS to {PhoneNumber}", traceId, phoneNumber);
                return StatusCode(500, $"Exceptions: {ex.Message}");
            }
            finally
            {
                _logWriter.LogRequestResponse(logBuilder);
            }
            string resultMessage = $"SMS processing completed: {successCount} successful, {failCount} failed.";
            _smsLogger.LogInformation("TraceId: {TraceId} - {Message}", traceId, resultMessage);

            using var doc1 = JsonDocument.Parse(responseContent1);
            var root1 = doc1.RootElement;

            var jsonResult = new
            {
                responseCode = root1.GetProperty("responseCode").GetString(),
                message = root1.GetProperty("message").GetString()
            };

            return Ok(jsonResult); // This will return proper JSON




        }

    }
}
