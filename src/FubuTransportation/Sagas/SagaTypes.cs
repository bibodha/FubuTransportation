﻿using System;
using FubuTransportation.Registration;

namespace FubuTransportation.Sagas
{
    public class SagaTypes
    {
        public const string CorrelationId = "CorrelationId";
        public const string Id = "Id";

        public Type HandlerType;
        public Type MessageType;
        public Type StateType;

        public object ToCorrelationIdFunc()
        {
            var property = MessageType.GetProperty(CorrelationId);

            return FuncBuilder.CompileGetter(property);
        }

        public object ToSagaIdFunc()
        {
            var property = StateType.GetProperty(Id);

            return FuncBuilder.CompileGetter(property);
        }

        public bool MatchesStateIdAndMessageCorrelationIdIdiom()
        {
            return MessageType.GetProperty(CorrelationId) != null && StateType.GetProperty(Id) != null;
        }

        public override string ToString()
        {
            return string.Format("HandlerType: {0}, MessageType: {1}, StateType: {2}", HandlerType, MessageType, StateType);
        }
    }
}