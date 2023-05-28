using Microsoft.ML;
using Microsoft.ML.Data;

namespace ConsoleApp1
{
    class Program
    {
        static void Main()
        {
            MLContext mlContext = new MLContext();
            string fileName = "btc_prices.csv";

            var lines = File.ReadLines(fileName)
                .Where(line => !line.Contains("NaN"))
                .ToArray();

            string tempFileName = "btc_prices_temp.csv";
            File.WriteAllLines(tempFileName, lines);

            var dataView = mlContext.Data.LoadFromTextFile<BitcoinPrice>(tempFileName, ',', true);

            var dataPipeline = mlContext.Transforms.Concatenate("Features", 
                                                                nameof(BitcoinPrice.Timestamp), 
                                                                //nameof(BitcoinPrice.Open), 
                                                                //nameof(BitcoinPrice.High), 
                                                                //nameof(BitcoinPrice.Low), 
                                                                nameof(BitcoinPrice.Close)
                                                                //nameof(BitcoinPrice.Volume_BTC), 
                                                                //nameof(BitcoinPrice.Volume_Currency),
                                                                /*nameof(BitcoinPrice.Label)*/)
                .Append(mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(mlContext.Regression.Trainers.Sdca());

            var model = dataPipeline.Fit(dataView);
            var predictionEngine = mlContext.Model.CreatePredictionEngine<BitcoinPrice, BitcoinPricePrediction>(model);

            // Crear una fecha local
            DateTime localDateTime = new DateTime(2020, 5, 16);

            // Convertir la fecha local a DateTimeOffset
            DateTimeOffset dateTimeOffset = new DateTimeOffset(localDateTime);

            // Convertir DateTimeOffset a tiempo Unix
            long unixTime = dateTimeOffset.ToUnixTimeSeconds();

            var sampleData = new BitcoinPrice { Timestamp = unixTime };

            var prediction = predictionEngine.Predict(sampleData);

            // Convertir la fecha Unix a DateTime
            dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds((long)sampleData.Timestamp);
            localDateTime = dateTimeOffset.LocalDateTime;

            Console.WriteLine($"El precio predicho para el {localDateTime} es: {prediction.Score}");
            Console.ReadLine();
        }
    }

    public class BitcoinPrice
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

    public class BitcoinPricePrediction
    {
        public float Score;
    }
}
