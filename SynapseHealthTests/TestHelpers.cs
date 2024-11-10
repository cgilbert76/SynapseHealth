using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SynapseHealthTests
{
    public class TestHelpers
    {
        public static string ReadFile(string file, [CallerFilePath] string filePath = "")
        {
            var directoryPath = Path.GetDirectoryName(filePath);
            var fullPath = Path.Join(directoryPath, file);
            return File.ReadAllText(fullPath);
        }
    }
}
