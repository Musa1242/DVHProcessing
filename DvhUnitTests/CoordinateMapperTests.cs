using NUnit.Framework;
using DvhProcessing;

namespace DvhProcessing.Tests
{
    [TestFixture]
    public class CoordinateMapperTests
    {
        private CoordinateMapper mapper;

        [SetUp]
        public void Setup()
        {
            
            double[] imagePositionPatient = { 0.0, 0.0, 0.0 };
            double[] imageOrientationPatient = { 1.0, 0.0, 0.0, 0.0, 1.0, 0.0 };
            double[] pixelSpacing = { 1.0, 1.0 };
            double[] gridFrameOffsetVector = { 0.0, 1.0, 2.0 };
            int[] gridSize = { 10, 10, 3 };

            mapper = new CoordinateMapper(imagePositionPatient, imageOrientationPatient, pixelSpacing, gridFrameOffsetVector, gridSize);
        }

        [Test]
        public void IndexToPatientCoordinates_ShouldReturnCorrectCoordinates()
        {
            int xIndex = 5;
            int yIndex = 5;
            int zIndex = 1;

            double[] patientCoords = mapper.IndexToPatientCoordinates(xIndex, yIndex, zIndex);

            Assert.That(patientCoords[0], Is.EqualTo(5.0));
            Assert.That(patientCoords[1], Is.EqualTo(5.0));
            Assert.That(patientCoords[2], Is.EqualTo(1.0));
        }

        [Test]
        public void PatientCoordinatesToIndex_ShouldReturnCorrectIndices()
        {
            double x = 5.0;
            double y = 5.0;
            double z = 1.0;

            int[] indices = mapper.PatientCoordinatesToIndex(x, y, z);

            Assert.That(indices[0], Is.EqualTo(5));
            Assert.That(indices[1], Is.EqualTo(5));
            Assert.That(indices[2], Is.EqualTo(1));
        }
    }
}
