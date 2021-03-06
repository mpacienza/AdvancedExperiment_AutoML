﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdvancedTaxiFarePrediction.DataStructures;
using Common;
using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using PLplot;

namespace AdvancedTaxiFarePrediction
{
    internal static class Program
    {

        private static string LabelTicket = "es7";
        private static string TestDataRelativePath = @"C:\Users\VS\Documents\DATA\"+ LabelTicket + ".csv";// $"{BaseDatasetsRelativePath}/axabctest.csv";///taxi-fare-test.csv";

        private static string BaseDatasetsRelativePath = "Data";

        private static string TrainDataRelativePath = @"C:\Users\VS\Documents\DATA\" + LabelTicket + ".csv";//$"{BaseDatasetsRelativePath}/axabc.csv";///taxi-fare-train.csv";
        private static string TrainDataPath = GetAbsolutePath(TrainDataRelativePath);
        private static IDataView TrainDataView = null;

      private static string TestDataPath = GetAbsolutePath(TestDataRelativePath);
        private static IDataView TestDataView = null;

        private static string BaseModelsRelativePath = @"../../../MLModels";
        private static string ModelRelativePath = $"{BaseModelsRelativePath}/TaxiFareModel.zip";
       // private static string ModelPath = GetAbsolutePath(ModelRelativePath);
        private static string ModelPath = @"C:\Users\VS\Documents\DATA\test\outt\Model" + LabelTicket + ".zip";
        private static string LabelColumnName = "bcd";

        static int iMetrics = 0;

        static void Main(string[] args) //If args[0] == "svg" a vector-based chart will be created instead a .png chart
        {


            Console.WriteLine("Ticker:");

            LabelTicket =  Console.ReadLine();
            Console.WriteLine("Metcics:");
            iMetrics = Convert.ToInt16(Console.ReadLine());

            TestDataRelativePath = @"C:\Users\VS\Documents\DATA\" + LabelTicket + ".csv";// $"{BaseDatasetsRelativePath}/axabctest.csv";///taxi-fare-test.csv";

        
          TrainDataRelativePath = @"C:\Users\VS\Documents\DATA\" + LabelTicket + ".csv";//$"{BaseDatasetsRelativePath}/axabc.csv";///taxi-fare-train.csv";
        ModelPath = @"C:\Users\VS\Documents\DATA\test\outt\Model" + LabelTicket + "_" + iMetrics.ToString() +".zip";
           


        var mlContext = new MLContext();

            // Infer columns in the dataset with AutoML.
            var columnInference = InferColumns(mlContext);

            // Load data from files using inferred columns.
            LoadData(mlContext, columnInference);

            // Run an AutoML experiment on the dataset.
            var experimentResult = RunAutoMLExperiment(mlContext, columnInference);

            // Evaluate the model and print metrics.
            EvaluateModel(mlContext, experimentResult.BestRun.Model, experimentResult.BestRun.TrainerName);

            // Save / persist the best model to a.ZIP file.
            SaveModel(mlContext, experimentResult.BestRun.Model);

            // Make a single test prediction loading the model from .ZIP file.
            TestSinglePrediction(mlContext);

            // Paint regression distribution chart for a number of elements read from a Test DataSet file.
           // PlotRegressionChart(mlContext, TestDataPath, 100, args);

            // Re-fit best pipeline on train and test data, to produce 
            // a model that is trained on as much data as is available.
            // This is the final model that can be deployed to production.
            var refitModel = RefitBestPipeline(mlContext, experimentResult, columnInference);

            // Save the re-fit model to a.ZIP file.
            SaveModel(mlContext, refitModel);


 



            Console.WriteLine("Press any key to exit..");
            Console.ReadLine();
        }

        /// <summary>
        /// Infer columns in the dataset with AutoML.
        /// </summary>
        private static ColumnInferenceResults InferColumns(MLContext mlContext)
        {
            ConsoleHelper.ConsoleWriteHeader("=============== Inferring columns in dataset ===============");
            ConsoleHelper.ConsoleWriteHeader(TrainDataPath);

           
            ColumnInferenceResults columnInference = mlContext.Auto().InferColumns(TrainDataPath, LabelColumnName, groupColumns: false, separatorChar:'|');
            ConsoleHelper.Print(columnInference);
            return columnInference;
        }

