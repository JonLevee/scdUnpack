using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scdUnpack.Extensions
{
    internal static class GeneralExtensions
    {
        public static void EnsureDirectoryExists(this string path)
        {
            if (Directory.Exists(path))
                return;
            var parent = Path.GetDirectoryName(path);
            Debug.Assert(parent != null);
            EnsureDirectoryExists(parent);
            Directory.CreateDirectory(path);
        }
    }
}
