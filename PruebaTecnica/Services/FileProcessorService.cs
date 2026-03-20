using PruebaTecnica.Helpers;
using PruebaTecnica.Models;
using PruebaTecnica.Validators;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PruebaTecnica.Services
{
    public class FileProcessorService
    {
        private readonly FieldValidators _validators;
        private readonly FileWriteHelper _fileWriteHelper;

        public FileProcessorService(FieldValidators? validators=null, FileWriteHelper? fileWriteHelper=null)
        {
            _validators = validators ?? new FieldValidators();
            _fileWriteHelper = fileWriteHelper ?? new FileWriteHelper();
        }

        public Resultado ProcesarArchivo(String Archivo, String Salida)
        {   
            var registrosValidos = new List<Dictionary<String, String>>();
            var registrosAll = new List<Dictionary<String, String>>();
            var registrosRechazados = new List<Rechazados>();
            var resumen = new Resumen();

            String[] lineas = File.ReadAllLines(Archivo);
            resumen.TotalRegistros = lineas.Length - 1;
            if (lineas.Length == 0)
            {
                throw new Exception("El archivo esta vacio");
            }

            String[] cabecera = lineas[0].Split('|');
            cabecera = cabecera.Select(x => x.Trim()).ToArray();

            String archivoRechazados = Path.Combine(Salida, $"Rechazos{Path.GetFileNameWithoutExtension(Archivo)}.txt");
            String archivoResumen = Path.Combine(Salida, $"Resumen{Path.GetFileNameWithoutExtension(Archivo)}.txt");

            var idVisto = new HashSet<String>();
            var sequenceVisto = new HashSet<String>();

            for (Int32 i = 1; i < lineas.Length; i++)
            {
                String[] valores = lineas[i].Split("|");

                if (valores.Length != cabecera.Length)
                {
                    registrosRechazados.Add(new Rechazados
                    {
                        numeroLinea = i,
                        motivo = $"Numero incorrecto de Columnas: {valores.Length} (Esperadas: {cabecera.Length})"

                    });
                    continue;
                }

                var fila = new Dictionary<String, String>();
                var errores = new List<String>();

                for (Int32 j = 0; j < cabecera.Length; j++)
                {
                    String valorLimpio = valores[j].Trim();
                    fila[cabecera[j]] = valorLimpio;


                        var errorescolumna = _validators.ValidarCampo(cabecera[j], valorLimpio);
                        if (errorescolumna.Any())
                        {
                            errores.AddRange(errorescolumna.Select(e => $"{cabecera[j]}: {e}"));
                        }


                }
                registrosAll.Add(fila);
                if (fila.ContainsKey("Id"))
                {
                    String id = fila["Id"].Trim();
                    if (idVisto.Contains(id))
                    {
                        errores.Add($"Id Duplicado: {id}");
                    }
                    else
                    {
                        idVisto.Add(id);
                    }
                }
                if (fila.ContainsKey("Sequence"))
                {
                    String sequence = fila["Sequence"].Trim();
                    if (sequenceVisto.Contains(sequence))
                    {
                        errores.Add($"Sequence Duplicada: {sequence}");
                    }
                    else
                    {
                        sequenceVisto.Add(sequence);
                    }
                }
                if (fila.ContainsKey("ServiceState") && fila.ContainsKey("CashValue"))
                {
                    String state = fila["ServiceState"].Trim();
                    String cash = fila["CashValue"].Trim();

                    if (state == "Rejected")
                    {
                        var culturaCO = new CultureInfo("es-CO");
                        if (decimal.TryParse(cash, NumberStyles.Float, culturaCO, out decimal valorCash) && valorCash != 0)
                        {
                            errores.Add($"Inconsistencia: Rejected debe tener CashValue = 0,00 (actual: {cash})");
                        }
                    }
                }

                if (errores.Any())
                {
                    registrosRechazados.Add(new Rechazados
                    {
                        numeroLinea = i,
                        motivo = String.Join(";", errores)

                    });


                }
                else
                {
                    registrosValidos.Add(fila);
                }

            }
            if (registrosRechazados.Count() == 0)
            {
                Console.WriteLine("No hay registros que hayan sido rechazados");
            }
            else
            {
                _fileWriteHelper.GuardarRechazados(registrosRechazados, archivoRechazados);
            }

            var cashValueDate = registrosValidos
                .Where(x => x.TryGetValue("CashValue", out var cash) &&
                       x.TryGetValue("DateSettlement", out var date) &&
                          Decimal.TryParse(cash, out _))
                .GroupBy(
                       x => x["DateSettlement"],
                       x => Decimal.Parse(x["CashValue"]))
                .Select(c => new Dictionary<string, Object>
                {
                    ["DateSettlement"] = c.Key,
                    ["CashValueTotal"] = c.Sum()
                }).ToList();

            var totalCash = registrosAll
                .Where(x => x.TryGetValue("CashValue", out var cash) &&
                       x.TryGetValue("ServiceState", out var state) &&
                       x.TryGetValue("TellerId", out var teller) &&
                          Decimal.TryParse(cash, out _))
                .GroupBy(
                       x => x["TellerId"],
                        x => new
                        {
                            Estado = x["ServiceState"],
                            Valor = Decimal.Parse(x["CashValue"])
                        }
                       )
                .Select(c => new Dictionary<string, Object>
                {
                    ["TellerId"] = c.Key,
                    ["AcceptedCashValueTotal"] = c
                    .Where(x => x.Estado == "Accepted" || x.Estado == "accepted")
                    .Sum(x => x.Valor),

                    ["RejectedCashValueTotal"] = c
                    .Where(x => x.Estado == "Rejected")
                    .Sum(x => x.Valor)
                }).ToList();



            resumen.TotalEfectivo = totalCash;
            resumen.TotalCashValue = cashValueDate;
            resumen.RegistrosValidos = registrosValidos.Count();
            resumen.RegistrosRechazados = registrosRechazados.Count();
            resumen.ArchivoRechazados = archivoRechazados;
            _fileWriteHelper.GuardarResumen(resumen, archivoResumen, Archivo);

            return new Resultado
            {
                registrosValidos = registrosValidos,
                RegistrosRechazados = registrosRechazados,
                ArchivoRecazados = $"{Path.GetFileNameWithoutExtension(archivoRechazados)}.txt"
            };

        }

    }
}
