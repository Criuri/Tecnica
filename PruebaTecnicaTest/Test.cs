using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Moq;
using PruebaTecnica.Helpers;
using PruebaTecnica.Models;
using PruebaTecnica.Services;
using PruebaTecnica.Validators;
using System.Text;
using System.Text.Json;

namespace PruebaTecnicaTest
{
    [TestClass]
    public class FileProcessorserviceTest 
    {
        private Mock<FieldValidators> _mockValidators;
        private Mock<FileWriteHelper> _mockWriteHelper;
        private FileProcessorService _service;

        [TestInitialize]
        public void TestInitialize() 
        { 
            _mockValidators = new Mock<FieldValidators>();
            _mockWriteHelper = new Mock<FileWriteHelper>();
            _service = new FileProcessorService();

            _mockValidators
                .Setup(v => v.ValidarCampo(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new List<string>());

            _mockWriteHelper.
                Setup(w => w.GuardarRechazados(It.IsAny<List<Rechazados>>(), It.IsAny<string>()))
                .Verifiable();

            _mockWriteHelper
               .Setup(w => w.GuardarResumen(It.IsAny<Resumen>(), It.IsAny<string>(), It.IsAny<string>()))
               .Verifiable();

            _service = new FileProcessorService(
               validators: _mockValidators.Object,
               fileWriteHelper: _mockWriteHelper.Object
           );

        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "El archivo esta vacio")]
        public void ProcesarArchivoVacio() 
        {
            string archivoVacio = Archivotempo("");
            try
            {
                _service.ProcesarArchivo(archivoVacio, "salida");
                Assert.ThrowsException<ArgumentException>(() => { });
            }
            finally
            {
                if (File.Exists(archivoVacio)) File.Delete(archivoVacio);
            }
        }

        private string Archivotempo(string contenido) 
        { 
            var tempfile = Path.GetTempFileName();
            File.WriteAllText(tempfile, contenido);
            return tempfile;
        }

        [TestMethod]
        public void ProcesarArchivo_ColumnasIncorrectas_RegistroRechazado()
        {
            var cabeceraLinea = "Id|Sequence|CashValue|ServiceState|TellerId|DateSettlement";
            var registroLinea = "1|A|100|Accepted|2025-01-01";

            string archivoMock = Path.GetTempFileName();
            File.WriteAllLines(archivoMock, new[] { cabeceraLinea, registroLinea });

            string salida = Path.Combine(Path.GetTempPath(), "salida");

            var resultado = _service.ProcesarArchivo(archivoMock, salida);

            Assert.AreEqual(0, resultado.registrosValidos.Count);
            Assert.AreEqual(1, resultado.RegistrosRechazados.Count);
            Assert.IsTrue(resultado.RegistrosRechazados[0].motivo.Contains("Numero incorrecto de Columnas"));
        }


        [TestMethod]
        public void ProcesarArchivo_IdDuplicado_RegistroRechazado()
        {
            var cabeceraLinea = "Id|Sequence|CashValue|ServiceState";
            var lineas = new[]
            {
                "1|A|100|Accepted",
                "1|B|200|Accepted"
            };

            string archivoMock = Path.GetTempFileName();
            File.WriteAllLines(archivoMock, new[] { cabeceraLinea }.Concat(lineas).ToArray());

            string salida = Path.Combine(Path.GetTempPath(), "salida");

            var resultado = _service.ProcesarArchivo(archivoMock, salida);

            Assert.AreEqual(1, resultado.RegistrosRechazados.Count);
            Assert.IsTrue(resultado.RegistrosRechazados[0].motivo.Contains("Id Duplicado"));
        }

        [TestMethod]
        public void ProcesarArchivo_RejectedConCashMayorA0_RegistroRechazado()
        {
            // Arrange
            var cabeceraLinea = "Id|Sequence|CashValue|ServiceState";
            var lineas = new[]
            {
                "1|A|150,00|Rejected"
            };

            string archivoMock = Path.GetTempFileName();
            File.WriteAllLines(archivoMock, new[] { cabeceraLinea }.Concat(lineas).ToArray());

            string salida = Path.Combine(Path.GetTempPath(), "salida");

            // Act
            var resultado = _service.ProcesarArchivo(archivoMock, salida);

            // Assert
            Assert.AreEqual(1, resultado.RegistrosRechazados.Count);
            Assert.IsTrue(resultado.RegistrosRechazados[0].motivo.Contains("Rejected debe tener CashValue = 0,00"));
        }

