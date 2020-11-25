using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace TextMining
{
    class Program
    {
        static void Main(string[] args)
        {
            string sourceDirectory = @"..\..\..\..\Reuters_34";
            TextMiningEngine engine = new TextMiningEngine();
            string contentFromFiles = engine.GetNodesValuesFromXML(sourceDirectory, "title", "text");
            contentFromFiles = engine.FilterByDelimiters(contentFromFiles);


            /*
             
            engine.MakeDictionary(contentFromFiles);
            //engine.PrintWordsDictionary();
           
            engine.MakeListOfDictionaries(contentFromFiles);
            //engine.PrintListOfDictionaries();
            
            engine.ApplyStowordsFiltering();

            engine.SortAndPrintWordsDictionary();

            engine.MakeVectors();
            //engine.PrintVectors();
            
            */



            engine.ExtractCodeTopicsFromXML(sourceDirectory);
            engine.FeatureSelectionStep();
            engine.PrintTopicsDictionary();
        }
    }
}
