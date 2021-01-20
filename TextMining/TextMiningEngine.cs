using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;

namespace TextMining
{
    class arff
    {
        public double gainRatioValue;
        public string attribute;
        public int index;
        public int newIndex;

        public arff(string attribute, int index, double gainRatioValue)
        {
            this.gainRatioValue = gainRatioValue;
            this.attribute = attribute;
            this.index = index;
            this.newIndex = 0;
        }
    }

    class confusionMatrix
    {
        public int true_pozitive;
        public int true_negative;
        public int flase_positive;
        public int flase_negative;

        public confusionMatrix()
        {
            true_pozitive = 0;
            true_negative = 0;
            flase_positive = 0;
            flase_negative = 0;
        }

        public double computeAccuracy()
        {
            return (double)(true_pozitive + true_negative) / (true_pozitive + true_negative + flase_negative + flase_positive);
        }

        public double computePrecision()
        {
            return (double)(true_pozitive) / (true_pozitive + flase_positive);
        }

        public double computeRecall()
        {
            return (double)(true_pozitive) / (true_pozitive + flase_negative);
        }

        public double computeErrorRate()
        {
            return (double)(flase_positive + flase_negative) / (true_pozitive + true_negative + flase_negative + flase_positive);
        }

    }

    enum Distance { Cosine, Manhattan, Euclid }

    class TextMiningEngine
    {
        private Dictionary<string, int> wordsDictionary = new Dictionary<string, int>();
        private Dictionary<int, string> topicsDictionary = new Dictionary<int, string>();
        private List<string> stopwords = new List<string>();
        private List<Dictionary<string, int>> listOfDictionaries = new List<Dictionary<string, int>>();
        private PorterStemmer _porter = new PorterStemmer();
        private List<string> sortedWordsDictionary = new List<string>();
        private List<List<byte>> VectorXMLs = new List<List<byte>>();


        //---------------------------------------------------------------------------------------
        public string GetNodesValuesFromXML(string path, string tag1, string tag2)
        {
            string workingDirectory = Environment.CurrentDirectory;
            string contentPath = Directory.GetParent(workingDirectory).Parent.Parent.FullName + "\\temp-aux.txt";
            if (File.Exists(contentPath))
                File.Delete(contentPath);

            foreach (string file in Directory.EnumerateFiles(path, "*.xml"))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(file);

                XmlNodeList titleNodes = xmlDoc.GetElementsByTagName(tag1);
                XmlNodeList textNodes = xmlDoc.GetElementsByTagName(tag2);

                string content = "";
                using (FileStream fs = new FileStream(contentPath, FileMode.Append))
                {
                    for (int i = 0; i < titleNodes.Count; i++)
                        content += titleNodes[i].InnerText.ToString();
                    for (int i = 0; i < textNodes.Count; i++)
                        content += textNodes[i].InnerText.ToString();
                    content += " #endfile# ";

                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine(content);
                    sw.Flush();
                }
            }
            return contentPath;
        }

        public void ExtractCodeTopicsFromXML(string path)
        {
            int keyPerFile = 0;
            foreach (string file in Directory.EnumerateFiles(path, "*.xml"))
            {
                string codePerFile = "";
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(file);

                XmlNodeList codeNodes = xmlDoc.GetElementsByTagName("codes");
                for (int i = 0; i < codeNodes.Count; i++)
                {
                    if (codeNodes[i].Attributes[0].InnerText.ToString() == "bip:topics:1.0")
                    {
                        foreach (XmlNode child in codeNodes[i].ChildNodes)
                            codePerFile += child.Attributes[0].InnerText.ToString() + " ";
                        topicsDictionary.Add(keyPerFile, codePerFile);
                    }
                }
                keyPerFile++;
            }

        }

