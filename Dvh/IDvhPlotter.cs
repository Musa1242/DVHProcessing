using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DvhProcessing
{
    public interface IDvhPlotter
    {
        void Plot(List<DvhData> dvhData);
    }
}
