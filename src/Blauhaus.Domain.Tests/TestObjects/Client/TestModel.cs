﻿using System;
using System.Collections.Generic;
using System.Linq;
using Blauhaus.Domain.Abstractions.Entities;

namespace Blauhaus.Domain.Tests.TestObjects.Client
{
    public class TestModel : IClientEntity<Guid>
    {
        public TestModel(Guid id, EntityState entityState, long modifiedAtTicks, string name)
        {
            Id = id;
            EntityState = entityState;
            ModifiedAtTicks = modifiedAtTicks;
            Name = name;
        }

        public Guid Id { get; }
        public EntityState EntityState { get; }
        public long ModifiedAtTicks { get; }


        public override string ToString()
        {
            return Name;
        }

        public string Name { get; }

        public static List<TestModel> GenerateList(int numberToGenerate, DateTime start = default)
        {
            var models = new List<TestModel>();
            if (start == default) start = DateTime.UtcNow;

            for (var i = 0; i < numberToGenerate; i++)
            {
                models.Add(new TestModel(Guid.NewGuid(), EntityState.Active, start.AddDays(-i).Ticks, i.ToString()));
            }
            return models;
        }

        public static List<TestModel> GenerateOlderThan(DateTime olderThan, int numberToGenerate)
        {
            var models = new List<TestModel>();
            for (var i = 0; i < numberToGenerate; i++)
            {
                models.Add(new TestModel(Guid.NewGuid(), EntityState.Active, olderThan.AddDays(-(i+1)).Ticks, "older " + i.ToString()));
            }
            return models.OrderByDescending(x => x.ModifiedAtTicks).ToList();
        }

        public static List<TestModel> GenerateNewerThan(DateTime newerThan, int numberToGenerate)
        {
            var models = new List<TestModel>();
            for (var i = 0; i < numberToGenerate; i++)
            {
                models.Add(new TestModel(Guid.NewGuid(), EntityState.Active, newerThan.AddDays((i+1)).Ticks, "newer " + i.ToString()));
            }
            return models.OrderBy(x => x.ModifiedAtTicks).ToList();
        }
    }
}