        /// <summary>
        /// Load data from files using inferred columns.
        /// </summary>
        private static void LoadData(MLContext mlContext, ColumnInferenceResults columnInference)
        {
            TextLoader textLoader = mlContext.Data.CreateTextLoader(columnInference.TextLoaderOptions);
            TrainDataView = textLoader.Load(TrainDataPath);
            TestDataView = textLoader.Load(TestDataPath);
        }

        private static ExperimentResult<RegressionMetrics> RunAutoMLExperiment(MLContext mlContext, 
            ColumnInferenceResults columnInference)
        {
            // STEP 1: Display first few rows of the training data.
            ConsoleHelper.ShowDataViewInConsole(mlContext, TrainDataView);

            // STEP 2: Build a pre-featurizer for use in the AutoML experiment.
            // (Internally, AutoML uses one or more train/validation data splits to 
            // evaluate the models it produces. The pre-featurizer is fit only on the 
            // training data split to produce a trained transform. Then, the trained transform 
            // is applied to both the train and validation data splits.)
           IEstimator<ITransformer> preFeaturizer = mlContext.Transforms.Conversion.MapValue("is_cash",
                new[] { new KeyValuePair<string, bool>("CSH", true) }, "payment_type");

            // STEP 3: Customize column information returned by InferColumns API.
            ColumnInformation columnInformation = columnInference.ColumnInformation;
            // columnInformation.IgnoredColumnNames.Add("sma6");
            // columnInformation.IgnoredColumnNames.Add("sma4");
            /*
            columnInformation.CategoricalColumnNames.Remove("date");
            columnInformation.TextColumnNames.Remove("date");
           */  /* columnInformation.NumericColumnNames.Remove("oscporc");
             columnInformation.NumericColumnNames.Remove("xab");
             columnInformation.NumericColumnNames.Remove("abc");
            // columnInformation.NumericColumnNames.Remove("bcd");
             columnInformation.NumericColumnNames.Remove("maximo");
             columnInformation.NumericColumnNames.Remove("operacion");
             columnInformation.NumericColumnNames.Remove("zonaposicion");
             columnInformation.NumericColumnNames.Remove("enzona");
             columnInformation.NumericColumnNames.Remove("retracement");
             columnInformation.NumericColumnNames.Remove("oscvalor");
             columnInformation.NumericColumnNames.Remove("oscvalor70");
             columnInformation.NumericColumnNames.Remove("oscstatus");
             columnInformation.NumericColumnNames.Remove("wxa");
             columnInformation.NumericColumnNames.Remove("wbc");
             columnInformation.NumericColumnNames.Remove("max1000");
             columnInformation.NumericColumnNames.Remove("vh1");
             columnInformation.NumericColumnNames.Remove("vh2");
             columnInformation.NumericColumnNames.Remove("vol1");
             columnInformation.NumericColumnNames.Remove("vol2");
             columnInformation.NumericColumnNames.Remove("ccia");
             columnInformation.NumericColumnNames.Remove("ccic");
             columnInformation.NumericColumnNames.Remove("time");
             columnInformation.NumericColumnNames.Remove("osca");
             columnInformation.NumericColumnNames.Remove("oscb");
             columnInformation.NumericColumnNames.Remove("oscw");
            // columnInformation.NumericColumnNames.Remove("abc2");
            // columnInformation.NumericColumnNames.Remove("retracement2");
             columnInformation.NumericColumnNames.Remove("atrf");
             columnInformation.NumericColumnNames.Remove("atrl");
             columnInformation.NumericColumnNames.Remove("emaf");
             columnInformation.NumericColumnNames.Remove("emam");
             columnInformation.NumericColumnNames.Remove("emal");
             columnInformation.NumericColumnNames.Remove("dm");
             columnInformation.NumericColumnNames.Remove("dip");
             columnInformation.NumericColumnNames.Remove("din");
             columnInformation.NumericColumnNames.Remove("sma6");
             columnInformation.NumericColumnNames.Remove("sma4");
             //columnInformation.CategoricalColumnNames.Remove("payment_type");
             // columnInformation.IgnoredColumnNames.Add("payment_type");
 */
            preFeaturizer = null;

            //columnInformation = null;

            // STEP 4: Initialize a cancellation token source to stop the experiment.
            var cts = new CancellationTokenSource();

            // STEP 5: Initialize our user-defined progress handler that AutoML will 
            // invoke after each model it produces and evaluates.
            var progressHandler = new RegressionExperimentProgressHandler();

            // STEP 6: Create experiment settings
            var experimentSettings = CreateExperimentSettings(mlContext, cts);

            // STEP 7: Run AutoML regression experiment.
            var experiment = mlContext.Auto().CreateRegressionExperiment(experimentSettings);
            ConsoleHelper.ConsoleWriteHeader("=============== Running AutoML experiment ===============");
            Console.WriteLine($"Running AutoML regression experiment...");
            var stopwatch = Stopwatch.StartNew();
            // Cancel experiment after the user presses any key.
            CancelExperimentAfterAnyKeyPress(cts);
            ExperimentResult<RegressionMetrics> experimentResult = experiment.Execute(TrainDataView, columnInformation, preFeaturizer, progressHandler);
            Console.WriteLine($"{experimentResult.RunDetails.Count()} models were returned after {stopwatch.Elapsed.TotalSeconds:0.00} seconds{Environment.NewLine}");

            // Print top models found by AutoML.
            PrintTopModels(experimentResult);

            return experimentResult;
        }

