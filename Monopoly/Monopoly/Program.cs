using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{
    class Program
    {
        //////////////////////////////////////////////////////////////////////////
        
        // configure string command constants
        const string OPTION_ROLL_DICE = "Roll dice";
        const string OPTION_ROLL_DICE_JAIL = "Roll dice (doubles will escape jail)";
        const string OPTION_PAY_JAIL = "Pay $50 to escape jail";
        const string OPTION_MORTGAGE = "Mortgage property";
        const string OPTION_BUILD = "Build house / hotel";
        const string OPTION_TRADE = "Trade property";
        const string OPTION_BUY = "Buy property";
        const string OPTION_END_TURN = "End turn";
        const string OPTION_VIEW_PROPERTIES = "View owned properties";
        const string OPTION_VIEW_MONOPOLIES = "View owned monopolies";
        const string OPTION_PAY_RENT = "Pay rent";
        const string OPTION_TILE_INFO = "View information about the current tile";
        const string stars = "*********************************************************";

        //////////////////////////////////////////////////////////////////////////
        // configure container variables

        static List<Player> players = new List<Player>();
        static List<Property> board = new List<Property>();
        static Random dice = new Random();
        static List<string> available_characters = new List<string>() { "Battle Ship", "Top Hat", "Shoe", "Dog", "Wheelbarrow", "Race Car", "Iron", "Thimble" };

        //////////////////////////////////////////////////////////////////////////
        // initialize global game-level constants

        static int start_money = 1500;
        static int go_value = 200;
        static int num_players = 2;

        //////////////////////////////////////////////////////////////////////////



        static void Main(string[] args)
        {

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
                board.Add(new Property(row));
            }

            //foreach (Property p in board)
            //{
            //    Console.WriteLine(p + "\n");
            //}

            //////////////////////////////////////////////////////////////////////////
            // configure game settings

            Console.WriteLine("Welcome to Hylandopoly!");
            start_money = input_int("\nPlease enter the desired starting amount of H-bucks for each player.", 100, Int32.MaxValue );            
            Console.WriteLine("Each player will start with ${0}.", start_money);

            go_value = input_int("\nPlease enter the desired amount of money earned upon passing go.", 0, Int32.MaxValue);
            Console.WriteLine("Each player will earn ${0} upon passing go.", go_value);

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

                players.Add(new Player(name, character, start_money, go_value));
                Console.WriteLine("Welcome, {0} the {1}!", name, character);
            }

            //////////////////////////////////////////////////////////////////////////
            // determine who begins game

            Console.WriteLine("\nLet's get started!");

            foreach (Player p in players)
            {
                Console.WriteLine("\n{0}, please roll the dice to determine your starting position. Press enter to continue.", p.get_nickname());
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

            while (players.Count > 1)
            {
                List<Player> eliminated_players = new List<Player>();
                foreach (Player p in players)
                {
                    Console.WriteLine("\n{0}\n\nIt is {1} the {2}'s turn.", 
                        stars, p.get_name(), p.get_char());
                    p.reset_double_count();
                    bool turn_is_over = false; //indicates whether player may roll or not
                    bool turn_ended = false; //indicates whether player has elected to end turn (after purchases, etc.)
                    bool has_rolled = false; //indicates whether player has rolled at least once and is thus eligible to buy
                    bool rent_paid = false; //indicates whether player has paid rent (where applicable - see take_action())
                    while (!turn_ended)
                    {
                        Console.WriteLine("\n***\n\nYou currently are on {0} [index {1}] and have ${2}", board[p.get_position()].get_name(), p.get_position(), p.get_money());
                        List<string> options = generate_options(p, turn_is_over, has_rolled, rent_paid);
                        Console.WriteLine("You may:\n");
                        for (int i = 0; i < options.Count; i++)
                        {
                            Console.WriteLine("{0}: {1}", i, options[i]);
                        }
                        int choice = input_int("\nEnter the number corresponding to the desired action.", 0, options.Count - 1);
                        take_action(p, options, choice, ref turn_is_over, ref has_rolled, ref turn_ended, ref rent_paid);
                        

                        //check after every action that player is still valid
                        if (p.get_money() <= 0)
                        {
                            Console.WriteLine("\nYou've gone bankrupt! You're eliminated!");
                            eliminated_players.Add(p);
                            turn_ended = true;
                        }
                    }
                    if (players.Count - eliminated_players.Count <= 1)
                    {
                        //don't continue to the next player; only one remains
                        break;
                    }
                }
                foreach (Player p in eliminated_players)
                {
                    players.Remove(p);
                }
            }
            Console.WriteLine("\n{0}\nGAME OVER!\n{1} the {2} wins!", stars, players[0].get_name(), players[0].get_char());
        }

        private static List<string> generate_options(Player p, bool turn_is_over, bool has_rolled, bool rent_paid)
        {
            List<string> options = new List<string>();

            options.Add(OPTION_VIEW_PROPERTIES);
            options.Add(OPTION_VIEW_MONOPOLIES);
            options.Add(OPTION_TILE_INFO);
            //////////////////////////////////////////////////////////////////////////
            if (p.jailed() == 0) //player has full freedom
            {
                if (!turn_is_over)
                {
                    options.Add(OPTION_ROLL_DICE);
                }
                //////////////////////////////////////////////////////////////////////////
                if (has_rolled  &&
                    board[p.get_position()].get_owner() == null &&
                    (board[p.get_position()].get_type() == "Street" || 
                    board[p.get_position()].get_type() == "Railroad" || 
                    board[p.get_position()].get_type() == "Utility"))
                {
                    options.Add(OPTION_BUY);
                }
                //////////////////////////////////////////////////////////////////////////
                if (p.get_properties().Count > 0)
                {
                    options.Add(OPTION_TRADE);
                    options.Add(OPTION_MORTGAGE);
                }
                //////////////////////////////////////////////////////////////////////////
                if (p.get_monopolies().Count > 0)
                {
                    options.Add(OPTION_BUILD);
                }
            }
            else if (p.jailed() >= 1 && p.jailed() <= 3) //player can pay or try to roll to get out of jail
            {
                options.Add(OPTION_ROLL_DICE_JAIL);
                options.Add(OPTION_PAY_JAIL);
            }
            else //player must pay to get out of jail
            {
                options.Add(OPTION_PAY_JAIL);
            }
            //////////////////////////////////////////////////////////////////////////            
            if (turn_is_over)
            {
                options.Add(OPTION_END_TURN);
            }
            //////////////////////////////////////////////////////////////////////////
            if (board[p.get_position()].get_owner() != null && board[p.get_position()].get_owner() != p &&
                rent_paid == false && has_rolled == true)
            {
                //clear all options and force payment
                options.Clear();
                Console.WriteLine("This property is owned by {0}!", board[p.get_position()].get_owner().get_nickname());
                options.Add(OPTION_PAY_RENT);
            }
            //////////////////////////////////////////////////////////////////////////

            return options;
        }

        private static void take_action(Player p, List<string> options, int choice, ref bool turn_is_over, 
            ref bool has_rolled, ref bool turn_ended, ref bool rent_paid)
        {
            string action = options[choice];


            if (action.Equals(OPTION_ROLL_DICE))
            {
                bool doubles = false;
                int roll = roll_dice(dice, ref doubles);
                if (doubles && p.get_double_count() < 2)
                {
                    Console.WriteLine("You rolled a {0} on doubles!", roll);
                    turn_is_over = false;
                    p.increment_double_count();
                    has_rolled = true;
                    p.advance(roll);
                }
                else if (doubles && p.get_double_count() == 2)
                {
                    Console.WriteLine("Cheater! You rolled doubles three times in a row; you're going to jail!");
                    turn_is_over = true;
                    has_rolled = true;
                    p.go_to_jail();
                }
                else
                {
                    Console.WriteLine("\nYou rolled a {0}.", roll);
                    turn_is_over = true;
                    has_rolled = true;
                    p.advance(roll);
                }
                

                if (board[p.get_position()].get_name().Equals("Go To Jail"))
                {
                    Console.WriteLine("Oof - you landed on go to jail!");
                    has_rolled = false;
                    turn_is_over = true;
                    p.go_to_jail();
                }
            }
            //////////////////////////////////////////////////////////////////////////
            if (action.Equals(OPTION_PAY_JAIL))
            {
                p.pay_for_jail();
                turn_is_over = true;
            }
            //////////////////////////////////////////////////////////////////////////
            if (action.Equals(OPTION_ROLL_DICE_JAIL))
            {
                bool doubles = false;
                roll_dice(dice, ref doubles);
                if (doubles)
                {
                    p.release_from_jail();
                    turn_is_over = true;
                }
                else
                {
                    p.increment_jail();
                    turn_is_over = true; 
                }
            }
            //////////////////////////////////////////////////////////////////////////
            if (action.Equals(OPTION_PAY_RENT))
            {
                rent_paid = true;
                int rent = board[p.get_position()].get_rent();
                if (p.get_money() > rent)
                {
                    Player owner = board[p.get_position()].get_owner();
                    p.pay_rent(owner, rent);
                    Console.WriteLine("Thank you for paying $ {0} in rent to {1}", rent, owner.get_nickname());
                    Console.WriteLine("{0} now has ${1}", owner.get_nickname(), owner.get_money());
                    Console.WriteLine("{0} now has ${1}", p.get_nickname(), p.get_money());
                }
                else
                {
                    Console.WriteLine("You cannot afford rent!");                    
                }
            }
            //////////////////////////////////////////////////////////////////////////
            if (action.Equals(OPTION_BUY))
            {
                p.buy(board[p.get_position()]);
            }
            //////////////////////////////////////////////////////////////////////////
            if (action.Equals(OPTION_VIEW_PROPERTIES))
            {
                Console.WriteLine("Owned Properties:\n");
                foreach (Property property in p.get_properties())
                {
                    Console.WriteLine(property + "\n");
                }
                if (p.get_properties().Count == 0)
                {
                    Console.WriteLine("You do not own any properties.");
                }
            }
            //////////////////////////////////////////////////////////////////////////
            if (action.Equals(OPTION_VIEW_MONOPOLIES))
            {
                Console.WriteLine("Owned Monopolies:\n");
                foreach (Property property in p.get_monopolies())
                {
                    Console.WriteLine(property + "\n");
                }
                if (p.get_monopolies().Count == 0)
                {
                    Console.WriteLine("You do not own any monopolies.");
                }
            }
            //////////////////////////////////////////////////////////////////////////
            if (action.Equals(OPTION_TILE_INFO))
            {
                Console.WriteLine("Current Tile:\n" + board[p.get_position()]);
            }
            //////////////////////////////////////////////////////////////////////////
            if (action.Equals(OPTION_BUILD))
            {
                List<Property> monopolies = p.get_monopolies();
                List<Property> buildable = new List<Property>();
                Console.WriteLine("You may build on one of the following properties\n");
                foreach (Property property in monopolies)
                {
                    if (property.get_houses() < 5)
                    {
                        Console.WriteLine(property + "\n");
                        buildable.Add(property);
                    }
                }
                Console.WriteLine("\nSelect a property from the list below to build upon:");
                for (int i = 0; i < buildable.Count; i++)
                {
                    Console.WriteLine("{0}: {1}", i, buildable[i].get_name());
                }
                Console.WriteLine("{0}: Cancel", buildable.Count);
                int number = input_int("\nEnter the number corresponding to the desired action.", 0, buildable.Count);
                if (number < buildable.Count)
                {
                    Property property = buildable[number];
                    property.build();
                    if (property.get_houses() < 5)
                    {
                        // houses
                        Console.WriteLine("Congratulations! {0} now has {1} houses on it.", property.get_name(), property.get_houses());

                    }
                    else
                    {
                        // hotel
                        Console.WriteLine("Congratulations! {0} now has a hotel on it! You may not build any further on this property.", property.get_name());
                    }
                }
                else
                {
                    Console.WriteLine("House building cancelled.");
                }
            }
            //////////////////////////////////////////////////////////////////////////
            if (action.Equals(OPTION_END_TURN))
            {
                turn_ended = true;
            }
        }

        private static int roll_dice(Random dice)
        {
            return dice.Next(1, 7) + dice.Next(1, 7);
        }

        private static int roll_dice(Random dice, ref bool doubles)
        {
            //int roll_one = dice.Next(1, 7);
            //int roll_two = dice.Next(1, 7);
            int roll_one = input_int("HACKER MODE (enter first dice number)", int.MinValue, int.MaxValue);
            int roll_two = input_int("HACKER MODE (enter second dice number)", int.MinValue, int.MaxValue);
            if (roll_one == roll_two)
            {
                doubles = true;
            } 
            else
            {
                doubles = false;
            }
            return roll_one + roll_two;
        }

        private static int input_int(string message, int min, int max)
        {
            Console.WriteLine(message);
            int num;
            while (true)
            {
                if (Int32.TryParse(Console.ReadLine(), out num) && num >= min && num <= max)
                {
                    Console.WriteLine("\n***\n");
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
