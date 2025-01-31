using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileReader
{
    internal class Manager
    {
        private HashSet<string> forbiddenWords = new();
        public async void RunMainMenu()
        {
            string prompt = "\r\n  ______ _ _        ______            _                     \r\n |  ____(_) |      |  ____|          | |                    \r\n | |__   _| | ___  | |__  __  ___ __ | | ___  _ __ ___ _ __ \r\n |  __| | | |/ _ \\ |  __| \\ \\/ / '_ \\| |/ _ \\| '__/ _ \\ '__|\r\n | |    | | |  __/ | |____ >  <| |_) | | (_) | | |  __/ |   \r\n |_|    |_|_|\\___| |______/_/\\_\\ .__/|_|\\___/|_|  \\___|_|   \r\n                               | |                          \r\n                               |_|                          \r\n";
            string[] options = { "Scan all files", "Edit forbidden word list", "View reports", "Exit" };
            Menu menu = new(prompt, options);

            while (true)
            {
                int selectedIndex = await menu.Run();

                switch (selectedIndex)
                {
                    case 0:
                        ProcessAllFiles();
                        break;
                    case 1:
                        EditForbiddenWordList();
                        break;
                    case 2:
                        ViewReports();
                        break;
                    case 3:
                        Exit();
                        break;
                }
            }
        }
        public void ProcessAllFiles()
        {
            FileExplorer explorer = new FileExplorer();
            Task execution = explorer.ProcessFiles(forbiddenWords);
        }
        public async void EditForbiddenWordList()
        {
            Console.Clear();
            string prompt = "\n-------- OPTIONS --------\n";
            Menu editMenu = new Menu(prompt, new string[] { "Add", "Add from file", "Remove", "Back" });
            
            while(true)
            {
                int chosenOption = await editMenu.Run();
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
                        finally { Console.ResetColor(); }
                        
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
        public void ViewReports()
        {
            //----------------
        }
        public void Exit()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n\n\t\tExiting . . .\n\n");
            Console.ResetColor();
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
