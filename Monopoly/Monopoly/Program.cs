using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Hey there! Testing a pull request.

namespace Monopoly
{
    class Program
    {
        //////////////////////////////////////////////////////////////////////////
        
        // configure string command constants
        const string OPTION_ROLL_DICE = "Roll dice";
        const string OPTION_ROLL_DICE_JAIL = "Roll dice (doubles will escape jail)";
        const string OPTION_PAY_JAIL = "Pay to escape jail";
        const string OPTION_JAIL_CARD = "Use get out of jail free card";
        const string OPTION_MORTGAGE = "Mortgage property";
        const string OPTION_UNMORTGAGE = "Unmortgage property";
        const string OPTION_BUILD = "Build house / hotel";
        const string OPTION_SELL_HOUSE = "Sell house / hotel";
        const string OPTION_TRADE = "Trade property";
        const string OPTION_BUY = "Buy property";
        const string OPTION_END_TURN = "End turn";
        const string OPTION_VIEW_PROPERTIES = "View owned properties";
        const string OPTION_VIEW_MONOPOLIES = "View owned monopolies";
        const string OPTION_VIEW_MORTGAGES = "View mortgaged properties";
        const string OPTION_PAY_RENT = "Pay rent";
        const string OPTION_TILE_INFO = "View information about the current tile";
        const string OPTION_INCOME_TAX_FIXED = "Pay fixed income tax sum";
        const string OPTION_INCOME_TAX_PCT = "Pay 10% income tax on total net worth";
        const string OPTION_LUXURY_TAX = "Pay fixed luxury tax";
        const string OPTION_FREE_PARKING = "Collect free parking";
        const string OPTION_PERSONAL_DATA = "View personal data";
        const string OPTION_CHANCE = "Draw a chance card";
        const string OPTION_CHEST = "Draw a community chest card";
        const string OPTION_BOARD_INFO = "View global board information";
        const string OPTION_GRAPHICS = "View graphical board";

        const string stars = "*********************************************************";

        //////////////////////////////////////////////////////////////////////////
        // configure container variables

        static List<Player> players = new List<Player>();
        static List<Property> board = new List<Property>();
        static List<Card> chance = new List<Card>();
        static List<Card> chest = new List<Card>();
        static List<int[]> spaces_player = new List<int[]>();
        static List<int[]> spaces_houses = new List<int[]>();
        static Random dice = new Random();
        static string[] board_art;
        static List<string> available_characters = new List<string>() { "Battle Ship", "Top Hat", "Shoe", "Dog", "Wheelbarrow", "Race Car", "Iron", "Thimble" };

        //////////////////////////////////////////////////////////////////////////
        // initialize global game-level constants

        static int start_money = 1500;
        static int go_value = 200;
        static int go_bonus = 200;
        static int num_players = 2;
        static int free_parking_default = 50;
        const int LUXURY_TAX = 75;
        const int INCOME_TAX = 200;
        const int INCOME_TAX_PCT = 10;
        const int JAIL_FEE = 50;
        static int free_parking = free_parking_default;
        static int bank_houses = 32;
        static int bank_hotels = 12;

        //////////////////////////////////////////////////////////////////////////
               
