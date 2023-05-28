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

                    var price = registro.Price.Split(".");
                    var market_cap = registro.Market_Cap.Split(".");
                    if (market_cap[0] == "")
                    {
                        market_cap[0] = "0";
                    }
                    
                    var totalVolume = registro.Total_Volume.Split(".");


                    listaDatosSalida.Add(new DatosSalida
                    {
                        Date = fechaUnix,
                        Price = int.Parse(price[0]),
                        Market_Cap = long.Parse(market_cap[0]),
                        Total_Volume = long.Parse(totalVolume[0])
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
            public int Price { get; set; }
            public long Market_Cap { get; set; }
            public long Total_Volume { get; set; }
        }
    }
}
