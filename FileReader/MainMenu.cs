using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileReader
{
    public class Menu
    {
        private string[] options;
        private int chosenOption;
        private string prompt;

        public Menu(string prompt, string[] options)
        {
            this.prompt = prompt;
            this.options = options;
            chosenOption = 0;
        }
        public void DisplayOptions()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(prompt);
            Console.ResetColor();
            for(int i = 0; i < options.Length; i++)
            {
                if(i == chosenOption)
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.DarkYellow;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                Console.WriteLine($"\t<< {options[i]} >>");
            }
            Console.ResetColor();
        }
        public async Task<int> Run()
        {
            ConsoleKey keyPressed;
            do
            {
                Console.Clear();
                DisplayOptions();

                ConsoleKeyInfo key = Console.ReadKey(true);
                keyPressed = key.Key;
              
                if(keyPressed == ConsoleKey.UpArrow)
                {
                    chosenOption--;
                    if(chosenOption < 0) chosenOption = options.Length - 1;
                }
                else if(keyPressed == ConsoleKey.DownArrow)
                {
                    chosenOption++;
                    if (chosenOption >= options.Length) chosenOption = 0;
                }
            } while (keyPressed != ConsoleKey.Enter);

            return chosenOption;
        }
    }
}
