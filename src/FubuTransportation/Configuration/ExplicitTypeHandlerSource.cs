﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace FubuTransportation.Configuration
{
    public class ExplicitTypeHandlerSource : IHandlerSource
    {
        private readonly IList<Type> _types = new List<Type>();

        public ExplicitTypeHandlerSource(params Type[] types)
        {
            _types.Fill(types);
        }

        public IEnumerable<HandlerCall> FindCalls()
        {
            foreach (var type in _types)
            {
                foreach (var method in type.GetMethods().Where(HandlerCall.IsCandidate))
                {
                    yield return new HandlerCall(type, method);
                }
            }
        }
    }
}