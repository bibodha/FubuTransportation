﻿using System.Linq;
using Bottles.Services;
using Bottles.Services.Messaging.Tracking;
using StoryTeller.Engine;
using Wait = Serenity.Wait;

namespace FubuTransportation.Serenity
{
    public abstract class FubuTransportActFixture : Fixture
    {
        public int TimeoutInMilliseconds = 10000;

        public sealed override void SetUp(ITestContext context)
        {
            MessageHistory.ClearAll();
            setup();
        }

        protected virtual void setup()
        {
            
        }

        public sealed override void TearDown()
        {
            teardown();
            Wait.Until(() => !MessageHistory.Outstanding().Any() && MessageHistory.All().Any(), timeoutInMilliseconds: TimeoutInMilliseconds);
        }

        protected virtual void teardown()
        {
            
        }
    }
}