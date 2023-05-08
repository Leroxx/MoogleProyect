using SpanishStemmer;
using MoogleTools;

namespace MoogleDocs;
public class HandleDocs
{
   /// Cargando los archivos del directorio Content
   private static FileInfo[] files = LoadDocs();
   public static string[] files_names = GetFilesName();
   public static string[] documents_texts = LoadDocumentText();
   public static string[] stemming_texts = StemmingDocuments(documents_texts);
   public static Dictionary<string, int> dictionary = GetDictionary(stemming_texts);
   public static Vector idf_vector = Vector.CalculateIDF(dictionary, stemming_texts.Length); /// IDF de los términos
   public static Matrix matrix_tf_idf = Matrix.CalculateTimeFrecuency(Matrix.WordCount(stemming_texts, dictionary)) * idf_vector;

   public static void CheckFiles()
   {
      FileInfo[] check_files = LoadDocs();
      string[] check_names = GetFilesName();

      if (check_files.Length != files.Length || !CheckNames(check_names, files_names))
      {
         files = LoadDocs();
         files_names = GetFilesName();
         documents_texts = LoadDocumentText();
         stemming_texts = StemmingDocuments(documents_texts);
         dictionary = GetDictionary(stemming_texts);
         idf_vector = Vector.CalculateIDF(dictionary, stemming_texts.Length); /// IDF de los términos
         matrix_tf_idf = Matrix.CalculateTimeFrecuency(Matrix.WordCount(stemming_texts, dictionary)) * idf_vector;
      }
   }

   public static bool CheckNames(string[] check, string[] names)
   {
      int aux = 0;

      while (aux < check.Length)
      {
         if (check[0] == names[0])
            aux++;
         else
            return false;
      }
      return true;
   }

   private static FileInfo[] LoadDocs()
   {
      string path = Path.GetFullPath("Content");
      DirectoryInfo directory = new DirectoryInfo(path);
      FileInfo[] files = directory.GetFiles("*.txt");

      return files;
   }

   /// Array de string que contiene el nombre de los archivos
   private static string[] GetFilesName()
   {
      string[] names = new string[files.Length];
      int pos = 0;

      foreach (FileInfo file in files)
      {
         names[pos] = file.Name;
         pos++;
      }
      return names;
   }

   /// Array de string que contiene el texto de los archivos
   private static string[] LoadDocumentText()
   {
      string[] documents_text = new string[files.Length];
      int pos = 0;

      foreach (FileInfo file in files)
      {
         StreamReader lectura = file.OpenText();
         string text = lectura.ReadToEnd();
         lectura.Close();
         documents_text[pos] = text;
         pos++;
      }
      return documents_text;
   }

   public static string LoadText(int pos)
   {
      StreamReader lectura = files[pos].OpenText();
      string text = lectura.ReadToEnd();

      return text;
   }

   public static string LoadTitle(int pos)
   {
      string title = files[pos].Name;

      return title;
   }

   public static string GetSnippet(string text, string word)
   {
      int index = text.IndexOf(word, 0);
      int lenght = 70;
      string snippet = "";

      if (text.Length - index >= 70)
      {
         snippet = text.Substring(index, lenght) + "...";
         return snippet;
      }
      else
         return word + "...";
   }

   /// Lemantizando de las palabras de los documentos
   private static string[] StemmingDocuments(string[] files_texts)
   {
      string[] stemming_texts = new string[files_texts.Length];
      Stemmer stemmer = new Stemmer();
      string text = "";

      /// Recorriendo el texto de los documentos
      for (int i = 0; i < files_texts.Length; i++)
      {
         string[] file_words = files_texts[i].Split();

         /// A cada palabra del documento se lleva a su raíz
         for (int j = 0; j < file_words.Length; j++)
         {
            file_words[j] = stemmer.Execute(file_words[j]);
            text += file_words[j] + " ";
         }
         stemming_texts[i] = text;
         text = "";
      }

      return stemming_texts;
   }

   /// Creando el diccionario con las palabras contenidas en los documentos una vez lemantizados los mismos
   private static Dictionary<string, int> GetDictionary(string[] files_texts)
   {
      Dictionary<string, int> words = new Dictionary<string, int>();
      int aux = 0;

      /// Recorriendo el texto de los documentos
      for (int i = 0; i < files_texts.Length; i++)
      {
         string[] file_words = files_texts[i].Split();

         /// A cada palabra del documento se agrega al diccionario si la misma no es encuentra en el
         for (int j = 0; j < file_words.Length; j++)
         {
            if (!words.ContainsKey(file_words[j]) && file_words[j].Length >= 3)
            {
               for (int k = 0; k < files_texts.Length; k++)
               {
                  /// Si la palabra no se encuentra en el diccionario se agrega la mismas
                  /// Y la cantidad de documentos donde esta aparece, manejado con el entero "aux"
                  if (files_texts[k].Contains(file_words[j]))
                     aux++;
               }
               words.Add(file_words[j], aux);
            }
            aux = 0;
         }
      }

      return words;
   }

