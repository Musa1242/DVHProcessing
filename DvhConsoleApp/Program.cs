using System;
using System.Collections.Generic;
using DvhProcessing;
using System.IO;

namespace DvhConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            
            string dicomDirectory = @"C:\Users\mtu10\source\repos\GroceryShop\TestCases\Pca1";

            try
            {
                
                DicomReader dicomReader = new DicomReader();

                
                Console.WriteLine($"Processing DICOM files in directory: {dicomDirectory}");
                dicomReader.Read(dicomDirectory);

                
                List<DvhData> dvhDataList = dicomReader.GetDvhData();

                
                if (dvhDataList == null || dvhDataList.Count == 0)
                {
                    Console.WriteLine("No DVH data was generated.");
                }
                else
                {
                    
                    DVHPlotter plotter = new DVHPlotter();

                   
                    string outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "DVHPlots");
                    Directory.CreateDirectory(outputDirectory);

                    
                    foreach (var dvhData in dvhDataList)
                    {
                        Console.WriteLine($"\nStructure: {dvhData.StructureName}");
                        Console.WriteLine($"Number of Dose Values: {dvhData.DoseValues.Count}");
                        Console.WriteLine($"Number of DVH Points: {dvhData.CumulativeDvh.Count}");

                        
                        string outputFilePath = Path.Combine(outputDirectory, $"{dvhData.StructureName}_DVH.png");
                        plotter.PlotDVHToFile(dvhData.StructureName, dvhData.CumulativeDvh, dvhData.MinDose, dvhData.BinSize, outputFilePath);
                        Console.WriteLine($"DVH plot for {dvhData.StructureName} saved to {outputFilePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            
            Console.WriteLine("\nProcessing complete. Press any key to exit.");
            Console.ReadKey();
        }
    }
}
