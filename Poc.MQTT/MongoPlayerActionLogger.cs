using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MongoDB.Driver;
using Prometheus;

namespace Poc.MQTT
{
    public class MongoPlayerActionLogger : IPlayerActionLogger
    {
        private static readonly Histogram InsertionDuration = Metrics.CreateHistogram("InsertionDuration", "Times it took to insert one element", new HistogramConfiguration()
        {
            LabelNames = new[] { "logType" },
        });

        private readonly IMongoDatabase _database;

        public MongoPlayerActionLogger(IMongoClient client)
        {
            _database = client.GetDatabase("playerActions");
        }

        public async Task SaveAsync<T>(T action) where T : IPlayerAction
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            using (InsertionDuration.WithLabels(typeof(T).Name).NewTimer())
            {
                await _database.GetCollection<T>(typeof(T).Name).InsertOneAsync(action);
            }

            stopWatch.Stop();

            Console.WriteLine($"{typeof(T).Name} log inserted in {stopWatch.ElapsedMilliseconds}ms");
        }
    }
}