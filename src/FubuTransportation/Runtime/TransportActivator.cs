﻿using System.Collections.Generic;
using Bottles;
using Bottles.Diagnostics;
using FubuCore;
using FubuTransportation.Configuration;

namespace FubuTransportation.Runtime
{
    public class TransportActivator : IActivator
    {
        private readonly ChannelGraph _graph;
        private readonly IServiceLocator _services;
        private readonly ISubscriptions _subscriptions;

        public TransportActivator(ChannelGraph graph, IServiceLocator services, ISubscriptions subscriptions)
        {
            _graph = graph;
            _services = services;
            _subscriptions = subscriptions;
        }

        public void Activate(IEnumerable<IPackageInfo> packages, IPackageLog log)
        {
            _graph.ReadSettings(_services);
            _subscriptions.Start();
        }
    }
}