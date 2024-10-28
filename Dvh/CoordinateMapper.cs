using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DvhProcessing
{
    public class CoordinateMapper
    {
        public double[] ImagePositionPatient { get; private set; } // (X, Y, Z) of first voxel
        public double[] ImageOrientationPatient { get; private set; } // 6 values
        public double[] PixelSpacing { get; private set; } // (dx, dy)
        public double[] GridFrameOffsetVector { get; private set; } // array of z positions
        public int[] GridSize { get; private set; } // (Rows, Columns, Number of Frames)

        private double[,] directionCosines; // 3x3 matrix

        public CoordinateMapper(double[] imagePositionPatient, double[] imageOrientationPatient, double[] pixelSpacing, double[] gridFrameOffsetVector, int[] gridSize)
        {
            this.ImagePositionPatient = imagePositionPatient;
            this.ImageOrientationPatient = imageOrientationPatient;
            this.PixelSpacing = pixelSpacing;
            this.GridFrameOffsetVector = gridFrameOffsetVector;
            this.GridSize = gridSize;


            // Build the direction cosines matrix
            directionCosines = new double[3, 3];
            directionCosines[0, 0] = imageOrientationPatient[0];
            directionCosines[0, 1] = imageOrientationPatient[1];
            directionCosines[0, 2] = imageOrientationPatient[2];
            directionCosines[1, 0] = imageOrientationPatient[3];
            directionCosines[1, 1] = imageOrientationPatient[4];
            directionCosines[1, 2] = imageOrientationPatient[5];

            // Compute the slice direction cosines (cross product of row and column direction cosines)
            directionCosines[2, 0] = directionCosines[0, 1] * directionCosines[1, 2] - directionCosines[0, 2] * directionCosines[1, 1];
            directionCosines[2, 1] = directionCosines[0, 2] * directionCosines[1, 0] - directionCosines[0, 0] * directionCosines[1, 2];
            directionCosines[2, 2] = directionCosines[0, 0] * directionCosines[1, 1] - directionCosines[0, 1] * directionCosines[1, 0];
        }

        public double[] IndexToPatientCoordinates(int xIndex, int yIndex, int zIndex)
        {
            double x = ImagePositionPatient[0] + xIndex * PixelSpacing[0] * directionCosines[0, 0] + yIndex * PixelSpacing[1] * directionCosines[1, 0] + GridFrameOffsetVector[zIndex] * directionCosines[2, 0];
            double y = ImagePositionPatient[1] + xIndex * PixelSpacing[0] * directionCosines[0, 1] + yIndex * PixelSpacing[1] * directionCosines[1, 1] + GridFrameOffsetVector[zIndex] * directionCosines[2, 1];
            double z = ImagePositionPatient[2] + xIndex * PixelSpacing[0] * directionCosines[0, 2] + yIndex * PixelSpacing[1] * directionCosines[1, 2] + GridFrameOffsetVector[zIndex] * directionCosines[2, 2];
            return new double[] { x, y, z };
        }

        public int[] PatientCoordinatesToIndex(double x, double y, double z)
        {
            // First, get zIndex
            int zIndex = GetZIndexFromZPosition(z);
            if (zIndex < 0)
                return new int[] { -1, -1, -1 };

            // Compute zOffset
            double zOffset = GridFrameOffsetVector[zIndex];

            // Compute deltaX and deltaY
            double deltaX = x - (ImagePositionPatient[0] + zOffset * directionCosines[2, 0]);
            double deltaY = y - (ImagePositionPatient[1] + zOffset * directionCosines[2, 1]);

            // Set up the 2x2 matrix
            double[,] M = new double[2, 2];
            M[0, 0] = directionCosines[0, 0] * PixelSpacing[0];
            M[0, 1] = directionCosines[1, 0] * PixelSpacing[1];
            M[1, 0] = directionCosines[0, 1] * PixelSpacing[0];
            M[1, 1] = directionCosines[1, 1] * PixelSpacing[1];

            // Invert the matrix
            double det = M[0, 0] * M[1, 1] - M[0, 1] * M[1, 0];
            if (Math.Abs(det) < 1e-6)
                return new int[] { -1, -1, -1 };

            double invDet = 1.0 / det;
            double[,] invM = new double[2, 2];
            invM[0, 0] = M[1, 1] * invDet;
            invM[0, 1] = -M[0, 1] * invDet;
            invM[1, 0] = -M[1, 0] * invDet;
            invM[1, 1] = M[0, 0] * invDet;

         
            double xIndex = invM[0, 0] * deltaX + invM[0, 1] * deltaY;
            double yIndex = invM[1, 0] * deltaX + invM[1, 1] * deltaY;

           
            int xi = (int)Math.Round(xIndex);
            int yi = (int)Math.Round(yIndex);

            return new int[] { xi, yi, zIndex };
        }

        public int GetZIndexFromZPosition(double zPosition)
        {
            // Find the zIndex where gridFrameOffsetVector[zIndex] + imagePositionPatient[2] is closest to zPosition
            double minDifference = double.MaxValue;
            int closestZIndex = -1;
            for (int z = 0; z < GridFrameOffsetVector.Length; z++)
            {
                double zGridPosition = ImagePositionPatient[2] + GridFrameOffsetVector[z];
                double difference = Math.Abs(zGridPosition - zPosition);
                if (difference < minDifference)
                {
                    minDifference = difference;
                    closestZIndex = z;
                }
            }
            return closestZIndex;
        }
    }
}
