using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace HelloRedis
{
    class Program
    {
        private static ConnectionMultiplexer connection;
        private static string connectionString = "<connStr>";

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
