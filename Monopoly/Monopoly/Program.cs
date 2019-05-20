using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{
    class Program
    {
        static void Main(string[] args)
        {
            //////////////////////////////////////////////////////////////////////////
            // configure container variables

            List<Player> players = new List<Player>();
            List<Property> properties = new List<Property>();
            List<string> available_characters = new List<string>() { "Battle Ship", "Top Hat", "Shoe", "Dog", "Wheelbarrow", "Race Car", "Iron", "Thimble" };
            Random dice = new Random();

            //////////////////////////////////////////////////////////////////////////
            // initialize constants

            int start_money = 1500;
            int go_value = 200;
            int num_players = 2;

            //////////////////////////////////////////////////////////////////////////
            // parse property file into memory

            string path = @"M:\monopoly\beginner_board.csv";
            var parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(path);
            parser.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
            parser.SetDelimiters(new string[] { ";" });          
            parser.ReadFields(); // skip first (header) row of CSV
            while (!parser.EndOfData)
            {
                string[] row = parser.ReadFields()[0].Split(',');
                properties.Add(new Property(row));
            }

            //foreach (Property p in properties)
            //{
            //    Console.WriteLine(p + "\n");
            //}

            //////////////////////////////////////////////////////////////////////////
            // configure game settings

            Console.WriteLine("Welcome to Hylandopoly!");
            start_money = input_int("\nPlease enter the desired starting amount of H-bucks for each player.", 100, Int32.MaxValue );            
            Console.WriteLine("Each player will start with ${0}.", start_money);

            num_players = input_int("\nPlease enter the number of players (at least 2, and at most 8) who will be participating.", 2, 8);            
            Console.WriteLine("{0} players will be participating.", num_players);

            //////////////////////////////////////////////////////////////////////////
            // configure players
            for (int i = 0; i < num_players; i++)
            {
                Console.WriteLine("\nPlease enter player {0}'s name.", i+1);
                string name = Console.ReadLine();
                Console.WriteLine("\n{0}, please select a character from the following list", name);
                int index = 0;
                foreach (string s in available_characters)
                {
                    Console.WriteLine("{0}: {1}", index, s);
                    index++;
                }
                int choice = input_int("Enter the number corresponding to the desired character.", 0, available_characters.Count - 1);
                string character = available_characters[choice];
                available_characters.RemoveAt(choice);

                players.Add(new Player(name, character, start_money));
                Console.WriteLine("Welcome, {0} the {1}!", name, character);
            }

            //////////////////////////////////////////////////////////////////////////
            // determine who begins game

            Console.WriteLine("\nLet's get started!");

            foreach (Player p in players)
            {
                Console.WriteLine("\n{0} the {1}, please roll the dice to determine your starting position. Press enter to continue.", p.get_name(), p.get_char());
                Console.ReadLine();
                int roll = roll_dice(dice);
                p.set_start_roll(roll);
                Console.WriteLine("{0}, you rolled a {1}", p.get_name(), roll);
            }

            players.Sort((x, y) => y.get_start_roll().CompareTo(x.get_start_roll())); // sort in descending order of start roll

            Console.WriteLine("\nThe starting order is as follows:");
            foreach (Player p in players)
            {
                Console.WriteLine(p.get_name());
            }

            //////////////////////////////////////////////////////////////////////////
            // run game



        }

        private static int roll_dice(Random dice)
        {
            return dice.Next(1, 7) + dice.Next(1, 7);
        }

        private static int input_int(string message, int min, int max)
        {
            Console.WriteLine(message);
            int num;
            while (true)
            {
                if (Int32.TryParse(Console.ReadLine(), out num) && num >= min && num <= max)
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Please enter an integral value between {0} and {1}", min, max);
                }
            }
            return num;            
        }

    }
}
