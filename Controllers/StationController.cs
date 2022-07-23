using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestApi.Models;
using TestApi.Redis;
using TradevanPackage.SqlServer;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StationController : ControllerBase
    {
        private readonly StationRepository Repo;
        private readonly RedisHaFactory Redis;

        public StationController(StationRepository repo, RedisHaFactory redis)
        {
            Repo = repo;
            Redis = redis;
        }

        // GET: api/<StationController>
        [HttpGet]
        public List<Station> Get()
        {
            if (Redis.ListCount("station") > 0)
                return Redis.GetStationList("station");

            return Repo.GetStations();
        }

        // POST api/<StationController>
        [HttpPost]
        public string Post([FromBody] Station entity)
        {
            var result = Repo.CreateStation(entity);

            var list = Repo.GetStations();

            Redis.InsertBy_CreateBatch(list, "station");

            return result ? "Success" : "Failed";
        }

    }
}
