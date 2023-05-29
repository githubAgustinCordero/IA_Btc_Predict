using Microsoft.ML;
using Microsoft.ML.Data;

namespace ConsoleApp1
{
    class Program
    {
        static void Main()
        {
            MLContext mlContext = new MLContext();
            //string fileName = "btc_prices.csv";
            string entrada = "2012_2020_Halving.csv";
            string salida = "btc_prices_temp.csv";

            Conversor conversorFechas = new Conversor();
            conversorFechas.ConvertirFechasCSV(entrada, salida);

            var lines = File.ReadLines(salida)
                .Where(line => !line.Contains("NaN"))
                .ToArray();

            //string tempFileName = "btc_prices_temp.csv";
            File.WriteAllLines(salida, lines);

            //var dataView = mlContext.Data.LoadFromTextFile<btc_prices>(tempFileName, ',', true);
            var dataView = mlContext.Data.LoadFromTextFile<btc_2015_al_2023>(salida, ',', true);

            //var dataPipeline = mlContext.Transforms.Concatenate("Features", 
            //                                                    nameof(btc_prices.Timestamp), 
            //                                                    //nameof(BitcoinPrice.Open), 
            //                                                    //nameof(BitcoinPrice.High), 
            //                                                    //nameof(BitcoinPrice.Low), 
            //                                                    nameof(btc_prices.Close)
            //                                                    //nameof(BitcoinPrice.Volume_BTC), 
            //                                                    //nameof(BitcoinPrice.Volume_Currency),
            //                                                    /*nameof(BitcoinPrice.Label)*/)

            var dataPipeline = mlContext.Transforms.Concatenate("Features",
                                                                nameof(btc_2015_al_2023.Date),
                                                                //nameof(btc_2015_al_2023.Label),
                                                                nameof(btc_2015_al_2023.Markect_Cap),
                                                                nameof(btc_2015_al_2023.Total_Volume)
                                                                )
    .Append(mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(mlContext.Regression.Trainers.Sdca());

            var model = dataPipeline.Fit(dataView);
            var predictionEngine = mlContext.Model.CreatePredictionEngine<btc_2015_al_2023, BitcoinPricePrediction>(model);

            // Crear una fecha local
            DateTime localDateTime = new DateTime(2023, 5, 29); // año, mes, dia

            // Convertir la fecha local a DateTimeOffset
            DateTimeOffset dateTimeOffset = new DateTimeOffset(localDateTime);

            // Convertir DateTimeOffset a tiempo Unix
            long unixTime = dateTimeOffset.ToUnixTimeSeconds();

            //var sampleData = new btc_prices { Timestamp = unixTime };
            var sampleData = new btc_2015_al_2023 { 
                Date = unixTime,
                Markect_Cap = 523846408825,
                Total_Volume = 12732238815
            };

            //conclusiones:
            // es necesario tener antes el valor de marketcap, para luego ingresarlo para obtener un precio
            // ideal seria tener tambien una prediccion del volumen, así como de datos pero clasificados por cada halving
            // incluso los valores de monedas en circulacion
            

            var prediction = predictionEngine.Predict(sampleData);

            // Convertir la fecha Unix a DateTime
            //dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds((long)sampleData.Timestamp);
            dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds((long)sampleData.Date);
            localDateTime = dateTimeOffset.LocalDateTime;

            //Console.WriteLine($"El precio predicho para el {localDateTime} es: {prediction.Score}");
            Console.WriteLine($"El precio predicho para el {localDateTime} es: {prediction.Score}");
            Console.ReadLine();
        }
    }

    public class btc_prices
    {
        [LoadColumn(0)]
        public float Timestamp;

        [LoadColumn(1)]
        public float Open;

        [LoadColumn(2)]
        public float High;

        [LoadColumn(3)]
        public float Low;

        [LoadColumn(4)]
        public float Close;

        [LoadColumn(5)]
        public float Volume_BTC;

        [LoadColumn(6)]
        public float Volume_Currency;

        [LoadColumn(7)]
        public float Label;
    }


    //Date,Price,Market cap,Total volume
    public class btc_2015_al_2023
    {
        [LoadColumn(0)]
        public float Date;

        [LoadColumn(1)]
        public Single Label; //Price

        [LoadColumn(2)]
        public Single Markect_Cap;

        [LoadColumn(3)]
        public Single Total_Volume;

    }
    public class BitcoinPricePrediction
    {
        public Single Score;
    }
}
