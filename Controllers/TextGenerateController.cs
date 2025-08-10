using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using ApiGateway.Models;

namespace ApiGateway.Controllers
{
    [ApiController]
    [Route("api/text-generate")]
    public class TextGenerateController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TextGenerateController> _logger;

        public TextGenerateController(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<TextGenerateController> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            var baseUrl = _configuration["TextGenerateService:BaseUrl"] ?? "http://text-generate-app:80";
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("TextGenerateService:Timeout", 30));
        }

        /// <summary>
        /// Get server information from text-generate service
        /// </summary>
        [HttpGet("system/server-info")]
        public async Task<IActionResult> GetServerInfo()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/system/server-info");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Ok(JsonSerializer.Deserialize<object>(content));
                }
                
                return StatusCode((int)response.StatusCode, new { error = "Failed to get server info" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting server info from text-generate service");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// List files from text-generate service
        /// </summary>
        [HttpGet("system/list")]
        public async Task<IActionResult> ListFiles()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/system/list");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Ok(JsonSerializer.Deserialize<object>(content));
                }
                
                return StatusCode((int)response.StatusCode, new { error = "Failed to list files" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing files from text-generate service");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Delete all files from text-generate service
        /// </summary>
        [HttpDelete("system/delete-all")]
        public async Task<IActionResult> DeleteAllFiles()
        {
            try
            {
                var response = await _httpClient.DeleteAsync("/api/system/delete-all");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Ok(JsonSerializer.Deserialize<object>(content));
                }
                
                return StatusCode((int)response.StatusCode, new { error = "Failed to delete all files" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting all files from text-generate service");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Delete specific file from text-generate service
        /// </summary>
        [HttpDelete("system/delete/{fileName}")]
        public async Task<IActionResult> DeleteFile(string fileName)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/system/delete/{fileName}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Ok(JsonSerializer.Deserialize<object>(content));
                }
                
                return StatusCode((int)response.StatusCode, new { error = $"Failed to delete file {fileName}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FileName} from text-generate service", fileName);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Generate bank bill using text-generate service
        /// </summary>
        [HttpPost("bank-bill/generate")]
        public async Task<IActionResult> GenerateBankBill([FromBody] object requestData)
        {
            try
            {
                var jsonData = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync("/api/bank-bill/generate", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return Ok(JsonSerializer.Deserialize<object>(responseContent));
                }
                
                return StatusCode((int)response.StatusCode, new { error = "Failed to generate bank bill" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating bank bill from text-generate service");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/system/server-info");
                
                if (response.IsSuccessStatusCode)
                {
                    return Ok(new { status = "healthy", service = "text-generate", timestamp = DateTime.UtcNow });
                }
                
                return StatusCode(503, new { status = "unhealthy", service = "text-generate", timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed for text-generate service");
                return StatusCode(503, new { status = "unhealthy", service = "text-generate", error = ex.Message, timestamp = DateTime.UtcNow });
            }
        }
    }
}
