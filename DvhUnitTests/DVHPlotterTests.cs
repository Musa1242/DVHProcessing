using NUnit.Framework;
using DvhProcessing;
using OxyPlot;
using OxyPlot.Series;
using System.Collections.Generic;
using System.IO;

namespace DvhProcessing.Tests
{
    [TestFixture]
    public class DVHPlotterTests
    {
        private DVHPlotter _plotter;

        [SetUp]
        public void Setup()
        {
            _plotter = new DVHPlotter();
        }

        [Test]
        public void TestPlotModelCreation()
        {
            
            string structureName = "TestStructure";
            List<double> cumulativeDvh = new List<double> { 1.0, 0.8, 0.6, 0.4, 0.2, 0.0 };
            double minDose = 0.0;
            double binSize = 2.0;

           
            var plotModel = _plotter.CreateDVHPlotModel(structureName, cumulativeDvh, minDose, binSize);

            
            Assert.That(plotModel, Is.Not.Null, "PlotModel should not be null.");
            Assert.That(plotModel.Title, Is.EqualTo($"DVH for {structureName}"), "Plot title does not match.");
            Assert.That(plotModel.Series.Count, Is.EqualTo(1), "PlotModel should contain one series.");
        }

        [Test]
        public void TestPlotDVHToFile()
        {
          
            string structureName = "TestStructure";
            List<double> cumulativeDvh = new List<double> { 1.0, 0.75, 0.5, 0.25, 0.0 };
            double minDose = 0.0;
            double binSize = 1.0;
            string outputFilePath = Path.Combine(Path.GetTempPath(), "TestStructure_DVH.png");

           
            _plotter.PlotDVHToFile(structureName, cumulativeDvh, minDose, binSize, outputFilePath);

         
            Assert.That(File.Exists(outputFilePath), Is.True, "DVH plot file was not created.");

          
            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }
        }

        [Test]
        public void TestPlotDataPoints()
        {
           
            string structureName = "TestStructure";
            List<double> cumulativeDvh = new List<double> { 1.0, 0.8, 0.6 };
            double minDose = 0.0;
            double binSize = 2.0;

      
            var plotModel = _plotter.CreateDVHPlotModel(structureName, cumulativeDvh, minDose, binSize);

          
            Assert.That(plotModel.Series.Count, Is.EqualTo(1), "PlotModel should contain one series.");
            var lineSeries = plotModel.Series[0] as LineSeries;
            Assert.That(lineSeries, Is.Not.Null, "LineSeries should not be null.");
            Assert.That(lineSeries.Points.Count, Is.EqualTo(cumulativeDvh.Count), "Number of points in LineSeries does not match cumulative DVH data count.");

            for (int i = 0; i < cumulativeDvh.Count; i++)
            {
                double expectedDose = minDose + i * binSize;
                double expectedVolume = cumulativeDvh[i] * 100; // Convert fraction to percentage
                var point = lineSeries.Points[i];

                Assert.That(point.X, Is.EqualTo(expectedDose).Within(1e-6), $"Dose at point {i} does not match.");
                Assert.That(point.Y, Is.EqualTo(expectedVolume).Within(1e-6), $"Volume at point {i} does not match.");
            }
        }

        [Test]
        public void TestPlotModelCreationWithEmptyData()
        {
          
            string structureName = "EmptyStructure";
            List<double> cumulativeDvh = new List<double>();
            double minDose = 0.0;
            double binSize = 1.0;

           
            var plotModel = _plotter.CreateDVHPlotModel(structureName, cumulativeDvh, minDose, binSize);

          
            Assert.That(plotModel, Is.Not.Null, "PlotModel should not be null even with empty data.");
            Assert.That(plotModel.Series.Count, Is.EqualTo(1), "PlotModel should contain one series even with empty data.");

            var lineSeries = plotModel.Series[0] as LineSeries;
            Assert.That(lineSeries.Points.Count, Is.EqualTo(0), "LineSeries should contain no points with empty data.");
        }

        [Test]
        public void TestPlotModelCreationWithNullData()
        {
           
            string structureName = "NullStructure";
            List<double> cumulativeDvh = null;
            double minDose = 0.0;
            double binSize = 1.0;

         
            Assert.That(() => _plotter.CreateDVHPlotModel(structureName, cumulativeDvh, minDose, binSize), Throws.ArgumentNullException, "Creating a plot with null DVH data should throw an ArgumentNullException.");
        }
    }
}
