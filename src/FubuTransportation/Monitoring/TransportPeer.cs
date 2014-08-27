﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FubuTransportation.Subscriptions;

namespace FubuTransportation.Monitoring
{
    public class TransportPeer : ITransportPeer
    {
        private readonly TransportNode _node;
        private readonly ISubscriptionRepository _subscriptions;
        private readonly IServiceBus _serviceBus;

        public TransportPeer(TransportNode node, ISubscriptionRepository subscriptions, IServiceBus serviceBus)
        {
            _node = node;
            _subscriptions = subscriptions;
            _serviceBus = serviceBus;

            if (!_node.Addresses.Any())
            {
                throw new ArgumentOutOfRangeException("node", "The TransportNode must have at least one reply Uri");
            }
        }

        public Task<OwnershipStatus> TakeOwnership(Uri subject)
        {
            return _serviceBus.Request<TakeOwnershipResponse>(new TakeOwnershipRequest(subject),
                new RequestOptions {Destination = _node.Addresses.FirstOrDefault()})
                .ContinueWith(t => {
                    var ownershipStatus = t.Result.Status;

                    if (ownershipStatus == OwnershipStatus.AlreadyOwned ||
                        ownershipStatus == OwnershipStatus.OwnershipActivated)
                    {
                        _node.AddOwnership(subject);
                        _subscriptions.Persist(_node);
                    }

                    return ownershipStatus;
                });
        }

        public Task<TaskHealthResponse> CheckStatusOfOwnedTasks()
        {
            var request = new TaskHealthRequest
            {
                Subjects = CurrentlyOwnedSubjects().ToArray()
            };

            return _serviceBus.Request<TaskHealthResponse>(request, new RequestOptions
            {
                // TODO -- this is smelly. Introduce the idea of a "control" queue?
                Destination = _node.Addresses.FirstOrDefault()
            });
        }

        public IEnumerable<Uri> CurrentlyOwnedSubjects()
        {
            return _node.OwnedTasks;
        }


        public string NodeId
        {
            get
            {
                return _node.Id;
            }
        }

        public string MachineName
        {
            get
            {
                return _node.MachineName;
            }
        }

        public IEnumerable<Uri> ReplyAddresses
        {
            get
            {
                return _node.Addresses;
            }
        }
    }
}