        public string FilterByDelimiters(string content)
        {
            var reader = new StreamReader(content);
            string workingDirectory = Environment.CurrentDirectory;
            string contentPath = Directory.GetParent(workingDirectory).Parent.Parent.FullName + "\\temp.txt";

            string[] delimiters = new string[] { " ", ".", ",", ":", ";", "!", "?", "%", "&", "$", "@", "-", "+", "/", "\t", "*", "'", "=",
                           "[","]", "\\", "'s", "'re", "'d", "\"", "(", ")", "<", ">", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", };

            if (File.Exists(contentPath))
                File.Delete(contentPath);

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] parts = line.Split(delimiters, StringSplitOptions.None);
                string filteredContent = "";
                for (int i = 0; i < parts.Length; i++)
                    filteredContent += _porter.StemWord(parts[i].ToLower()) + " ";

                using (FileStream fs = new FileStream(contentPath, FileMode.Append))
                {
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine(filteredContent);
                    sw.Flush();
                }
            }
            reader.Dispose();
            File.Delete(content);
            return contentPath;
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

        private void MakeDictionary(string content)
        {
            string lines = File.ReadAllText(content);
            string[] delimiters = new string[] { " ", "\r\n" };
            string[] parts = lines.Split(delimiters, StringSplitOptions.None);
            
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

        private void MakeListOfDictionaries(string content)
        {
            string lines = File.ReadAllText(content);
            string[] delimiters = new string[] { " " };
            string[] parts = lines.Split(delimiters, StringSplitOptions.None);
            Dictionary<string, int> dictionary = new Dictionary<string, int>();

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

        private void ApplyStowordsFiltering()
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

        private void SortAndPrintWordsDictionary()
        {
            try
            {
                sortedWordsDictionary = wordsDictionary.Keys.ToList();
                sortedWordsDictionary.Sort();
                Console.WriteLine("(Keep in mind the byte - int cast error rate)");
                Console.WriteLine("\n[:::All words extracted and sorted successfully:::]");
                foreach (var key in sortedWordsDictionary)
                    Console.WriteLine("{0}: {1}", key, wordsDictionary[key]);

                var temp = new Dictionary<string, int>();
                for (int i = 0; i < sortedWordsDictionary.Count; i++)
                    temp.Add(sortedWordsDictionary[i], wordsDictionary[sortedWordsDictionary[i]]);
                wordsDictionary = temp;
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

        private void MakeVectors()
        {
            int i;
            byte frequency = 0;
            var vectorList = InitVector();

            foreach (var dictionary in listOfDictionaries)
            {
                foreach (KeyValuePair<string, int> pair in dictionary)
                {
                    i = -1;
                    foreach (var pairGlobal in sortedWordsDictionary)
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
                VectorXMLs.Add(vectorList);
                vectorList = InitVector();
            }
        }
        
        public void ApplyFeatureExtraction_Step1(string XmlContentFromFiles)
        {
            if (!String.IsNullOrEmpty(XmlContentFromFiles))
            {
                MakeDictionary(XmlContentFromFiles);
                MakeListOfDictionaries(XmlContentFromFiles);
                ApplyStowordsFiltering();
                SortAndPrintWordsDictionary();
                MakeVectors();
            }
            else Console.WriteLine("Error while reading XML files; Check if the root path is correct!");
        }
        
        //---------------------------------------------------------------------------------------

        private Dictionary<string, int> ProcessingTopicsDictionary()
        {
            var firstTopicDictionary = new Dictionary<string, int>();
            
            var keys = new List<string>();
            var values = new List<int>();

            if (topicsDictionary.Count >= 1)
            {
                var topics = GetFirstColumnFromTopicsDictionary();
                var allTopics = GetAllTopicsFromTopicsDictionary();
                var wrongTopics = GetTopicsWithWrongProbability(allTopics);
                
                RemakeTopicsDictionaryAccordingToWrongTopics(wrongTopics);
                topics = GetFirstColumnFromTopicsDictionary();
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

        private List<string> GetFirstColumnFromTopicsDictionary()
        {
            var topics = new List<string>();

            foreach (KeyValuePair<int, string> pair in topicsDictionary)    
            {
                string[] classes = pair.Value.Split(' ');
                for (int i = 0; i < classes.Count(); i++)
                {
                    if (classes[0] != "")
                    {
                        topics.Add(classes[0]);
                        break;
                    }
                }
            }
            return topics;
        }
        
        private List<string> GetAllTopicsFromTopicsDictionary()
        {
            var allTopics = new List<string>();
            foreach (KeyValuePair<int, string> pair in topicsDictionary)
            {
                string[] classes = pair.Value.Split(' ');

                foreach (var target in classes)
                    if (target != "")
                        allTopics.Add(target);
            }
            return allTopics;
        }

        private List<string> GetTopicsWithWrongProbability(List<string> topics)
        {
            var topicsWithWrongProbability = new List<string>();
            float probability = 0.00f;
            float counter = 0.00f;

            foreach (var group in topics.GroupBy(s => s))
            {
                counter = group.Count();
                probability = counter / topicsDictionary.Count;
                if (probability < 0.05 || probability > 0.95)  
                    topicsWithWrongProbability.Add(group.Key);
            }

            return topicsWithWrongProbability;
        }

        private List<int> GetNullStringIndexesFromTopicsDictionary()
        {
            var indexes = new List<int>();
            foreach (KeyValuePair<int, string> pair in topicsDictionary.ToList())
            {
                if (string.IsNullOrWhiteSpace(pair.Value)) 
                {
                    indexes.Add(pair.Key);
                    topicsDictionary.Remove(pair.Key);
                }
            }
            return indexes;
        }

        private void RemakeTopicsDictionaryAccordingToWrongTopics(List<string> wrongTopics)
        {
            var temp = new Dictionary<int, string>();
            foreach (KeyValuePair<int, string> pair in topicsDictionary)
            {
                string value = pair.Value;
                foreach (var word in wrongTopics)
                {
                    value = value.Replace(word, "");
                }

                string[] rowTopics = value.Split(' ');
                int ct = 0;
                for (int i = 0; i < rowTopics.Count(); i++)
                {
                    if (rowTopics[i] != "" && rowTopics[i].Length >= 2)
                    {
                        temp.Add(pair.Key, rowTopics[i]); break;
                    }
                    else ct++;
                }
                if (ct == rowTopics.Count())
                    temp.Add(pair.Key, "");
            }
            topicsDictionary.Clear();
            topicsDictionary = temp;
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

        private void AdjustVectorsAndTopicsDictionary()
        {
            var forbiddenIndexes = GetNullStringIndexesFromTopicsDictionary();
            var temp = new List<List<byte>>();

            for (int i = 0; i < VectorXMLs.Count(); i++) 
            {
                if (forbiddenIndexes.Contains(i) == false)
                    temp.Add(VectorXMLs[i]);
            }
            VectorXMLs = temp;
        }

        private List<double> ComputeInfoGain(double globalEntropy)
        {
            var distinctAttrValue = new List<byte>();
            var dictValueTarget = new Dictionary<string, int>();
            var gainRatio = new List<double>();

            // Extract Target Classes
            var targetClasses = GetFirstColumnFromTopicsDictionary();

            // Extract Column Atribut Values 
            for (int i = 0; i < VectorXMLs[0].Count; i++) 
            {
                var columnAttr = GetColumnFromVectorXML(i);
                double partialGain = globalEntropy;
                double split = 0;
                int sum = 0;
                //-----------------
                distinctAttrValue = columnAttr.Distinct().ToList();
                foreach(var distElem in distinctAttrValue)
                {
                    dictValueTarget.Clear();
                    sum = 0;
                    for (int j = 0; j < columnAttr.Count; j++) 
                    {
                        if(distElem == columnAttr[j])
                        {
                            sum++;
                            if (dictValueTarget.ContainsKey(targetClasses[j]))
                                dictValueTarget[targetClasses[j]]++;
                            else
                                dictValueTarget.Add(targetClasses[j], 1);
                        }
                    }
                    partialGain -= ((double)sum / columnAttr.Count) * CalculateEntropy(dictValueTarget);
                    split -= ((double)sum/ columnAttr.Count) * Math.Log(((double)sum / columnAttr.Count), 2);
                }
                if (split != 0)
                    gainRatio.Add(partialGain / split);
                else
                    gainRatio.Add(partialGain);
                //-----------------
            }
            return gainRatio;
        }

        private List<arff> GetRelevantAttrFromGainRatio(List<double> gainRatioList, int percentage)
        {
            var relevantAttributes = new Dictionary<int, double>();
            int numberOfRelevants = (gainRatioList.Count * percentage) / 100;
            
            var gainRatioObjList = new List<arff>();
            for (int i = 0; i < gainRatioList.Count; i++)
                gainRatioObjList.Add(new arff(sortedWordsDictionary[i], i, gainRatioList[i]));

            var temp1 = gainRatioObjList.OrderByDescending(x => x.gainRatioValue).ToList();
            var temp2 = new List<arff>();
            for (int i = 0; i < numberOfRelevants; i++)
                temp2.Add(temp1[i]);

            gainRatioObjList = temp2;

            return gainRatioObjList.OrderBy(x=>x.index).ToList();
        }

        private List<arff> CreateNewIndexDomainAfter10ProcFiltering(List<arff> arffs)
        {
            for (int i = 0; i < arffs.Count; i++)
                arffs[i].newIndex = i;

            return arffs;
        }

        private List<byte> GetColumnFromVectorXML(int x)
        {
            var temp = new List<byte>();
            foreach (var list in VectorXMLs)
                temp.Add(list.ElementAt(x));
            return temp;
        }

        private void AttachAttributesInExport_arff(string filePath, List<arff> arffAttributes, List<string> targetClasses)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);

            using (FileStream fs = new FileStream(filePath, FileMode.Append))
            {
                StreamWriter sw = new StreamWriter(fs);
                string allClasses = "";

                foreach (var itemAttr in arffAttributes)
                    sw.WriteLine("@attribute " + itemAttr.attribute + " " + itemAttr.newIndex);///

                foreach (var targetClass in targetClasses)
                    allClasses += targetClass + ",";

                sw.WriteLine("@classes {" + allClasses + "}");
                sw.WriteLine();
                sw.WriteLine("@data");
                sw.Flush();
            }
        }

        private List<List<byte>> GetRareVectors(List<arff> arffAttributes, List<int> rareIndexes)
        {
            var rareVectors = new List<List<byte>>();
            int counter = 0;
            foreach (var list in VectorXMLs)
            {
                string vectLine = "";
                foreach (var itemAttr in arffAttributes)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (i == itemAttr.index && list[i] != 0)
                            vectLine += itemAttr.index.ToString() + ":" + list[i].ToString() + ",";
                    }
                }
                if (vectLine == "" && counter < VectorXMLs.Count)
                {
                    rareVectors.Add(VectorXMLs[counter]);
                    rareIndexes.Add(counter);
                }
                counter++;
            }
            return rareVectors;
        }

        private List<string> GetFilteredTargetClasses(List<int> indexes)
        {
            var newTargetClasses = new List<string>();
            var targetClasses = GetFirstColumnFromTopicsDictionary();

            for (int i = 0; i < targetClasses.Count; i++)
                if (!indexes.Contains(i))
                    newTargetClasses.Add(targetClasses[i]);
            
            return newTargetClasses;
        }

        private List<List<byte>> getRandom30ProcOfVectorXMLEntries(List<List<byte>> xmlVectors, List<int> randomNumbers)
        {
            var rand = new Random(DateTime.UtcNow.Millisecond);
            var temp30ProcOfVectorXML = new List<List<byte>>();
            int threshold = 30 * xmlVectors.Count() / 100;
            //var randomNumbers = new List<int>();
            int counter = 0;

            while (counter < threshold)
            {
                int random = rand.Next(0, xmlVectors.Count());
                if (randomNumbers.IndexOf(random) != -1)
                {
                    int value = random;
                    while (value == random || randomNumbers.Contains(random) == true)
                        random = rand.Next(0, xmlVectors.Count());
                }
                temp30ProcOfVectorXML.Add(xmlVectors[random]);
                randomNumbers.Add(random);
                counter++;
            }
            return temp30ProcOfVectorXML;
        }

        public void ApplyFeatureSelection_Step2()
        {
            //  Process only the first topic from the sample
            var xmlClasses = ProcessingTopicsDictionary();
            var globalEntropy = CalculateEntropy(xmlClasses);


            //  Entropy DONE
            Console.WriteLine("\n[:::Calculated Entropy for attribute set : {0} :::]", globalEntropy);
            Console.WriteLine(">> Classes frequency after feature selection :");
            foreach (KeyValuePair<string, int> pair in xmlClasses)
                Console.WriteLine("{0}:{1} ", pair.Key, pair.Value);


            //  Needed because of first topic filtering
            AdjustVectorsAndTopicsDictionary();


            //  Compute GainRatio -> took only 10% most relevant attributes
            var GainRatioList = ComputeInfoGain(globalEntropy);
            var arffAttributes = CreateNewIndexDomainAfter10ProcFiltering(GetRelevantAttrFromGainRatio(GainRatioList, 10));///
            GainRatioList.Clear();


            //--------------------------------------------------------------------------------------------------------------------
            //  Generate .arff export file// Extract Target Classes
            string workingDirectory = Environment.CurrentDirectory;
            string projectTestDir = Directory.GetParent(workingDirectory).Parent.Parent.FullName + "\\Export-Test.arff";
            string projectTrainingDir = Directory.GetParent(workingDirectory).Parent.Parent.FullName + "\\Export-Training.arff";

            var rareIndexes = new List<int>();
            var rareVectorsXML = GetRareVectors(arffAttributes, rareIndexes);

            var filteredVectorXML = new List<List<byte>>(VectorXMLs.Except(rareVectorsXML));

            var targetClasses = GetFilteredTargetClasses(rareIndexes);

            var randomTestIndexes = new List<int>();
            var VectorXMLTestSet = getRandom30ProcOfVectorXMLEntries(filteredVectorXML, randomTestIndexes);

            AttachAttributesInExport_arff(projectTestDir, arffAttributes, targetClasses);
            AttachAttributesInExport_arff(projectTrainingDir, arffAttributes, targetClasses);

            int lineCt = 0;
            // Writing the trainingSet file
            foreach (var list in filteredVectorXML.Except(VectorXMLTestSet))
            {
                string vectLine = "";
                for (int i = 0; i < list.Count; i++)
                {
                    foreach (var itemAttr in arffAttributes)
                    {
                        if (i == itemAttr.index && list[i] != 0)
                            vectLine += itemAttr.newIndex + ":" + list[i] + ",";///
                    }
                }
                using (FileStream fs = new FileStream(projectTrainingDir, FileMode.Append))
                {
                    if (vectLine != "")
                    {
                        var stringBuilder = new StringBuilder(vectLine);
                        stringBuilder.Remove(vectLine.LastIndexOf(","), 1);
                        stringBuilder.Insert(vectLine.LastIndexOf(","), " # ");

                        while (randomTestIndexes.Contains(lineCt))
                            lineCt++;

                        vectLine = stringBuilder.ToString() + targetClasses[lineCt];

                        StreamWriter sw = new StreamWriter(fs);
                        sw.WriteLine(vectLine);
                        sw.Flush();
                    }
                    if (lineCt < targetClasses.Count)
                        lineCt++;
                }
            }
            Console.WriteLine("\n>> 70% Training Set exported successfully!");

            // Writing the testSET file
            lineCt = 0;
            foreach (var list in VectorXMLTestSet)
            {
                string vectLine = "";
                for (int i = 0; i < list.Count; i++)
                {
                    foreach (var itemAttr in arffAttributes)
                    {
                        if (i == itemAttr.index && list[i] != 0)
                            vectLine += itemAttr.newIndex + ":" + list[i] + ",";///
                    }
                }
                using (FileStream fs = new FileStream(projectTestDir, FileMode.Append))
                {
                    if (vectLine != "")
                    {
                        var stringBuilder = new StringBuilder(vectLine);
                        stringBuilder.Remove(vectLine.LastIndexOf(","), 1);
                        stringBuilder.Insert(vectLine.LastIndexOf(","), " # ");

                        vectLine = stringBuilder.ToString() + targetClasses[randomTestIndexes[lineCt]];

                        StreamWriter sw = new StreamWriter(fs);
                        sw.WriteLine(vectLine);
                        sw.Flush();
                    }
                    if (lineCt < targetClasses.Count)
                        lineCt++;
                }
            }
            Console.WriteLine(">> 30% Test Set exported successfully!\n");
        }
        
        //---------------------------------------------------------------------------------------

        private List<List<double>> ApplyNormalization(string arffPath, List<string> outClasses)
        {
            var temp = new List<List<double>>();
            var reader = new StreamReader(arffPath);
            var line = ""; 
            var classTag = "#";
            var dataTag = "@data";
            var attrTag = "@attribute";
            int numOfAttributes = 0;
            bool interestingData = false;
            string[] delimiters = new string[] { ",", ":", " ", "" };
            var vectorMatrix = new List<Dictionary<int, byte>>();
            
            
            // Import classes and attributes from arff
            outClasses.Clear();
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
                var dictionary = new Dictionary<int, byte>();

                if (line.Contains(attrTag))
                    numOfAttributes++;

                if (interestingData == true)
                {
                    if (line.Contains(classTag))
                        outClasses.Add(line.Substring(line.IndexOf(classTag) + 1));

                    string[] words = line.Split(delimiters, StringSplitOptions.None);
                    int index = 0;
                    byte attrib = 0;

                    for (int i = 0; i < words.Count(); i+=2)
                    {
                        if (words[i] == "#")
                            break;

                        if (i % 2 == 0)   
                            index = Int32.Parse(words[i]);

                        if ((i + 1) % 2 != 0)
                            attrib = Byte.Parse(words[i + 1]);

                        dictionary.Add(index, attrib); 
                    }
                    vectorMatrix.Add(dictionary);
                }

                if (line.StartsWith(dataTag))
                    interestingData = true;
            }
            reader.Close();

            // Start Cornell Smart Normalization
            var normalizationList = new List<double>();
            double norm = 0;
            foreach (var vector in vectorMatrix)
            {
                normalizationList.Clear();
                for (int i = 0; i < numOfAttributes; i++)
                {
                    foreach (KeyValuePair<int, byte> elem in vector)
                    {
                        if (i == elem.Key)
                        {
                            if (elem.Value != 0)
                                norm = 1 + Math.Log(1 + Math.Log(elem.Value, 10), 10);
                            break;
                        }
                        else norm = 0;
                    }
                    normalizationList.Add(norm);
                }
                temp.Add(normalizationList.ToList());
            }
            return temp;
        }

        private List<double> DivAllElementsByScalar(List<double> list, int scalar)
        {
            if(scalar >= 1)
            {
                for (int i = 0; i < list.Count; i++)
                    list[i] /= scalar;
            }
            return list;
        }

        private double ComputeCosineDistanceSimilarity(List<double> vect1, List<double> vect2)
        {
            double distance = 0, numerator = 0, denominator1 = 1, denominator2 = 1;
            for (int i = 0; i < vect1.Count; i++)
            {
                numerator += vect1[i] * vect2[i];
                denominator1 += vect1[i] * vect1[i];
                denominator2 += vect2[i] * vect2[i];
            }
            distance = numerator / (Math.Sqrt(denominator1) * Math.Sqrt(denominator2));
            return 1 - distance; // (1-disimilarity) = similarity
        }

        private double ComputeManhattanDistanceSimilarity(List<double> vect1, List<double> vect2)
        {
            double distance = 0;
            for (int i = 0; i < vect1.Count; i++)
                distance += Math.Abs(vect1[i] - vect2[i]);
            return distance;
        }

        private double ComputeEuclideanDistanceSimilarity(List<double> vect1, List<double> vect2)
        {
            double distance = 0;
            for (int i = 0; i < vect1.Count; i++)
            {
                distance += Math.Pow(Math.Abs(vect1[i] - vect2[i]), 2);
            }
            return Math.Sqrt(distance);
        }

        private List<string> ComputeRocchioPredictedClasses(List<List<double>> testNormalized, List<List<double>> centroids, List<string> centroidsClasses, Distance dist)
        {
            double distance;
            var distanceList = new List<double>();
            var predictedClasses = new List<string>();

            // Compute testing-training distances
            for (int i = 0; i < testNormalized.Count; i++)
            {
                distanceList.Clear();
                for (int j = 0; j < centroids.Count; j++)
                {
                    switch (dist)
                    {
                        case Distance.Cosine:
                            {
                                distance = ComputeCosineDistanceSimilarity(testNormalized[i], centroids[j]);
                                distanceList.Add(distance);
                                break;
                            }
                        case Distance.Manhattan:
                            {
                                distance = ComputeManhattanDistanceSimilarity(testNormalized[i], centroids[j]);
                                distanceList.Add(distance);
                                break;
                            }
                        case Distance.Euclid:
                            {
                                distance = ComputeEuclideanDistanceSimilarity(testNormalized[i], centroids[j]);
                                distanceList.Add(distance);
                                break;
                            }
                        default:break;
                    }
                }
                var minValue = distanceList.Min(x => x);
                var index = distanceList.IndexOf(minValue);
                predictedClasses.Add(centroidsClasses[index]);
            }
            return predictedClasses;
        }

        private List<confusionMatrix> EvaluateRoochioPrediction(List<string> predictedClasses, List<string> actualClasses)
        {
            var confMattrList = new List<confusionMatrix>();
            var distinctPredictions = predictedClasses.Distinct().ToList();

            for (int i = 0; i < distinctPredictions.Count; i++)
            {
                var confMattrForOneClass = new confusionMatrix();
                for (int j = 0; j < predictedClasses.Count; j++) 
                {
                    //...
                    if (distinctPredictions[i] == predictedClasses[j] && distinctPredictions[i] == actualClasses[j])
                        confMattrForOneClass.true_pozitive++;
                    if (distinctPredictions[i] != predictedClasses[j] && distinctPredictions[i] == actualClasses[j])
                        confMattrForOneClass.flase_negative++;
                    if (distinctPredictions[i] == predictedClasses[j] && distinctPredictions[i] != actualClasses[j])
                        confMattrForOneClass.flase_positive++;
                    if (distinctPredictions[i] != predictedClasses[j] && distinctPredictions[i] != actualClasses[j])
                        confMattrForOneClass.true_negative++;
                }
                confMattrList.Add(confMattrForOneClass);
            }
            return confMattrList;
        }

        private void PrintRoochioPredictionAndActualClasses(List<string> predictedClasses, List<string> actualClasses, Distance dist)
        {
            Console.WriteLine("[:::Printing Rocchio Learning Algorithm Predictions:::]");
            switch (dist)
            {
                case Distance.Cosine:
                    {
                        Console.WriteLine(">> Cosine Distance is computed for evaluating testing - training sets!");
                        break;
                    }
                case Distance.Manhattan:
                    {
                        Console.WriteLine(">> Manhattan Distance is computed for evaluating testing - training sets!");
                        break;
                    }
                case Distance.Euclid:
                    {
                        Console.WriteLine(">> Euclidian Distance is computed for evaluating testing - training sets!");
                        break;
                    }
                default:
                    break;
            }
            for (int i = 0; i < predictedClasses.Count; i++)
                Console.WriteLine(">> Prediction Class:{0}\t Actual Class:{1}", predictedClasses[i], actualClasses[i]);
        }

        private void PrintRoochioEvaluationMatrixResults(List<confusionMatrix> confusionMatrices)
        {
            double accuracy = 0, precision = 0, recall = 0, err_rate = 0;
            for (int i = 0; i < confusionMatrices.Count; i++)
            {
                accuracy += confusionMatrices[i].computeAccuracy();
                precision += confusionMatrices[i].computePrecision();
                recall += confusionMatrices[i].computeRecall();
                err_rate += confusionMatrices[i].computeErrorRate();
            }

            accuracy = accuracy / confusionMatrices.Count();
            precision = precision / confusionMatrices.Count();
            recall = recall / confusionMatrices.Count();
            err_rate = err_rate / confusionMatrices.Count();

            Console.WriteLine(">> Evaluation Results :\n   [Accuracy:{0}]\n   [Precision:{1}]\n   [Recall:{2}]\n   [ErrorRate:{3}]\n", accuracy, precision, recall, err_rate);
        }

        public void ApplyLearningAlgRocchio_Step3()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string testingFilePath = Directory.GetParent(workingDirectory).Parent.Parent.FullName + "\\Export-Test.arff";
            string trainingFilePath = Directory.GetParent(workingDirectory).Parent.Parent.FullName + "\\Export-Training.arff";

            var testClasses = new List<string>();
            var testNormalized = ApplyNormalization(testingFilePath, testClasses);

            var trainingClasses = new List<string>();
            var trainingNormalized = ApplyNormalization(trainingFilePath, trainingClasses);

            var counter = 0;
            var aux_avg = new List<double>();
            var duplicates = new List<string>();
            var centroids = new List<List<double>>();
            var centroidsClasses = new List<string>(trainingClasses.Distinct());
            
            // Compute Rocchio Centroids
            for (int i = 0; i < trainingClasses.Count; i++)
            {
                counter = 1;
                aux_avg = trainingNormalized[i];
                for (int j = i + 1; j < trainingClasses.Count; j++)
                {                
                    if (trainingClasses[i] == trainingClasses[j] && duplicates.Contains(trainingClasses[i]) == false) 
                    {
                        counter++;
                        for (int k = 0; k < aux_avg.Count; k++)
                                aux_avg[k] += trainingNormalized[j][k];
                    }
                }

                if (i == trainingClasses.Count - 1 || counter == 1)
                    aux_avg = trainingNormalized[i];

                if (duplicates.Contains(trainingClasses[i]) == false)
                    centroids.Add(DivAllElementsByScalar(aux_avg, counter));

                duplicates.Add(trainingClasses[i]);
                
            }
            trainingNormalized.Clear();

            // --- ROOCHIO WITH MANHATTAN DISTANCE --- //
                // Get Rocchio Predicted Classes
                var predictedClasses = ComputeRocchioPredictedClasses(testNormalized, centroids, centroidsClasses, Distance.Manhattan);
                //
                // Print Rocchio Results
                PrintRoochioPredictionAndActualClasses(predictedClasses, testClasses, Distance.Manhattan);
                //
                // For each predicted class we need to compute it's own confusion matrix
                var evalMatrixList = EvaluateRoochioPrediction(predictedClasses, testClasses);
                //
                // Print Evaluation Final Result
                PrintRoochioEvaluationMatrixResults(evalMatrixList);
            // ---------------------------------------- //
            

            // --- ROOCHIO WITH EUCLIDIAN DISTANCE --- //
                // Get Rocchio Predicted Classes
                predictedClasses = ComputeRocchioPredictedClasses(testNormalized, centroids, centroidsClasses, Distance.Euclid);
                //
                // Print Rocchio Results
                PrintRoochioPredictionAndActualClasses(predictedClasses, testClasses, Distance.Euclid);
                //
                // For each predicted class we need to compute it's own confusion matrix
                evalMatrixList = EvaluateRoochioPrediction(predictedClasses, testClasses);
                //
                // Print Evaluation Final Result
                PrintRoochioEvaluationMatrixResults(evalMatrixList);
                // ---------------------------------------- //
            

            // --- ROOCHIO WITH COSINE DISTANCE --- //
                // Get Rocchio Predicted Classes
                predictedClasses = ComputeRocchioPredictedClasses(testNormalized, centroids, centroidsClasses, Distance.Cosine);
                //
                // Print Rocchio Results
                PrintRoochioPredictionAndActualClasses(predictedClasses, testClasses, Distance.Cosine);
                //
                // For each predicted class we need to compute it's own confusion matrix
                evalMatrixList = EvaluateRoochioPrediction(predictedClasses, testClasses);
                //
                // Print Evaluation Final Result
                PrintRoochioEvaluationMatrixResults(evalMatrixList);
                // ---------------------------------------- //
        }


        //---------------------------------------------------------------------------------------
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
            foreach(var list in VectorXMLs)
            {
                foreach (var item in list)
                    Console.Write("{0} ", item);
                Console.WriteLine();
            }
        }

        //---------------------------------------------------------------------------------------
    }
}
