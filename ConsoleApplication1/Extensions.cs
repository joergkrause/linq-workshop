using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ConsoleApplication1
{
    public static class Extensions
    {

        # region Trivia

        public static IEnumerable<string> GetChunk(this StreamReader sr)
        {
            string row;
            if (sr == null) throw new ArgumentNullException("sr");
            while ((row = sr.ReadLine()) != null)
            {
                yield return row;
            }
        }

        # endregion

    }
}
