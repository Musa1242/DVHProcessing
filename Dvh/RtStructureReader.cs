using FellowOakDicom;
using System;
using System.Collections.Generic;
using System.IO;
using OpenCvSharp;
using System.Linq; 
using OpenCvPoint = OpenCvSharp.Point;
namespace DvhProcessing
{
    public class RtStructureReader
    {
        private List<StructureData> _structures;
        private Dictionary<string, List<(int zIndex, Mat mask)>> _binaryMasks; 
        private CoordinateMapper coordinateMapper;

        public RtStructureReader()
        {
            _structures = new List<StructureData>();
            _binaryMasks = new Dictionary<string, List<(int zIndex, Mat mask)>>(); 
        }

        
        public void SetCoordinateMapper(CoordinateMapper mapper)
        {
            this.coordinateMapper = mapper;
        }

        public void Read(string filePath)
        {
            _structures = new List<StructureData>();
            _binaryMasks = new Dictionary<string, List<(int zIndex, Mat mask)>>();

            try
            {
                var dicomFile = DicomFile.Open(filePath);
                var dataset = dicomFile.Dataset;

                var structureSetROISequence = dataset.GetSequence(DicomTag.StructureSetROISequence);
                var roiContourSequence = dataset.GetSequence(DicomTag.ROIContourSequence);
                if (roiContourSequence == null)
                {
                    Console.WriteLine("ROIContourSequence not found in RT-STRUCTURE file.");
                    //
                    //
                    return;
                }

                foreach (var roiContourItem in roiContourSequence.Items)
                {
                    var referencedROINumber = roiContourItem.GetSingleValue<int>(DicomTag.ReferencedROINumber);
                    var contourSequence = roiContourItem.GetSequence(DicomTag.ContourSequence);

                    
                    var structureItem = structureSetROISequence.Items.FirstOrDefault(
                        item => item.GetSingleValue<int>(DicomTag.ROINumber) == referencedROINumber);

                    if (structureItem == null)
                    {
                        Console.WriteLine($"Structure with ROINumber {referencedROINumber} not found in StructureSetROISequence.");
                        continue;
                    }

                    string structureName = structureItem.GetSingleValue<string>(DicomTag.ROIName);

                    var structureData = new StructureData(structureName);
                    var masks = new List<(int zIndex, Mat mask)>();

                    foreach (var contourItem in contourSequence)
                    {
                        var contourData = contourItem.GetValues<double>(DicomTag.ContourData);
                        int numPoints = contourData.Length / 3;
                        if (numPoints == 0)
                            continue;

                        double zPos = contourData[2]; 

                       
                        int zIndex = coordinateMapper.GetZIndexFromZPosition(zPos);
                        if (zIndex < 0 || zIndex >= coordinateMapper.GridSize[2])
                            continue; 

                        var points = new List<OpenCvPoint>();

                        for (int i = 0; i < numPoints; i++)
                        {
                            double x = contourData[i * 3];
                            double y = contourData[i * 3 + 1];
                            

                            // Convert patient coordinates to voxel indices
                            int[] indices = coordinateMapper.PatientCoordinatesToIndex(x, y, zPos);

                            int xIndex = indices[0];
                            int yIndex = indices[1];

                            if (xIndex < 0 || xIndex >= coordinateMapper.GridSize[0] ||
                                yIndex < 0 || yIndex >= coordinateMapper.GridSize[1])
                                continue;

                            points.Add(new OpenCvPoint(xIndex, yIndex));
                            structureData.AddVoxelIndex(xIndex, yIndex, zIndex);
                        }

                        if (points.Count == 0)
                            continue;

                        // Create a binary mask for the current contour slice
                        Mat mask = new Mat(coordinateMapper.GridSize[1], coordinateMapper.GridSize[0], MatType.CV_8UC1, Scalar.All(0)); // Rows, Columns
                        Cv2.FillPoly(mask, new[] { points.ToArray() }, Scalar.All(1));
                        masks.Add((zIndex, mask));
                    }

                    if (masks.Count > 0)
                    {
                        _structures.Add(structureData);
                        _binaryMasks[structureName] = masks;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while reading the RT-STRUCTURE file: {ex.Message}");
            }
        }

        public List<StructureData> GetStructures()
        {
            return _structures;
        }

        
        public Dictionary<string, List<(int zIndex, Mat mask)>> GetBinaryMasks()
        {
            return _binaryMasks;
        }
    }
}






