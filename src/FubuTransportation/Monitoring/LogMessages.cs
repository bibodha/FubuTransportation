﻿using System;
using FubuCore;
using FubuCore.Logging;
using FubuTransportation.Configuration;
using FubuTransportation.ErrorHandling;

namespace FubuTransportation.Monitoring
{
    // TODO -- UT and register this thing
    public class PersistentTaskMessageModifier : ILogModifier
    {
        private readonly ChannelGraph _graph;

        public PersistentTaskMessageModifier(ChannelGraph graph)
        {
            _graph = graph;
        }

        public bool Matches(Type logType)
        {
            return logType is PersistentTaskMessage;
        }

        public void Modify(object log)
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public abstract class PersistentTaskMessage : LogRecord
    {
        public PersistentTaskMessage(Uri subject)
        {
            Subject = subject;
        }

        public PersistentTaskMessage()
        {
        }

        public Uri Subject { get; set; }
        public string NodeId { get; set; }
        public string Machine { get; set; }

        protected bool Equals(PersistentTaskMessage other)
        {
            return Equals(Subject, other.Subject);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PersistentTaskMessage) obj);
        }

        public override int GetHashCode()
        {
            return (Subject != null ? Subject.GetHashCode() : 0);
        }
    }

    public class TaskActivationFailure : PersistentTaskMessage
    {
        public TaskActivationFailure(Uri subject) : base(subject)
        {
        }

        public TaskActivationFailure()
        {
        }
    }

    public class TookOwnershipOfPersistentTask : PersistentTaskMessage
    {
        public TookOwnershipOfPersistentTask()
        {
        }

        public TookOwnershipOfPersistentTask(Uri subject) : base(subject)
        {
        }
    }

    public class FailedToActivatePersistentTask : PersistentTaskMessage
    {
        public FailedToActivatePersistentTask(Uri subject) : base(subject)
        {
        }

        public FailedToActivatePersistentTask()
        {
        }
    }

    public class StoppedTask : PersistentTaskMessage
    {
        public StoppedTask(Uri subject) : base(subject)
        {
        }

        public StoppedTask()
        {
        }
    }

    public class FailedToStopTask : PersistentTaskMessage
    {
        public FailedToStopTask(Uri subject) : base(subject)
        {
        }

        public FailedToStopTask()
        {
        }
    }

    public class TaskAvailabilityFailed : PersistentTaskMessage
    {

        public TaskAvailabilityFailed(Uri subject) : base(subject)
        {

        }

        public string ExceptionText { get; set; }

        public string ExceptionType { get; set; }

        public TaskAvailabilityFailed()
        {
        }
    }
}