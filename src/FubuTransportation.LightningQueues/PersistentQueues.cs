using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FubuCore.Logging;
using FubuCore.Util;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Delayed;
using LightningQueues;
using LightningQueues.Model;

namespace FubuTransportation.LightningQueues
{
    public class PersistentQueues : IPersistentQueues
    {
        private readonly ILogger _logger;
        private readonly IDelayedMessageCache<MessageId> _delayedMessages;
        public const string EsentPath = "fubutransportation.esent";

        private readonly Cache<IPEndPoint, QueueManager> _queueManagers;

        public PersistentQueues(ILogger logger, IDelayedMessageCache<MessageId> delayedMessages)
        {
            _logger = logger;
            _delayedMessages = delayedMessages;
            _queueManagers = new Cache<IPEndPoint, QueueManager>(ip => new QueueManager(ip, EsentPath + "." + ip.Port, new QueueManagerConfiguration(), _logger));
        }

        public void Dispose()
        {
            _queueManagers.Each(x => x.Dispose());
        }

        public IQueueManager ManagerFor(IPEndPoint endpoint)
        {
            return _queueManagers[endpoint];
        }

        public void Start(IEnumerable<LightningUri> uriList)
        {
            uriList.GroupBy(x => x.Endpoint).Each(group =>
            {
                string[] queueNames = group.Select(x => x.QueueName).ToArray();

                var queueManager = _queueManagers[@group.Key];
                queueManager.CreateQueues(queueNames);
                queueManager.CreateQueues(LightningQueuesTransport.DelayedQueueName);
                queueManager.CreateQueues(LightningQueuesTransport.ErrorQueueName);

                queueManager.Start();
            });
        }

        public void CreateQueue(LightningUri uri)
        {
            _queueManagers[uri.Endpoint].CreateQueues(uri.QueueName);
        }

        public IEnumerable<EnvelopeToken> ReplayDelayed(DateTime currentTime)
        {
            return _queueManagers.SelectMany(x => ReplayDelayed(x, currentTime));
        }

        public IEnumerable<EnvelopeToken> ReplayDelayed(QueueManager queueManager, DateTime currentTime)
        {
            var list = new List<EnvelopeToken>();

            var transactionalScope = queueManager.BeginTransactionalScope();
            try
            {
                var readyToSend = _delayedMessages.AllMessagesBefore(currentTime);
                readyToSend.Each(x =>
                {
                    var message = transactionalScope.ReceiveById(LightningQueuesTransport.DelayedQueueName, x);
                    var uri = message.Headers[Envelope.ReceivedAtKey].ToLightningUri();
                    transactionalScope.EnqueueDirectlyTo(uri.QueueName, message.ToPayload());
                    list.Add(message.ToToken());
                });
                transactionalScope.Commit();
            }

            catch (Exception e)
            {
                transactionalScope.Rollback();
                _logger.Error("Error trying to move delayed messages back to the original queue", e);
            }

            return list;
        }
    }
}