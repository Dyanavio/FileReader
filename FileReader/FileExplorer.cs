using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileReader
{
    //@"D:\FilesForProject"
    
    public class FileExplorer
    {
        private static SemaphoreSlim semaphore = new(1);
        private List<string> files = new List<string>();
        private List<string> reports = new List<string>();
        public async Task ProcessFiles(HashSet<string> forbiddenWords)
        {
            await semaphore.WaitAsync();
            string report = $"----- Report for {DateTime.Now} -----\n";
            int fileIndex = 1;
            SortedList<string, int> wordCount = new SortedList<string, int>();
            foreach(string word in forbiddenWords)
            {
                wordCount.Add(word, 0);
            }

            files.Clear();
            GetLocalFiles(@"D:\FilesForProject");
            //GetAllFiles();

            try
            {
                foreach (var file in files)
                {
                    StreamReader reader = new StreamReader(file);
                    string? line;
                    string correctedText = "";
                    int replacements = 0;
                    bool isIncorrect = false;

                    while((line = reader.ReadLine()) != null)
                    {
                        char[] punctuation = line.Where(Char.IsPunctuation).Distinct().ToArray();
                        IEnumerable<string> words = line.Split().Select(x => x.Trim(punctuation));

                        foreach(string word in words)
                        {
                            int index = line.IndexOf(word);
                            foreach(string forbiddenWord in forbiddenWords)
                            {
                                if(word.ToLower() == forbiddenWord.ToLower())
                                {
                                    isIncorrect = true;
                                    wordCount[forbiddenWord]++;
                                    line = line.Remove(index, word.Length).Insert(index, "*******");
                                    replacements++;
                                }
                            }
                        }
                        correctedText += line + "\n";
                    }
                    if(isIncorrect)
                    {
                        File.Copy(file, Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + $@"\FIles\ForbiddenFIles\{Path.GetFileNameWithoutExtension(file) + "0"}.txt", true);
                        using (StreamWriter writer = File.CreateText(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @$"\FIles\CorrectedFiles\{Path.GetFileNameWithoutExtension(file) + "_Corrected"}.txt"))
                        {
                            writer.Write(correctedText);
                        }
                        report += $"{fileIndex}. {file} | Size: {new System.IO.FileInfo(file).Length} | Replacements: {replacements}\n";
                        fileIndex++;
                    }
                }
                using (StreamWriter writer = File.CreateText(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @$"\FIles\Reports\{DateTime.Now.Date.Day}-{DateTime.Now.Date.Month}-{DateTime.Now.Date.Year}.txt"))
                {
                    writer.Write(report);
                }
            }
            catch(Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
            }
            finally
            {
                Console.ResetColor();
                semaphore.Release();
            }
        }
        public void CreateReport()
        {
            // -------------------------------
        }
        public void GetLocalFiles(string directory)
        {
            files.AddRange(Directory.GetFiles(@"D:\FilesForProject", "*.txt"));
         
            string[] directories = Directory.GetDirectories(directory, "*", new EnumerationOptions() { RecurseSubdirectories = true, IgnoreInaccessible = true });
            foreach (string d in directories)
            {
                try
                {
                    files.AddRange((Directory.GetFiles(d, "*.txt")));
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Message);
                    Console.ResetColor();
                }
            }
        }
        public void GetAllFiles()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (var drive in drives)
            {
                try
                {
                    if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                    {
                        Console.WriteLine($"Reading files from drive: {drive.Name}");
                        string[] directories = Directory.GetDirectories(drive.Name, "*", new EnumerationOptions() { RecurseSubdirectories = true, IgnoreInaccessible = true });

                        foreach (string directory in directories)
                        {
                            try
                            {
                                files.AddRange(files.Concat<string>(Directory.GetFiles(directory, "*.txt")));
                            }
                            catch (Exception e)
                            { 
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(e.Message);
                                Console.ResetColor();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Message);
                    Console.ResetColor();
                }
            }
        }
    }
}
