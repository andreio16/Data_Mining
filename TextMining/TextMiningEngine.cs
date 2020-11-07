using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TextMining
{
    class TextMiningEngine
    {
        private Dictionary<string, int> wordsDictionary = new Dictionary<string, int>();
        private Dictionary<int, string> topicsDictionary = new Dictionary<int, string>();
        private List<string> stopwords = new List<string>();
        private List<Dictionary<string, int>> listOfDictionaries = new List<Dictionary<string, int>>();
        private PorterStemmer _porter = new PorterStemmer();
        private List<string> _sortedWordsDictionary = new List<string>();
        private List<List<byte>> _VectorXMLs = new List<List<byte>>();

        public string GetNodesValuesFromXML(string path, string tag1, string tag2)
        {
            string content = "";
            foreach (string file in Directory.EnumerateFiles(path, "*.xml"))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(file);

                XmlNodeList titleNodes = xmlDoc.GetElementsByTagName(tag1);
                XmlNodeList textNodes = xmlDoc.GetElementsByTagName(tag2);

                for (int i = 0; i < titleNodes.Count; i++)
                    content +=  titleNodes[i].InnerText.ToString();
                for (int i = 0; i < textNodes.Count; i++)
                    content += textNodes[i].InnerText.ToString();
                content += " #endfile# ";
            }
            return content;
        }

        public string FilterByDelimiters(string content)
        {
            string[] delimiters = new string[] { " ", ".", ",", ":", ";", "!", "?", "%", "&", "$", "@", "-", "+", "/", "\t", "*", "'",
                            "\\", "'s", "'re", "'d", "\"", "(", ")", "<", ">", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            string[] parts = content.Split(delimiters, StringSplitOptions.None);
            string filteredContent = "";
            for (int i = 0; i < parts.Length; i++)
                filteredContent += _porter.StemWord(parts[i].ToLower()) + " ";
            return filteredContent;
        }

        public string GetLastKeysFromXmlFiles(string content)
        {
            // Extra function
            string[] delimiters = new string[] { "#endfile#" };
            string[] parts = content.Split(delimiters, StringSplitOptions.None);
            string lastKey = " ";
            for (int i = 0; i < parts.Length; i++)
                lastKey += parts[i].Split(' ').Reverse().ElementAt(1).ToString() + " ";
            return lastKey;
        }

        public void MakeDictionary(string content)
        {
            string[] delimiters = new string[] { " " };
            string[] parts = content.Split(delimiters, StringSplitOptions.None);

            wordsDictionary.Add(parts[0], 1);
            for (int i = 1; i < parts.Length; i++)
            {
                int counter = 0;
                if (parts[i] != "" && parts[i] != "#endfile#") 
                    foreach (KeyValuePair<string, int> pair in wordsDictionary)
                    {
                        if (pair.Key == parts[i])
                        {
                            wordsDictionary[pair.Key]++;
                            break;
                        } else counter++;

                        if (counter == wordsDictionary.Count)
                        {
                            wordsDictionary.Add(parts[i], 1);
                            break;
                        }
                    }
                
            }
        }

        public void MakeListOfDictionaries(string content)
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            string[] delimiters = new string[] { " " };
            string[] parts = content.Split(delimiters, StringSplitOptions.None);

            dictionary.Add(parts[0], 1);
            for (int i = 1; i < parts.Length; i++)
            {
                int counter = 0;
                if (parts[i] != "#endfile#" && parts[i] != "") 
                {
                        foreach (KeyValuePair<string, int> pair in dictionary)
                        {
                            if (pair.Key == parts[i])
                            {
                                dictionary[pair.Key]++;
                                break;
                            }
                            else counter++;

                            if (counter == dictionary.Count)
                            {
                                dictionary.Add(parts[i], 1);
                                break;
                            }
                        }
                }
                if (parts[i] == "#endfile#" && ((i + 1) < parts.Length))
                {
                    listOfDictionaries.Add(new Dictionary<string, int>(dictionary));
                    dictionary.Clear();
                    dictionary.Add(parts[i + 1], 1);
                    i++;
                }
            }
        }

        public void ExtractCodeTopicsFromXML(string path)
        {
            int keyPerFile = 1;
            foreach (string file in Directory.EnumerateFiles(path, "*.xml"))
            {
                string codePerFile = "";
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(file);
                
                XmlNodeList codeNodes = xmlDoc.GetElementsByTagName("codes");
                for (int i = 0; i < codeNodes.Count; i++)
                {
                    if(codeNodes[i].Attributes[0].InnerText.ToString() == "bip:topics:1.0")
                    {
                        foreach (XmlNode child in codeNodes[i].ChildNodes)
                            codePerFile += child.Attributes[0].InnerText.ToString() + " ";
                        topicsDictionary.Add(keyPerFile,codePerFile);
                    }
                }
                keyPerFile++;
            }
            
        }

        private void GetStopWordsFromTXT(string path)
        {
            StreamReader reader = new StreamReader(path);
            string currentLine = reader.ReadLine();
            while (currentLine != null)
            {
                stopwords.Add(currentLine);
                currentLine = reader.ReadLine();
            }
        }

        public void ApplyStowordsFiltering()
        {
            string stopwordsPath = @"..\..\..\..\stopwords.txt";
            GetStopWordsFromTXT(stopwordsPath);

            // Filtering words global dictionary (1)
            foreach (KeyValuePair<string, int> pair in wordsDictionary.ToList())
                foreach (string str in stopwords)
                    if (pair.Key == str)
                        wordsDictionary.Remove(pair.Key);
            // Stopwords removed + print (1)
            PrintWordsDictionary_Length();


            // Clear checkers + Filtering the list of dictionaries (2)
            foreach (var dictionary in listOfDictionaries)          
                foreach (KeyValuePair<string, int> pair in dictionary.ToList())                
                    foreach (string str in stopwords)
                        if (pair.Key == str)
                            dictionary.Remove(pair.Key);                             
            // Stopwords removed + print (2)
            PrintNrOfAllWordsFromList();
        }
        
        public void SortAndPrintWordsDictionary()
        {
            try
            {
                _sortedWordsDictionary = wordsDictionary.Keys.ToList();
                _sortedWordsDictionary.Sort();
                foreach(var key in _sortedWordsDictionary)
                    Console.WriteLine("{0}: {1}", key, wordsDictionary[key]);
            }
            catch (Exception)
            {
                Console.WriteLine("\"wordsDictionary\" cannot be null!");
            }
        }



        private List<byte> InitVector()
        {
            List<byte> list = new List<byte>();
            int ct = 0;
            while (ct < wordsDictionary.Count) 
            {
                list.Add(0);
                ct++;
            }
            return list;
        }

        public void MakeVectors()
        {
            int i;
            byte frequency = 0;
            var vectorList = InitVector();

            foreach (var dictionary in listOfDictionaries)
            {
                foreach (KeyValuePair<string, int> pair in dictionary)
                {
                    i = -1;
                    foreach (var pairGlobal in _sortedWordsDictionary)
                    {
                        i++;
                        if (pair.Key == pairGlobal)
                        {
                            frequency = (byte)pair.Value;
                            break;
                        }
                    }
                    if (i != -1) 
                        vectorList[i] = frequency;
                }
                _VectorXMLs.Add(vectorList);
                vectorList = InitVector();
            }
        }



        private Dictionary<string, int> ProcessingTopicsDictionary()
        {
            var firstTopicDictionary = new Dictionary<string, int>();
            var topics = new List<string>();
            var keys = new List<string>();
            var values = new List<int>();

            if (topicsDictionary.Count >= 1)
            {
                foreach (KeyValuePair<int, string> pair in topicsDictionary)
                {
                    string[] classes = pair.Value.Split(' ');
                    topics.Add(classes[0]);           
                }

                keys = topics.Distinct().ToList();

                foreach (var group in topics.GroupBy(s => s))
                    values.Add(group.Count());

                for (int i = 0; i < keys.Count; i++)
                    firstTopicDictionary.Add(keys[i], values[i]);
                

                return firstTopicDictionary;
            }
            else
            {
                return new Dictionary<string, int>();
            }
        }

        private double CalculateEntropy(Dictionary<string, int> dataSet)
        {
            double entropy = 0.00f;
            int totalClasses = 0;

            foreach (KeyValuePair<string, int> pair in dataSet)
                totalClasses += pair.Value;

            foreach (KeyValuePair<string, int> pair in dataSet)
                entropy -= ((double)pair.Value / totalClasses) * Math.Log((double)pair.Value / totalClasses, 2);

            return Math.Round(entropy, 2);
        }

        public void FeatureSelectionStep()
        {
            var xmlClasses = ProcessingTopicsDictionary();

            //  Entropy DONE
            Console.WriteLine(CalculateEntropy(xmlClasses));
            foreach (KeyValuePair<string, int> pair in xmlClasses)
                Console.WriteLine("{0}:{1} ", pair.Key, pair.Value);

        }



        // ~~~ All Print Functions !!! ~~~  //

        public void PrintWordsDictionary()
        {
            foreach (KeyValuePair<string, int> pair in wordsDictionary)
                Console.WriteLine("{0}:{1} ", pair.Key, pair.Value);
        }

        public void PrintTopicsDictionary()
        {
            foreach (KeyValuePair<int, string> pair in topicsDictionary)
                Console.WriteLine("{0}:{1} ", pair.Key, pair.Value);
        }

        public void PrintListOfDictionaries()
        {
            int fileNr = 1;
            foreach (var dictionary in listOfDictionaries)
            {
                Console.WriteLine("- - - - - - - - - - - - {0} - - - - - - - - - - - -", fileNr++);
                foreach (KeyValuePair<string, int> pair in dictionary)
                    Console.WriteLine("{0}:{1} ", pair.Key, pair.Value);
            }
        }

        public void PrintWordsDictionary_Length()
        {
            Console.WriteLine("Nr of unique words : {0}", wordsDictionary.Count());
        }

        public void PrintNrOfAllWordsFromList()
        {
            int wordsSum = 0;
            foreach (var dictionary in listOfDictionaries)
                wordsSum += dictionary.Count();
            Console.WriteLine("Nr of all words from all dictionaries : {0}", wordsSum);
        }

        public void PrintVectors()
        {
            foreach(var list in _VectorXMLs)
            {
                foreach (var item in list)
                    Console.Write("{0} ", item);
                Console.WriteLine();
            }
        }
    }
}
