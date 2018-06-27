using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace HelloRedis
{
    class Program
    {
        private static ConnectionMultiplexer connection;
        private static string connectionString = "packtredis.redis.cache.windows.net:6380,password=S48BKrl1Z8l1xhH1vcgv+2fiOps0c2mTcvLq3ABKxfk=,ssl=True,abortConnect=False";

        private static IDatabase database;

        static void Main(string[] args)
        {
            connection = ConnectionMultiplexer.ConnectAsync(connectionString).Result;
            database = connection.GetDatabase(0);

            saveMessage().Wait();
            fetchMessage().Wait();

            Console.WriteLine("Press any key to continue...");
            Console.Read();
        }
        
        private static async Task saveMessage()
        {
            await database.StringSetAsync("msg", "World!");
        }

        private static async Task fetchMessage()
        {
            var msg = await database.StringGetAsync("msg");
            Console.WriteLine($"Hello, {msg}");
        }
    }
}
