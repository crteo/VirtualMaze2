using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VirtualMaze.Assets.Scripts.Utils
{
    public abstract class TwoDArrayUtils
    {
        public static T[] GetColumn<T>(T[,] matrix, int columnNumber)
        {
            
            return Enumerable.Range(0, matrix.GetLength(1))
                .Select(x => matrix[columnNumber, x])
                .ToArray();
        }

        public T[] GetRow<T>(T[,] matrix, int rowNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(0))
                .Select(x => matrix[rowNumber, x])
                .ToArray();
        }
    }
}
