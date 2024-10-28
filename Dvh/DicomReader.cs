using FellowOakDicom;
using System;
using System.Collections.Generic;
using System.IO;

namespace DvhProcessing
{
    public class DicomReader : IDicomReader
    {
        private List<DvhData> _dvhData;
        private RtStructureReader _rtStructureReader;
        private double[,,] _doseMatrix;
        private int[] _gridSize;
        private double[] _gridSpacing;

        public CoordinateMapper CoordinateMapper { get; private set; } 

        public DicomReader()
        {
            _rtStructureReader = new RtStructureReader();
            _dvhData = new List<DvhData>();
        }

        public void Read(string directoryPath)
        {
            _dvhData = new List<DvhData>();
            Console.WriteLine($"Reading DICOM files from directory: {directoryPath}");

            ReadRtDoseFiles(directoryPath);

        
            if (CoordinateMapper == null)
            {
                throw new InvalidOperationException("CoordinateMapper is not initialized.");
            }
            _rtStructureReader.SetCoordinateMapper(CoordinateMapper);

            ReadRtStructureFiles(directoryPath);
            IntegrateDoseAndStructureData();
        }

        private void ReadRtDoseFiles(string directoryPath)
        {
            var rtDoseFiles = Directory.GetFiles(directoryPath, "RD*.dcm", SearchOption.AllDirectories);
            foreach (var filePath in rtDoseFiles)
            {
                ProcessRtDoseFile(filePath);
            }
        }

        private void ReadRtStructureFiles(string directoryPath)
        {
            var rtStructFiles = Directory.GetFiles(directoryPath, "RS*.dcm", SearchOption.AllDirectories);
            if (rtStructFiles.Length > 0)
            {
                _rtStructureReader.Read(rtStructFiles[0]);
            }
            else
            {
                Console.WriteLine("No RT-STRUCTURE files found in the directory.");
            }
        }

        private void ProcessRtDoseFile(string filePath)
        {
            Console.WriteLine($"Processing file: {filePath}");
            var file = DicomFile.Open(filePath);

            if (IsRtDoseFile(file))
            {
                Console.WriteLine($"RT Dose file found: {filePath}");
                ExtractDoseData(file); 
            }
            else
            {
                Console.WriteLine($"File is not an RT Dose file: {filePath}");
            }
        }

        private void ExtractDoseData(DicomFile file)
        {
            
            _gridSize = new int[3]
            {
        file.Dataset.GetSingleValue<int>(DicomTag.Columns),
        file.Dataset.GetSingleValue<int>(DicomTag.Rows),
        file.Dataset.GetSingleValue<int>(DicomTag.NumberOfFrames)
            };

            // Get grid spacing (mm)
            _gridSpacing = file.Dataset.GetValues<double>(DicomTag.PixelSpacing); // [dy, dx]
            Array.Resize(ref _gridSpacing, 2);
            double[] pixelSpacing = new double[2] { _gridSpacing[1], _gridSpacing[0] }; // [dx, dy]

            
            double[] imagePositionPatient = file.Dataset.GetValues<double>(DicomTag.ImagePositionPatient);

            
            double[] imageOrientationPatient = file.Dataset.GetValues<double>(DicomTag.ImageOrientationPatient);

            
            double[] gridFrameOffsetVector = file.Dataset.GetValues<double>(DicomTag.GridFrameOffsetVector);

            
            CoordinateMapper = new CoordinateMapper(imagePositionPatient, imageOrientationPatient, pixelSpacing, gridFrameOffsetVector, _gridSize);

           

            
            double doseGridScaling = file.Dataset.GetSingleValue<double>(DicomTag.DoseGridScaling);

            
            var doseValues = file.Dataset.GetValues<ushort>(DicomTag.PixelData);

            _doseMatrix = new double[_gridSize[0], _gridSize[1], _gridSize[2]];

            int index = 0;
            for (int z = 0; z < _gridSize[2]; z++)
            {
                for (int y = 0; y < _gridSize[1]; y++)
                {
                    for (int x = 0; x < _gridSize[0]; x++)
                    {
                        
                        _doseMatrix[x, y, z] = doseValues[index++] * doseGridScaling;
                    }
                }
            }
        }

        private bool IsRtDoseFile(DicomFile file)
        {
            var sopClassUid = file.Dataset.GetSingleValue<string>(DicomTag.SOPClassUID);
            return sopClassUid == DicomUID.RTDoseStorage.UID;
        }

        private void IntegrateDoseAndStructureData()
        {
            var structures = _rtStructureReader.GetStructures();
            var binaryMasks = _rtStructureReader.GetBinaryMasks();

            foreach (var structure in structures)
            {
                if (!binaryMasks.ContainsKey(structure.StructureName))
                    continue;

                var masks = binaryMasks[structure.StructureName];
                var doseValues = new List<double>();

                foreach (var (zIndex, mask) in masks)
                {
                    for (int y = 0; y < mask.Rows; y++)
                    {
                        for (int x = 0; x < mask.Cols; x++)
                        {
                            if (mask.At<byte>(y, x) > 0)
                            {
                                if (x >= 0 && x < _gridSize[0] && y >= 0 && y < _gridSize[1] && zIndex >= 0 && zIndex < _gridSize[2])
                                {
                                    doseValues.Add(_doseMatrix[x, y, zIndex]);
                                }
                            }
                        }
                    }
                }

                // Generate DVH Data
                var dvhData = new DvhData(structure.StructureName, doseValues, new List<double>()); 
                dvhData.CalculateDvh();
                _dvhData.Add(dvhData);
            }
        }

        public List<DvhData> GetDvhData()
        {
            return _dvhData;
        }

        // Expose properties for testing
        public int[] GetGridSize()
        {
            return _gridSize;
        }

        public double[] GetGridSpacing()
        {
            return _gridSpacing;
        }

        public double[] GetImagePositionPatient()
        {
            return CoordinateMapper?.ImagePositionPatient;
        }

        public double[] GetImageOrientationPatient()
        {
            return CoordinateMapper?.ImageOrientationPatient;
        }

        public double[] GetGridFrameOffsetVector()
        {
            return CoordinateMapper?.GridFrameOffsetVector;
        }
    }
}