        /// <summary>
        /// Create AutoML regression experiment settings.
        /// </summary>
        private static RegressionExperimentSettings CreateExperimentSettings(MLContext mlContext, 
            CancellationTokenSource cts)
        {
            var experimentSettings = new RegressionExperimentSettings();
            experimentSettings.MaxExperimentTimeInSeconds = 240;
            experimentSettings.CancellationToken = cts.Token;

            // Set the metric that AutoML will try to optimize over the course of the experiment.
            experimentSettings.OptimizingMetric = (RegressionMetric) iMetrics ;

            // Set the cache directory to null.
            // This will cause all models produced by AutoML to be kept in memory 
            // instead of written to disk after each run, as AutoML is training.
            // (Please note: for an experiment on a large dataset, opting to keep all 
            // models trained by AutoML in memory could cause your system to run out 
            // of memory.)
            experimentSettings.CacheDirectory = null;

            // Don't use LbfgsPoissonRegression and OnlineGradientDescent trainers during this experiment.
            // (These trainers sometimes underperform on this dataset.)
            experimentSettings.Trainers.Remove(RegressionTrainer.LbfgsPoissonRegression);
            experimentSettings.Trainers.Remove(RegressionTrainer.OnlineGradientDescent);

            return experimentSettings;
        }
        
        /// <summary>
        /// Print top models from AutoML experiment.
        /// </summary>
        private static void PrintTopModels(ExperimentResult<RegressionMetrics> experimentResult)
        {
            // Get top few runs ranked by root mean squared error.
            var topRuns = experimentResult.RunDetails
                .Where(r => r.ValidationMetrics != null && !double.IsNaN(r.ValidationMetrics.RootMeanSquaredError))
                .OrderBy(r => r.ValidationMetrics.RootMeanSquaredError).Take(3);

            Console.WriteLine("Top models ranked by root mean squared error --");
            ConsoleHelper.PrintRegressionMetricsHeader();
            for (var i = 0; i < topRuns.Count(); i++)
            {
                var run = topRuns.ElementAt(i);
                ConsoleHelper.PrintIterationMetrics(i + 1, run.TrainerName, run.ValidationMetrics, run.RuntimeInSeconds);
            }
        }

        /// <summary>
        /// Re-fit best pipeline on all available data.
        /// </summary>
        private static ITransformer RefitBestPipeline(MLContext mlContext, ExperimentResult<RegressionMetrics> experimentResult,
            ColumnInferenceResults columnInference)
        {
            ConsoleHelper.ConsoleWriteHeader("=============== Re-fitting best pipeline ===============");
            var textLoader = mlContext.Data.CreateTextLoader(columnInference.TextLoaderOptions);
            var combinedDataView = textLoader.Load(new MultiFileSource(TrainDataPath, TestDataPath));
            RunDetail<RegressionMetrics> bestRun = experimentResult.BestRun;
            return bestRun.Estimator.Fit(combinedDataView);
        }

