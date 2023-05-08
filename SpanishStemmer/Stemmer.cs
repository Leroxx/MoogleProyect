using System.Text;

namespace SpanishStemmer
{
    public class Stemmer
    {
        public string Execute(string word)
        {
            return Execute(word, false);
        }

        public string Execute(string word, bool useStopWords)
        {
            string result = word;

            if (!useStopWords && Specials.stop_words.IndexOf(word) < 0)
            {
                if (word.Length >= 3)
                {
                    StringBuilder sb = new StringBuilder(word.ToLower());

                    if (sb[0] == '\'') sb.Remove(0, 1);

                    int r1 = 0, r2 = 0, rv = 0;
                    computeR1R2RV(sb, ref r1, ref r2, ref rv);

                    step0(sb, rv);

                    int cont = sb.Length;
                    
                    step1(sb, r1, r2);

                    if (sb.Length == cont)
                    {
                        step2a(sb, rv);
                        if (sb.Length == cont)
                        {
                            step2b(sb, rv);
                        }
                    }

                    step3(sb, rv);
                    
                    RemoveAcutes(sb);
                    RemoveSpecialChar(sb);

                    result = sb.ToString().ToLower();
                }
                return result;
            }
            else
                return "";
        }

        private void computeR1R2RV(StringBuilder sb, ref int r1, ref int r2, ref int rv)
        {
            r1 = sb.Length;
            r2 = sb.Length;
            rv = sb.Length;

            //R1
            for (int i = 1; i < sb.Length; i++)
            {
                if ((!isVowel(sb[i])) && (isVowel(sb[i - 1])))
                {
                    r1 = i + 1;
                    break;
                }
            }

            //R2
            for (int i = r1 + 1; i < sb.Length; i++)
            {
                if ((!isVowel(sb[i])) && (isVowel(sb[i - 1])))
                {
                    r2 = i + 1;
                    break;
                }
            }

            //RV
            if (sb.Length >= 2)
            {
                if (!isVowel(sb[1]))
                {
                    for (int i = 1; i < sb.Length; i++)
                    {
                        if (isVowel(sb[i]))
                        {
                            rv = sb.Length > i ? i + 1 : sb.Length;
                            break;
                        }
                    }
                }
                else
                {
                    if (isVowel(sb[0]) && isVowel(sb[1]))
                    {
                        for (int i = 1; i < sb.Length; i++)
                        {
                            if (!isVowel(sb[i]))
                            {
                                rv = sb.Length > i ? i + 1 : sb.Length;
                                break;
                            }
                        }
                    }
                    else
                    {
                        rv = sb.Length >= 3 ? 3 : sb.Length;
                    }
                }
            }
        }

        private bool isVowel(char c)
        {
            return Specials.Vocales.IndexOf(c) >= 0;
        }

        private void step0(StringBuilder sb, int rv)
        {
            int index = -1;
            int pos = -1;

            for (int i = 5; i > 1 && index < 0; --i)
            {
                if (sb.Length >= i)
                {
                    /// Buscando el sufijo
                    index = Specials.Step0.LastIndexOf(sb.ToString(sb.Length - i, i));

                    /// Si lo encuentro
                    if (index >= 0)
                    {
                        string aux = Specials.Step0[index];
                        int aux_index = sb.ToString().LastIndexOf(aux);
                        string word = sb.ToString().Substring(0, aux_index);

                        /// Buscando la palabra que lo debe preceder
                        foreach (string s in Specials.AfterStep0)
                        {
                            int index_after = word.LastIndexOf(s);
                            if (index_after >= 0)
                                pos = index_after;
                        }

                        /// Si encuentro la palabra
                        if (pos >= 0)
                        {
                            string word0 = word.Substring(pos);
                            int word0_pos = Specials.AfterStep0.LastIndexOf(word0);

                            if (word0_pos >= 0 && Specials.AfterStep0[word0_pos] == "yendo" && sb[pos - 1] == 'u' && pos >= rv)
                            {
                                    sb.Remove(sb.Length - index, index);
                            }
                            else
                            {
                                sb.Remove(aux_index, sb.Length - aux_index);
                                for (int j = pos; j < sb.Length; j++)
                                    sb[j] = Specials.EliminaAcento(sb[j]);
                            }
                        }
                    }
                }
            }
        }

