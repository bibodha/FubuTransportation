﻿using System;
using System.Linq;
using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Core.Registration.ObjectGraph;
using FubuMVC.StructureMap;
using FubuTransportation.Configuration;
using FubuTransportation.InMemory;
using FubuTransportation.Registration.Nodes;
using FubuTransportation.Sagas;
using FubuTransportation.Testing.ScenarioSupport;
using NUnit.Framework;
using FubuTestingSupport;
using Rhino.Mocks;
using StructureMap;

namespace FubuTransportation.Testing.Sagas
{
    [TestFixture]
    public class StatefulSagaConventionTester
    {
        [Test]
        public void is_saga_handler_positive()
        {
            StatefulSagaConvention.IsSagaHandler(HandlerCall.For<SimpleSagaHandler>(x => x.Start(null)))
                .ShouldBeTrue();

            StatefulSagaConvention.IsSagaHandler(HandlerCall.For<SimpleSagaHandler>(x => x.Second(null)))
                .ShouldBeTrue();

            StatefulSagaConvention.IsSagaHandler(HandlerCall.For<SimpleSagaHandler>(x => x.Last(null)))
                .ShouldBeTrue();
        }

        [Test]
        public void is_saga_handler_negative()
        {
            StatefulSagaConvention.IsSagaHandler(HandlerCall.For<SimpleHandler<OneMessage>>(x => x.Handle(null)))
                .ShouldBeFalse();
        }

        [Test]
        public void is_saga_chain_is_false_for_regular_behavior_chain()
        {
            StatefulSagaConvention.IsSagaChain(new BehaviorChain())
                .ShouldBeFalse();
        }

        [Test]
        public void is_saga_chain_is_false_for_handler_chain_with_no_saga_handlers()
        {
            var call = HandlerCall.For<SimpleHandler<OneMessage>>(x => x.Handle(null));

            var chain = new HandlerChain();
            chain.AddToEnd(call);

            StatefulSagaConvention.IsSagaChain(chain)
                .ShouldBeFalse();
        }

        [Test]
        public void is_saga_chain_is_true_for_handler_chain_with_a_saga_handler()
        {
            var call = HandlerCall.For<SimpleSagaHandler>(x => x.Last(null));

            var chain = new HandlerChain();
            chain.AddToEnd(call);

            StatefulSagaConvention.IsSagaChain(chain)
                .ShouldBeTrue();
        }

        [Test]
        public void to_saga_types_for_a_handler_call()
        {
            var call = HandlerCall.For<SimpleSagaHandler>(x => x.Last(null));
            var types = StatefulSagaConvention.ToSagaTypes(call);

            types.HandlerType.ShouldEqual(typeof (SimpleSagaHandler));
            types.MessageType.ShouldEqual(typeof (SagaMessageThree));
            types.StateType.ShouldEqual(typeof (MySagaState));
        }


        [Test]
        public void no_registered_storage_and_matches_the_idiom_so_use_in_memory_cache()
        {
            var types = new SagaTypes
            {
                MessageType = typeof (SagaMessageOne),
                StateType = typeof (MySagaState)
            };

            StatefulSagaConvention.DetermineSagaRepositoryDef(new TransportSettings(), types)
                                  .Type.ShouldEqual(typeof (InMemorySagaRepository<MySagaState, SagaMessageOne>));
        }

        [Test]
        public void use_matching_storage_to_build_the_repository()
        {
            var types = new SagaTypes
            {
                MessageType = GetType(),
                StateType = typeof (MySagaState)
            };

            var storage1 = MockRepository.GenerateMock<ISagaStorage>();
            var storage2 = MockRepository.GenerateMock<ISagaStorage>();
            var storage3 = MockRepository.GenerateMock<ISagaStorage>();

            var settings = new TransportSettings();
            settings.SagaStorageProviders.Add(storage1);
            settings.SagaStorageProviders.Add(storage2);
            settings.SagaStorageProviders.Add(storage3);

            var def = new ObjectDef();
            storage3.Stub(x => x.RepositoryFor(types))
                    .Return(def);

            StatefulSagaConvention.DetermineSagaRepositoryDef(settings, types)
                                  .ShouldBeTheSameAs(def);
        }