        /// <summary>
        /// Evaluate the model and print metrics.
        /// </summary>
        private static void EvaluateModel(MLContext mlContext, ITransformer model, string trainerName)
        {
            ConsoleHelper.ConsoleWriteHeader("===== Evaluating model's accuracy with test data =====");
            IDataView predictions = model.Transform(TestDataView);
            var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: LabelColumnName, scoreColumnName: "Score");
            ConsoleHelper.PrintRegressionMetrics(trainerName, metrics);
        }

        /// <summary>
        /// Save/persist the best model to a .ZIP file
        /// </summary>
        private static void SaveModel(MLContext mlContext, ITransformer model)
        {
            ConsoleHelper.ConsoleWriteHeader("=============== Saving the model ===============");
            mlContext.Model.Save(model, TrainDataView.Schema, ModelPath);
            Console.WriteLine($"The model is saved to {ModelPath}");
        }

        private static void CancelExperimentAfterAnyKeyPress(CancellationTokenSource cts)
        {
            Task.Run(() =>
            {
                Console.WriteLine("Press any key to stop the experiment run...");
                Console.ReadKey();
                cts.Cancel();
            });
        }

        private static void TestSinglePrediction(MLContext mlContext)
        {
            ConsoleHelper.ConsoleWriteHeader("=============== Testing prediction engine ===============");

            // Sample: 
            // vendor_id,rate_code,passenger_count,trip_time_in_secs,trip_distance,payment_type,fare_amount
            //


            var taxiTripSample = new AXabc()
            {

                oscporc = 48.04f,
                xab = 1.36f,
                abc = 0.57f,
                bcd = 0.87f,
                maximo = 26.02f,
                operacion = 0f,
                zonaposicion = 9.35f,
                enzona = 1f,
                retracement = 200f,
                oscvalor = -0.148f,
                oscvalor70 = -0.242f,
                oscstatus = -4f,
                wxa = 1.02f,
                wbc = 1.77f,
                max1000 = 65.43f,
                vh1 = 0.97f,
                vh2 = 1f,
                vol1 = 47693f,
                vol2 = 8485f,
                ccia = 109.63f,
                ccic = -25.66f,
                time = 131000f,
                osca = 0.16f,
                oscb = -0.5f,
                oscw = -0.08f,
                abc2 = 3f,
                retracement2 = 5f,
                atrf = 0.54f,
                atrl = 0.92f,
                emaf = 1f,
                emam = 1f,
                emal = 1f,
                dm = 28.87f,
                dip = 15.59f,
                din = 21.53f,
                sma6 = 1f,
                sma4 = 1f,

            };


            ITransformer trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);

            // Create prediction engine related to the loaded trained model.
            var predEngine = mlContext.Model.CreatePredictionEngine<AXabc, TaxiTripFarePrediction>(trainedModel);

            // Score.
            var predictedResult = predEngine.Predict(taxiTripSample);

