using System;

namespace TestApi.Models
{
    public class Station
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime LastModifiedDate { get; set; }
    }
}
