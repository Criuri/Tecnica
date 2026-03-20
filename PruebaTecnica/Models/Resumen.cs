using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PruebaTecnica.Models
{
    public class Resumen
    {
        public Int32 TotalRegistros { get; set; }
        public Int32 RegistrosValidos { get; set; }
        public Int32 RegistrosRechazados { get; set; }
        public String ArchivoRechazados { get; set; }
        public List<Dictionary<String, Object>> TotalCashValue { get; set; }
        public List<Dictionary<String, Object>> TotalEfectivo { get; set; }

    }
}
