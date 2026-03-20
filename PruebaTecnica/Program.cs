
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using PruebaTecnica.Models;
using PruebaTecnica.Validators;
using PruebaTecnica.Services;

class Program {

    static void Main(String[] args) {

        Console.WriteLine("Ingrese la ruta del archivo: ");
        String archivo = Console.ReadLine();
        String salida = Path.Combine(Path.GetDirectoryName(archivo), "Salida");
        try
        {
            Directory.CreateDirectory(salida);
            var processor = new FileProcessorService();

            var Resultado = processor.ProcesarArchivo(archivo, salida);


            Console.WriteLine($"Archivos generados en:{salida}");

        }
        catch (Exception e) { 
            
            Console.WriteLine($"Error: {e}");
        }

    }


    //static Resultado ProcesarArchivo(String Archivo, String Salida) {

    //    var registrosValidos= new List<Dictionary<String, String>>();
    //    var registrosAll = new List<Dictionary<String, String>>();
    //    var registrosRechazados = new List<Rechazados>();
    //    var resumen = new Resumen();

    //    String[] lineas= File.ReadAllLines(Archivo);
    //    resumen.TotalRegistros = lineas.Length -1;
    //    if (lineas.Length == 0) {
    //        throw new Exception("El archivo esta vacio");
    //    }

    //    String[] cabecera = lineas[0].Split('|');
    //    cabecera = cabecera.Select(x => x.Trim()).ToArray();

    //    var validaciones = new Dictionary<String, Func<String, List<String>>>
    //    {
    //        ["Id"] = ValidarId,
    //        ["Sequence"] =ValidarSequence,
    //        ["ServiceName"] =ValidarServiceName,
    //        ["ServiceState"] =ValidarServiceState,
    //        ["CashValue"] =ValidarCashValue,
    //        ["TellerId"] =ValidarTellerId,
    //        ["DateSettlement"] =ValidarDateSettlement
    //    };

    //    String archivoRechazados = Path.Combine(Salida, $"Rechazos{Path.GetFileNameWithoutExtension(Archivo)}.txt");
    //    String archivoResumen = Path.Combine(Salida, $"Resumen{Path.GetFileNameWithoutExtension(Archivo)}.txt");

    //    var idVisto = new HashSet<String>();
    //    var sequenceVisto = new HashSet<String>();

    //    for (Int32 i = 1 ; i < lineas.Length; i++) {
    //        String[] valores = lineas[i].Split("|");

    //        if (valores.Length != cabecera.Length)
    //        {
    //            registrosRechazados.Add( new Rechazados { 
    //            numeroLinea = i,
    //            motivo = $"Numero incorrecto de Columnas: {valores.Length} (Esperadas: {cabecera.Length})"
                
    //            });
    //            continue;
    //        }
            
    //        var fila = new Dictionary<String, String>();
    //        var errores = new List<String>();

    //        for (Int32 j = 0; j < cabecera.Length; j++) 
    //        {
    //            String valorLimpio = valores[j].Trim();
    //            fila[cabecera[j]]= valorLimpio;

    //            if (validaciones.ContainsKey(cabecera[j])) {
    //                var errorescolumna = validaciones[cabecera[j]](valorLimpio);
    //                if (errorescolumna.Any()) { 
    //                    errores.AddRange(errorescolumna.Select(e => $"{cabecera[j]}: {e}"));
    //                }
                    
    //            }
                
    //        }
    //        registrosAll.Add(fila);
    //        if (fila.ContainsKey("Id"))
    //        {
    //            String id = fila["Id"].Trim();
    //            if (idVisto.Contains(id))
    //            {
    //                errores.Add($"Id Duplicado: {id}");
    //            }
    //            else { 
    //                idVisto.Add(id);
    //            }
    //        }
    //        if (fila.ContainsKey("Sequence"))
    //        {
    //            String sequence = fila["Sequence"].Trim();
    //            if (sequenceVisto.Contains(sequence))
    //            {
    //                errores.Add($"Sequence Duplicada: {sequence}");
    //            }
    //            else
    //            {
    //                sequenceVisto.Add(sequence);
    //            }
    //        }
    //        if (fila.ContainsKey("ServiceState") && fila.ContainsKey("CashValue"))
    //        {
    //            String state = fila["ServiceState"].Trim();
    //            String cash = fila["CashValue"].Trim();

