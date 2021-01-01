using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace EventHub
{
    public class SmartEventHubConsumer
    {
        private string _eventHubConnectionString;
        private string _eventHubName;
        private ConcurrentDictionary<Guid, BlockingCollection<EventData>> _eventQueues = new ConcurrentDictionary<Guid, BlockingCollection<EventData>>();

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
            BlockingCollection<EventData> queue;
            bool result = false;

            if(_eventQueues.TryGetValue(id, out queue))
            {
                result = _eventQueues.TryRemove(id, out _);
            }

            return result;
        }


        public IEnumerable<string> GetEvents(Guid id)
        {
            BlockingCollection<EventData> queue;
            int count = 0;

            while(true)
            {
                if (_eventQueues.TryGetValue(id, out queue))
                {
                    var eventData = queue.Take();

                    if(!eventData.Body.IsEmpty)
                    {
                        //Body has data, indicating this is the result of a command that has been executed
                        count++;
                        yield return Encoding.UTF8.GetString(eventData.Body.ToArray()); ;

                        object userCount;
                        if (eventData.Properties.TryGetValue("user-command-count", out userCount) && int.Parse((string)userCount) == count)
                            break;
                    }
                    
                    object success;
                    if (eventData.Properties.TryGetValue("user-success", out success) && !bool.Parse((string)success))
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
                    object idString;
                    partitionEvent.Data.Properties.TryGetValue("user-id", out idString);

                    if (idString != null)
                    {
                        var id = Guid.Parse(idString.ToString());

                        if (_eventQueues.ContainsKey(id))
                        {
                            Debug.WriteLine($"Adding from queue with ID {id}");
                            _eventQueues[id].Add(partitionEvent.Data);
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