        static void Main(string[] args)
        {

            //////////////////////////////////////////////////////////////////////////
            // parse board csv file into memory

            string path = @"M:\monopoly\board.csv";
            var parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(path);
            parser.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
            parser.SetDelimiters(new string[] { ";" });
            parser.ReadFields(); // skip first (header) row of CSV
            while (!parser.EndOfData)
            {
                string[] row = parser.ReadFields()[0].Split('|');
                board.Add(new Property(row));
            }
                        
            //////////////////////////////////////////////////////////////////////////
            // parse card csv file into memory

            path = @"M:\monopoly\cards.csv";
            parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(path);
            parser.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
            parser.SetDelimiters(new string[] { ";" });
            parser.ReadFields(); // skip first (header) row of CSV
            while (!parser.EndOfData)
            {
                string[] row = parser.ReadFields()[0].Split('|');
                if (row[2].Equals("Chance"))
                {
                    chance.Add(new Card(row));
                }
                else if (row[2].Equals("Community Chest"))
                {
                    chest.Add(new Card(row));
                }
            }
            shuffle_cards(ref chance);
            shuffle_cards(ref chest);

            //////////////////////////////////////////////////////////////////////////
            // parse board art into memory

            path = @"M:\monopoly\board_art.txt";
            board_art = File.ReadAllLines(path);

            //////////////////////////////////////////////////////////////////////////
            // parse board art player placement index csv file into memory

            path = @"M:\monopoly\spaces_player.csv";
            parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(path);
            parser.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
            parser.SetDelimiters(new string[] { ";" });
            parser.ReadFields(); // skip first (header) row of CSV
            while (!parser.EndOfData)
            {
                string[] row = parser.ReadFields()[0].Split('|');
                int[] int_row = Array.ConvertAll(row, int.Parse);
                spaces_player.Add(int_row);
            }
            
            /////////////////////////////////////////////////////////////////////////
            // parse board art house placement index csv file into memory

            path = @"M:\monopoly\spaces_houses.csv";
            parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(path);
            parser.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
            parser.SetDelimiters(new string[] { ";" });
            parser.ReadFields(); // skip first (header) row of CSV
            while (!parser.EndOfData)
            {
                string[] row = parser.ReadFields()[0].Split('|');
                int[] int_row = Array.ConvertAll(row, int.Parse);
                spaces_houses.Add(int_row);
            }

            //////////////////////////////////////////////////////////////////////////
            // configure game settings

            Console.WriteLine("Welcome to Monopoly!");
            start_money = input_int("\nPlease enter the desired starting amount of money for each player.", 100, Int32.MaxValue );            
            Console.WriteLine("Each player will start with ${0}.", start_money);

            go_value = input_int("\nPlease enter the desired amount of money earned upon passing go.", 0, Int32.MaxValue);
            Console.WriteLine("Each player will earn ${0} upon passing go.", go_value);

            go_bonus = input_int("\nPlease enter the desired amount of bonus money earned for landing on go.", 0, Int32.MaxValue);
            Console.WriteLine("Each player will earn a bonus of ${0} for landing on go.", go_bonus);

            free_parking_default = input_int("\nPlease enter the desired initial value of free parking.", 0, Int32.MaxValue);
            reset_free_parking();
            Console.WriteLine("The free parking pot will be initialized to ${0} and reset to ${0} after each collection.", free_parking_default);

            num_players = input_int("\nPlease enter the number of players (at least 2, and at most 8) who will be participating.", 2, 8);            
            Console.WriteLine("{0} players will be participating.", num_players);

            //////////////////////////////////////////////////////////////////////////
            // configure players
            for (int i = 0; i < num_players; i++)
            {
                string name = input_string(string.Format("\nPlease enter player {0}'s name.", i + 1), 1, 25);
                Console.WriteLine("{0}, please select a character from the following list.", name);
                int index = 0;
                foreach (string s in available_characters)
                {
                    Console.WriteLine("{0}: {1}", index, s);
                    index++;
                }
                int choice = input_int("Enter the number corresponding to the desired character.", 0, available_characters.Count - 1);
                string character = available_characters[choice];
                available_characters.RemoveAt(choice);

                players.Add(new Player(name, character, start_money, go_value, go_bonus));
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
                Console.WriteLine("{0}, you rolled a {1}.", p.get_name(), roll);
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
                    p.refresh_properties();
                    Console.WriteLine("\n{0}\n\nIt is {1} the {2}'s turn.", 
                        stars, p.get_name(), p.get_char());
                    p.reset_double_count();
                    int dice_roll = 0; //for use in determining utility payments
                    bool turn_is_over = false; //indicates whether player may roll or not
                    bool turn_ended = false; //indicates whether player has elected to end turn (after purchases, etc.)
                    bool has_rolled = false; //indicates whether player has rolled at least once and is thus eligible to buy
                    bool rent_paid = false; //indicates whether player has paid rent (where applicable - see take_action())
                    while (!turn_ended)
                    {
                        //check before every action that player is still valid
                        if (p.get_money() <= 0)
                        {
                            Console.WriteLine("\nYou've gone bankrupt! You're eliminated!");
                            eliminated_players.Add(p);
                            p.kill();
                            turn_ended = true;
                            break;
                        }

                        //run turn
                        if (board[p.get_position()].get_name().Equals("Jail"))
                        {
                            if (p.jailed() == 0)
                            {
                                Console.WriteLine("\n***\n\nYou are currently at {0} (just visiting) [index {1}] and have ${2}.", board[p.get_position()].get_name(), p.get_position(), p.get_money());
                            }
                            else
                            {
                                Console.WriteLine("\n***\n\nYou are currently in {0} [index {1}] and have ${2}.", board[p.get_position()].get_name(), p.get_position(), p.get_money());
                            }
                        }
                        else
                        {
                            Console.WriteLine("\n***\n\nYou are currently on {0} [index {1}] and have ${2}.", board[p.get_position()].get_name(), p.get_position(), p.get_money());
                        }
                        List<string> options = generate_options(p, turn_is_over, has_rolled, rent_paid);
                        Console.WriteLine("You may:\n");
                        for (int i = 0; i < options.Count; i++)
                        {
                            Console.WriteLine("{0}: {1}", i, options[i]);
                        }
                        int choice = input_int("\nEnter the number corresponding to the desired action.", 0, options.Count - 1);
                        take_action(p, options, choice, ref turn_is_over, ref has_rolled, ref turn_ended, ref rent_paid, ref dice_roll);             
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
            Console.WriteLine("\n\n{0}\nGAME OVER!\n{1} the {2} wins!", stars, players[0].get_name(), players[0].get_char());
            Console.ReadLine();
        }

        private static List<string> generate_options(Player p, bool turn_is_over, bool has_rolled, bool rent_paid)
        {
            List<string> options = new List<string>();

            options.Add(OPTION_VIEW_PROPERTIES);
            options.Add(OPTION_VIEW_MONOPOLIES);
            options.Add(OPTION_VIEW_MORTGAGES);
            options.Add(OPTION_TILE_INFO);
            options.Add(OPTION_BOARD_INFO);
            options.Add(OPTION_GRAPHICS);
            options.Add(OPTION_PERSONAL_DATA);
            //////////////////////////////////////////////////////////////////////////
            if (p.jailed() == 0) //player has full freedom
            {
                if (!turn_is_over)
                {
                    options.Add(OPTION_ROLL_DICE);
                }
                //////////////////////////////////////////////////////////////////////////
                if (has_rolled  &&
                    !board[p.get_position()].owned() &&
                    !board[p.get_position()].mortgaged() &&
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
                if (p.get_mortgaged_properties().Count > 0)
                {
                    options.Add(OPTION_UNMORTGAGE);
                }
                //////////////////////////////////////////////////////////////////////////
                if (p.get_monopolies().Count > 0 && bank_houses > 0 && bank_hotels > 0)
                {
                    options.Add(OPTION_BUILD);
                    if (p.get_house_properties().Count > 0)
                    {
                        options.Add(OPTION_SELL_HOUSE);
                    }
                }
            }
            else if (p.jailed() >= 1 && p.jailed() <= 3) //player can pay or try to roll or use card to get out of jail
            {
                if (!turn_is_over)
                {
                    options.Add(OPTION_ROLL_DICE_JAIL);
                    options.Add(OPTION_PAY_JAIL);
                    if (p.has_jail_free_card())
                    {
                        options.Add(OPTION_JAIL_CARD);
                    }
                }                
            }
            else //player must pay to get out of jail or use card
            {
                if (!turn_is_over)
                {
                    options.Add(OPTION_PAY_JAIL);
                }
                if (p.has_jail_free_card())
                {
                    options.Add(OPTION_JAIL_CARD);
                }
            }
            //////////////////////////////////////////////////////////////////////////            
            if (turn_is_over)
            {
                options.Add(OPTION_END_TURN);
            }
            //////////////////////////////////////////////////////////////////////////
            if (board[p.get_position()].owned() && !board[p.get_position()].mortgaged() && board[p.get_position()].get_owner() != p &&
                rent_paid == false && has_rolled == true)
            {
                //clear all options and force rent payment
                options.Clear();
                Console.WriteLine("This property is owned by {0}!", board[p.get_position()].get_owner().get_nickname());
                options.Add(OPTION_PAY_RENT);
            }
            //////////////////////////////////////////////////////////////////////////
            if (board[p.get_position()].get_name().Equals("Income Tax") && rent_paid == false && has_rolled == true)
            {
                //clear all options and force income tax payment
                options.Clear();
                Console.WriteLine("You must pay income tax!");
                options.Add(OPTION_INCOME_TAX_FIXED);
                options.Add(OPTION_INCOME_TAX_PCT);
            }
            //////////////////////////////////////////////////////////////////////////
            if (board[p.get_position()].get_name().Equals("Luxury Tax") && rent_paid == false && has_rolled == true)
            {
                //clear all options and force luxury tax payment
                options.Clear();
                Console.WriteLine("You must pay luxury tax!");
                options.Add(OPTION_LUXURY_TAX);
            }
            //////////////////////////////////////////////////////////////////////////
            if (board[p.get_position()].get_name().Equals("Free Parking") && rent_paid == false && has_rolled == true)
            {
                //clear all options and force free parking collection
                options.Clear();
                Console.WriteLine("Congratulations! You've landed on free parking!");
                options.Add(OPTION_FREE_PARKING);
            }
            //////////////////////////////////////////////////////////////////////////
            if (board[p.get_position()].get_name().Equals("Chance") && rent_paid == false && has_rolled == true)
            {
                //clear all options and force chance draw
                options.Clear();
                options.Add(OPTION_CHANCE);
            }
            //////////////////////////////////////////////////////////////////////////
            if (board[p.get_position()].get_name().Equals("Community Chest") && rent_paid == false && has_rolled == true)
            {
                //clear all options and force community chest draw
                options.Clear();
                options.Add(OPTION_CHEST);
            }
            //////////////////////////////////////////////////////////////////////////

            return options;
        }

        private static void take_action(Player p, List<string> options, int choice, ref bool turn_is_over, 
            ref bool has_rolled, ref bool turn_ended, ref bool rent_paid, ref int dice_roll)
        {
            string action = options[choice];


            if (action.Equals(OPTION_ROLL_DICE))
            {
                bool doubles = false;
                dice_roll = roll_dice(dice, ref doubles);
                if (doubles && p.get_double_count() < 2)
                {
                    Console.WriteLine("You rolled a {0} on doubles!", dice_roll);
                    turn_is_over = false;
                    rent_paid = false;
                    p.increment_double_count();
                    has_rolled = true;
                    p.advance(dice_roll);
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
                    Console.WriteLine("You rolled a {0}.", dice_roll);
                    turn_is_over = true;
                    rent_paid = false;
                    has_rolled = true;
                    p.advance(dice_roll);
                }
                

                if (board[p.get_position()].get_name().Equals("Go To Jail"))
                {
                    Console.WriteLine("\nOof - you landed on go to jail!");
                    has_rolled = false;
                    turn_is_over = true;
                    p.go_to_jail();
                }
            }
            //////////////////////////////////////////////////////////////////////////
            if (action.Equals(OPTION_PAY_JAIL))
            {
                p.pay_for_jail(JAIL_FEE);
                add_free_parking(JAIL_FEE);
                Console.WriteLine("You've successfully paid ${0} in bond to escape jail. You now have ${1}. The free parking pot now has ${2}.",
                    JAIL_FEE, p.get_money(), free_parking);
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
                    Console.WriteLine("Congratulations! You rolled doubles, and are free from jail!");
                    turn_is_over = true;
                }
                else
                {
                    p.increment_jail();
                    Console.WriteLine("Darn! You didn't roll doubles, and are still in jail. You have {0} more chances " +
                        "to roll doubles before you'll have to pay bond.", 3 - p.jailed() + 1);
                    turn_is_over = true; 
                }
            }
            //////////////////////////////////////////////////////////////////////////
            if (action.Equals(OPTION_JAIL_CARD))
            {
                Console.WriteLine("Congratulations - you've successfully used your get out of jail free card to escape jail.");
                Card jail_free = p.use_jail_free_card();
                if (jail_free.get_type().Equals("Chance"))
                {
                    chance.Add(jail_free);
                }
                else
                {
                    chest.Add(jail_free);
                }
            }
            //////////////////////////////////////////////////////////////////////////
            if (action.Equals(OPTION_PAY_RENT))
            {
                rent_paid = true;
                int rent = board[p.get_position()].get_rent(dice_roll);
                if (p.get_money() > rent)
                {
                    Player owner = board[p.get_position()].get_owner();
                    p.pay_rent(owner, rent);
                    Console.WriteLine("Thank you for paying ${0} in rent to {1}.", rent, owner.get_nickname());
                    Console.WriteLine("{0} now has ${1}.", owner.get_nickname(), owner.get_money());
                    Console.WriteLine("{0} now has ${1}.", p.get_nickname(), p.get_money());
                }
                else
                {
                    Player owner = board[p.get_position()].get_owner();
                    Console.WriteLine("You cannot afford rent!");
                    p.pay_rent(owner, rent);
                }
            }
            //////////////////////////////////////////////////////////////////////////
            if (action.Equals(OPTION_BUY))
            {
                p.buy(board[p.get_position()]);
            }
            //////////////////////////////////////////////////////////////////////////
            if (action.Equals(OPTION_INCOME_TAX_FIXED))
            {
                rent_paid = true;
                p.pay(INCOME_TAX);
                add_free_parking(INCOME_TAX);
                Console.WriteLine("Thank you for paying ${0} in fixed income tax. You now have ${1}. The free parking pot now has ${2}.",
                    INCOME_TAX, p.get_money(), free_parking);
            }
            //////////////////////////////////////////////////////////////////////////
            if (action.Equals(OPTION_INCOME_TAX_PCT))
            {
                rent_paid = true;
                int net_worth = p.get_net_worth();
                int tax = Convert.ToInt32((double)net_worth * ((double)INCOME_TAX_PCT / 100.0));
                p.pay(tax);
                add_free_parking(tax);
                Console.WriteLine("Thank you for paying ${0} as {1}% income tax on your net worth of ${2}. You now have ${3}. " +
                    "The free parking pot now has ${4}.", 
                    tax, INCOME_TAX_PCT, net_worth, p.get_money(), free_parking);
            }
            //////////////////////////////////////////////////////////////////////////
            if (action.Equals(OPTION_LUXURY_TAX))
            {
                rent_paid = true;
                p.pay(LUXURY_TAX);
                add_free_parking(LUXURY_TAX);
                Console.WriteLine("Thank you for paying ${0} in fixed luxury tax. You now have ${1}. The free parking pot now has ${2}.",
                    LUXURY_TAX, p.get_money(), free_parking);
            }
            //////////////////////////////////////////////////////////////////////////
            if (action.Equals(OPTION_FREE_PARKING))
            {
                rent_paid = true;               
                p.receive_payment(free_parking);
                Console.WriteLine("You've collected ${0} in free parking. You now have ${1}. The free parking pot now has ${2}.",
                    free_parking, p.get_money(), free_parking_default);
                reset_free_parking();
            }
            //////////////////////////////////////////////////////////////////////////
            if (action.Equals(OPTION_CHANCE))
            {
                rent_paid = true;
                bool has_moved = false;
                // do chance draw from front of queue then replace at back of queue
                Console.WriteLine("You've drawn the following Chance card:");
                Card draw = chance[0];
                Console.WriteLine(draw);
                chance.Remove(draw);
                if (!draw.get_category().Equals("item"))
                {
                    chance.Add(draw); // add back to end of queue if NOT get out of jail free card; else, player keeps get out of jail free card
                }
                int parking_addition = p.take_card_action(draw, ref players, ref has_moved);
                add_free_parking(parking_addition);
                if (has_moved)
                {
                    has_rolled = true;
                    rent_paid = false;
                }
                Console.WriteLine("Chance action successfully taken. The new status of each player is displayed below.\n");
                foreach (Player other in players)
                {
                    Console.WriteLine(other + "\n");
                }
                Console.WriteLine("The free parking pot now has ${0}.", free_parking);
            }
            //////////////////////////////////////////////////////////////////////////
            if (action.Equals(OPTION_CHEST))
            {
                rent_paid = true;
                bool has_moved = false;
                // do chest draw from front of queue then replace at back of queue
                Console.WriteLine("You've drawn the following Community Chest card:");
                Card draw = chest[0];
                Console.WriteLine(draw);
                chest.Remove(draw);
                if (!draw.get_category().Equals("item"))
                {
                    chest.Add(draw); // add back to end of queue if NOT get out of jail free card; else, player keeps get out of jail free card
                }
                int parking_addition = p.take_card_action(draw, ref players, ref has_moved);
                add_free_parking(parking_addition);
                if (has_moved)
                {
                    has_rolled = true;
                    rent_paid = false;
                }
                Console.WriteLine("Community Chest action successfully taken. The new status of each player is displayed below.\n");
                foreach (Player other in players)
                {
                    Console.WriteLine(other + "\n");
                }
                Console.WriteLine("The free parking pot now has ${0}.", free_parking);
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
            if (action.Equals(OPTION_PERSONAL_DATA))
            {
                Console.WriteLine("Personal Data:\n");
                Console.WriteLine(p.ToString());
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
            if (action.Equals(OPTION_VIEW_MORTGAGES))
            {
                Console.WriteLine("Mortgaged properties:\n");
                foreach (Property property in p.get_mortgaged_properties())
                {
                    Console.WriteLine(property + "\n");
                }
                if (p.get_mortgaged_properties().Count == 0)
                {
                    Console.WriteLine("You do not own any mortgaged properties.");
                }
            }
            //////////////////////////////////////////////////////////////////////////
            if (action.Equals(OPTION_TILE_INFO))
            {
                Console.WriteLine("Current Tile:\n\n" + board[p.get_position()]);
            }
            //////////////////////////////////////////////////////////////////////////
            if (action.Equals(OPTION_BOARD_INFO))
            {
                string available_properties = "";
                string mortgaged_properties = "";
                string owned_properties = "";
                string active_players = "";
                string detailed_players = "";
                foreach (Property property in board)
                {
                    if (property.get_type().Equals("Street") || property.get_type().Equals("Railroad") || property.get_type().Equals("Utility"))
                    {
                        if (!property.owned())
                        {
                            available_properties += property.get_name() + ", ";
                        }
                        else if (property.mortgaged())
                        {
                            mortgaged_properties += property.get_name() + ", ";
                        }
                        else
                        {
                            owned_properties += property.get_name() + ", ";
                        }
                    }
                }
                foreach (Player other in players)
                {
                    active_players += other.get_nickname() + ", ";
                    detailed_players += other.ToString() + "\n\n";
                }
                if (available_properties.Length > 0)
                {
                    available_properties = available_properties.Substring(0, available_properties.Length - 2);
                }
                if (mortgaged_properties.Length > 0)
                {
                    mortgaged_properties = mortgaged_properties.Substring(0, mortgaged_properties.Length - 2);
                }
                if (owned_properties.Length > 0)
                {
                    owned_properties = owned_properties.Substring(0, owned_properties.Length - 2);
                }
                if (active_players.Length > 0)
                {
                    active_players = active_players.Substring(0, active_players.Length - 2);
                }
                if (detailed_players.Length > 0)
                {
                    detailed_players = detailed_players.Substring(0, detailed_players.Length - 2);
                }
                Console.WriteLine("Global Board Information:\n");
                Console.WriteLine(@"Free Parking: ${0}
Free Parking Default: ${1}
Passing Go Value: ${2}
Landing on Go Bonus: ${3}
Number of Houses in Bank: {4}
Number of Hotels in Bank: {5}

Owned Properties: {6}

Mortgaged Properties: {7}

Available Properties: {8}

Active Players: {9}

Detailed Player Information:

{10}", free_parking, free_parking_default, go_value, go_bonus, bank_houses, bank_hotels, owned_properties, 
mortgaged_properties, available_properties, active_players, detailed_players);
            }
            //////////////////////////////////////////////////////////////////////////
            if (action.Equals(OPTION_GRAPHICS))
            {
                string[] rendered = Graphics.render(players, board, spaces_player, spaces_houses, board_art, free_parking);
                foreach (String row in rendered)
                {
                    Console.WriteLine(row);
                }
            }
            //////////////////////////////////////////////////////////////////////////
            if (action.Equals(OPTION_TRADE))
            {
                rent_paid = true; 
                //select trading partner
                Console.WriteLine("With whom would you like to trade?:\n");
                List<Player> others = new List<Player>();
                foreach (Player other in players) //compile list of potential trading partners
                {
                    if (!p.Equals(other))
                    {
                        others.Add(other);
                    }
                }
                for (int i = 0; i < others.Count; i++)
                {
                    Console.WriteLine("{0}: {1}", i, others[i].get_nickname());
                }
                int input = input_int("\nEnter the number corresponding to the desired player.", 0, others.Count - 1);
                Player trader = others[input];

                //select property to receive
                Console.WriteLine("Below are the properties owned by {0} available for trading:\n", trader.get_nickname());
                List<Property> trader_properties = trader.get_tradable_properties();
                
                foreach (Property property in trader_properties)
                {
                    Console.WriteLine(property + "\n");
                }
                Console.WriteLine("Select the property you would like to receive from {0} from the list below.", trader.get_nickname());
                for (int i = 0; i < trader_properties.Count; i++)
                {
                    Console.WriteLine("{0}: {1}", i, trader_properties[i].get_name());
                }
                Console.WriteLine("{0}: None", trader_properties.Count);
                trader_properties.Add(new Property());
                input = input_int("\nEnter the number corresponding to the desired property.", 0, trader_properties.Count - 1);
                Property property_to_receive = trader_properties[input];

                //select property to give
                Console.WriteLine("Below are your properties available for trading:\n");
                List<Property> my_properties = p.get_tradable_properties();
                foreach (Property property in my_properties)
                {
                    Console.WriteLine(property + "\n");
                }
                Console.WriteLine("Select the property you would like to give up from the list below.");
                for (int i = 0; i < my_properties.Count; i++)
                {
                    Console.WriteLine("{0}: {1}", i, my_properties[i].get_name());
                }
                Console.WriteLine("{0}: None", my_properties.Count);
                my_properties.Add(new Property());

                input = input_int("\nEnter the number corresponding to the desired property.", 0, my_properties.Count - 1);
                Property property_to_give = my_properties[input];

                //determine trade amount
                Console.WriteLine("Who will be paying whom for this trade?\n");
                Console.WriteLine("0: I'm paying {0}", trader.get_nickname());
                Console.WriteLine("1: {0} is paying me", trader.get_nickname());
                Console.WriteLine("2: No money is being exchanged in this trade");
                input = input_int("\nEnter the number corresponding to your choice", 0, 2);

                //make trade
                if (input == 0)
                {
                    //p pays trader
                    int amount = input_int(String.Format("Enter the emount of money you are paying {0} for the trade.", trader.get_nickname()), 0, int.MaxValue);
                    p.pay(amount);
                    trader.receive_payment(amount);
                    p.send_property(trader, property_to_give);
                    trader.send_property(p, property_to_receive);
                    Console.WriteLine("Trade successful. Each player's updated status is displayed below:\n");
                    Console.WriteLine(p + "\n");
                    Console.WriteLine(trader);
                }
                else if (input == 1)
                {
                    //trader pays p
                    int amount = input_int(String.Format("Enter the emount of money {0} is paying for the trade.", trader.get_nickname()), 0, int.MaxValue);
                    trader.pay(amount);
                    p.receive_payment(amount);
                    p.send_property(trader, property_to_give);
                    trader.send_property(p, property_to_receive);
                    Console.WriteLine("Trade successful. Each player's updated status is displayed below:\n");
                    Console.WriteLine(p + "\n");
                    Console.WriteLine(trader);
                }
                else
                {
                    //no money is moved; just swap owners
                    p.send_property(trader, property_to_give);
                    trader.send_property(p, property_to_receive);
                    Console.WriteLine("Trade successful. Each player's updated status is displayed below:\n");
                    Console.WriteLine(p + "\n");
                    Console.WriteLine(trader);
                }
            }
            //////////////////////////////////////////////////////////////////////////
            if (action.Equals(OPTION_MORTGAGE))
            {
                List<Property> properties = p.get_tradable_properties(); //only get properties without houses on them
                Console.WriteLine("You may mortgage one of the following properties:\n");
                foreach (Property property in properties)
                {
                    Console.WriteLine(property + "\n");
                }
                Console.WriteLine("\nSelect a property from the list below to mortgage: (note: properties with houses / hotels may not be mortgaged; houses / hotels must first be sold to the bank.)");
                for (int i = 0; i < properties.Count; i++)
                {
                    Console.WriteLine("{0}: {1}", i, properties[i].get_name());
                }
                Console.WriteLine("{0}: Cancel", properties.Count);
                int number = input_int("\nEnter the number corresponding to the desired property.", 0, properties.Count);
                if (number < properties.Count)
                {
                    p.mortgage_property(properties[number]);
                    Console.WriteLine("{0} is now mortgaged.", properties[number].get_name());
                }
                else
                {
                    Console.WriteLine("Mortgaging cancelled.");
                }
            }
            //////////////////////////////////////////////////////////////////////////
            if (action.Equals(OPTION_UNMORTGAGE))
            {
                List<Property> mortgaged = p.get_mortgaged_properties();
                Console.WriteLine("You may unmortgage one of the following properties:\n");
                foreach (Property property in mortgaged)
                {
                    Console.WriteLine(property + "\n");
                }
                Console.WriteLine("\nSelect a property from the list below to unmortgage:");
                for (int i = 0; i < mortgaged.Count; i++)
                {
                    Console.WriteLine("{0}: {1}", i, mortgaged[i].get_name());
                }
                Console.WriteLine("{0}: Cancel", mortgaged.Count);
                int number = input_int("\nEnter the number corresponding to the desired property.", 0, mortgaged.Count);
                if (number < mortgaged.Count)
                {
                    Property property = mortgaged[number];
                    p.unmortgage_property(property);
                    Console.WriteLine("{0} is now unmortgaged.", property.get_name());
                }
                else
                {
                    Console.WriteLine("Unmortgaging cancelled.");
                }
            }
            //////////////////////////////////////////////////////////////////////////
            if (action.Equals(OPTION_BUILD))
            {
                List<Property> monopolies = p.get_monopolies();
                List<Property> buildable = new List<Property>();
                Console.WriteLine("You may build on one of the following properties:\n");
                foreach (Property property in monopolies)
                {
                    if (property.get_houses() < 5 && property.is_buildable(monopolies))
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
                int number = input_int("\nEnter the number corresponding to the desired property.", 0, buildable.Count);
                if (number < buildable.Count)
                {
                    Property property = buildable[number];
                    property.build();
                    if (property.get_houses() < 5)
                    {
                        // houses
                        Console.WriteLine("Congratulations! {0} now has {1} houses on it. You now have ${2}.", property.get_name(), property.get_houses(), p.get_money());
                        bank_houses--;
                    }
                    else
                    {
                        // hotel
                        Console.WriteLine("Congratulations! {0} now has a hotel on it! You may not build any further on this property. You now have ${1}.", property.get_name(), p.get_money());
                        bank_houses += 4;
                        bank_hotels--;
                    }
                }
                else
                {
                    Console.WriteLine("House building cancelled.");
                }
            }
            //////////////////////////////////////////////////////////////////////////
            if (action.Equals(OPTION_SELL_HOUSE))
            {
                List<Property> sellable = p.get_house_properties();

                Console.WriteLine("You may sell a house / hotel from one of the following properties:\n");
                foreach (Property property in sellable)
                {
                    Console.WriteLine(property + "\n");                   
                }
                Console.WriteLine("\nSelect a property from the list below to sell from:");
                for (int i = 0; i < sellable.Count; i++)
                {
                    Console.WriteLine("{0}: {1}", i, sellable[i].get_name());
                }
                Console.WriteLine("{0}: Cancel", sellable.Count);
                int number = input_int("\nEnter the number corresponding to the desired action.", 0, sellable.Count);
                if (number < sellable.Count)
                {
                    Property property = sellable[number];
                    if (property.get_houses() == 5)
                    {
                        bank_hotels++;
                        bank_houses -= 4;
                    }
                    else
                    {
                        bank_houses++;
                    }
                    property.sell_house();                    
                    Console.WriteLine("{0} now has {1} houses on it. You now have ${2}.", property.get_name(), property.get_houses(), p.get_money());
                }
                else
                {
                    Console.WriteLine("House / hotel selling cancelled.");
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
            int roll_one = input_int("Debug Mode: enter first dice number", int.MinValue, int.MaxValue);
            int roll_two = input_int("Debug Mode: enter second dice number", int.MinValue, int.MaxValue);
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

        private static void reset_free_parking()
        {
            free_parking = free_parking_default;
        }

        private static void add_free_parking(int payment)
        {
            free_parking += payment;
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
                    Console.WriteLine("Please enter an integral value between {0} and {1}.", min, max);
                }
            }
            return num;            
        }

        private static string input_string(string message, int min_len, int max_len)
        {
            Console.WriteLine(message);
            string input;
            while (true)
            {
                input = Console.ReadLine();
                if (input.Length >= min_len && input.Length <= max_len)
                {
                    Console.WriteLine("\n***\n");
                    break;
                }
                else
                {
                    Console.WriteLine("Please enter a string of length between {0} and {1}.", min_len, max_len);
                }
            }
            return input;            
        }
        
        private static void shuffle_cards(ref List<Card> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = dice.Next(n + 1);
                Card value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
