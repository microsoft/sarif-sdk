using System;
using System.IO;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.WorkItems;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;

namespace ConsoleApp1
{
    internal class Program
    {
        private static void Main(string[] _)
        {
            string directory = @"E:\src\sarif\src\Test.FunctionalTests.Sarif\v2\ConverterTestData";

            WalkDirectory(directory);
        }

        private static void WalkDirectory(string directory)
        {
            foreach (string file in Directory.GetFiles(directory, "*.sarif"))
            {
                try
                {
                    string fileText = File.ReadAllText(file);
                    SarifLog sarifLog = PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(fileText, Formatting.None, out string sarifLogText);
                    string title = sarifLog.Runs?[0].CreateWorkItemTitle();

                    if (string.IsNullOrEmpty(title))
                    {
                        Console.WriteLine("NULL title: " + file);
                    }
                    else
                    {
                        //Console.WriteLine(file);
                        Console.WriteLine(sarifLog.Runs?[0].CreateWorkItemTitle());
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Load failed: " + file);
                    Console.WriteLine(e);
                }

            }

            foreach (string childDirectory in Directory.GetDirectories(directory))
            {
                WalkDirectory(childDirectory);
            }
        }
    }
}
