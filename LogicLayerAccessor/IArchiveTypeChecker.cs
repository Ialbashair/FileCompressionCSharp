using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataObjects;

namespace LogicLayerInterface
{
    public interface IArchiveTypeChecker
    {
        ArchiveType GetArchiveType(string filePath);
    }
}
