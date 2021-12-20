using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EventHub
{
    public class SmartEventHubConsumer
    {
        private readonly string _eventHubConnectionString;
        private readonly string _eventHubName;
        private readonly ConcurrentDictionary<Guid, BlockingCollection<EventData>> _eventQueues = new();

        public SmartEventHubConsumer(string eventHubConnectionString, string eventHubName)
        {
            _eventHubConnectionString = eventHubConnectionString;
            _eventHubName = eventHubName;
        }

        public bool RegisterEventQueue(Guid id)
        {
            return _eventQueues.TryAdd(id, new BlockingCollection<EventData>());
        }

        public bool DeregisterEventQueue(Guid id)
        {
            bool result = false;
            if (_eventQueues.TryGetValue(id, out _))
            {
                result = _eventQueues.TryRemove(id, out _);
            }

            return result;
        }

        public IEnumerable<T> GetEvents<T>(Guid id, TimeSpan? maxEventWaitTime = null)
        {
            foreach(var json in GetEvents(id, maxEventWaitTime))
            { 
                yield return JObject.Parse(json).ToObject<T>();
            }
        }

        public IEnumerable<string> GetEvents(Guid id, TimeSpan? maxEventWaitTime = null)
        {
            int count = 0;

            while (true)
            {
                if (_eventQueues.TryGetValue(id, out BlockingCollection<EventData> queue))
                {
                    EventData eventData;
                    if (maxEventWaitTime != null)
                    {
                        try
                        {
                            using var source = new CancellationTokenSource((TimeSpan)maxEventWaitTime);
                            eventData = queue.Take(source.Token);
                        }
                        catch(OperationCanceledException)
                        {
                            break;
                        }
                    }
                    else
                    {
                        eventData = queue.Take();
                    }
                    
                    count++;
                    yield return Encoding.UTF8.GetString(eventData.Body.ToArray());

                    if (eventData.Properties.TryGetValue("user-command-count", out object userCount) && int.Parse((string)userCount) == count)
                        break;

                    if (eventData.Properties.TryGetValue("user-success", out object success) && !bool.Parse((string)success))
                        break;
                }
                else yield break;
            }

            yield break;
        }

        public async Task ReceiveEventsFromDeviceAsync(CancellationToken ct)
        {
            await using var consumer = new EventHubConsumerClient(
                EventHubConsumerClient.DefaultConsumerGroupName,
                _eventHubConnectionString,
                _eventHubName);

            try
            {
                await foreach (PartitionEvent partitionEvent in consumer.ReadEventsAsync(ct))
                {
                    partitionEvent.Data.Properties.TryGetValue("user-id", out object idString);

                    if (idString != null)
                    {
                        var id = Guid.Parse(idString.ToString());

                        if (_eventQueues.ContainsKey(id))
                        {
                            _eventQueues[id].Add(partitionEvent.Data, ct);
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
        }
    }
}