        private void step1(StringBuilder sb, int r1, int r2)
        {
            int pos = -1;
            int collection = -1;
            string found = "";
            string find = sb.ToString();

            /*** Recorro las listas desde Step1_1 a Step1_9 si el string contiene algunos
            de los sufijos encontrados los elimino ***/

            foreach (string s in Specials.Step1_1)
            {
                int index = find.LastIndexOf(s);
                if (index >= 0)
                {
                    string palabra = find.Substring(index);
                    int aux = -1;

                    aux = Specials.Step1_1.LastIndexOf(palabra);
                    if (aux >= 0 && Specials.Step1_1[aux].Length > found.Length)
                    {
                        found =  Specials.Step1_1[aux];
                        pos = index;
                        collection = 1;
                    }
                }
            }

            foreach (string s in Specials.Step1_2)
            {
                int index = find.LastIndexOf(s);
                if (index >= 0)
                {
                    string palabra = find.Substring(index);
                    int aux = -1;

                    aux = Specials.Step1_2.LastIndexOf(palabra);
                    if (aux >= 0 && Specials.Step1_2[aux].Length > found.Length)
                    {
                        found = Specials.Step1_2[aux];
                        pos = index;
                        collection = 2;
                    }
                }
            }

            foreach (string s in Specials.Step1_3)
            {
                int index = find.LastIndexOf(s);
                if (index >= 0)
                {
                    string palabra = find.Substring(index);
                    int aux = -1;

                    aux = Specials.Step1_3.LastIndexOf(palabra);
                    if (aux >= 0 && Specials.Step1_3[aux].Length > found.Length)
                    {
                        found = Specials.Step1_3[aux];
                        pos = index;
                        collection = 3;
                    }
                }
            }

            foreach (string s in Specials.Step1_4)
            {
                int index = find.LastIndexOf(s);
                if (index >= 0)
                {
                    string palabra = find.Substring(index);
                    int aux = -1;

                    aux = Specials.Step1_4.LastIndexOf(palabra);
                    if (aux >= 0 && Specials.Step1_4[aux].Length > found.Length)
                    {
                        found = Specials.Step1_4[aux];
                        pos = index;
                        collection = 4;
                    }
                }
            }

            foreach (string s in Specials.Step1_5)
            {
                int index = find.LastIndexOf(s);
                if (index >= 0)
                {
                    string palabra = find.Substring(index);
                    int aux = -1;

                    aux = Specials.Step1_5.LastIndexOf(palabra);
                    if (aux >= 0 && Specials.Step1_5[aux].Length > found.Length)
                    {
                        found = Specials.Step1_5[aux];
                        pos = index;
                        collection = 5;
                    }
                }
            }

            foreach (string s in Specials.Step1_6)
            {
                int index = find.LastIndexOf(s);
                if (index >= 0)
                {
                    string palabra = find.Substring(index);
                    int aux = -1;

                    aux = Specials.Step1_6.LastIndexOf(palabra);
                    if (aux >= 0 && Specials.Step1_6[aux].Length > found.Length)
                    {
                        found = Specials.Step1_6[aux];
                        pos = index;
                        collection = 6;
                    }
                }
            }

            foreach (string s in Specials.Step1_7)
            {
                int index = find.LastIndexOf(s);
                if (index >= 0)
                {
                    string palabra = find.Substring(index);
                    int aux = -1;

                    aux = Specials.Step1_7.LastIndexOf(palabra);
                    if (aux >= 0 && Specials.Step1_7[aux].Length > found.Length)
                    {
                        found = Specials.Step1_7[aux];
                        pos = index;
                        collection = 7;
                    }
                }
            }

            foreach (string s in Specials.Step1_8)
            {
                int index = find.LastIndexOf(s);
                if (index >= 0)
                {
                    string palabra = find.Substring(index);
                    int aux = -1;

                    aux = Specials.Step1_8.LastIndexOf(palabra);
                    if (aux >= 0 && Specials.Step1_8[aux].Length > found.Length)
                    {
                        found = Specials.Step1_8[aux];
                        pos = index;
                        collection = 8;
                    }
                }
            }

            foreach (string s in Specials.Step1_9)
            {
                int index = find.LastIndexOf(s);
                if (index >= 0)
                {
                    string palabra = find.Substring(index);
                    int aux = -1;

                    aux = Specials.Step1_9.LastIndexOf(palabra);
                    if (aux >= 0 && Specials.Step1_9[aux].Length > found.Length)
                    {
                        found = Specials.Step1_9[aux];
                        pos = index;
                        collection = 9;
                    }
                }
            }

            if (pos >= 0)
            {
                switch (collection)
                {
                    case 1:
                        if (pos >= r2)
                            sb.Remove(pos, sb.Length - pos);
                        break;
                    case 2:
                        if (pos >= r2)
                            sb.Remove(pos, sb.Length - pos);
                        break;
                    case 3:
                        if (pos >= r2)
                        {
                            sb.Remove(pos, sb.Length - pos);
                            sb.Append("log");
                        }
                        break;
                    case 4:
                        if (pos >= r2)
                        {
                            sb.Remove(pos, sb.Length - pos);
                            sb.Append("u");
                        }
                        break;
                    case 5:
                        if (pos >= r2)
                        {
                            sb.Remove(pos, sb.Length - pos);
                            sb.Append("ente");
                        }
                        break;
                    case 6:
                        if (pos >= r1)
                            sb.Remove(pos, sb.Length - pos);
                        else
                        {
                            string aux = sb.ToString(0, pos);
                            if (aux.Substring(0, aux.Length - 2) == "iv" &&
                                aux.Substring(0, aux.Length - 2) == "oc" &&
                                aux.Substring(0, aux.Length - 2) == "ic" &&
                                aux.Substring(0, aux.Length - 2) == "ad" && pos >= r2)
                            {
                                sb.Remove(pos, sb.Length - pos);
                            }
                        }
                        break;
                    case 7:
                    case 8:
                    case 9:
                        if (pos >= r2)
                        {
                            sb.Remove(pos, sb.Length - pos);
                        }
                        break;
                }
            }
        }

