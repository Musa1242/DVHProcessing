using System;
using System.Collections.Generic;
using System.Linq;

namespace DvhProcessing
{
    public class DvhData
    {
        public string StructureName { get; set; }
        public List<double> DoseValues { get; set; }
        public List<double> DifferentialDvh { get; set; }
        public List<double> CumulativeDvh { get; set; }
        public double MinDose { get; private set; }
        public double MaxDose { get; private set; }
        public double BinSize { get; private set; }

        public DvhData(string structureName, List<double> doseValues, List<double> volumeValues)
        {
            StructureName = structureName;
            DoseValues = doseValues;
            DifferentialDvh = new List<double>();
            CumulativeDvh = new List<double>();
        }

        public void CalculateDvh()
        {
            if (DoseValues == null || DoseValues.Count == 0)
                return;

            
            DoseValues.Sort();

            int numBins = 100;
            MaxDose = DoseValues.Max();
            MinDose = DoseValues.Min();
            BinSize = (MaxDose - MinDose) / numBins;

            
            var histogram = new int[numBins];
            foreach (var dose in DoseValues)
            {
                int bin = (int)((dose - MinDose) / BinSize);
                if (bin >= numBins)
                    bin = numBins - 1;
                histogram[bin]++;
            }

            // Calculate differential DVH
            DifferentialDvh = histogram.Select(h => (double)h / DoseValues.Count).ToList();

            // Calculate cumulative DVH
            CumulativeDvh = new List<double>(DifferentialDvh.Count);
            double cumulative = 1.0;
            for (int i = 0; i < DifferentialDvh.Count; i++)
            {
                cumulative -= DifferentialDvh[i];
                CumulativeDvh.Add(cumulative);
            }
        }
    }
}
