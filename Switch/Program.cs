using System;

class SimpleCalculator
{
    static void Main()
    {
        Console.WriteLine("Set the price:");
        string input = Console.ReadLine() ?? string.Empty;

        string[] parts = input.Split(' ');

        double result = 0;

        if (parts.Length == 1)
        {
            if (double.TryParse(parts[0], out double number))
            {
                result = number;
                Console.WriteLine($"The price was set to {result}.");
            }
            else
            {
                Console.WriteLine("Invalid number input.");
            }
        }
        else if (parts.Length == 3)
        {
            if (double.TryParse(parts[0], out double num1) &&
                double.TryParse(parts[2], out double num2))
            {
                string op = parts[1].ToLower();

                switch (op)
                {
                    case "+":
                    case "plus":
                    case "add":
                        result = num1 + num2;
                        break;

                    case "-":
                    case "minus":
                    case "subtract":
                        result = num1 - num2;
                        break;

                    case "*":
                    case "times":
                    case "multiply":
                        result = num1 * num2;
                        break;

                    case "/":
                    case "divided":
                    case "dividedby":
                    case "divide":
                        if (num2 != 0)
                            result = num1 / num2;
                        else
                        {
                            Console.WriteLine("Cannot divide by zero.");
                            return;
                        }
                        break;

                    default:
                        Console.WriteLine("Unknown operator.");
                        return;
                }

                Console.WriteLine($"The price was set to {result}.");
            }
            else
            {
                Console.WriteLine("Invalid number format.");
            }
        }
        else if (parts.Length == 4 && parts[1].ToLower() == "divided" && parts[2].ToLower() == "by")
        {
            if (double.TryParse(parts[0], out double num1) &&
                double.TryParse(parts[3], out double num2))
            {
                if (num2 != 0)
                {
                    result = num1 / num2;
                    Console.WriteLine($"The price was set to {result}.");
                }
                else
                {
                    Console.WriteLine("Cannot divide by zero.");
                }
            }
            else
            {
                Console.WriteLine("Invalid number format.");
            }
        }
        else
        {
            Console.WriteLine("Invalid input format. Use formats like '2 + 2' or '3 divided by 4'.");
        }
    }
}
