using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TestApi.Models;

namespace TestApi.Redis
{
    public  class RedisHaFactory
    {
        public readonly Lazy<ConnectionMultiplexer> Connection;

        /// <summary>
        /// Use EndPoint to connection.
        /// </summary>
        public RedisHaFactory(IConfiguration configuration)
        {
            var list = new List<string>();
            configuration.GetSection("RedisSetting:ConnectionStrings").Bind(list);


            ConfigurationOptions options = new ConfigurationOptions
            {
                 //定義使用的資料庫
                DefaultDatabase =0
            };

            foreach (var item in list)
                options.EndPoints.Add(item);

            Connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(options));
        }

        public ConnectionMultiplexer GetConnection => Connection.Value;

        public IDatabase RedisDB => GetConnection.GetDatabase();


        public bool Lock(string key, TimeSpan expiry)
        {
            // Lock失敗就等200毫秒，再重試，最多10次
            var number = 0;
            do
            {
                try
                {
                    var lockKey = $"Lock_{key}";
                    if (RedisDB.LockTake(lockKey, Environment.MachineName, expiry, CommandFlags.DemandMaster))
                    {
                        return true;
                    }
                    else
                        throw new Exception();
                }
                catch (Exception)
                {
                    Task.Delay(200);
                    number++;
                }
            } while (number < 10);

            return false;
        }

        public bool LockRelease(string key)
        {
            var lockKey = $"Lock_{key}";
            return RedisDB.LockRelease(lockKey, Environment.MachineName, CommandFlags.DemandMaster);
        }

        public async Task SetString(string key, RedisValue value)
        {
            await RedisDB.StringSetAsync(key, value);
        }

        public async Task<RedisValue> GetString(string key)
        {
            return await RedisDB.StringGetAsync(key,CommandFlags.PreferReplica);
        }


        public async Task<long> StringDecrement(string key)
        {
            return await RedisDB.StringDecrementAsync(key);
        }
        private string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        private T Deserialize<T>(string serialized)
        {
            return JsonConvert.DeserializeObject<T>(serialized);
        }

        public void Add(Station item,string key)
        {
            RedisDB.ListRightPush(key, Serialize(item));
        }

        public void InsertBy_CreateBatch(IEnumerable<Station> input, string key)
        {
            if (ListCount(key) > 0 && Lock(key, TimeSpan.FromSeconds(60)))
            {
                try
                {
                    var batch = RedisDB.CreateBatch();

                    if (ListCount(key) > 0)
                        Clear(key);

                    foreach (var entity in input)
                        RedisDB.ListRightPush(key, Serialize(entity));

                    batch.Execute();
                }
                finally
                {
                    // 完成後要把Lock釋放
                   //LockRelease(key);
                }
            }           
        }

        public void Clear(string key)
        {
            RedisDB.KeyDelete(key);
        }

        public List<Station> GetStationList(string key)
        {
            var result = new List<Station>();
            for (int i = 0; i < ListCount(key); i++)
                result.Add(Deserialize<Station>(RedisDB.ListGetByIndex(key, i, CommandFlags.DemandReplica).ToString()));

            return result;
        }

        public int ListCount(string key)
        {
            return (int)RedisDB.ListLength(key, CommandFlags.DemandReplica);
        }
    }
}