            Console.WriteLine("**********************************************************************");
            Console.WriteLine($"Predicted fare: {predictedResult.FareAmount:0.####}, actual fare: 0.87");
            Console.WriteLine("**********************************************************************");
        }

        private static void PlotRegressionChart(MLContext mlContext,                                               
                                                string testDataSetPath,
                                                int numberOfRecordsToRead,
                                                string[] args)
        {
            ITransformer trainedModel;
            using (var stream = new FileStream(ModelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                trainedModel = mlContext.Model.Load(stream, out var modelInputSchema);
            }

            // Create prediction engine related to the loaded trained model
            var predFunction = mlContext.Model.CreatePredictionEngine<TaxiTrip, TaxiTripFarePrediction>(trainedModel);

            string chartFileName = "";
            using (var pl = new PLStream())
            {
                // use SVG backend and write to SineWaves.svg in current directory.
                if (args.Length == 1 && args[0] == "svg")
                {
                    pl.sdev("svg");
                    chartFileName = "TaxiRegressionDistribution.svg";
                    pl.sfnam(chartFileName);
                }
                else
                {
                    pl.sdev("pngcairo");
                    chartFileName = "TaxiRegressionDistribution.png";
                    pl.sfnam(chartFileName);
                }

                // use white background with black foreground.
                pl.spal0("cmap0_alternate.pal");

                // Initialize plplot.
                pl.init();

                // Set axis limits.
                const int xMinLimit = 0;
                const int xMaxLimit = 35; // Rides larger than $35 are not shown in the chart.
                const int yMinLimit = 0;
                const int yMaxLimit = 35;  // Rides larger than $35 are not shown in the chart.
                pl.env(xMinLimit, xMaxLimit, yMinLimit, yMaxLimit, AxesScale.Independent, AxisBox.BoxTicksLabelsAxes);

                // Set scaling for mail title text 125% size of default.
                pl.schr(0, 1.25);

                // The main title.
                pl.lab("Measured", "Predicted", "Distribution of Taxi Fare Prediction");

                // plot using different colors
                // see http://plplot.sourceforge.net/examples.php?demo=02 for palette indices
                pl.col0(1);

                int totalNumber = numberOfRecordsToRead;
                var testData = new TaxiTripCsvReader().GetDataFromCsv(testDataSetPath, totalNumber).ToList();

                // This code is the symbol to paint
                var code = (char)9;

                // plot using other color
                //pl.col0(9); //Light Green
                //pl.col0(4); //Red
                pl.col0(2); //Blue

                double yTotal = 0;
                double xTotal = 0;
                double xyMultiTotal = 0;
                double xSquareTotal = 0;

                for (int i = 0; i < testData.Count; i++)
                {
                    var x = new double[1];
                    var y = new double[1];

                    // Make Prediction.
                    var farePrediction = predFunction.Predict(testData[i]);

                    x[0] = testData[i].FareAmount;
                    y[0] = farePrediction.FareAmount;

                    // Paint a dot
                    pl.poin(x, y, code);

                    xTotal += x[0];
                    yTotal += y[0];

                    double multi = x[0] * y[0];
                    xyMultiTotal += multi;

                    double xSquare = x[0] * x[0];
                    xSquareTotal += xSquare;

                    double ySquare = y[0] * y[0];

                    Console.WriteLine("-------------------------------------------------");
                    Console.WriteLine($"Predicted : {farePrediction.FareAmount}");
                    Console.WriteLine($"Actual:    {testData[i].FareAmount}");
                    Console.WriteLine("-------------------------------------------------");
                }

                // Regression Line calculation explanation:
                // https://www.khanacademy.org/math/statistics-probability/describing-relationships-quantitative-data/more-on-regression/v/regression-line-example

                double minY = yTotal / totalNumber;
                double minX = xTotal / totalNumber;
                double minXY = xyMultiTotal / totalNumber;
                double minXsquare = xSquareTotal / totalNumber;

                double m = ((minX * minY) - minXY) / ((minX * minX) - minXsquare);

                double b = minY - (m * minX);

                // Generic function for Y for the regression line
                // y = (m * x) + b;

                double x1 = 1;

                // Function for Y1 in the line
                double y1 = (m * x1) + b;

                double x2 = 39;

                // Function for Y2 in the line
                double y2 = (m * x2) + b;

                var xArray = new double[2];
                var yArray = new double[2];
                xArray[0] = x1;
                yArray[0] = y1;
                xArray[1] = x2;
                yArray[1] = y2;

                pl.col0(4);
                pl.line(xArray, yArray);

                // End page (writes output to disk)
                pl.eop();

                // Output version of PLplot
                pl.gver(out var verText);
                Console.WriteLine("PLplot version " + verText);

            } // The pl object is disposed here

            // Open chart file in Microsoft Photos App (or default app for .svg or .png, like browser)

            Console.WriteLine("Showing chart...");
            var p = new Process();
            string chartFileNamePath = @".\" + chartFileName;
            p.StartInfo = new ProcessStartInfo(chartFileNamePath)
            {
                UseShellExecute = true
            };
            p.Start();
        }

        public static string GetAbsolutePath(string relativePath)
        {
            var _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
    }

    public class TaxiTripCsvReader
    {
        public IEnumerable<TaxiTrip> GetDataFromCsv(string dataLocation, int numMaxRecords)
        {
            IEnumerable<TaxiTrip> records =
                File.ReadAllLines(dataLocation)
                .Skip(1)
                .Select(x => x.Split(','))
                .Select(x => new TaxiTrip()
                {
                    VendorId = x[0],
                    RateCode = float.Parse(x[1]),
                    PassengerCount = float.Parse(x[2]),
                    TripTime = float.Parse(x[3]),
                    TripDistance = float.Parse(x[4]),
                    PaymentType = x[5],
                    FareAmount = float.Parse(x[6])
                })
                .Take<TaxiTrip>(numMaxRecords);

            return records;
        }
    }

}
