﻿using System.Diagnostics;
using FubuTestingSupport;
using FubuTransportation.Subscriptions;
using NUnit.Framework;

namespace FubuTransportation.Testing.Subscriptions
{
    [TestFixture]
    public class Subscription_equality_tester
    {
        [Test]
        public void equals_if_all_are_equal()
        {
            var s1 = new Subscription(typeof(Message1))
            {
                NodeName = "Service1",
                Receiver = "foo://1".ToUri(),
                Source = "foo://2".ToUri(),
                Role = SubscriptionRole.Subscribes
            };

            var s2 = new Subscription(typeof(Message1))
            {
                NodeName = s1.NodeName,
                Receiver = s1.Receiver,
                Source = s1.Source,
                Role = SubscriptionRole.Subscribes
            };

            s1.ShouldEqual(s2);
            s2.ShouldEqual(s1);

            s2.NodeName = "different";
            s1.ShouldNotEqual(s2);

            s2.NodeName = s1.NodeName;
            s2.MessageType = typeof (Message2).AssemblyQualifiedName;
            s2.ShouldNotEqual(s1);

            s2.MessageType = s1.MessageType;
            s2.Receiver = "foo://3".ToUri();
            s2.ShouldNotEqual(s1);

            s2.Receiver = s1.Receiver;
            s2.Source = "foo://4".ToUri();
            s2.ShouldNotEqual(s1);

            s2.Source = s1.Source;
            s2.Role = SubscriptionRole.Publishes;
            s2.ShouldNotEqual(s1);
        }
    }
}