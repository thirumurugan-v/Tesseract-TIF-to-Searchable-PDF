using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Tesseract.Demo
{
    public class Program
    {
        public const string TesseractData = @"./tessdata";
        public static void Main()
        {
            //Convert multiple files by adding them to this list.
            var file = new List<File>
            {
                new File { inputFilePath = "./Input/multipage_tiff_example.tif", outputFilePath = @"./output/result" }
            };

            try
            {
                //conver the files using Tesseract engine in parallel.
                foreach (var i in file)
                {
                    Task.Run(() => Convert(i.inputFilePath, i.outputFilePath));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
            Console.WriteLine("Initiating OCR process in background.");
            Console.ReadKey(true);
        }

        /// <summary>
        /// Converts the given input TIF file to searchable PDF using tesseract engine.
        /// </summary>
        /// <param name="inputFilePath">TIF file path</param>
        /// <param name="outputFilePath">Searchable PDF file path</param>
        public static void Convert(string inputFilePath, string outputFilePath)
        {
            using (var renderer = ResultRenderer.CreatePdfRenderer(outputFilePath, TesseractData))
            {
                ProcessImageFile(renderer, inputFilePath);
                Console.WriteLine("Conversion completed for file: " + inputFilePath);
            }
        }


        private static void ProcessImageFile(IResultRenderer renderer, string filename)
        {
            var fileName = Path.GetFileNameWithoutExtension(filename);
            using (var engine = new TesseractEngine(TesseractData, "eng", EngineMode.Default))
            {
                using (var pixA = ReadImageFileIntoPixArray(filename))
                {
                    int expectedPageNumber = -1;

                    foreach (var pix in pixA)
                    {
                        using (var page = engine.Process(pix, fileName))
                        {
                            using (renderer.BeginDocument("multipage_tiff_example"))
                            {
                                var addedPage = renderer.AddPage(page);
                                expectedPageNumber++;
                            }
                        }
                    }

                }
            }
        }

        private static PixArray ReadImageFileIntoPixArray(string filename)
        {
            if (filename.ToLower().EndsWith(".tif") || filename.ToLower().EndsWith(".tiff"))
            {
                return PixArray.LoadMultiPageTiffFromFile(filename);
            }
            else
            {
                PixArray pa = PixArray.Create(0);
                pa.Add(Pix.LoadFromFile(filename));
                return pa;
            }
        }
    }
}
