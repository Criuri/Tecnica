using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PruebaTecnica.Models
{
    public class Resultado
    {
        public List<Dictionary<String, String>> registrosValidos { get; set; }
        public List<Rechazados> RegistrosRechazados { get; set; }
        public String ArchivoRecazados { get; set; }
    }
}
