using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RSQueueClient
{
    // Core data models
    public class Message
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("content_hash")]
        public string ContentHash { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("receipt_handle")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guid? ReceiptHandle { get; set; }

        [JsonPropertyName("visible_after")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? VisibleAfter { get; set; }
    }

    public class QueueSpec
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("visibility_timeout_seconds")]
        public ulong VisibilityTimeoutSeconds { get; set; }

        [JsonPropertyName("enable_deduplication")]
        public bool EnableDeduplication { get; set; }

        [JsonPropertyName("deduplication_window_seconds")]
        public ulong DeduplicationWindowSeconds { get; set; }
    }

    public class QueueInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("visibility_timeout_seconds")]
        public ulong VisibilityTimeoutSeconds { get; set; }

        [JsonPropertyName("enable_deduplication")]
        public bool EnableDeduplication { get; set; }

        [JsonPropertyName("deduplication_window_seconds")]
        public ulong DeduplicationWindowSeconds { get; set; }

        [JsonPropertyName("dedup_cache_size")]
        public int DedupCacheSize { get; set; }
    }

    public class QueueDetailInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("visibility_timeout_seconds")]
        public ulong VisibilityTimeoutSeconds { get; set; }

        [JsonPropertyName("enable_deduplication")]
        public bool EnableDeduplication { get; set; }

        [JsonPropertyName("deduplication_window_seconds")]
        public ulong DeduplicationWindowSeconds { get; set; }

        [JsonPropertyName("dedup_cache_size")]
        public int DedupCacheSize { get; set; }

        [JsonPropertyName("messages_pending")]
        public int MessagesPending { get; set; }

        [JsonPropertyName("messages_in_flight")]
        public int MessagesInFlight { get; set; }

        [JsonPropertyName("visible_messages")]
        public int VisibleMessages { get; set; }

        [JsonPropertyName("recent_messages")]
        public List<MessagePreview> RecentMessages { get; set; }
    }

    public class MessagePreview
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("status")]
        public MessageStatus Status { get; set; }

        [JsonPropertyName("visible_after")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? VisibleAfter { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MessageStatus
    {
        [JsonPropertyName("pending")]
        Pending,
        [JsonPropertyName("in_flight")]
        InFlight
    }

    // Request models
    public class CreateQueueRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("visibility_timeout_seconds")]
        public ulong VisibilityTimeoutSeconds { get; set; } = 120; // 2 minutes default

        [JsonPropertyName("enable_deduplication")]
        public bool EnableDeduplication { get; set; } = false;

        [JsonPropertyName("deduplication_window_seconds")]
        public ulong DeduplicationWindowSeconds { get; set; } = 300; // 5 minutes default
    }

    public class UpdateQueueRequest
    {
        [JsonPropertyName("visibility_timeout_seconds")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ulong? VisibilityTimeoutSeconds { get; set; }

        [JsonPropertyName("enable_deduplication")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? EnableDeduplication { get; set; }

        [JsonPropertyName("deduplication_window_seconds")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ulong? DeduplicationWindowSeconds { get; set; }
    }

    public class EnqueueRequest
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

    public class BatchEnqueueRequest
    {
        [JsonPropertyName("messages")]
        public List<string> Messages { get; set; }
    }

    public class GetMessagesRequest
    {
        [JsonPropertyName("count")]
        public int Count { get; set; } = 1;
    }

    public class PeekMessagesRequest
    {
        [JsonPropertyName("count")]
        public int Count { get; set; } = 1;

        [JsonPropertyName("offset")]
        public int Offset { get; set; } = 0;
    }

    // Response models
    public class EnqueueResponse
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("error")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Error { get; set; }
    }

    public class BatchEnqueueResponse
    {
        [JsonPropertyName("results")]
        public List<EnqueueResponse> Results { get; set; }

        [JsonPropertyName("successful")]
        public int Successful { get; set; }

        [JsonPropertyName("failed")]
        public int Failed { get; set; }
    }

    // Health and monitoring models
    public class HealthStatus
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("uptime_seconds")]
        public ulong UptimeSeconds { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("active_queues")]
        public int ActiveQueues { get; set; }

        [JsonPropertyName("total_messages")]
        public int TotalMessages { get; set; }
    }

    public class MetricsSummary
    {
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("messages_enqueued_total")]
        public ulong MessagesEnqueuedTotal { get; set; }

        [JsonPropertyName("messages_dequeued_total")]
        public ulong MessagesDequeuedTotal { get; set; }

        [JsonPropertyName("messages_deleted_total")]
        public ulong MessagesDeletedTotal { get; set; }

        [JsonPropertyName("duplicate_messages_rejected_total")]
        public ulong DuplicateMessagesRejectedTotal { get; set; }

        [JsonPropertyName("messages_expired_total")]
        public ulong MessagesExpiredTotal { get; set; }

        [JsonPropertyName("queues_created_total")]
        public ulong QueuesCreatedTotal { get; set; }

        [JsonPropertyName("queues_deleted_total")]
        public ulong QueuesDeletedTotal { get; set; }

        [JsonPropertyName("queues_purged_total")]
        public ulong QueuesPurgedTotal { get; set; }

        [JsonPropertyName("active_queues")]
        public long ActiveQueues { get; set; }

        [JsonPropertyName("total_messages_pending")]
        public long TotalMessagesPending { get; set; }

        [JsonPropertyName("total_messages_in_flight")]
        public long TotalMessagesInFlight { get; set; }

        [JsonPropertyName("http_requests_total")]
        public ulong HttpRequestsTotal { get; set; }
    }

    public class QueueMetrics
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("messages_pending")]
        public int MessagesPending { get; set; }

        [JsonPropertyName("messages_in_flight")]
        public int MessagesInFlight { get; set; }

        [JsonPropertyName("total_size")]
        public int TotalSize { get; set; }

        [JsonPropertyName("visible_messages")]
        public int VisibleMessages { get; set; }

        [JsonPropertyName("dedup_cache_size")]
        public int DedupCacheSize { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("visibility_timeout_seconds")]
        public ulong VisibilityTimeoutSeconds { get; set; }

        [JsonPropertyName("enable_deduplication")]
        public bool EnableDeduplication { get; set; }
    }
}