        [TestMethod]
        public void ProcesarArchivo_ArchivoValido_LlenaRegistrosYResumen()
        {
            // Arrange
            var cabeceraLinea = "Id|Sequence|CashValue|ServiceState|DateSettlement|TellerId";
            var lineas = new[]
            {
                "1|A|100|Accepted|2025-01-01|T1",
                "2|B|200|Rejected|2025-01-01|T1",
                "3|C|300|Accepted|2025-01-01|T2"
            };

            string archivoMock = Path.GetTempFileName();
            File.WriteAllLines(archivoMock, new[] { cabeceraLinea }.Concat(lineas).ToArray());

            string salida = Path.Combine(Path.GetTempPath(), "salida");

            // Act
            var resultado = _service.ProcesarArchivo(archivoMock, salida);

            // Assert
            Assert.AreEqual(2, resultado.registrosValidos.Count);   // 1 y 3 válidos
            Assert.AreEqual(1, resultado.RegistrosRechazados.Count); // 2 rechazado por estado

            // Verifica que se guardó el resumen
            _mockWriteHelper
                .Verify(w => w.GuardarResumen(It.IsAny<Resumen>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void ProcesarArchivo_NoHayRechazos_NoSeGuardanRechazos()
        {
            // Arrange
            var cabeceraLinea = "Id|Sequence|CashValue|ServiceState|DateSettlement";
            var lineas = new[]
            {
                "1|A|100|Accepted|2025-01-01"
            };

            string archivoMock = Path.GetTempFileName();
            File.WriteAllLines(archivoMock, new[] { cabeceraLinea }.Concat(lineas).ToArray());

            string salida = Path.Combine(Path.GetTempPath(), "salida");

            // Act
            var resultado = _service.ProcesarArchivo(archivoMock, salida);

            // Assert
            Assert.AreEqual(0, resultado.RegistrosRechazados.Count);

            // Verifica que no se escribió archivo de rechazos
            _mockWriteHelper
                .Verify(w => w.GuardarRechazados(It.IsAny<List<Rechazados>>(), It.IsAny<string>()), Times.Never);
        }



    }
    [TestClass]
    public class FileWriterTest
    {
        private FileWriteHelper _writer;

        [TestInitialize]
        public void TestInitialize() 
        { 
            _writer = new FileWriteHelper();
        }

        [TestMethod]
        public void GuardarRechazadosCorrecto()
        {
            var rechazados = new List<Rechazados>
            {
                 new() { numeroLinea = 2, motivo = "Id invalido" },
                new() { numeroLinea = 3, motivo = "CashValue negativo" }
            };
            string archivo = Path.GetTempFileName();

            try
            {
                _writer.GuardarRechazados(rechazados, archivo);
                string contenido = File.ReadAllText(archivo, Encoding.UTF8);

                StringAssert.Contains(contenido, "Número de la fila original|Mensaje de error o de validación fallida");
                StringAssert.Contains(contenido, "2|Id invalido");
                StringAssert.Contains(contenido, "3|CashValue negativo");
            }
            finally
            {
                if (File.Exists(archivo))
                    File.Delete(archivo);
            }
        }


        [TestMethod]
        public void GuardarResumen_Correcto()
        {
            var estadisticas = new Resumen
            {
                TotalRegistros = 100,
                RegistrosValidos = 90,
                RegistrosRechazados = 10,
                ArchivoRechazados = "rechazados.csv",
                TotalCashValue = new List<Dictionary<string, object>>
                {
                    new() { ["DateSettlement"] = "2026-03-19", ["CashValueTotal"] = 50000.0 },
                    new() { ["DateSettlement"] = "2026-03-19", ["CashValueTotal"] = 30000.0 }
                },
                TotalEfectivo = new List<Dictionary<string, object>>
                {
                    new() { ["TellerId"] = "Cajero1", ["AcceptedCashValueTotal"] = 10000.0 },
                    new() { ["TellerId"] = "Cajero2", ["AcceptedCashValueTotal"] = 40000.0 }
                }
            };

            string archivoSalida = Path.GetTempFileName();
            string archivoEntrada = Path.GetTempFileName();

            try
            {
                _writer.GuardarResumen(estadisticas, archivoSalida, archivoEntrada);

                string contenido = File.ReadAllText(archivoSalida, Encoding.UTF8);

                StringAssert.Contains(contenido, "\"TotalRegistros\": 100");
                StringAssert.Contains(contenido, "\"RegistrosValidos\": 90");
                StringAssert.Contains(contenido, "\"RegistrosRechazados\": 10");
                StringAssert.Contains(contenido, "\"NombreArchivoRechazado\": \"rechazados.csv\"");

                StringAssert.Contains(contenido, "\"CashValuePorFecha\"");
                StringAssert.Contains(contenido, "\"DateSettlement\": \"2026-03-19\"");
                StringAssert.Contains(contenido, "\"CashValueTotal\": 50000");

                StringAssert.Contains(contenido, "\"TotalCajerosEstados\"");
                StringAssert.Contains(contenido, "\"TellerId\": \"Cajero1\"");
                StringAssert.Contains(contenido, "\"AcceptedCashValueTotal\": 10000");
                StringAssert.Contains(contenido, "\"AcceptedCashValueTotal\": 40000");

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var actualObj = JsonSerializer.Deserialize<Resumen>(contenido, options);
                Assert.AreEqual(100, actualObj.TotalRegistros);
                Assert.AreEqual(90, actualObj.RegistrosValidos);
                Assert.AreEqual(10, actualObj.RegistrosRechazados);
            }
            finally
            {
                if (File.Exists(archivoSalida))
                    File.Delete(archivoSalida);
                if (File.Exists(archivoEntrada))
                    File.Delete(archivoEntrada);
            }
        }


    }

    [TestClass]
    public class ValidatorsTest
    {
        private FieldValidators _validators;

        [TestInitialize]
        public void TestInitialize()
        {
            _validators = new FieldValidators();
        }

        [TestMethod]
        public void validarIdValido()
        {
            var errores = _validators.ValidarCampo("Id", "123");
            Assert.AreEqual(0, errores.Count);
        }

        [TestMethod]
        public void validarIdVacio()
        {
            var errores = _validators.ValidarCampo("Id", "");
            Assert.AreEqual(1, errores.Count);
            Assert.AreEqual("Id vacio o nulo", errores[0]);
        }

        [TestMethod]
        public void validarSequenceVacio()
        {
            var errores = _validators.ValidarCampo("Sequence", "");
            Assert.AreEqual(1, errores.Count);
            Assert.AreEqual("Sequence vacio o nulo", errores[0]);
        }

        [TestMethod]
        public void validarSquenceValido()
        {
            var errores = _validators.ValidarCampo("Sequence", "12");
            Assert.AreEqual(0, errores.Count);
        }

        [TestMethod]
        public void validarServiceNameVacio()
        {
            var errores = _validators.ValidarCampo("ServiceName", "");
            Assert.AreEqual(1, errores.Count);
            Assert.AreEqual("ServiceName Vacio", errores[0]);
        }
        [TestMethod]
        public void validarServiceStateVacio()
        {
            var errores = _validators.ValidarCampo("ServiceState", "");
            Assert.AreEqual(1, errores.Count);
            Assert.AreEqual("serviceState vacio o nulo", errores[0]);
        }
        [TestMethod]
        public void validarServiceStateInvalido()
        {
            var valoresPermitidos = new List<string> { "Accepted", "Rejected" };
            var errores = _validators.ValidarCampo("ServiceState", "accepted");
            Assert.AreEqual(1, errores.Count);
            Assert.AreEqual($"ServiceState inválido -> accepted. Solo se permiten: {string.Join(", ", valoresPermitidos)}", errores[0]);
        }

        [TestMethod]
        public void validarCashValueVacio()
        {
            var errores = _validators.ValidarCampo("CashValue", "");
            Assert.AreEqual(1, errores.Count);
            Assert.IsTrue(errores[0].Contains("CashValue vacio o nulo"));
        }

        [TestMethod]
        public void validarCashValueNegativo()
        {
            var errores = _validators.ValidarCampo("CashValue", "-123,0");
            Assert.AreEqual(1, errores.Count);
            Assert.IsTrue(errores[0].Contains("CashValue no puede ser negativo -> -123,0"));
        }

        [TestMethod]
        public void validarCashValueInvalido()
        {
            var errores = _validators.ValidarCampo("CashValue", "123.0");
            Assert.AreEqual(1, errores.Count);
            Assert.IsTrue(errores[0].Contains("Formato de CashValue invalido -> 123.0"));
        }

        [TestMethod]
        public void validarTellerIdVacio()
        {
            var errores = _validators.ValidarCampo("TellerId", "");
            Assert.AreEqual(1, errores.Count);
            Assert.IsTrue(errores[0].Contains("TellerId vacio o nulo"));
        }

        [TestMethod]
        public void validarTellerInvalido()
        {
            var errores = _validators.ValidarCampo("TellerId", "2341");
            Assert.AreEqual(1, errores.Count);
            Assert.IsTrue(errores[0].Contains("TellerId es invalido -> 2341"));
        }

        [TestMethod]
        public void validarTellerIdLetras()
        {
            var errores = _validators.ValidarCampo("TellerId", "ABC");
            Assert.AreEqual(1, errores.Count);
            Assert.IsTrue(errores[0].Contains("TellerId no es numerico -> ABC"));
        }

        [TestMethod]
        public void validarDateSettlementVacio()
        {
            var errores = _validators.ValidarCampo("DateSettlement", "");
            Assert.AreEqual(1, errores.Count);
            Assert.IsTrue(errores[0].Contains("DateSettlement vacio o nulo"));
        }

        [TestMethod]
        public void validarDateSettlementInvalido()
        {
            var errores = _validators.ValidarCampo("DateSettlement", "12-01-2022");
            Assert.AreEqual(1, errores.Count);
            Assert.IsTrue(errores[0].Contains("Formato de DateSettlement invalido -> 12-01-2022"));
        }


    }
}
