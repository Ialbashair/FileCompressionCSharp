using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogicLayerInterface
{
    public interface ISlidingWindow
    {
        Task<bool> Compress(string filePath, string outputPath, CancellationToken ct);

        Task<bool> DeCompress(string filePath, string outputPath, CancellationToken ct);
    }
}
