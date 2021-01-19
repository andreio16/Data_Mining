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
            string sourceDirectory = @"..\..\..\..\Reuters_100";  //  Reuters_34 Reuters_100  Reuters_7083
            TextMiningEngine engine = new TextMiningEngine();
            string contentFromFiles = engine.GetNodesValuesFromXML(sourceDirectory, "title", "text");
            contentFromFiles = engine.FilterByDelimiters(contentFromFiles);
            engine.ExtractCodeTopicsFromXML(sourceDirectory);





            engine.ApplyFeatureExtraction_Step1(contentFromFiles);
            //engine.PrintVectors();

            // - Target classes PROCESSED -
            engine.ApplyFeatureSelection_Step2();
            //engine.PrintTopicsDictionary();

            engine.ApplyLearningAlgRocchio_Step3();

        }
    }
}
