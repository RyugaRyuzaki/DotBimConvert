// See https://aka.ms/new-console-template for more information
﻿using System;
using System.Collections.Generic;
using File=dotbim.File;
namespace CompressDotBim
{
    internal class Program {
        static void Main(string[] args) {
            

             if (args.Length <3 )
            {
                Console.WriteLine("Usage: dotnet run <filePath> <output_folder> <option:com|deCom>");
                return;
            }
            string filePath  = args[0];
            string output = args[1];
            string option = args[2];
            if (null == output) output = "./output";
            // check existing file
            if (!System.IO.File.Exists(filePath ))
            {
                Console.WriteLine($"File '{filePath }' not found.");
                return;
            }
            if (!Directory.Exists(output))
            {
                try
                {
                    Directory.CreateDirectory(output);
                    Console.WriteLine($"Directory '{output}' created successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating directory: {ex.Message}");
                     return;
                }
            }

            // check option
            if(null==option) {
                Console.WriteLine($"Missing option");
                return;
            }

            var startTime = DateTime.Now;
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            if(option=="com") {
                try
                {
                    File file= File.Read(filePath);
                    byte[] bytes = Utils.CompressDotBimFile(file);
                    System.IO.File.WriteAllBytes(output+"/"+fileName+"-csharp-com"+".gz", bytes);
                    Console.WriteLine($"Compress c# completed in {(DateTime.Now - startTime).Microseconds/1000.0} seconds.");
                }
                catch (Exception ex)
                {
                    throw  new Exception($"{ex.Message}");
                }
               
            }else if(option=="deCom"){
                try
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(filePath);
                    File file = Utils.DeCompressDotBimFile(bytes);
                    file.Save(output + "/" + fileName+"-csharp-deCom" + ".bim", true);
                    Console.WriteLine($"DeCompress c# completed in {(DateTime.Now - startTime).Microseconds/1000.0} seconds.");
                }
                catch (Exception ex)
                {
                    throw  new Exception($"{ex.Message}");
                }
                
            }else {
                throw  new Exception($"Option must be 'com' or 'deCom'");
            }
            
            
        }


    }
}