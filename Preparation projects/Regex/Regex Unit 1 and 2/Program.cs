using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Regex_1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var manual = File.ReadAllLines("MonsterManual.txt");

            var monsterNames = new List<string>();
            var flyingMonsters = new List<bool>();
            var tenPlusRolls = new List<bool>();
            var slowFlyers = new List<string>();

            var alignmentsList = new List<string>();
            var alignedMonsters = new List<string>();

            int monsterNameLine = 0;
            
            string currentName = "";


            foreach (string line in manual)
            {
                if (monsterNameLine == 0)
                {
                    monsterNames.Add(line);
                    currentName = line;
                }

                if (line.Length == 0)
                {
                    monsterNameLine = 0;
                }

                else
                {
                    monsterNameLine++;
                }

                // checks for slow flyers
                /*if (monsterNameLine == 5)
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(line, "fly [1-4]\\d ft\\."))
                        {
                            slowFlyers.Add(currentName);                        
                        }

                }*/

                // checks for Alignment
                /*if (monsterNameLine == 2)
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(line, "//chaotic|lawful|neutral|good|evil|neutral"))
                    {
                       
                        if (line.Split(", ").Length > 2) alignmentsList.Add(line.Split(", ")[2]);
                        else alignmentsList.Add(line.Split(", ")[1]);
                        alignedMonsters.Add(currentName);

                    }
                    continue;
                }*/

                // checks for fly ability
                /*bool canFly =  false;
                if (line.Contains("speed", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (line.Contains("fly", StringComparison.CurrentCultureIgnoreCase)) canFly = true;
                    else canFly = false;
                    flyingMonsters.Add(canFly);
                }*/

                // checks for 10+ dice rolls
                bool tenPlus = false;
                if (line.Contains("Hit", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(line, "\\d\\dd", RegexOptions.IgnoreCase))
                    {
                        tenPlus = true;
                    }
                    else tenPlus = false;
                    tenPlusRolls.Add(tenPlus);
                }

            }

            Console.WriteLine("Monsters in the manual are:");
            foreach (string name in monsterNames)
            {
                Console.WriteLine(name);
            }

            /*Console.WriteLine("Monsters requiring 10+ dice rolls are:");
            for (int i = 0; i < tenPlusRolls.Count; i++)
            {
                Console.WriteLine($"{monsterNames[i]} - 10+ dice rolls: {tenPlusRolls[i]}");
                continue;
            }*/

            /*Console.WriteLine("Monsters that can fly are:");
            for (int i = 0; i < flyingMonsters.Count; i++)
            {
                Console.WriteLine($"{monsterNames[i]} - Can fly: {flyingMonsters[i]}");
                continue;
            }*/

            /*Console.WriteLine("Monsters that can fly 10-49 feet per turn:");
            for (int i = 0; i < slowFlyers.Count; i++)
            {
                Console.WriteLine($"{slowFlyers[i]}");
            }*/

            /*Console.WriteLine("Monsters with a specific alignment:");
            for (int i = 0; i < alignmentsList.Count; i++)
            {
                Console.WriteLine($"{alignedMonsters[i]}: {alignmentsList[i]}");
            }*/

        }
    }
}
