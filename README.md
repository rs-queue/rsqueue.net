# RSQueue .NET Client

A comprehensive .NET client library for interacting with the RSQueue message queue service.

## Features

- Full API coverage for all RSQueue endpoints
- Support for Basic Authentication
- Async/await pattern throughout
- Strongly typed request/response models
- Built-in retry logic and error handling
- Helper methods for common operations

## Installation

### From Source
```bash
cd RSQueueClient
dotnet build
```

### NuGet Package (when published)
```bash
dotnet add package RSQueueClient
```

## Quick Start

```csharp
using RSQueueClient;

// Create client without authentication
var client = new RSQueueApiClient("http://localhost:4000");

// Or with authentication
var client = new RSQueueApiClient("http://localhost:4000", "admin", "password123");

// Create a queue
var queue = await client.CreateQueueAsync("my-queue");

// Enqueue a message
var response = await client.EnqueueMessageAsync("my-queue", "Hello World!");

// Get messages
var messages = await client.GetMessagesAsync("my-queue", count: 5);
foreach (var msg in messages)
{
    Console.WriteLine($"Message: {msg.Content}");

    // Delete the message after processing
    if (msg.ReceiptHandle.HasValue)
        await client.DeleteMessageAsync("my-queue", msg.ReceiptHandle.Value);
}
```

## API Methods

### Health & Monitoring
- `GetHealthAsync()` - Get service health status
- `GetMetricsAsync()` - Get raw Prometheus metrics
- `GetMetricsSummaryAsync()` - Get metrics summary
- `GetQueueMetricsAsync(queueName)` - Get metrics for specific queue

### Queue Management
- `CreateQueueAsync(request)` - Create a new queue
- `ListQueuesAsync()` - List all queues
- `DeleteQueueAsync(queueName)` - Delete a queue
- `UpdateQueueSettingsAsync(queueName, request)` - Update queue settings
- `PurgeQueueAsync(queueName)` - Remove all messages from queue
- `GetQueueDetailsAsync(queueName)` - Get detailed queue information

### Message Operations
- `EnqueueMessageAsync(queueName, content)` - Add single message
- `EnqueueBatchAsync(queueName, messages)` - Add multiple messages
- `GetMessagesAsync(queueName, count)` - Dequeue messages
- `PeekMessagesAsync(queueName, count, offset)` - View messages without dequeuing
- `ListAllMessagesAsync(queueName)` - List all messages in queue
- `DeleteMessageAsync(queueName, receiptHandle)` - Delete specific message

### Helper Methods
- `CreateQueueAsync(queueName)` - Create queue with default settings
- `ProcessMessagesAsync(queueName, processor, batchSize)` - Process messages with callback

## Configuration Options

### Queue Creation
```csharp
var request = new CreateQueueRequest
{
    Name = "my-queue",
    VisibilityTimeoutSeconds = 30,      // Default: 120
    EnableDeduplication = true,          // Default: false
    DeduplicationWindowSeconds = 300     // Default: 300
};
var queue = await client.CreateQueueAsync(request);
```

### Queue Updates
```csharp
var updateRequest = new UpdateQueueRequest
{
    VisibilityTimeoutSeconds = 60,
    EnableDeduplication = false
};
await client.UpdateQueueSettingsAsync("my-queue", updateRequest);
```

## Message Processing Pattern

```csharp
// Process messages with automatic deletion
await client.ProcessMessagesAsync(
    "my-queue",
    async (message) =>
    {
        try
        {
            // Process the message
            await ProcessMessage(message.Content);
            return true; // Return true to delete the message
        }
        catch
        {
            return false; // Return false to keep the message
        }
    },
    batchSize: 10
);
```

## Error Handling

```csharp
try
{
    var response = await client.EnqueueMessageAsync("my-queue", "Test");
    if (!string.IsNullOrEmpty(response.Error))
    {
        // Handle application-level error (e.g., duplicate message)
        Console.WriteLine($"Error: {response.Error}");
    }
}
catch (HttpRequestException ex)
{
    // Handle HTTP-level errors
    Console.WriteLine($"HTTP Error: {ex.Message}");
}
```

## Running the Example

1. Start the RSQueue service:
```bash
docker run -p 4000:4000 rsqueue:latest
```

2. Run the example application:
```bash
cd ExampleApp
dotnet run
```

## Project Structure

```
dotnet-client/
├── RSQueueClient/
│   ├── RSQueueClient.cs      # Main client class
│   ├── Models.cs              # Request/response models
│   └── RSQueueClient.csproj  # Library project file
├── ExampleApp/
│   ├── Program.cs             # Example usage
│   └── ExampleApp.csproj     # Example project file
└── README.md                  # This file
```

## Authentication

The client supports Basic Authentication when RSQueue is configured with AUTH_USER and AUTH_PASSWORD:

```csharp
// With authentication
var client = new RSQueueApiClient(
    "https://queue.example.com",
    "username",
    "password"
);
```

## Thread Safety

The `RSQueueApiClient` class uses a single `HttpClient` instance and is thread-safe for concurrent operations.

## License

MIT