﻿using System;
using FubuCore;
using FubuMVC.Core;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Querying;
using FubuTransportation.Configuration;
using FubuTransportation.Testing.Runtime;
using NUnit.Framework;
using System.Linq;
using FubuTestingSupport;
using System.Collections.Generic;

namespace FubuTransportation.Testing
{
    [TestFixture]
    public class FubuTransportRegistry_application_of_policies_Tester
    {
        

        [Test]
        public void policies_can_be_applied_locally()
        {
            var graph = BehaviorGraph.BuildFrom(x => {
                x.Import<BlueRegistry>();
                x.Import<GreenRegistry>();
                x.Policies.Add<ImportHandlers>(); // This would be here by Bottles normally
            });

            var cache = new ChainResolutionCache(new TypeResolver(), graph);

            cache.FindUniqueByType(typeof(Message1)).IsWrappedBy(typeof(GreenWrapper)).ShouldBeTrue();
            cache.FindUniqueByType(typeof(Message2)).IsWrappedBy(typeof(GreenWrapper)).ShouldBeTrue();
            cache.FindUniqueByType(typeof(Message3)).IsWrappedBy(typeof(GreenWrapper)).ShouldBeTrue();

            cache.FindUniqueByType(typeof(Message4)).IsWrappedBy(typeof(GreenWrapper)).ShouldBeFalse();
            cache.FindUniqueByType(typeof(Message5)).IsWrappedBy(typeof(GreenWrapper)).ShouldBeFalse();
            cache.FindUniqueByType(typeof(Message6)).IsWrappedBy(typeof(GreenWrapper)).ShouldBeFalse();

            cache.FindUniqueByType(typeof(Message1)).IsWrappedBy(typeof(BlueWrapper)).ShouldBeFalse();
            cache.FindUniqueByType(typeof(Message2)).IsWrappedBy(typeof(BlueWrapper)).ShouldBeFalse();
            cache.FindUniqueByType(typeof(Message3)).IsWrappedBy(typeof(BlueWrapper)).ShouldBeFalse();

            cache.FindUniqueByType(typeof(Message4)).IsWrappedBy(typeof(BlueWrapper)).ShouldBeTrue();
            cache.FindUniqueByType(typeof(Message5)).IsWrappedBy(typeof(BlueWrapper)).ShouldBeTrue();
            cache.FindUniqueByType(typeof(Message6)).IsWrappedBy(typeof(BlueWrapper)).ShouldBeTrue();
        }

        [Test]
        public void global_policies_on_all_handlers()
        {
            var graph = BehaviorGraph.BuildFrom(x =>
            {
                x.Import<BlueRegistry>();
                x.Import<GreenRegistry>();
                x.Import<RedRegistry>();

                x.Policies.Add<ImportHandlers>(); // This would be here by Bottles normally
            });

            graph.Behaviors.Any(x => x is HandlerChain).ShouldBeTrue();
            graph.Behaviors.OfType<HandlerChain>().Each(chain => {
                chain.IsWrappedBy(typeof (RedWrapper)).ShouldBeTrue();
            });
        }

        [Test]
        public void does_not_apply_policies_to_fubumvc_handlers()
        {
            var graph = BehaviorGraph.BuildFrom(x =>
            {
                x.Import<BlueRegistry>();
                x.Import<GreenRegistry>();
                x.Import<RedRegistry>();

                x.Policies.Add<ImportHandlers>(); // This would be here by Bottles normally
            });

            graph.BehaviorFor<SomethingEndpoint>(x => x.get_hello())
                .IsWrappedBy(typeof(RedWrapper))
                .ShouldBeFalse();
        }
    }

    public class RedRegistry : FubuTransportRegistry
    {
        public RedRegistry()
        {
            Policies.Global<WrapPolicy<RedWrapper>>();
        }
    }

    public class GreenRegistry : FubuTransportRegistry
    {
        public GreenRegistry()
        {
            Handlers.Include<GreenHandler>();
            Policies.Local<WrapPolicy<GreenWrapper>>();
        }
    }

    public class BlueRegistry : FubuTransportRegistry
    {
        public BlueRegistry()
        {
            Handlers.Include<BlueHandler>();
            Policies.Local<WrapPolicy<BlueWrapper>>();
        }
    }

    public class GreenHandler
    {
        public void Handle(Message1 message){}
        public void Handle(Message2 message){}
        public void Handle(Message3 message){}
    }

    public class BlueHandler
    {
        public void Handle(Message4 message) { }
        public void Handle(Message5 message) { }
        public void Handle(Message6 message) { }
    }

    public class WrapPolicy<T> : Policy where T : IActionBehavior
    {
        public WrapPolicy()
        {
            Wrap.WithBehavior<T>();
        }
    }

    public class GreenWrapper : WrappingBehavior
    {
        protected override void invoke(Action action)
        {
            action();
        }
    }

    public class BlueWrapper : WrappingBehavior
    {
        protected override void invoke(Action action)
        {
            action();
        }
    }

    public class RedWrapper : WrappingBehavior
    {
        protected override void invoke(Action action)
        {
            action();
        }
    }

    public class FakeBehavior4 : WrappingBehavior
    {
        protected override void invoke(Action action)
        {
            action();
        }
    }

    public class SomethingEndpoint
    {
        public string get_hello()
        {
            return "hello";
        }
    }
}