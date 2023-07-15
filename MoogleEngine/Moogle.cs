using System;
using MoogleDocs;
using MoogleTools;
using SpanishStemmer;
using System.Text.RegularExpressions;

namespace MoogleEngine;

public static class Moogle
{
   public static SearchResult Query(string query)
   {
      HandleDocs.CheckFiles();

      /// Llamando el metodo que devuelve el SearchItem de los documentos relevantes a la búsqueda
      SearchItem[] items = GetResults(query, HandleDocs.stemming_texts);

      /// Obteniendo la sugerencia
      string suggestion = HandleDocs.GetSuggestion(query, HandleDocs.documents_texts);

      return new SearchResult(items, suggestion);
   }

   public static SearchItem[] GetResults(string query, string[] texts)
   {
      int dictionary_items = HandleDocs.dictionary.Count;

      /// Guardando los operadores
      var tuple = FindOperator(query);

      string[] operators = tuple.Item1;
      query = tuple.Item2;

      /// Lemantizando el query y llenado el diccionario si no estan contenidas en el las palabras del query
      query = StemmingQuery(query);
      FillDictionary(query, HandleDocs.dictionary, texts);

      if (dictionary_items != HandleDocs.dictionary.Count)
      {
         HandleDocs.idf_vector = Vector.CalculateIDF(HandleDocs.dictionary, HandleDocs.stemming_texts.Length); /// IDF de los términos
         HandleDocs.matrix_tf_idf = Matrix.CalculateTimeFrecuency(Matrix.WordCount(HandleDocs.stemming_texts, HandleDocs.dictionary)) * HandleDocs.idf_vector;
      }

      Vector query_vector = VectorizingQuery(HandleDocs.dictionary, query, HandleDocs.idf_vector); /// TF_IDF correspondiente al query        

      /*** Matriz de score de n filas y dos columnas (Cada fila corresponde a un documento)
      En la primera columna guardo el score y en la segunda guardo la posicion de documento al cual corresponde ***/
      Matrix cos_similarity = Matrix.CosineSimilarity(HandleDocs.matrix_tf_idf, query_vector);

      /// Ordenando la matriz de forma descendente segun el score
      Matrix result = Matrix.ShortMatrix(cos_similarity);

      /// Array para guardar los documentos relevantes al query
      string[] text_result = new string[result.Rows];

      string[] words = query.Split();

      /// Leyendo los documentos resultantes
      for (int i = 0; i < result.Rows; i++)
         text_result[i] = HandleDocs.LoadText(Convert.ToInt32(result[i, 1]));

      /// Si el array de operadores no es null
      if (CheckNullOperators(operators))
      {
         /// Modificando el score y organizando nuevamente la matriz
         Matrix items = Matrix.ShortMatrix(HandleOperator(operators, text_result, result));

         SearchItem[] search_items = new SearchItem[items.Rows];
         string[] titles = new string[items.Rows];
         string[] operator_text_result = new string[items.Rows];

         /// LLenando el array con los titulos de los documentos
         for (int i = 0; i < items.Rows; i++)
            titles[i] = HandleDocs.LoadTitle(Convert.ToInt32(items[i, 1]));

         /// Leyendo los documentos resultantes
         for (int i = 0; i < result.Rows; i++)
            operator_text_result[i] = HandleDocs.LoadText(Convert.ToInt32(result[i, 1]));

         for (int i = 0; i < items.Rows; i++)
         {
            double score = items[i, 0];
            string snippet = "";

            /// Find Snippet
            for (int j = 0; j < words.Length; j++)
            {
               if (operator_text_result[i].Contains(words[j]))
                  snippet += HandleDocs.GetSnippet(operator_text_result[i], words[j]);
            }
            /// LLenando el array de SearchItem
            search_items[i] = new SearchItem(titles[i], snippet, score);
         }
         return search_items;
      }
      else
      {
         /// Si el array de operadores es null lleno el array de SearchItem
         SearchItem[] search_items = new SearchItem[result.Rows];

         for (int i = 0; i < result.Rows; i++)
         {
            int pos = Convert.ToInt32(result[i, 1]);
            double score = result[i, 0];
            string snippet = "";

            /// Find Snippet
            for (int j = 0; j < words.Length; j++)
            {
               if (text_result[i].Contains(words[j]))
                  snippet += HandleDocs.GetSnippet(text_result[i], words[j]);
            }

            search_items[i] = new SearchItem(HandleDocs.files_names[pos], snippet, score);
         }

         return search_items;
      }
   }

   public static string StemmingQuery(string query)
   {
      string[] query_words = query.Split();
      Stemmer stemmer = new Stemmer();
      string text = "";

      /// Llevando a su raíz todas las palabras contenidas en el query
      for (int i = 0; i < query_words.Length; i++)
      {
         query_words[i] = stemmer.Execute(query_words[i]);
         text += query_words[i] + " ";
      }

      return text;
   }

