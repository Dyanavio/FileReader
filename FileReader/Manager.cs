using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileReader
{
    internal class Manager
    {
        private string initialDirectory = @"D:\FilesForProject";
        private HashSet<string> forbiddenWords = new();
        private static SemaphoreSlim semaphore = new(1, 1);
        private List<string> files = new List<string>();
        private List<string> reports = new List<string>();
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = cancellationTokenSource.Token;

        public Manager()
        {
            try
            {
                var files = Directory.GetFiles(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @$"\FIles\Reports", "*.txt");
                foreach (string file in files)
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        Task<string> report = reader.ReadToEndAsync();
                        reports.Add(report.Result);
                    }
                }
            }
            catch { }
        }
        public async void RunMainMenu()
        {
            string prompt = "\r\n  ______ _ _        ______            _                     \r\n |  ____(_) |      |  ____|          | |                    \r\n | |__   _| | ___  | |__  __  ___ __ | | ___  _ __ ___ _ __ \r\n |  __| | | |/ _ \\ |  __| \\ \\/ / '_ \\| |/ _ \\| '__/ _ \\ '__|\r\n | |    | | |  __/ | |____ >  <| |_) | | (_) | | |  __/ |   \r\n |_|    |_|_|\\___| |______/_/\\_\\ .__/|_|\\___/|_|  \\___|_|   \r\n                               | |                          \r\n                               |_|                          \r\n";
            string[] options = { "Scan all files", "Edit forbidden word list", "Set searching directory", "View reports", "Exit" };
            Menu menu = new(prompt, options);

            while (true)
            {
                int selectedIndex = menu.Run();

                switch (selectedIndex)
                {
                    case 0:
                        Task processFiles = ProcessFilesAsync();
                        break;
                    case 1:
                        EditForbiddenWordList();
                        break;
                    case 2:
                        SetSearchingDirectory();
                        break;
                    case 3:
                        ViewReports();
                        break;
                    case 4:
                        Exit();
                        break;
                }
            }
        }
        
        public async Task ProcessFilesAsync()
        {
            //await Task.Delay(20000);
            await semaphore.WaitAsync();
            token.ThrowIfCancellationRequested();

            string report = $"----- Report for {DateTime.Now} -----\n";
            int fileIndex = 1;
            SortedList<string, int> wordCount = new SortedList<string, int>();
            foreach (string word in forbiddenWords)
            {
                wordCount.Add(word, 0);
            }

            files.Clear();
            await GetLocalFiles();
            //GetAllFiles();

            try
            {
                foreach (var file in files)
                {
                    token.ThrowIfCancellationRequested();

                    StreamReader reader = new StreamReader(file);
                    string? line;
                    string correctedText = "";
                    int replacements = 0;
                    bool isIncorrect = false;

                    while ((line = reader.ReadLine()) != null)
                    {
                        token.ThrowIfCancellationRequested();

                        char[] punctuation = line.Where(Char.IsPunctuation).Distinct().ToArray();
                        IEnumerable<string> words = line.Split().Select(x => x.Trim(punctuation));

                        foreach (string word in words)
                        {
                            int index = line.IndexOf(word);
                            foreach (string forbiddenWord in forbiddenWords)
                            {
                                if (word.ToLower() == forbiddenWord.ToLower())
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
                    token.ThrowIfCancellationRequested();
                    if (isIncorrect)
                    {
                        File.Copy(file, Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + $@"\FIles\ForbiddenFIles\{Path.GetFileNameWithoutExtension(file) + "0"}.txt", true);
                        using (StreamWriter writer = File.CreateText(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @$"\FIles\CorrectedFiles\{Path.GetFileNameWithoutExtension(file) + "_Corrected"}.txt"))
                        {
                            await writer.WriteAsync(new ReadOnlyMemory<char>(correctedText.ToCharArray()), token);
                        }
                        report += $"{fileIndex}. {file} | Size: {new System.IO.FileInfo(file).Length} | Replacements: {replacements}\n";
                        fileIndex++;
                    }
                }
                int places = Math.Min(3, wordCount.Count);
                foreach(var pair in wordCount)
                {
                    report += $"  - {pair.Key} - {pair.Value}\n";
                    if (--places == 0) break;
                }
                using (StreamWriter writer = File.CreateText(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @$"\FIles\Reports\{DateTime.Now.Date.Day}-{DateTime.Now.Date.Month}-{DateTime.Now.Date.Year} I {DateTime.Now.Date.Hour}-{DateTime.Now.Date.Minute}-{DateTime.Now.Date.Second}.txt"))
                {
                    await writer.WriteAsync(new ReadOnlyMemory<char>(report.ToCharArray()), token);
                }
                reports.Add(report);
            }
            catch (Exception e)
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
        public async Task GetLocalFiles()
        {
            files.AddRange(Directory.GetFiles(initialDirectory, "*.txt"));
            await Task.Delay(2000);

            string[] directories = Directory.GetDirectories(initialDirectory, "*", new EnumerationOptions() { RecurseSubdirectories = true, IgnoreInaccessible = true });
            foreach (string d in directories)
            {
                token.ThrowIfCancellationRequested();
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
        //public async Task  GetAllFiles()
        //{
        //    DriveInfo[] drives = DriveInfo.GetDrives();
        //    await Task.Delay(1);

        //    foreach (var drive in drives)
        //    {
        //        token.ThrowIfCancellationRequested();
        //        try
        //        {
        //            if (drive.IsReady && drive.DriveType == DriveType.Fixed)
        //            {
        //                //Console.WriteLine($"Reading files from drive: {drive.Name}");
        //                string[] directories = Directory.GetDirectories(drive.Name, "*", new EnumerationOptions() { RecurseSubdirectories = true, IgnoreInaccessible = true });

        //                foreach (string directory in directories)
        //                {
        //                    token.ThrowIfCancellationRequested();
        //                    try
        //                    {
        //                        files.AddRange(files.Concat<string>(Directory.GetFiles(directory, "*.txt")));
        //                    }
        //                    catch (Exception e)
        //                    {
        //                        Console.ForegroundColor = ConsoleColor.Red;
        //                        Console.WriteLine(e.Message);
        //                        Console.ResetColor();
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Console.ForegroundColor = ConsoleColor.Red;
        //            Console.WriteLine(e.Message);
        //            Console.ResetColor();
        //        }
        //    }
        //}
        public async void EditForbiddenWordList()
        {
            Console.Clear();
            if (semaphore.CurrentCount == 0)
            {

                Console.ForegroundColor= ConsoleColor.Red;
                Console.WriteLine("\n\n\tCannot edit word list while files are being processed");
                Console.ReadKey();
                return;
            }
            string prompt = "\n-------- OPTIONS --------\n";
            Menu editMenu = new Menu(prompt, new string[] { "Add", "Add from file", "Remove", "Back" });
            
            while(true)
            {
                int chosenOption = editMenu.Run();
                if(chosenOption == 2 && forbiddenWords.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n\tCollection of words is empty");
                    Console.ReadKey();
                    continue;
                }
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                switch (chosenOption)
                {
                    case 0:
                        Console.WriteLine("\n-------- ADDING --------\n");
                        Console.Write("Enter: ");
                        try
                        {
                            string? input = Console.ReadLine();
                            if (input == null || input == "") throw new Exception("String must not be empty");
                            forbiddenWords.Add(input);
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"\n\t{ex.Message}");
                            Console.ReadKey();
                        }
                        finally
                        {
                            Console.ResetColor();
                        }
                        break;
                    case 1:
                        Console.WriteLine("\n-------- ADDING FROM FILE --------\n");
                        Console.Write("Enter the file path: ");
                        try
                        {
                            string? input = Console.ReadLine();
                            if (!File.Exists(input)) throw new Exception($"File not found on the following path: {input}");
                            AddWordsFromFile(input);
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"\n\t{ex.Message}");
                            Console.ReadKey();
                        }
                        finally 
                        {
                            Console.ResetColor();
                        }
                        
                        break;
                     case 2:
                        Console.WriteLine("\n-------- REMOVING --------\n");
                        foreach (string word in forbiddenWords)
                        {
                            Console.WriteLine($"| {word}");
                        }
                        Console.Write("Enter the word: ");
                        try
                        {
                            string? word = Console.ReadLine();
                            if(word == null) throw new ArgumentNullException("Word");
                            if (!forbiddenWords.Contains(word)) throw new Exception($"No word such as \"{word}\" found in registered array");
                            forbiddenWords.Remove(word);
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"\n\t{ex.Message}");
                            Console.ReadKey();
                        }
                        finally
                        {
                            Console.ResetColor();
                        }
                        break;
                    case 3:
                        return;
                }
            }
        }
        public void SetSearchingDirectory()
        {
            Console.Clear();
            if (semaphore.CurrentCount == 0)
            {

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n\n\tCannot set searchin directory while files are being processed");
                Console.ReadKey();
                return;
            }

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("\n-------- SET DIRECTORY --------\n");
            try
            {
                Console.Write($"Current directory: {initialDirectory}\nEnter the searching directory: ");
                string? directory = Console.ReadLine();
                if (directory == null || directory == "") { throw new Exception("Empty directory. No changes applied"); }
                else if (!Path.Exists(directory)) { throw new Exception("No such directory exists"); } 
                else initialDirectory = directory;

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n\t{ex.Message}");
                Console.ReadKey();
            }
            finally
            {
                Console.ResetColor();
            }
        }
        public void ViewReports()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("---------- Reports History ---------\n\n");
            if(reports.Count == 0)
            {
                Console.WriteLine("\tNo reports in history");
                Console.ReadKey();
                return;
            }
            Console.ResetColor();
            foreach(string report in reports)
            {
                Console.WriteLine(report + "\n");
            }
            Console.ReadKey();
        }
        public async void Exit()
        {
            await cancellationTokenSource.CancelAsync();
            
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n\n\t\tExiting . . .\n\n");
            Console.ResetColor();

            cancellationTokenSource.Dispose();
            Environment.Exit(0);
        }
        public void AddWordsFromFile(string? path)
        {
            try
            {
                if (path == null) throw new ArgumentNullException("path");
                StreamReader reader = new StreamReader(path);
                string? line;
                while((line = reader.ReadLine()) != null)
                {
                    forbiddenWords.Add(line);
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n\tSuccessfully added from file");
                Console.ReadKey();
            }
            catch(Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }
            finally { Console.ResetColor(); }
            
        }
       
    }
}
