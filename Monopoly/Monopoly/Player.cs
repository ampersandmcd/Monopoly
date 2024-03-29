﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{
    public class Player
    {
        private string character;
        private string name;
        private int money;
        private int go_value;
        private int go_bonus;
        private int position;
        private int start_roll;
        private int turns_jailed;
        private int double_count;
        private Card get_out_of_jail_free;
        private List<Property> properties_owned;
        private List<Property> properties_mortgaged;
        private List<Property> monopolies;
        private List<Property> railroads;
        private List<Property> utilities;

        public Player(string input_name, string input_character, int start_money, int input_go_value, int input_go_bonus)
        {
            name = input_name;
            character = input_character;
            money = start_money;
            position = 0; // go
            properties_owned = new List<Property>();
            properties_mortgaged = new List<Property>();
            monopolies = new List<Property>();
            railroads = new List<Property>();
            utilities = new List<Property>();
            go_value = input_go_value;
            go_bonus = input_go_bonus;
            get_out_of_jail_free = null;
        }

        public void buy(Property property)
        {
            property.buy(this); //call buy method of property to set owner
            money -= property.get_price();
            properties_owned.Add(property);
            Console.WriteLine("Congratulations! You now own {0}. Your new bank balance is ${1}. " +
                "Below is more information about your purchase:\n\n{2}", property.get_name(), money, property);
            refresh_properties();
        }

        public void pay(int amount)
        {
            money -= amount;
        }

        public void pay_rent(Player owner, int rent)
        {
            money -= rent;
            owner.receive_payment(rent);
        }

        public void receive_payment(int payment)
        {
            money += payment;
        }

        public string get_name()
        {
            return name;
        }

        public string get_char()
        {
            return character;
        }

        public string get_nickname()
        {
            return name + " the " + character;
        }

        public int get_start_roll()
        {
            return start_roll;
        }

        public int get_position()
        {
            return position;
        }

        public int get_money()
        {
            return money;
        }

        public int get_net_worth()
        {
            int sum = money;
            foreach (Property p in properties_owned)
            {
                sum += p.get_price() + (p.get_houses() * p.get_price_build());
            }
            return sum;
        }

        public List<Property> get_properties()
        {
            return properties_owned;
        }

        public List<Property> get_mortgaged_properties()
        {
            return properties_mortgaged;
        }

        public List<Property> get_monopolies()
        {
            return monopolies;
        }

        public List<Property> get_house_properties()
        {
            refresh_properties();
            List<Property> house_properties = new List<Property>();
            foreach (Property p in monopolies)
            {
                if (p.get_houses() > 0)
                {
                    house_properties.Add(p);
                }
            }
            return house_properties;
        }

        public List<Property> get_tradable_properties()
        {
            List<Property> tradable = new List<Property>();
            foreach (Property p in properties_owned)
            {
                if (p.get_houses() == 0)
                {
                    //only add properties to tradable list if they don't have houses
                    tradable.Add(p);
                }
            }
            return tradable;
        }

        public List<Property> get_railroads()
        {
            return railroads;
        }

        public List<Property> get_utilities()
        {
            return utilities;
        }

        public int jailed()
        {
            return turns_jailed;
        }

        public int get_double_count()
        {
            return double_count;
        }

        public void increment_double_count()
        {
            double_count++;
        }

        public void reset_double_count()
        {
            double_count = 0;
        }
               
        public void set_start_roll(int index)
        {
            start_roll = index;
        }

        public void go_to_jail()
        {
            position = 10;
            turns_jailed = 1;
        }

        public void increment_jail()
        {
            turns_jailed++;
        }

        public void release_from_jail()
        {
            position = 10;
            turns_jailed = 0;
        }

        public void pay_for_jail(int payment)
        {
            position = 10;
            turns_jailed = 0;
            money -= payment;
        }

        public bool has_jail_free_card()
        {
            if (get_out_of_jail_free == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public Card use_jail_free_card()
        {
            Card jail_free = get_out_of_jail_free;
            get_out_of_jail_free = null;
            release_from_jail();
            return jail_free;
        }

        public void advance(int roll)
        {
            position += roll;
            if (position > 40)
            {
                money += go_value;
                Console.WriteLine("Congratulations - you passed go, and collected ${0}! You now have ${1}", go_value, money);
                position -= 40;
            }
            else if (position == 40)
            {
                money += go_value + go_bonus;
                Console.WriteLine("Congratulations - you landed on go, and collected ${0} plus a bonus of ${1}! You now have ${2}.", 
                    go_value, go_bonus, money);
                position -= 40;
            }
            else if (position < 0)
            {
                position += 40; // could occur on chance drawing with move backwards card
            }
        }

        public void send_property(Player other, Property property)
        {
            properties_owned.Remove(property);

            //put property in other's posession
            other.receive_property(property); 
            property.buy(other);

            refresh_properties();
        }

        public void receive_property(Property property)
        {
            properties_owned.Add(property);
            refresh_properties();
        }

        public void mortgage_property(Property property)
        {
            properties_owned.Remove(property);
            properties_mortgaged.Add(property);
            money += property.mortgage();
            refresh_properties();
        }

        public void unmortgage_property(Property property)
        {
            properties_mortgaged.Remove(property);
            properties_owned.Add(property);
            money -= property.get_price();
            property.unmortgage();
            refresh_properties();
        }

        public int take_card_action(Card card, ref List<Player> players, ref bool has_moved) //return amount of money going to free parking
        {
            string category = card.get_category();
            if (category.Equals("item"))
            {
                //only item is get out of jail free card
                get_out_of_jail_free = card;
                return 0;
            }
            else if (category.Equals("move"))
            {
                //move forward to a select index
                has_moved = true;
                int goto_index = card.get_effect();
                int distance = (goto_index - position + 40) % 40; //ensure movement is forward                
                advance(distance);
                return 0;
            }
            else if (category.Equals("move_abs"))
            {
                //move absolute distance from current pos
                has_moved = true;
                int distance = card.get_effect();
                advance(distance);
                return 0;
            }
            else if (category.Equals("move_utility"))
            {
                int dist_to_electric = (12 - position + 40) % 40;
                int dist_to_water = (28 - position + 40) % 40;
                if (dist_to_electric < dist_to_water)
                {
                    advance(dist_to_electric);
                    return 0;
                }
                else
                {
                    advance(dist_to_water);
                    return 0;
                }
            }
            else if (category.Equals("move_rr"))
            {
                int dist_to_reading = (5 - position + 40) % 40;
                int dist_to_penn = (15 - position + 40) % 40;
                int dist_to_bo = (25 - position + 40) % 40;
                int dist_to_short = (35 - position + 40) % 40;
                int min_dist = Math.Min(Math.Min(dist_to_reading, dist_to_penn), Math.Min(dist_to_bo, dist_to_short));
                if (min_dist == dist_to_reading)
                {
                    advance(dist_to_reading);
                    return 0;
                }
                else if (min_dist == dist_to_penn)
                {
                    advance(dist_to_penn);
                    return 0;
                }
                else if (min_dist == dist_to_bo)
                {
                    advance(dist_to_bo);
                    return 0;
                }
                else //min_dist == dist_to_short
                {
                    advance(dist_to_short);
                    return 0;
                }
            }
            else if (category.Equals("move_jail"))
            {
                //go to jail
                go_to_jail();
                return 0;
            }
            else if (category.Equals("money"))
            {
                int amount = card.get_effect();
                //gain or lose money to free parking
                if (amount > 0)
                {
                    //receive money from bank
                    money += amount;
                    return 0;
                }
                else
                {
                    //pay positive money to free parking from personal finances
                    amount = Math.Abs(amount);
                    money -= amount;
                    return amount;
                }
            }
            else if (category.Equals("money_houses"))
            {
                //pay based on houses
                int num_houses = 0;
                int num_hotels = 0;
                foreach (Property property in monopolies) //can only have buildings on monopolies
                {
                    if (property.get_houses() > 0 && property.get_houses() < 5)
                    {
                        num_houses += property.get_houses();
                    }
                    else if (property.get_houses() == 5)
                    {
                        num_hotels++;
                    }
                }

                if (card.get_tag().Equals("GENRP"))
                {
                    // cost is $25 per house and $100 per hotel
                    int amount = (25 * num_houses) + (100 * num_hotels);
                    money -= amount;
                    return amount;
                }
                else if (card.get_tag().Equals("STRRP"))
                {
                    //cost is $40 per house and $115 per hotel
                    int amount = (40 * num_houses) + (115 * num_hotels);
                    money -= amount;
                    return amount;
                }
                //shouldn't get here - catch case at end
            }
            else if (category.Equals("money_players"))
            {
                //pay money to other players
                int amount = card.get_effect();
                if (amount > 0)
                {
                    //receive money from other players
                    foreach (Player p in players)
                    {
                        p.pay(amount);
                        money += amount;
                    }
                }
                else
                {
                    //pay money to other players
                    amount = Math.Abs(amount);
                    foreach (Player p in players)
                    {
                        p.receive_payment(amount);
                        money -= amount;
                    }
                }
                return 0;
            }
            //shouldn't get here
            Console.WriteLine("Something's wrong -- card category not recognized.");
            return 0;
        }

        public void refresh_properties()
        {
            //clear out any "dummy" blank properties from trades
            List<Property> to_remove = new List<Property>();
            foreach (Property p in properties_owned)
            {
                if (String.IsNullOrWhiteSpace(p.get_name()))
                {
                    to_remove.Add(p);
                }
            }
            foreach (Property p in to_remove)
            {
                properties_owned.Remove(p);
            }

            //clear out any mortgaged properties
            foreach (Property p in properties_mortgaged)
            {
                if (properties_owned.IndexOf(p) != -1)
                {
                    properties_owned.Remove(p);
                }
            }

            //reset railroad, utility and monopoly counts in case of buy or trade
            railroads.Clear();
            utilities.Clear();
            monopolies.Clear();
            foreach (Property property in properties_owned)
            {
                string color = property.get_color();
                int color_count = 1;
                foreach (Property other_p in properties_owned) //check if this purchase completed a monopoly
                {
                    if (other_p.get_color().Equals(color) && !property.Equals(other_p))
                    {
                        color_count++;
                    }
                }
                if (property.get_type().Equals("Street") && (color_count == 2 && (color.Equals("Blue") || color.Equals("Brown")) ||
                    color_count == 3))
                {
                    foreach (Property other_p in properties_owned)
                    {
                        if (other_p.get_color().Equals(color) && monopolies.IndexOf(other_p) == -1) //not in list of monopolies yet
                        {
                            monopolies.Add(other_p);
                        }
                    }
                }
                if (property.get_type().Equals("Utility") && utilities.IndexOf(property) == -1)
                {
                    utilities.Add(property);
                }
                if (property.get_type().Equals("Railroad") && railroads.IndexOf(property) == -1)
                {
                    railroads.Add(property);
                }
            }            
        }

        public override string ToString()
        {

            string properties_list = "";
            foreach (Property p in properties_owned)
            {
                properties_list += p.get_name() + ", ";
            }
            if (properties_owned.Count > 0)
            {
                properties_list = properties_list.Substring(0, properties_list.Length - 2); // trim trailing slash
            }

            string monopolies_list = "";
            foreach (Property p in monopolies)
            {
                monopolies_list += p.get_name() + ", ";
            }
            if (monopolies.Count > 0)
            {
                monopolies_list = monopolies_list.Substring(0, monopolies_list.Length - 2); // trim trailing slash
            }

            string utilities_list = "";
            foreach (Property p in utilities)
            {
                utilities_list += p.get_name() + ", ";
            }
            if (utilities.Count > 0)
            {
                utilities_list = utilities_list.Substring(0, utilities_list.Length - 2); // trim trailing slash
            }

            string railroads_list = "";
            foreach (Property p in railroads)
            {
                railroads_list += p.get_name() + ", ";
            }
            if (railroads.Count > 0)
            {
                railroads_list = railroads_list.Substring(0, railroads_list.Length - 2); // trim trailing slash
            }

            string mortgaged_list = "";
            foreach (Property p in properties_mortgaged)
            {
                mortgaged_list += p.get_name() + ", ";
            }
            if (properties_mortgaged.Count > 0)
            {
                mortgaged_list = mortgaged_list.Substring(0, mortgaged_list.Length - 2);
            }

            string message = String.Format(@"{0} the {1}
Money: ${2}
Net Worth: ${3}
Position: {4}
Properties Owned: {5}
Monopolies Owned: {6}
Utilities Owned: {7}
Railroads Owned: {8}
Mortgaged Properties: {9}
Get Out of Jail Free: {10}", 
                name, character, money, get_net_worth(), position, properties_list, monopolies_list, utilities_list, railroads_list, mortgaged_list,
                (get_out_of_jail_free == null) ? "Not Owned" : "Owned");
            
            return message;
        }

        public void kill()
        {
            //call upon bankruptcy; returns all properties
            foreach (Property p in properties_owned)
            {
                p.return_to_bank();
            }
        }
    }

    
}
