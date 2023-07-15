using System.Text.RegularExpressions;

namespace MoogleTools;
public class Matrix
{
   private double[,] elements;

   public Matrix(double[,] elements)
   {
      if (elements == null)
         throw new ArgumentException("Matrix can't be null");

      this.elements = elements;
   }

   /// Properties
   public int Rows
   {
      get { return this.elements.GetLength(0); }
   }
   public int Columns
   {
      get { return this.elements.GetLength(1); }
   }

   /// Indexer
   public double this[int i, int j]
   {
      get { return this.elements[i, j]; }
   }

   /// Term Frequency
   public static Matrix WordCount(string[] texts, Dictionary<string, int> dictionary)
   {
      /*** Creando la matriz de los terminos donde cada fila corresponde
       a un documento y cada columna a los terminos del diccionario ***/
      double[,] result = new double[texts.Length, dictionary.Count];
      int aux;

      /// Recorriendo los documentos y el diccionario para tomar el TF de los terminos en los documentos
      for (int i = 0; i < texts.Length; i++)
      {
         aux = 0;
         foreach (KeyValuePair<string, int> word in dictionary)
         {
            result[i, aux] = Regex.Matches(texts[i], word.Key).Count;
            aux++;
         }
      }

      return new Matrix(result);
   }

   public static Matrix CalculateTimeFrecuency(Matrix matrix)
   {
      /*** Creando la matriz de los terminos donde cada fila corresponde
       a un documento y cada columna a los terminos del diccionario ***/
      double[,] result = new double[matrix.Rows, matrix.Columns];

      /// Por cada fila de la matriz (cada documento)
      for (int i = 0; i < matrix.Rows; i++)
      {
         /// Tomando el vector fila (terminos correspondientes a un documento)    
         Vector row = Vector.VectorRow(matrix, i);

         /// Normalizando la frecuencia (Cantidad de veces que se repite el termino / en el termino de mayor recurrencia)
         for (int j = 0; j < matrix.Columns; j++)
         {
            double n = row.MaxValue;

            if (n != 0)
               result[i, j] = matrix[i, j] / n;
            else
               result[i, j] = 0;
         }
      }

      return new Matrix(result);
   }

   private static void CheckNullMatrix(Matrix matrix)
   {
      // Verificar los valores de entrada
      if (matrix == null)
         throw new ArgumentException("Matrix can't be null");
   }

   /// Producto de una matriz * un vector
   private static Matrix DotProduct(Matrix matrix, Vector vector)
   {
      CheckNullMatrix(matrix);
      double[,] result = new double[matrix.Rows, vector.Size];

      if (matrix.Columns != vector.Size)
         throw new ArgumentException("Incompatible dimensions");

      for (int i = 0; i < matrix.Rows; i++)
      {
         for (int j = 0; j < vector.Size; j++)
            result[i, j] = matrix[i, j] * vector[j];
      }

      return new Matrix(result);
   }

   public static Matrix CosineSimilarity(Matrix matrix, Vector vector)
   {
      /*** Creando la matriz de score de n filas y dos columas (Cada fila corresponde a un documento) para guardar
      En la primera columna  el score y en la segunda columna la posicion del documento al cual corresponde***/
      double[,] result = new double[matrix.Rows, 2];

      for (int i = 0; i < matrix.Rows; i++)
      {
         /// Tomando el vector fila (terminos correspondientes a un documento)    
         Vector row = Vector.VectorRow(matrix, i);
         /// Multiplicando cada termino y sumando los resultados de la multiplicación de cada culumna (dividendo)
         double product = Vector.VectorSumatori(row * vector);
         /// Multiplicando la raiz cuadrada de la sumatoria de las potencias cuadradas de cada terminano del documento y del query (divisor)
         double div = Math.Sqrt(Vector.VectorSumatori(Vector.VectorPow(row))) * Math.Sqrt(Vector.VectorSumatori(Vector.VectorPow(row)));

         /// Guardando el score correspondiente al documento
         if (div != 0)
            result[i, 0] = product / div;
         else
            result[i, 0] = 0;
         /// Guardando la posicion que le corresponde al documento en el array de documentos
         result[i, 1] = i;
      }

      return new Matrix(result);
   }

   public static Matrix ShortMatrix(Matrix matrix)
   {
      double[,] array = new double[matrix.Rows, matrix.Columns];
      int count = 0;

      /// Copiando la matriz a un array bidimensional
      for (int i = 0; i < matrix.Rows; i++)
      {
         for (int j = 0; j < matrix.Columns; j++)
            array[i, j] = matrix[i, j];

         if (array[i, 0] != 0)
            count++;    /// Cantidad de scores distintos de 0
      }

      /// Organizando el array
      for (int i = 0; i < matrix.Rows; i++)
      {
         for (int j = i + 1; j < matrix.Rows; j++)
         {
            double n;
            double pos;

            if (array[i, 0] < array[j, 0])
            {
               /// Guardando el score
               n = array[i, 0];
               /// Guardando la posición
               pos = array[i, 1];
               /// Intercambiando los valores de score y posicion correspondintes a la fila
               array[i, 0] = array[j, 0];
               array[i, 1] = array[j, 1];
               array[j, 0] = n;
               array[j, 1] = pos;
            }
         }
      }

      /// Tomando solamente las posiciones distintas de 0 ya organizadas
      double[,] result = new double[count, 2];

      for (int i = 0; i < result.GetLength(0); i++)
         for (int j = 0; j < result.GetLength(1); j++)
            result[i, j] = array[i, j];

      return new Matrix(result);
   }

   #region Redefinition of operators
   public static Matrix operator *(Matrix matrix, Vector vector)
   {
      return DotProduct(matrix, vector);
   }
   #endregion
}
