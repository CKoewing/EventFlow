﻿using System;
using EventFlow.Configuration;
using EventFlow.TestHelpers;
using EventFlow.TestHelpers.Suites;
using EventFlow.Extensions;
using EventFlow.MongoDB.EventStore;
using EventFlow.MongoDB.Extensions;
using EventFlow.MongoDB.ValueObjects;
using NUnit.Framework;
using Mongo2Go;
using MongoDB.Driver;

namespace EventFlow.MongoDB.Tests.IntegrationTests.EventStores
{
	[Category(Categories.Integration)]
	[TestFixture]
	[NUnit.Framework.Timeout(30000)]
    public class MongoDbEventStoreTests : TestSuiteForEventStore
	{
		private MongoDbRunner _runner;
		
		protected override IRootResolver CreateRootResolver(IEventFlowOptions eventFlowOptions)
		{
		    _runner = MongoDbRunner.Start();
            var resolver = eventFlowOptions
				.ConfigureMongoDb(_runner.ConnectionString, "eventflow")
				.UseEventStore<MongoDbEventPersistence>()
				.CreateResolver();
		    PrepareIndexes(resolver);
			return resolver;
		}

	    private void PrepareIndexes(IRootResolver resolver)
	    {
	        var mongo = resolver.Resolve<IMongoDatabase>();
	        var events = mongo.GetCollection<MongoDbEventDataModel>("eventflow.events");
	        IndexKeysDefinition<MongoDbEventDataModel> keys =
	            Builders<MongoDbEventDataModel>.IndexKeys.Ascending("AggregateId")
	                .Ascending("AggregateSequenceNumber");
	        events.Indexes.CreateOne(
	            new CreateIndexModel<MongoDbEventDataModel>(keys, new CreateIndexOptions {Unique = true}));
	    }

        [TearDown]
		public void TearDown()
		{
			_runner.Dispose();
		}
	}
}