        [Test]
        public void use_matching_storage_to_build_the_repository_2()
        {
            var types = new SagaTypes
            {
                MessageType = GetType(),
                StateType = typeof(MySagaState)
            };

            var storage1 = MockRepository.GenerateMock<ISagaStorage>();
            var storage2 = MockRepository.GenerateMock<ISagaStorage>();
            var storage3 = MockRepository.GenerateMock<ISagaStorage>();

            var settings = new TransportSettings();
            settings.SagaStorageProviders.Add(storage1);
            settings.SagaStorageProviders.Add(storage2);
            settings.SagaStorageProviders.Add(storage3);

            var def = new ObjectDef();
            storage2.Stub(x => x.RepositoryFor(types))
                    .Return(def);

            StatefulSagaConvention.DetermineSagaRepositoryDef(settings, types)
                                  .ShouldBeTheSameAs(def);
        }


        [Test]
        public void use_matching_storage_to_build_the_repository_3()
        {
            var types = new SagaTypes
            {
                MessageType = GetType(),
                StateType = typeof(MySagaState)
            };

            var storage1 = MockRepository.GenerateMock<ISagaStorage>();
            var storage2 = MockRepository.GenerateMock<ISagaStorage>();
            var storage3 = MockRepository.GenerateMock<ISagaStorage>();

            var settings = new TransportSettings();
            settings.SagaStorageProviders.Add(storage1);
            settings.SagaStorageProviders.Add(storage2);
            settings.SagaStorageProviders.Add(storage3);

            var def = new ObjectDef();
            storage1.Stub(x => x.RepositoryFor(types))
                    .Return(def);

            StatefulSagaConvention.DetermineSagaRepositoryDef(settings, types)
                                  .ShouldBeTheSameAs(def);
        }

        [Test]
        public void unable_to_determine_a_saga_repository_blows_up()
        {
            Exception<SagaRepositoryUnresolvableException>.ShouldBeThrownBy(() => {
                StatefulSagaConvention.DetermineSagaRepositoryDef(new TransportSettings(), new SagaTypes
                {
                    MessageType = GetType(),
                    StateType = GetType()
                });
            });
        }
    }

    [TestFixture]
    public class when_using_a_custom_saga_storage
    {
        [Test]
        public void use_the_special_storage_just_fine()
        {
            var graph = FubuTransportRegistry.HandlerGraphFor(x => {
                x.SagaStorage<SpecialSagaStorage>();
            });

            var chain = graph.ChainFor(typeof (SagaMessageOne));
            chain.OfType<StatefulSagaNode>()
                .FirstOrDefault()  // there are two saga's using this message, just worry about the first one
                .Repository.Type.ShouldEqual(typeof (SpecialSagaRepository<MySagaState, SagaMessageOne>));
        }
    }

    public class SpecialSagaStorage : ISagaStorage
    {
        public ObjectDef RepositoryFor(SagaTypes sagaTypes)
        {
            return new ObjectDef(typeof(SpecialSagaRepository<,>), sagaTypes.StateType, sagaTypes.MessageType);
        }
    }

    public class SpecialSagaRepository<TState, TMessage> : ISagaRepository<TState, TMessage>
    {
        public void Save(TState state, TMessage message)
        {
            throw new NotImplementedException();
        }

        public TState Find(TMessage message)
        {
            throw new NotImplementedException();
        }

        public void Delete(TState state, TMessage message)
        {
            throw new NotImplementedException();
        }
    }

    public class MySagaState
    {
        public Guid Id { get; set; }
    }

    public class SagaMessageOne
    {
        public Guid CorrelationId { get; set; }
    }

    public class SagaMessageTwo
    {
        public Guid CorrelationId { get; set; }
    }

    public class SagaMessageThree
    {
        public Guid CorrelationId { get; set; }
    }

    public class SimpleSagaHandler : IStatefulSaga<MySagaState>
    {
        public bool IsCompleted()
        {
            throw new NotImplementedException();
        }

        public MySagaState State { get; set; }

        public void Start(SagaMessageOne one)
        {
            
        }

        public void Second(SagaMessageTwo two)
        {
            
        }

        public void Last(SagaMessageThree three)
        {
            
        }
    }
}