   #region Moogle operators
   public static bool ExclusionOperator(string operador, string text)
   {
      char a = '!';
      string[] words = operador.Split(a);

      /// Por cada palabra palabra guardada en el string operador, verifico si esta contenida en el texto
      for (int i = 0; i < words.Length; i++)
      {
         if (text.Contains(words[i]))
            return true;
      }
      return false;
   }

   public static bool ConstantOperator(string[] words, string text)
   {
      char a = '^';

      /// Por cada palabra palabra guardada en el string operador, verifico si esta contenida en el texto
      for (int i = 0; i < words.Length; i++)
      {
         string word = words[i].Remove(words[i].IndexOf(a), 1);
         if (text.Contains(word))
            return true;
      }
      return false;
   }

   public static int ClosenessOperator(string word, string text)
   {
      int result = int.MaxValue;
      char a = '~';
      List<int> pos_a = new List<int>();
      List<int> pos_b = new List<int>();
      string[] words = word.Split(a);
      string[] text_words = text.Split();
      int aux = 0;

      if (word.Contains(a))
      {
         for (int i = 0; i < words.Length - 1; i++)
         {
            int j = i + 1;
            pos_a.RemoveRange(0, pos_a.Count);
            pos_b.RemoveRange(0, pos_b.Count);

            /// Tomo las dos primeras posiciones dentro del string word que contiene las palabras con los operadores
            /// Si coincide con la primera palabra lo guardo en la lista "pos_a" de no se asi en "pos_b"
            for (int k = 0; k < text_words.Length; k++)
            {
               if (text_words[k].Contains(word[i]))
               {
                  pos_a.Add(k);
               }
               else if (text_words[k].Contains(word[j]))
               {
                  pos_b.Add(k);
               }
            }
         }

         /// Guardando la menos de las distancias  entre posiciones encontradas
         if (pos_a.Count > 0 && pos_b.Count > 0)
         {
            foreach (int i in pos_a)
            {
               if (aux < pos_b.Count)
               {
                  int n = Math.Abs(i - pos_b[aux]);
                  if (n < result)
                     result = n;
                  aux++;
               }
            }

         }
      }

      return result;
   }

   public static int RelevantOperator(string[] words, string text)
   {
      int count = 0;
      string word = "";
      char a = '*';

      /// Por cada palabra con el operador * tomo la cantidad de veces que este se repite y busco si esta contenida en documento
      for (int i = 0; i < words.Length; i++)
      {
         int aux = 0;

         foreach (char b in words[i])
         {
            if (a == b)
            {
               word = words[i].Remove(words[i].IndexOf(a), 1);
               aux++;
            }
         }

         if (text.Contains(word))
            count += aux;
      }

      return count;
   }
   #endregion

   #region Levenshtein
   public static string GetSuggestion(string query, string[] text)
   {
      string suggestion = "";
      string[] query_words = query.Split();
      List<string> word_list = new List<string>();
      string temporary_word = "";

      for (int i = 0; i < text.Length; i++)
      {
         string[] file_words = text[i].Split();

         /// A cada palabra del documento se agrega a la lista si la misma no es encuentra en ella
         for (int j = 0; j < file_words.Length; j++)
         {
            if (!word_list.Contains(file_words[j]))
               word_list.Add(file_words[j]);
         }
      }

      /*** Por cada palabra del query busco la palabra del documento que menor 
      distancia tenga con ella y es la que devuelvo en la sugerencia ***/
      for (int i = 0; i < query_words.Length; i++)
      {
         if (!word_list.Contains(query_words[i]))
         {
            int n = int.MaxValue;
            foreach (string s in word_list)
            {
               if (Math.Abs(query_words[i].Length - s.Length) <= 2)
               {
                  int x = Levenshtein(query_words[i], s);

                  if (x <= n)
                  {
                     n = x;
                     temporary_word = s;
                  }

                  if (n == 1)
                  {
                     query_words[i] = s;
                     break;
                  }
               }
            }
            query_words[i] = temporary_word;
         }
      }

      for (int i = 0; i < query_words.Length; i++)
         suggestion += query_words[i] + " ";

      return suggestion;
   }

   private static int Levenshtein(string s, string t)
   {
      int n = s.Length;
      int m = t.Length;
      int[,] d = new int[n + 1, m + 1];

      /// Verificando los argumentos
      if (n == 0)
      {
         return m;
      }

      if (m == 0)
      {
         return n;
      }

      /// Inicializando array
      for (int i = 0; i <= n; i++)
      {
         d[i, 0] = i;
      }

      for (int i = 1; i <= m; i++)
      {
         d[0, i] = i;
      }

      /// Recorriendo la matriz
      for (int i = 1; i <= n; i++)
      {
         for (int j = 1; j <= m; j++)
         {
            int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;

            d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1,                /// Eliminacion
                                            d[i, j - 1] + 1),           /// Inserccion
                                            d[i - 1, j - 1] + cost);    /// Sustitucion
         }
      }
      return d[n, m];
   }
   #endregion
}
