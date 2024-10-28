using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using System.Collections.Generic;
using System.IO;

namespace DvhProcessing
{
    public class DVHPlotter
    {
       
        public void PlotDVHToFile(string structureName, List<double> cumulativeDvh, double minDose, double binSize, string outputFilePath)
        {
            var plotModel = CreateDVHPlotModel(structureName, cumulativeDvh, minDose, binSize);

            
            using (var stream = File.Create(outputFilePath))
            {
                var pngExporter = new OxyPlot.SkiaSharp.PngExporter { Width = 800, Height = 600 };
                pngExporter.Export(plotModel, stream);
            }
        }

        
        public PlotModel CreateDVHPlotModel(string structureName, List<double> cumulativeDvh, double minDose, double binSize)
        {
            if (cumulativeDvh == null)
                throw new ArgumentNullException(nameof(cumulativeDvh));

            var plotModel = new PlotModel { Title = $"DVH for {structureName}" };

           
            plotModel.Background = OxyColors.White;

          
            var lineSeries = new LineSeries
            {
                Title = structureName,
                StrokeThickness = 2,
                Color = OxyColors.Blue,
                CanTrackerInterpolatePoints = false
             
            };

           
            for (int i = 0; i < cumulativeDvh.Count; i++)
            {
                double dose = minDose + i * binSize;
                double volumePercentage = cumulativeDvh[i] * 100; // Convert fraction to percentage
                lineSeries.Points.Add(new DataPoint(dose, volumePercentage));
            }

            
            plotModel.Series.Add(lineSeries);

            // Configure X and Y axes
            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Dose (Gy)",
                Minimum = minDose,
                Maximum = minDose + cumulativeDvh.Count * binSize
            });

            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Volume (%)",
                Minimum = 0,
                Maximum = 100
            });

            return plotModel;
        }
    }
}