   public static (string[], string) FindOperator(string query)
   {
      string sentence = "";
      string[] operators = new string[3];
      string[] words = query.Split();
      char[] chars = { '!', '^', '*' };

      /// Por cada palabra contenida en query compruebo si contiene algunos de los operadores
      for (int i = 0; i < words.Length; i++)
      {
         foreach (char a in words[i])
         {
            /*** Si contiene algunos de los operadores guardo el mismo el string operadores 
            junto a la palabra que les corresponde y los elimino del query ***/
            for (int j = 0; j < operators.Length; j++)
            {
               if (a == chars[j])
               {
                  switch (j)
                  {
                     case 0:
                        if (a.ToString() == words[i].Substring(0, 1))
                           operators[j] += words[i] + " ";
                        words[i] = words[i].Remove(words[i].IndexOf(a), 1);
                        break;
                     case 1:
                        if (a.ToString() == words[i].Substring(0, 1))
                           operators[j] += words[i] + " ";
                        words[i] = words[i].Remove(words[i].IndexOf(a), 1);
                        break;
                     case 2:
                        if (a.ToString() == words[i].Substring(0, 1))
                           operators[j] += words[i] + " ";

                        foreach (char c in words[i])
                           if (c == a)
                              words[i] = words[i].Remove(words[i].IndexOf(a), 1);
                        break;
                  }
               }
            }
         }
      }

      /// Devolviendo el query sin los operadores para poder ser procesado
      for (int i = 0; i < words.Length; i++)
         sentence += words[i] + " ";

      return (operators, sentence);
   }

   public static bool CheckNullOperators(string[] operators)
   {
      for (int i = 0; i < operators.Length; i++)
      {
         if (operators[i] != null)
            return true;
      }

      return false;
   }

   public static Matrix HandleOperator(string[] operators, string[] texts, Matrix matrix)
   {
      double[,] result = new double[matrix.Rows, matrix.Columns];
      string[] words;     /// cantidad de palabras con el mismo operador
      string text = "";   /// texto donde realizar la búsqueda

                          /// Copiando la matriz
      for (int i = 0; i < matrix.Rows; i++)
         for (int j = 0; j < matrix.Columns; j++)
            result[i, j] = matrix[i, j];

      /// Por cada documento recorro los operadores y ejecuto las acciones correspondientes a cada uno de ellos
      for (int i = 0; i < matrix.Rows; i++)
      {
         for (int j = 0; j < operators.Length; j++)
         {
            if (operators[j] != null)
            {
               switch (j)
               {
                  case 0:     ///< !
                     words = operators[j].Split();
                     text = texts[i];
                     /// Haciendo 0 el score para q ningún documento con la palabra antecedida por el operador ! sea devuelto
                     if (HandleDocs.ExclusionOperator(operators[j], text))
                        result[i, 0] = 0;
                     break;
                  case 1:     ///< ^
                     words = operators[j].Split();
                     text = texts[i];
                     /// Haciendo 0 el score para q ningún documento que no contenga palabra antecedida por el operador ^ sea devuelto
                     if (!HandleDocs.ConstantOperator(words, text))
                        result[i, 0] = 0;
                     break;
                  case 2:     ///< *
                     words = operators[j].Split();
                     text = texts[i];
                     int n = HandleDocs.RelevantOperator(words, text);
                     /*** Aumento en 0.10 el socre del documento donde fue encontrado la palabra 
                     antecedida por el operador * por cada operador encontrado***/
                     if (n >= 1)
                        result[i, 0] = result[i, 0] * (n * 0.1);
                     break;
               }
            }
         }
      }

      return new Matrix(result);
   }

   public static void FillDictionary(string query, Dictionary<string, int> dictionary, string[] files_texts)
   {
      string[] query_words = query.Split();
      int aux = 0;

      /*** Comprobando que las palabras del query estan contenidas en el diccionario en casi 
      contrario guardo las mismas y la cantidad de documentos donde estas aparecen ***/
      for (int i = 0; i < query_words.Length; i++)
      {
         if (!dictionary.ContainsKey(query_words[i]) && query_words[i].Length >= 3)
         {
            for (int k = 0; k < files_texts.Length; k++)
            {
               if (files_texts[k].Contains(query_words[i]))
                  aux++;
            }
            dictionary.Add(query_words[i], aux);
         }
         aux = 0;
      }
   }

   public static Vector VectorizingQuery(Dictionary<string, int> dictionary, string query, Vector idf_vector)
   {
      double[] result = new double[dictionary.Count];
      int aux = 0;

      /// Cantidad de veces que se repite cada término del diccionario en el query (TF)
      foreach (KeyValuePair<string, int> word in dictionary)
      {
         result[aux] = Regex.Matches(query, word.Key).Count;
         aux++;
      }

      /// Normalizando la frecuencia (Cantidad de veces que se repite el termino / en el termino de mayor recurrencia)
      for (int i = 0; i < result.Length; i++)
      {
         double n = result.Max();

         if (n != 0)
            result[i] = result[i] / n;
         else
            result[i] = 0;
      }

      /// Multiplicando la frecuencia de los términos con el su IDF para obtener el vector TF_IDF correspondiente al query
      for (int i = 0; i < result.Length; i++)
         result[i] = result[i] * idf_vector[i];

      return new Vector(result);
   }
}
