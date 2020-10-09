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
            string sourceDirectory = @"C:\PATH\Reuters_34mini";
            TextMiningEngine engine = new TextMiningEngine();
            string contentFromFiles = engine.GetNodesValuesFromXML(sourceDirectory, "title", "text");
            contentFromFiles = engine.FilterByDelimiters(contentFromFiles);
            /*
            Console.WriteLine(contentFromFiles);
            Console.WriteLine(engine.GetLastKeysFromXmlFiles(contentFromFiles));
            engine.MakeDictionary(contentFromFiles);
            engine.PrintWordsDictionary();
            */


            //engine.MakeListOfDictionaries(contentFromFiles);
            //engine.PrintListOfDictionaries();

            engine.ExtractCodeTopicsFromXML(sourceDirectory);
            engine.PrintTopicsDictionary();
        }
    }
}