    //            if (state == "Rejected")
    //            {
    //                var culturaCO = new CultureInfo("es-CO");
    //                if (decimal.TryParse(cash, NumberStyles.Float, culturaCO, out decimal valorCash) && valorCash != 0)
    //                {
    //                    errores.Add($"Inconsistencia: Rejected debe tener CashValue = 0,00 (actual: {cash})");
    //                }
    //            }
    //        }

    //        if (errores.Any())
    //        {
    //            registrosRechazados.Add(new Rechazados
    //            {
    //                numeroLinea = i,
    //                motivo = String.Join(";", errores)

    //            });
                

    //        }
    //        else {
    //            registrosValidos.Add(fila);
    //        }

    //    }
    //    if (registrosRechazados.Count() == 0)
    //    {
    //        Console.WriteLine("No hay registros que hayan sido rechazados");
    //    }
    //    else { 
    //        GuardarRechazados(registrosRechazados, archivoRechazados);
    //    }

    //    var cashValueDate = registrosValidos
    //        .Where(x => x.TryGetValue("CashValue", out var cash) &&
    //               x.TryGetValue("DateSettlement", out var date) &&
    //                  Decimal.TryParse(cash, out _))
    //        .GroupBy(
    //               x => x["DateSettlement"],
    //               x => Decimal.Parse(x["CashValue"]))
    //        .Select(c => new Dictionary<string, Object>
    //            {
    //                ["DateSettlement"] = c.Key,
    //                ["CashValueTotal"] = c.Sum()
    //            }).ToList();

    //    var totalCash = registrosAll
    //        .Where(x => x.TryGetValue("CashValue", out var cash) &&
    //               x.TryGetValue("ServiceState", out var state) &&
    //               x.TryGetValue("TellerId", out var teller) &&
    //                  Decimal.TryParse(cash, out _))
    //        .GroupBy(
    //               x => x["TellerId"],
    //                x => new
    //                {
    //                    Estado = x["ServiceState"],
    //                    Valor = Decimal.Parse(x["CashValue"])
    //                }
    //               )
    //        .Select(c => new Dictionary<string, Object>
    //        {
    //            ["TellerId"] = c.Key,
    //            ["AcceptedCashValueTotal"] = c
    //            .Where(x => x.Estado == "Accepted" || x.Estado == "accepted")
    //            .Sum(x => x.Valor),

    //            ["RejectedCashValueTotal"] = c
    //            .Where(x => x.Estado == "Rejected")
    //            .Sum(x => x.Valor)
    //        }).ToList();



    //    resumen.TotalEfectivo = totalCash;
    //    resumen.TotalCashValue = cashValueDate;
    //    resumen.RegistrosValidos=registrosValidos.Count();
    //    resumen.RegistrosRechazados=registrosRechazados.Count();
    //    resumen.ArchivoRechazados = archivoRechazados;
    //    GuardarResumen(resumen, archivoResumen, Archivo);

    //    return new Resultado
    //    {
    //        registrosValidos = registrosValidos,
    //        RegistrosRechazados = registrosRechazados,
    //        ArchivoRecazados = $"{Path.GetFileNameWithoutExtension(archivoRechazados)}.txt"
    //    };
    //}

    //static List<String> ValidarId(String Id)
    //{
    //    if (String.IsNullOrEmpty(Id)) { return new List<String> { "Id vacio o nulo" }; }
    //    if (!Int32.TryParse(Id, out Int32 ID) || ID < 0) {
    //        return new List<String>{$"Id Invalido -> {Id}"};
    //    }
    //    return new List<String>();
    //}
    //static List<String> ValidarSequence(String sequenceI)
    //{
    //    if (String.IsNullOrEmpty(sequenceI)) { return new List<String> { "Sequence vacio o nulo" }; }
    //    if (!Int32.TryParse(sequenceI, out Int32 sequence))
    //    {
    //        return new List<String> { $"Sequence Invalido -> {sequenceI}" };
    //    }
    //    return new List<String>();
    //}
    //static List<String> ValidarServiceName(String serviceName)
    //{
    //    if (String.IsNullOrEmpty(serviceName))
    //    {
    //        return new List<String> { "ServiceName Vacio" };
    //    }
    //    return new List<String>();
    //}

