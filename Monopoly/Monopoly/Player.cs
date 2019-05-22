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
        private int position;
        private int start_roll;
        private int turns_jailed;
        private int double_count;
        private List<Property> properties_owned;
        private List<Property> monopolies;
        private List<Property> railroads;
        private List<Property> utilities;

        public Player(string input_name, string input_character, int start_money, int input_go_value)
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
        }

        public void buy(Property property)
        {
            property.buy(this); //call buy method of property to set owner
            money -= property.get_price();
            properties_owned.Add(property);
            Console.WriteLine("Congratulations! You now own {0}. Your new bank balance is ${1}. " +
                "Below is more information about your purchase:\n\n{2}", property.get_name(), money, property);
            string color = property.get_color();
            int color_count = 0;
            foreach (Property other_p in properties_owned) //check if this purchase completed a monopoly
            {
                if (other_p.get_color().Equals(color))
                {
                    color_count++;
                }
            }
            if (property.get_type().Equals("Street") && (color_count == 2 && (color.Equals("Blue") || color.Equals("Brown")) || 
                color_count == 3))
            {
                Console.WriteLine("\nCongrats! You have a monopoly on {0}!", color);
                foreach (Property other_p in properties_owned)
                {
                    if (other_p.get_color().Equals(color))
                    {
                        monopolies.Add(other_p);
                    }
                }
            }
            if (property.get_type().Equals("Utility"))
            {
                utilities.Add(property);
            }
            if (property.get_type().Equals("Railroad"))
            {
                railroads.Add(property);
            }
        }

        public void pay(int amount)
        {
            money -= amount;
        }

        public void pay_rent(Player owner, int rent)
        {
            money -= rent;
            owner.receive_rent(rent);
        }

        public void receive_rent(int rent)
        {
            money += rent;
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

        public void pay_for_jail()
        {
            position = 10;
            turns_jailed = 0;
            money -= 50;
        }

        public void advance(int roll)
        {
            position += roll;
            if (position >= 40)
            {
                money += go_value;
                Console.WriteLine("Congrats - you passed go, and collected ${0}! You now have ${1}", go_value, money);
                position -= 40;
            }
        }

        public override string ToString()
        {
            string message = String.Format("{0} the {1}\nNet Worth: {2}\nPosition: {3}\nProperties Owned: ", name, character, money, position);
            foreach (Property p in properties_owned)
            {
                message += p.get_name() + " / ";
            }
            message = message.Substring(0, message.Length - 3); // trim trailing slash
            return message;
        }
    }

    
}
