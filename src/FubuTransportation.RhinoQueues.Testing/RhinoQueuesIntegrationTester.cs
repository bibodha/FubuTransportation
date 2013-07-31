﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FubuCore;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using NUnit.Framework;

namespace FubuTransportation.RhinoQueues.Testing
{
    [TestFixture]
    public class RhinoQueuesIntegrationTester
    {
        [SetUp]
        public void Setup()
        {
            if (Directory.Exists(PersistentQueues.EsentPath))
                Directory.Delete(PersistentQueues.EsentPath, true);
            if (Directory.Exists("test.esent"))
                Directory.Delete("test.esent", true);

            graph = new ChannelGraph();
            node = graph.ChannelFor<ChannelSettings>(x => x.Upstream);
            node.Uri = new Uri("rhino.queues://localhost:2020/upstream");
            node.Incoming = true;

            queues = new PersistentQueues();
            transport = new RhinoQueuesTransport(queues);

            transport.OpenChannels(graph);
        }

        private PersistentQueues queues;
        private RhinoQueuesTransport transport;
        private ChannelGraph graph;
        private ChannelNode node;


        [Test]
        [Platform(Exclude = "Mono", Reason = "Esent won't work on linux / mono")]
        public void send_a_message_and_get_it_back()
        {
            var envelope = new Envelope(null) {Data = new byte[] {1, 2, 3, 4, 5}};
            envelope.Headers["foo"] = "bar";

            var receiver = new StubReceiver();
            node.Channel.StartReceiving(receiver, new ChannelNode());

            node.Channel.As<RhinoQueuesChannel>().Send(envelope);
            Wait.Until(() => receiver.Received.Any());


            graph.Each(x => x.Channel.Dispose());
            queues.Dispose();

            receiver.Received.Any().ShouldBeTrue();

            Envelope actual = receiver.Received.Single();
            actual.Data.ShouldEqual(envelope.Data);
            actual.Headers["foo"].ShouldEqual("bar");
        }
    }

    public class ChannelSettings
    {
        public Uri Outbound { get; set; }
        public Uri Downstream { get; set; }
        public Uri Upstream { get; set; }
    }
}