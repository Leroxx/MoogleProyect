namespace MoogleTools;
public class Vector
{
   private double[] elements;

   public Vector(double[] elements)
   {
      if (elements == null)
         throw new ArgumentException("Vector can't be null");

      this.elements = elements;
   }

   //Properties
   public int Size
   {
      get { return this.elements.Length; }
   }

   public double MaxValue
   {
      get { return this.elements.Max(); }
   }

   //Indexer
   public double this[int i]
   {
      get { return this.elements[i]; }
   }

   /// Inverse Document Frequency
   public static Vector CalculateIDF(Dictionary<string, int> dict, int n)
   {
      /// Array de doble para guardar el idf de los terminos del diccionario
      double[] result = new double[dict.Count];
      int aux = 0;

      /// Tomando la cantidad de documentos donde se repite cada palabra
      foreach (KeyValuePair<string, int> word in dict)
      {
         result[aux] = word.Value;
         aux++;
      }

      /*** Calculado el idf de los terminos. Log de la division de la cantidad de 
      documentos (n) y la cantidad de documentos donde se repite el termino ***/
      for (int i = 0; i < result.Length; i++)
      {
         if (result[i] != 0)
            result[i] = Math.Log10(n / result[i]);
         else
            result[i] = 0;
      }

      return new Vector(result);
   }

   /// Vector correspondiente a una fila de la matriz
   public static Vector VectorRow(Matrix matrix, int row)
   {
      double[] vector = new double[matrix.Columns];

      for (int i = 0; i < matrix.Columns; i++)
         vector[i] = matrix[row, i];

      return new Vector(vector);
   }

   /// Multiplicando dos vectores
   public static Vector DotProduct(Vector v1, Vector v2)
   {
      double[] result = new double[v1.Size];

      for (int i = 0; i < v1.Size; i++)
         result[i] = v1[i] * v2[i];

      return new Vector(result);
   }

   /// Potencia cuadrada de todos los elementos de un vector
   public static Vector VectorPow(Vector vector)
   {
      double[] result = new double[vector.Size];

      for (int i = 0; i < vector.Size; i++)
         result[i] = Math.Pow(vector[i], 2);

      return new Vector(result);
   }

   /// Suma de todos los elementos de un vector
   public static double VectorSumatori(Vector vector)
   {
      double result = 0;

      for (int i = 0; i < vector.Size; i++)
         result += vector[i];

      return result;
   }

   #region Redefinition of operators
   public static Vector operator *(Vector v1, Vector v2)
   {
      return DotProduct(v1, v2);
   }
   #endregion
}