    //static List<String> ValidarServiceState(String serviceState)
    //{
    //    if (String.IsNullOrEmpty(serviceState)) {
    //        return new List<String> {"serviceState vacio o nulo"};
    //    }
    //    var valoresPermitidos = new List<String> { "Accepted", "Rejected" };
    //    if (!valoresPermitidos.Contains(serviceState))
    //    {
    //        return new List<String> { $"ServiceState inválido -> {serviceState}. Solo se permiten: {String.Join(", ", valoresPermitidos)}" };
    //    }
    //    return new List<String>();
    //}

    //static List<String> ValidarCashValue(String cashValue)
    //{
    //    if (String.IsNullOrEmpty(cashValue))
    //    {
    //        return new List<String> { "CashValue vacio o nulo" };
    //    }
    //    var culturaCO = new CultureInfo("es-CO");
    //    if (!Decimal.TryParse(cashValue, NumberStyles.Float, culturaCO, out decimal cash)) {
    //        return new List<String> { $"Formato de CashValue invalido -> {cashValue}" };
    //    }
    //    if (cash < 0)
    //    {
    //        return new List<String> { $"CashValue no puede ser negativo -> {cashValue}" };
    //    }
    //    return new List<String>();
    //}

    //static List<String> ValidarTellerId(String tellerId)
    //{
    //    if (String.IsNullOrEmpty(tellerId))
    //    {
    //        return new List<String> { "TellerId vacio o nulo" };
    //    }
    //    if (tellerId.Length !=3) { return new List<String> { $"TellerId es invalido -> {tellerId}" }; }
    //    if (!tellerId.All(char.IsDigit)) { return new List<String> { $"TellerId no es numerico -> {tellerId}" }; }
    //    return new List<String>();
    //}

    //static List<String> ValidarDateSettlement(String dateSettlement) {
    //    if (String.IsNullOrEmpty(dateSettlement))
    //    {
    //        return new List<String> { "DateSettlement vacio o nulo" };
    //    }
    //    if (!DateTime.TryParseExact(dateSettlement, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)) 
    //    {
    //        return new List<String> { $"Formato de DateSettlement invalido -> {dateSettlement}" };
    //    }
    //        return new List<String>();
    //}

    //static void GuardarResumen(Resumen estadisticas, String archivo, String entrada) 
    //{
    //    var resumen = new
    //    {
    //        Titulo = "Resumen de Procesado",
    //        TotalRegistros = estadisticas.TotalRegistros,
    //        RegistrosAceptados = estadisticas.RegistrosValidos,
    //        RegistrosRechazados = estadisticas.RegistrosRechazados,
    //        NombreArchivoRechazado = estadisticas.ArchivoRechazados,
    //        CashValuePorFecha = estadisticas.TotalCashValue,
    //        TotalCajerosEstados= estadisticas.TotalEfectivo
    //    };
    //    string json = JsonSerializer.Serialize(resumen, new JsonSerializerOptions
    //    {
    //        WriteIndented = true // Formato bonito, con saltos de línea y sangría
    //    });

    //    File.WriteAllText(archivo, json, Encoding.UTF8);
    //}



    //static void GuardarRechazados(List<Rechazados> rechazados, String archivo) { 
    //    var sb = new StringBuilder();
    //    sb.AppendLine("Número de la fila original|Mensaje de error o de validación fallida");
    //    foreach (var rech in rechazados) {
    //        sb.AppendLine($"{rech.numeroLinea}|{rech.motivo}");
    //    }

    //    File.WriteAllText(archivo, sb.ToString(), Encoding.UTF8);
    //}

   

}