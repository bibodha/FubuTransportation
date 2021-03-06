﻿using System;
using Bottles.Services.Messaging.Tracking;
using FubuCore;
using FubuTransportation.Events;
using FubuTransportation.Logging;
using FubuTransportation.Runtime;

namespace FubuTransportation.TestSupport
{
    public class MessageWatcher : IListener
        , IListener<ChainExecutionStarted>
        , IListener<ChainExecutionFinished>
        , IListener<EnvelopeSent>
        , IListener<MessageSuccessful>
        , IListener<MessageFailed>
    {
        public static readonly string MessageTrackType = "Handler Chain Execution";

        public void Handle(ChainExecutionFinished message)
        {
            if (message.Envelope.IsPollingJobRelated()) return;

            MessageTrack track = MessageTrack.ForReceived(message, message.Envelope.CorrelationId);
            track.Type = track.FullName = MessageTrackType;

            Bottles.Services.Messaging.EventAggregator.SendMessage(track);
        }

        public void Handle(ChainExecutionStarted message)
        {
            if (message.Envelope.IsPollingJobRelated()) return;

            MessageTrack track = MessageTrack.ForSent(message, message.Envelope.CorrelationId);
            track.Type = track.FullName = MessageTrackType;

            Bottles.Services.Messaging.EventAggregator.SendMessage(track);
        }

        public void Handle(EnvelopeSent message)
        {
            handle(message.Envelope, MessageTrack.Sent, message.Uri);
        }

        public void Handle(MessageFailed message)
        {
            handle(message.Envelope, MessageTrack.Received, message.Envelope.Destination);
        }

        public void Handle(MessageSuccessful message)
        {
            handle(message.Envelope, MessageTrack.Received, message.Envelope.Destination);
        }

        private void handle(EnvelopeToken envelope, string status, Uri uri)
        {
            if (envelope.IsPollingJobRelated()) return;

            var track = new MessageTrack
            {
                Type = "OutstandingEnvelope",
                Id = envelope.CorrelationId,
                FullName = "{0}@{1}".ToFormat(envelope.CorrelationId, uri),
                Status = status
            };

            Bottles.Services.Messaging.EventAggregator.SendMessage(track);
        }
    }
}