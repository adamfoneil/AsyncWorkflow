﻿using System.Text.Json;
using AsyncWorkflow.Interfaces;
using AsyncWorkflow.Records;

namespace AsyncWorkflow.Extensions;

public static class QueueExtensions
{
    public static async Task<string> EnqueueAsync<TPayload>(this IQueue queue, string machineName, string handler, TPayload payload, string? userName = null) where TPayload : notnull
    {
        var json = JsonSerializer.Serialize(payload);
        var message = new Message(handler, json, userName);
        await queue.EnqueueAsync(machineName, message);
        return message.Id;
    }
}
