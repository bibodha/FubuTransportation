﻿using System.Collections.Generic;
using System.Linq;
using FubuTransportation.Diagnostics;
using HtmlTags;
using Serenity;

namespace FubuTransportation.Serenity
{
    public class MessageContextualInfoProvider : IContextualInfoProvider
    {
        private readonly IMessagingSession _session;

        public MessageContextualInfoProvider(IMessagingSession session)
        {
            _session = session;
        }

        public void Reset()
        {
            _session.ClearAll();
        }

        public IEnumerable<HtmlTag> GenerateReports()
        {
            yield return new HtmlTag("h3").Text("Message History");

            yield return new HtmlTag("ol", x =>
            {
                foreach (MessageHistory topLevelMessage in _session.TopLevelMessages().ToList())
                {
                    x.Append(topLevelMessage.ToLeafTag());
                }
            });

            foreach (MessageHistory history in _session.AllMessages().ToList())
            {
                yield return new MessageHistoryTableTag(history);
            }
        }
    }

}