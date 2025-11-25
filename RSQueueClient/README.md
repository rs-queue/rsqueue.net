# RSQueueClient

.NET client library for RSQueue message queue service.

## Installation

Add a reference to the RSQueueClient project or install the NuGet package:

```bash
dotnet add package RSQueueClient
```

## Quick Start

```csharp
using RSQueueClient;

// Create client (without authentication)
using var client = new RSQueueApiClient("http://localhost:4000");

// Or with Basic Authentication
using var client = new RSQueueApiClient("http://localhost:4000", "admin", "password123");
```

## Usage Examples

### Health Check

```csharp
var health = await client.GetHealthAsync();
Console.WriteLine($"Status: {health.Status}");
Console.WriteLine($"Version: {health.Version}");
Console.WriteLine($"Uptime: {health.UptimeSeconds} seconds");
Console.WriteLine($"Active Queues: {health.ActiveQueues}");
```

### Queue Management

#### Create a Queue

```csharp
var request = new CreateQueueRequest
{
    Name = "my-queue",
    VisibilityTimeoutSeconds = 30,
    EnableDeduplication = true,
    DeduplicationWindowSeconds = 300
};
var queue = await client.CreateQueueAsync(request);

// Or with defaults
var queue = await client.CreateQueueAsync("my-queue");
```

#### List Queues

```csharp
var queues = await client.ListQueuesAsync();
foreach (var queue in queues)
{
    Console.WriteLine($"{queue.Name} - Size: {queue.Size}");
}
```

#### Get Queue Details

```csharp
var details = await client.GetQueueDetailsAsync("my-queue");
Console.WriteLine($"Messages pending: {details.MessagesPending}");
Console.WriteLine($"Messages in flight: {details.MessagesInFlight}");
```

#### Update Queue Settings

```csharp
var update = new UpdateQueueRequest
{
    VisibilityTimeoutSeconds = 60,
    EnableDeduplication = false
};
await client.UpdateQueueSettingsAsync("my-queue", update);
```

#### Delete a Queue

```csharp
await client.DeleteQueueAsync("my-queue");
```

#### Purge a Queue

```csharp
await client.PurgeQueueAsync("my-queue");
```

### Message Operations

#### Enqueue a Single Message

```csharp
var response = await client.EnqueueMessageAsync("my-queue", "Hello, World!");
Console.WriteLine($"Message ID: {response.Id}");
```

#### Enqueue Batch Messages

```csharp
var messages = new List<string>
{
    "Message 1",
    "Message 2",
    "Message 3"
};
var batch = await client.EnqueueBatchAsync("my-queue", messages);
Console.WriteLine($"Successful: {batch.Successful}, Failed: {batch.Failed}");
```

#### Get Messages (Dequeue)

```csharp
var messages = await client.GetMessagesAsync("my-queue", count: 5);
foreach (var msg in messages)
{
    Console.WriteLine($"ID: {msg.Id}, Content: {msg.Content}");

    // Process the message...

    // Delete after processing
    if (msg.ReceiptHandle.HasValue)
    {
        await client.DeleteMessageAsync("my-queue", msg.ReceiptHandle.Value);
    }
}
```

#### Peek Messages (Without Dequeuing)

```csharp
var messages = await client.PeekMessagesAsync("my-queue", count: 5);
foreach (var msg in messages)
{
    Console.WriteLine($"ID: {msg.Id}, Content: {msg.Content}, Status: {msg.Status}");
}
```

#### List All Messages

```csharp
var allMessages = await client.ListAllMessagesAsync("my-queue");
Console.WriteLine($"Total messages: {allMessages.Count}");
```

### Processing Messages

Use the built-in helper to process messages:

```csharp
await client.ProcessMessagesAsync(
    "my-queue",
    async (message) =>
    {
        Console.WriteLine($"Processing: {message.Content}");

        // Do your work here...
        await Task.Delay(100);

        // Return true to delete the message, false to leave it
        return true;
    },
    batchSize: 10
);
```

### Metrics

#### Get Queue Metrics

```csharp
var metrics = await client.GetQueueMetricsAsync("my-queue");
Console.WriteLine($"Pending: {metrics.MessagesPending}");
Console.WriteLine($"In flight: {metrics.MessagesInFlight}");
```

#### Get Metrics Summary

```csharp
var summary = await client.GetMetricsSummaryAsync();
Console.WriteLine($"Total enqueued: {summary.MessagesEnqueuedTotal}");
Console.WriteLine($"Total dequeued: {summary.MessagesDequeuedTotal}");
Console.WriteLine($"Duplicates rejected: {summary.DuplicateMessagesRejectedTotal}");
```

#### Get Raw Prometheus Metrics

```csharp
var rawMetrics = await client.GetMetricsAsync();
Console.WriteLine(rawMetrics);
```

## Cancellation Token Support

All async methods support `CancellationToken`:

```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
var health = await client.GetHealthAsync(cts.Token);
```

## License

MIT
