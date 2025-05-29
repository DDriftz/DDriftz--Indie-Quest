class Program
{
    static void Main()
    {
         string warriorName = "DDrifter";  
        string mageName = "Donut";      
 
        string narrative = "The party stared down the stone stairs into darkness. " +
            "\"We should've brought some torches with us,\" remarked WARRIOR. " +
            "MAGE turned around and replied, \"Worry not dear WARRIOR, let me shine some light for you,\" " +
            "as she cast a Continual Light spell.";

        
        narrative = narrative.Replace("WARRIOR", warriorName).Replace("MAGE", mageName);

        Console.WriteLine(narrative);
    }
}
