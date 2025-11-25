using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RSQueueClient
{
    /// <summary>
    /// Client for RSQueue message queue service
    /// </summary>
    public class RSQueueApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _baseUrl;
        private bool _disposed;

        /// <summary>
        /// Initialize RSQueue client without authentication
        /// </summary>
        /// <param name="baseUrl">Base URL of the RSQueue service (e.g., http://localhost:4000)</param>
        public RSQueueApiClient(string baseUrl) : this(baseUrl, null, null)
        {
        }

        /// <summary>
        /// Initialize RSQueue client with Basic Authentication
        /// </summary>
        /// <param name="baseUrl">Base URL of the RSQueue service</param>
        /// <param name="username">Username for Basic Auth (optional)</param>
        /// <param name="password">Password for Basic Auth (optional)</param>
        public RSQueueApiClient(string baseUrl, string username, string password)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _httpClient = new HttpClient();

            // Configure Basic Authentication if credentials provided
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                var authToken = Encoding.ASCII.GetBytes($"{username}:{password}");
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));
            }

            // Set default headers
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            // Configure JSON options
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        #region Health & Monitoring

        /// <summary>
        /// Get health status of the RSQueue service
        /// </summary>
        public async Task<HealthStatus> GetHealthAsync(CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/health", cancellationToken);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<HealthStatus>(json, _jsonOptions);
        }

        /// <summary>
        /// Get raw Prometheus metrics
        /// </summary>
        public async Task<string> GetMetricsAsync(CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/metrics", cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Get metrics summary
        /// </summary>
        public async Task<MetricsSummary> GetMetricsSummaryAsync(CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/metrics/summary", cancellationToken);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<MetricsSummary>(json, _jsonOptions);
        }

        /// <summary>
        /// Get metrics for a specific queue
        /// </summary>
        public async Task<QueueMetrics> GetQueueMetricsAsync(string queueName, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/queues/{Uri.EscapeDataString(queueName)}/metrics", cancellationToken);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<QueueMetrics>(json, _jsonOptions);
        }

        #endregion

        #region Queue Management

        /// <summary>
        /// Create a new queue
        /// </summary>
        public async Task<QueueInfo> CreateQueueAsync(CreateQueueRequest request, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/queues", content, cancellationToken);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<QueueInfo>(responseJson, _jsonOptions);
        }

        /// <summary>
        /// List all queues
        /// </summary>
        public async Task<List<QueueInfo>> ListQueuesAsync(CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/queues", cancellationToken);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<QueueInfo>>(json, _jsonOptions);
        }

        /// <summary>
        /// Delete a queue
        /// </summary>
        public async Task<bool> DeleteQueueAsync(string queueName, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/queues/{Uri.EscapeDataString(queueName)}", cancellationToken);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Update queue settings
        /// </summary>
        public async Task<bool> UpdateQueueSettingsAsync(string queueName, UpdateQueueRequest request, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_baseUrl}/queues/{Uri.EscapeDataString(queueName)}/settings", content, cancellationToken);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Purge all messages from a queue
        /// </summary>
        public async Task<bool> PurgeQueueAsync(string queueName, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/queues/{Uri.EscapeDataString(queueName)}/purge", null, cancellationToken);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Get detailed information about a queue
        /// </summary>
        public async Task<QueueDetailInfo> GetQueueDetailsAsync(string queueName, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/queues/{Uri.EscapeDataString(queueName)}/details", cancellationToken);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<QueueDetailInfo>(json, _jsonOptions);
        }

        #endregion

        #region Message Operations

        /// <summary>
        /// Enqueue a single message
        /// </summary>
        public async Task<EnqueueResponse> EnqueueMessageAsync(string queueName, string content, CancellationToken cancellationToken = default)
        {
            var request = new EnqueueRequest { Content = content };
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/queues/{Uri.EscapeDataString(queueName)}/messages", httpContent, cancellationToken);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<EnqueueResponse>(responseJson, _jsonOptions);
        }

        /// <summary>
        /// Enqueue multiple messages in batch
        /// </summary>
        public async Task<BatchEnqueueResponse> EnqueueBatchAsync(string queueName, List<string> messages, CancellationToken cancellationToken = default)
        {
            var request = new BatchEnqueueRequest { Messages = messages };
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/queues/{Uri.EscapeDataString(queueName)}/messages/batch", content, cancellationToken);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<BatchEnqueueResponse>(responseJson, _jsonOptions);
        }

        /// <summary>
        /// Get messages from queue (dequeue)
        /// </summary>
        public async Task<List<Message>> GetMessagesAsync(string queueName, int count = 1, CancellationToken cancellationToken = default)
        {
            var request = new GetMessagesRequest { Count = count };
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/queues/{Uri.EscapeDataString(queueName)}/messages/get", content, cancellationToken);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Message>>(responseJson, _jsonOptions);
        }

        /// <summary>
        /// Peek at messages without dequeuing them
        /// </summary>
        public async Task<List<MessagePreview>> PeekMessagesAsync(string queueName, int count = 1, int offset = 0, CancellationToken cancellationToken = default)
        {
            var request = new PeekMessagesRequest { Count = count, Offset = offset };
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/queues/{Uri.EscapeDataString(queueName)}/messages/peek", content, cancellationToken);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<MessagePreview>>(responseJson, _jsonOptions);
        }

        /// <summary>
        /// List all messages in a queue (for monitoring)
        /// </summary>
        public async Task<List<MessagePreview>> ListAllMessagesAsync(string queueName, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/queues/{Uri.EscapeDataString(queueName)}/messages/all", cancellationToken);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<MessagePreview>>(json, _jsonOptions);
        }

        /// <summary>
        /// Delete a message using its receipt handle
        /// </summary>
        public async Task<bool> DeleteMessageAsync(string queueName, Guid receiptHandle, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.DeleteAsync(
                $"{_baseUrl}/queues/{Uri.EscapeDataString(queueName)}/messages/{receiptHandle}",
                cancellationToken);
            return response.IsSuccessStatusCode;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Create a queue with default settings
        /// </summary>
        public async Task<QueueInfo> CreateQueueAsync(string queueName, CancellationToken cancellationToken = default)
        {
            return await CreateQueueAsync(new CreateQueueRequest { Name = queueName }, cancellationToken);
        }

        /// <summary>
        /// Process messages from a queue
        /// </summary>
        public async Task ProcessMessagesAsync(
            string queueName,
            Func<Message, Task<bool>> processor,
            int batchSize = 1,
            CancellationToken cancellationToken = default)
        {
            var messages = await GetMessagesAsync(queueName, batchSize, cancellationToken);

            foreach (var message in messages)
            {
                try
                {
                    var success = await processor(message);
                    if (success && message.ReceiptHandle.HasValue)
                    {
                        await DeleteMessageAsync(queueName, message.ReceiptHandle.Value, cancellationToken);
                    }
                }
                catch
                {
                    // Message will become visible again after visibility timeout
                }
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _httpClient?.Dispose();
                }
                _disposed = true;
            }
        }

        #endregion
    }
}