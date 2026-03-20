using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PruebaTecnica.Validators
{
    public class FieldValidators
    {   
        private readonly Dictionary<String, Func<String, List<String>>> _validaciones;
        
        public FieldValidators() {

            _validaciones = new()
            {
                ["Id"] = ValidarId,
                ["Sequence"] = ValidarSequence,
                ["ServiceName"] = ValidarServiceName,
                ["ServiceState"] = ValidarServiceState,
                ["CashValue"] = ValidarCashValue,
                ["TellerId"] = ValidarTellerId,
                ["DateSettlement"] = ValidarDateSettlement
            };

        }

        public virtual List<String> ValidarCampo(String campo, String Valor) { 
            return _validaciones.GetValueOrDefault(campo, _=>new())?.Invoke(Valor) ?? new();
        }
        
        static List<String> ValidarId(String Id)
        {
            if (String.IsNullOrEmpty(Id)) { return new List<String> { "Id vacio o nulo" }; }
            if (!Int32.TryParse(Id, out Int32 ID) || ID < 0)
            {
                return new List<String> { $"Id Invalido -> {Id}" };
            }
            return new List<String>();
        }
        static List<String> ValidarSequence(String sequenceI)
        {
            if (String.IsNullOrEmpty(sequenceI)) { return new List<String> { "Sequence vacio o nulo" }; }
            if (!Int32.TryParse(sequenceI, out Int32 sequence))
            {
                return new List<String> { $"Sequence Invalido -> {sequenceI}" };
            }
            return new List<String>();
        }
        static List<String> ValidarServiceName(String serviceName)
        {
            if (String.IsNullOrEmpty(serviceName))
            {
                return new List<String> { "ServiceName Vacio" };
            }
            return new List<String>();
        }

        static List<String> ValidarServiceState(String serviceState)
        {
            if (String.IsNullOrEmpty(serviceState))
            {
                return new List<String> { "serviceState vacio o nulo" };
            }
            var valoresPermitidos = new List<String> { "Accepted", "Rejected" };
            if (!valoresPermitidos.Contains(serviceState))
            {
                return new List<String> { $"ServiceState inválido -> {serviceState}. Solo se permiten: {String.Join(", ", valoresPermitidos)}" };
            }
            return new List<String>();
        }

        static List<String> ValidarCashValue(String cashValue)
        {
            if (String.IsNullOrEmpty(cashValue))
            {
                return new List<String> { "CashValue vacio o nulo" };
            }
            var culturaCO = new CultureInfo("es-CO");
            if (!Decimal.TryParse(cashValue, NumberStyles.Float, culturaCO, out decimal cash))
            {
                return new List<String> { $"Formato de CashValue invalido -> {cashValue}" };
            }
            if (cash < 0)
            {
                return new List<String> { $"CashValue no puede ser negativo -> {cashValue}" };
            }
            return new List<String>();
        }

        static List<String> ValidarTellerId(String tellerId)
        {
            if (String.IsNullOrEmpty(tellerId))
            {
                return new List<String> { "TellerId vacio o nulo" };
            }
            if (tellerId.Length != 3) { return new List<String> { $"TellerId es invalido -> {tellerId}" }; }
            if (!tellerId.All(char.IsDigit)) { return new List<String> { $"TellerId no es numerico -> {tellerId}" }; }
            return new List<String>();
        }

        static List<String> ValidarDateSettlement(String dateSettlement)
        {
            if (String.IsNullOrEmpty(dateSettlement))
            {
                return new List<String> { "DateSettlement vacio o nulo" };
            }
            if (!DateTime.TryParseExact(dateSettlement, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                return new List<String> { $"Formato de DateSettlement invalido -> {dateSettlement}" };
            }
            return new List<String>();
        }

    }
}
