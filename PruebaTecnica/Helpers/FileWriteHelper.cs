using PruebaTecnica.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PruebaTecnica.Helpers
{
    public class FileWriteHelper
    {

        public virtual void GuardarResumen(Resumen estadisticas, String archivo, String entrada)
        {
            var resumen = new
            {
                Titulo = "Resumen de Procesado",
                TotalRegistros = estadisticas.TotalRegistros,
                RegistrosValidos = estadisticas.RegistrosValidos,
                RegistrosRechazados = estadisticas.RegistrosRechazados,
                NombreArchivoRechazado = estadisticas.ArchivoRechazados,
                CashValuePorFecha = estadisticas.TotalCashValue,
                TotalCajerosEstados = estadisticas.TotalEfectivo
            };
            string json = JsonSerializer.Serialize(resumen, new JsonSerializerOptions
            {
                WriteIndented = true 
            });

            File.WriteAllText(archivo, json, Encoding.UTF8);
        }



        public virtual void GuardarRechazados(List<Rechazados> rechazados, String archivo)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Número de la fila original|Mensaje de error o de validación fallida");
            foreach (var rech in rechazados)
            {
                sb.AppendLine($"{rech.numeroLinea}|{rech.motivo}");
            }

            File.WriteAllText(archivo, sb.ToString(), Encoding.UTF8);
        }
    }
}
