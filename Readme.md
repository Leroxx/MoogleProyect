# Moogle!       Richard Avila Entenza C111
> Proyecto de Programación I. Facultad de Matemática y Computación. Universidad de La Habana. Curso 2021.

Moogle! es una aplicación *totalmente original* cuyo propósito es buscar inteligentemente un texto en un conjunto de documentos utilizando 
el modelo vectorial de recuperación de la información.

Es una aplicación web, desarrollada con tecnología .NET Core 6.0, específicamente usando Blazor como *framework* web para la interfaz gráfica, y en el lenguaje C#.

La aplicación está dividida en dos componentes fundamentales:

- `MoogleServer` es un servidor web que renderiza la interfaz gráfica y sirve los resultados.
- `MoogleEngine` es una biblioteca de clases donde está implementada la lógica del algoritmo de búsqueda.
- `MoogleDocs` es una biblioteca de clases donde se realiza todo el procesamiento de texto de los documentos.
- `MoogleTools` es una biblioteca de clases donde se realiza el procesamiento de la vectorización de los documentos y los algoritmos del modelo vectorial.
- `SpanishStemmer` es una biblioteca de clases que dada una palabra lleva la misma a su raíz.

### Operadores de búsqueda
- Un símbolo `!` delante de una palabra (e.j., `"algoritmos de búsqueda !ordenación"`) indica que esa palabra **no debe aparecer** en ningún documento que sea devuelto.
- Un símbolo `^` delante de una palabra (e.j., `"algoritmos de ^ordenación"`) indica que esa palabra **tiene que aparecer** en cualquier documento que sea devuelto.
- Cualquier cantidad de símbolos `*` delante de un término indican que ese término es más importante, por lo que su influencia en el `score` debe ser mayor que la tendría normalmente (este efecto será acumulativo por cada `*`, por ejemplo `"algoritmos de **ordenación"` indica que la palabra `"ordenación"` tiene dos veces más prioridad que `"algoritmos"`).

### Algoritmos de búsqueda
El proyecto proyecto utiliza el modelo vectorial de recuperación de la información mediante el cual se presenta una matriz de N filas correspondientes a los documentos procesados y Ni términos los cuales contienen el TF_IDF, TF -> Frecuencia de los términos en los documentos, IDF -> Frecuencia de termino inversa, la multiplicación de estos nos indica que tan importante es el termino dentro del documento. Cada fila de esta matriz se calcula su similitud con la consulta `query` introducida por el usuario, esta `similitud de coseno` mientras más alto sea su valor más relevante es el documento con respecto al `query`.

El metodo `Moogle.Query` que está en la clase `Moogle` del proyecto `MoogleEngine`.

    public static SearchResult Query(string query) 
    {
        /// Cargando el texto de los documentos
        string[] texts = HandleDocs.LoadDocumentText();

        /// Llamando el metodo que devuelve el SearchItem de los documentos relevantes a la búsqueda
        SearchItem[] items = GetResults(query, texts);

        /// Obteniendo la sugerencia
        string suggestion = HandleDocs.GetSuggestion(query, texts);

        return new SearchResult(items, suggestion);
    }

Este método devuelve un objeto de tipo `SearchResult`. Este objeto contiene los resultados de la búsqueda realizada por el usuario, que viene en un parámetro de tipo `string` llamado `query`.

El tipo `SearchResult` recibe en su constructor dos argumentos: `items` y `suggestion`. El parámetro `items` es un array de objetos de tipo `SearchItem`. Cada uno de estos objetos representa un posible documento que coincide al menos parcialmente con la consulta en `query`.

Cada `SearchItem` recibe 3 argumentos en su constructor: `title`, `snippet` y `score`. El parámetro `title` debe ser el título del documento (el nombre del archivo de texto correspondiente). El parámetro `snippet` contiene una porción del documento donde se encontró el contenido del `query`. El parámetro `score` tendrá un valor de tipo `double` que será más alto mientras más relevante sea este item. Los item son devueltos de mayor a menor segun el valor de `score`.

El parámetro `suggestion` de la clase `SearchResult` es para darle una sugerencia al usuario cuando su búsqueda da muy pocos resultados. Esta sugerencia es algo similar a la consulta del usuario pero que sí existe, de forma que si el usuario se equivoca podra sugerirle una palabra que si está contenida en los documentos.

El metodo `LoadDocumentText` de la clase `HandleDocs` lee el texto de los documentos y los guarda en un `array` de `string` en el cual cada posición del array corresponde al texto de un documento.
El parámetro `items` se declara llamando el método `Moogle.GetResults` que recibe dos argumentos: un valor de tipo `string` el cual es la consulta `query` introducida por el usuario y un `array` de tipo `string` que es el texto de los documentos sobre los cuales se realiza la búsqueda.

En el método `Moogle.GetResults` se lemantizan los documentos con el método `HandleDocs.StemmingDocuments` el cual lleva cada palabra de los documentos a su raíz usando la clase `SpanishStemmer` con el `array` resultante se inicializa el `Dictionary<string, int>` llamando el metodo `MoogleDocs.HandleDocs`, el valor tipo `string` contiene los términos de los documentos y el valor tipo `int` contiene la cantidad de documentos donde aparece dicho termino.

El método `Moogle.FindOperator` procesa los operadores del `query` y los guarda en un `tuple (string[], string)`: `string[]` contiene los operadores y `string` el `query` luego de ser eliminados los operadores.

Los métodos `Moogle.StemmingQuery` y `Moogle.FillDictionary` procesan el `query` de igual forma que se hiso anteriormente con los documentos para así completar el diccionario con todos los términos.

    Vector idf_vector = Vector.CalculateIDF(dictionary, texts.Length); /// IDF de los términos
    Vector query_vector = VectorizingQuery(dictionary, query, idf_vector); /// TF_IDF correspondiente al query

    /// TF_IDF de los terminos de los documentos
    Matrix matrix_tf_idf = Matrix.CalculateTimeFrecuency(Matrix.WordCount(texts, dictionary)) * idf_vector;

    /*** Matriz de score de n filas y dos columnas (Cada fila corresponde a un documento)
    En la primera columna guardo el score y en la segunda guardo la posicion de documento al cual corresponde ***/
    Matrix cos_similarity = Matrix.CosineSimilarity(matrix_tf_idf, query_vector);

    /// Ordenando la matriz de forma descendente segun el score
    Matrix result = Matrix.ShortMatrix(cos_similarity);

Los valores del tipo `Matrix result` son procesados de dos formas distintas, varia de si el `query` introducido por el usuario contenía o no operadores.
Luego es llenado tipo `SearchResult items` a ser devuelto.

El valor tipo `string` suggestion llama el metodo `HandleDocs.GetSuggestion` en el cual mediante el algoritmo de Levenshtein en caso de alguna palabra del `query` no aparezca en los documentos se sugiere una que podría coincidir con los objetivos de la búsqueda
