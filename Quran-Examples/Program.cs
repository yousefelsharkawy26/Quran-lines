using QuranLinesService.Examples;
using System.Text;


var width = Console.WindowWidth;

Console.OutputEncoding = Encoding.UTF8;
Console.BackgroundColor = ConsoleColor.Green;
Console.SetCursorPosition((width / 2) - 25, 0);
Console.WriteLine(new string('=', 50));
Console.SetCursorPosition((width / 2) - 25, 2);
Console.WriteLine("Welcome in Quran Lines Api Service");
Console.SetCursorPosition((width / 2) - 25, 4);
Console.WriteLine(new string('=', 50));
Console.ResetColor();
await ApiUsageExamples.RunAllExamples();


Console.ReadKey();