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
            engine.ExtractCodeTopicsFromXML(sourceDirectory);





            engine.ApplyFeatureExtraction_Step1(contentFromFiles);
            //engine.PrintVectors();

            // - Target classes PROCESSED -
            engine.ApplyFeatureSelection_Step2();
            //engine.PrintTopicsDictionary();

            var outClasses = new List<string>();
            var normalization = engine.ApplyNormalization_Step3(@"C:\Users\Andrei\Desktop\ACS_1\TextMining\Labs1\TextMining\Export-Test.arff", outClasses);
        }
    }
}
