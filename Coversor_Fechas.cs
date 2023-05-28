using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using CsvHelper;

namespace ConsoleApp1
{
    class Conversor
    {        
        public void ConvertirFechasCSV(string archivoEntrada, string archivoSalida)
        {
            var listaDatosSalida = new List<DatosSalida>();

            // Leer archivo de entrada
            using (var reader = new StreamReader(archivoEntrada))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var registros = csv.GetRecords<DatosEntrada>();
                foreach (var registro in registros)
                {
                    var fecha = DateTime.ParseExact(registro.Date.Split(' ')[0], "yyyy-MM-dd", CultureInfo.InvariantCulture);

                    var fechaUnix = ConvertirFechaUnix(fecha);

                    listaDatosSalida.Add(new DatosSalida
                    {
                        Date = fechaUnix,
                        Price = registro.Price,
                        Market_Cap = registro.Market_Cap,
                        Total_Volume = registro.Total_Volume
                    });
                }
            }

            // Escribir archivo de salida
            using (var writer = new StreamWriter(archivoSalida))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(listaDatosSalida);
            }
        }

        public long ConvertirFechaUnix(DateTime fecha)
        {
            return ((DateTimeOffset)fecha.ToUniversalTime()).ToUnixTimeSeconds();
        }

        public class DatosEntrada
        {
            public string Date { get; set; }
            public string Price { get; set; }
            public string Market_Cap { get; set; }
            public string Total_Volume { get; set; }
        }

        public class DatosSalida
        {
            public long Date { get; set; }
            public string Price { get; set; }
            public string Market_Cap { get; set; }
            public string Total_Volume { get; set; }
        }
    }
}