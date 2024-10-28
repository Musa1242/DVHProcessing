using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DvhProcessing
{
    public interface IDicomReader
    {
        void Read(string filePath);
        List<DvhData> GetDvhData();
    }
}
