﻿using System;
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
        private List<Dictionary<string, int>> listOfDictionaries = new List<Dictionary<string, int>>();


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
                    content += titleNodes[i].InnerText.ToString();
                for (int i = 0; i < textNodes.Count; i++)
                    content += textNodes[i].InnerText.ToString();
                content += "#endfile# ";
            }
            return content;
        }

        public string FilterByDelimiters(string content)
        {
            string[] delimiters = new string[] { " ", ".", ",", ":", ";", "!", "?", "%", "&", "$", "@", "-", "+", "\'s",
                            "\\", "/", "\'re", "\"", "(", ")", "<", ">", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            string[] parts = content.Split(delimiters, StringSplitOptions.None);
            string filteredContent = "";
            for (int i = 0; i < parts.Length; i++)
                filteredContent += parts[i].ToLower() + " ";
            return filteredContent;
        }

        public string GetLastKeysFromXmlFiles(string content)
        {
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
            foreach (var dictionary in listOfDictionaries)
            {
                foreach (KeyValuePair<string, int> pair in dictionary)
                    Console.WriteLine("{0}:{1} ", pair.Key, pair.Value);
                Console.WriteLine();
            }
        }

    }
}