        private void step2a(StringBuilder sb, int rv)
        {
            string selected = "";
            int pos = -1;
            int index = -1;

            /// Busco los sufijos y los elimino si estan precedidos por la vocal u
            foreach(string s in Specials.Step2_a)
            {
                index = sb.ToString().LastIndexOf(s);
                if (index >= 0)
                {
                    string palabra = sb.ToString().Substring(index);
                    int aux = index;

                    index = Specials.Step2_a.LastIndexOf(palabra);
                    if (index >= 0)
                    {
                        selected = Specials.Step2_a[index];
                        pos = aux;
                    }
                }
            }

            if (pos >= rv && sb.ToString(pos - 1, 1) == "u")
            {
                sb.Remove(pos - 1, sb.Length - pos + 1);
            }
        }

        private void step2b(StringBuilder sb, int rv)
        {
            string selected = "";
            int pos = -1;
            int index = -1;

            /// Busco los sufijos y los elimino si estan precedidos por "gu"
            foreach(string s in Specials.Step2_b1)
            {
                index = sb.ToString().LastIndexOf(s);
                if (index >= 0)
                {
                    string palabra = sb.ToString().Substring(index);
                    int aux = index;

                    index = Specials.Step2_b1.LastIndexOf(palabra);
                    if (index >= 0)
                    {
                        selected = Specials.Step2_b1[index];
                        pos = aux;
                    }
                }
            }

            if (pos >= rv && sb.ToString(pos - 2, 2) == "gu")
                sb.Remove(pos - 1, sb.Length - pos + 1);

            pos = -1;
            index = -1;
            selected = "";

            foreach (string s in Specials.Step2_b2)
            {
                index = sb.ToString().LastIndexOf(s);
                if (index >= 0)
                {
                    string word = sb.ToString().Substring(index);
                    int aux = index;

                    index = Specials.Step2_b2.LastIndexOf(word);
                    if (index >= 0)
                    {
                        selected = Specials.Step2_b2[index];
                        pos = aux;
                    }
                }
            }

            if (pos > rv)
                sb.Remove(pos, sb.Length - pos);
        }

        private void step3(StringBuilder sb, int rv)
        {
            string selected = "";
            int pos = -1;
            int index = -1;

            /// Busco los sufijos y los elimino si estan precedidos por "gu"
            foreach (string s in Specials.Step3_1)
            {
                index = sb.ToString().LastIndexOf(s);
                if (index >= 0)
                {
                    string word = sb.ToString().Substring(index);
                    int aux = index;

                    index = Specials.Step3_1.LastIndexOf(word);
                    if (index >= 0)
                    {
                        selected = Specials.Step3_1[index];
                        pos = aux;
                    }
                }
            }

            if (pos >= rv)
                sb.Remove(pos, sb.Length - pos);

            pos = -1;
            index = -1;
            selected = "";

            foreach (string s in Specials.Step3_2)
            {
                index = sb.ToString().LastIndexOf(s);
                if (index >= 0)
                {
                    string word = sb.ToString().Substring(index);
                    int aux = index;

                    index = Specials.Step3_2.LastIndexOf(word);
                    if (index >= 0)
                    {
                        selected = Specials.Step3_2[index];
                        pos = aux;
                    }
                }
            }
            if (pos >= 0 && sb.ToString(pos - 2, 2) == "gu" && pos - 1 >= rv)
                sb.Remove(pos - 1, sb.Length - pos + 1);
        }

        private void RemoveAcutes(StringBuilder sb)
        {
            /// Removiendo los acentos
            for(int i = 0; i < sb.Length; i++)
            {
                char c = sb[i];
                sb[i] = Specials.EliminaAcento(c);
            }
        }

        private void RemoveSpecialChar(StringBuilder sb)
        {
            /// Removiendo los caracteres especiales
            foreach(char a in Specials.special_char)
            {
                for (int i = 0; i < sb.Length; i++)
                {
                    if (sb[i] == a)
                        sb.Remove(i, 1);
                }
            }
        }
    }
}
