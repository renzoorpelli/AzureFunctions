using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Entidades.Domain.DTO
{
    public class VueloDTO
    {
        [JsonPropertyName("Id")]
        public int Id { get; set; }

        [JsonPropertyName("NumeroVuelo")]
        public string NumeroVuelo { get; set; }

        [JsonPropertyName("FechaVuelo")]
        public DateTime FechaVuelo { get; set; }
    }
}
