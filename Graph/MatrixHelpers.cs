using System;
using System.Linq;
using System.Text;

namespace Graph
{
    public static class MatrixHelpers
    {
        public static T[] GetColumn<T>(this T[,] matrix, int columnNumber)
            => Enumerable.Range(0, matrix.GetLength(0))
                .Select(x => matrix[x, columnNumber])
                .ToArray();

        public static T[] GetRow<T>(this T[,] matrix, int rowNumber)
            => Enumerable.Range(0, matrix.GetLength(1))
                .Select(x => matrix[rowNumber, x])
                .ToArray();

        public static string Print2DArray<T>(this T[,] matrix)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < matrix.GetLength(0); i++) // rows
            {
                Console.Write(i + "\t");
                for (int j = 0; j < matrix.GetLength(1); j++) // columns
                {

                    sb.Append(matrix[i, j] + "\t");
                    Console.Write(matrix[i, j] + "\t");
                }
                sb.Append("\n");
                Console.WriteLine();
            }

            return sb.ToString();
        }
    }
}
