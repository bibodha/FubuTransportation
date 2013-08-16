﻿using System;
using System.Reflection;
using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Core.Registration.ObjectGraph;

namespace FubuTransportation.Sagas
{
    public interface IStatefulSaga<TState>
    {
        TState State { get; set; }
        bool IsCompleted();
    }

    // I know I didn't like the sound of the double generics before,
    // but I think it makes sense.  This lets us put the responsibility 
    // for resolving the TMessage from IFubuRequest into the SagaBehavior
    // instead of each SagaRepository
    // Also think you need the TState in the signature for Raven to determine how
    // it's going to resolve the TState by it's Id
    public interface ISagaRepository<TState, TMessage>
    {
        void Save(TState state);
        TState Find(TMessage message);
        void Delete(TState state);
    }

    public class StatefulSagaNode : BehaviorNode
    {
        private readonly SagaTypes _types;

        public StatefulSagaNode(SagaTypes types)
        {
            _types = types;
        }

        public ObjectDef Repository { get; set; }

        public override BehaviorCategory Category
        {
            get { return BehaviorCategory.Wrapper; }
        }

        public Type StateType
        {
            get { return _types.StateType; }
        }

        public Type MessageType
        {
            get { return _types.MessageType; }
        }

        protected override ObjectDef buildObjectDef()
        {
            if (Repository == null)
            {
                throw new InvalidOperationException(
                    "something descriptive here saying you don't know how to do the repo for the saga");
            }

            var def = new ObjectDef(typeof (SagaBehavior<,,>), _types.StateType, _types.MessageType, _types.HandlerType);
            var repositoryType = typeof (ISagaRepository<,>).MakeGenericType(_types.StateType, _types.MessageType);
            def.Dependency(repositoryType, Repository);

            return def;
        }
    }

    public interface ISagaStorage
    {
        /// <summary>
        /// Can be null!
        /// </summary>
        /// <param name="sagaTypes"></param>
        /// <returns></returns>
        ObjectDef RepositoryFor(SagaTypes sagaTypes);
    }
}