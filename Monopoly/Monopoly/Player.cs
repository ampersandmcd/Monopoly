using System;
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
        private List<Property> properties_owned;
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
            monopolies = new List<Property>();
            railroads = new List<Property>();
            utilities = new List<Property>();
            go_value = input_go_value;
            go_bonus = input_go_bonus;
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

        public List<Property> get_monopolies()
        {
            return monopolies;
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

            string message = String.Format(@"{0} the {1}
Money: ${2}
Net Worth: ${3}
Position: {4}
Properties Owned: {5}
Monopolies Owned: {6}
Utilities Owned: {7}
Railroads Owned: {8}", 
                name, character, money, get_net_worth(), position, properties_list, monopolies_list, utilities_list, railroads_list);
            
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
