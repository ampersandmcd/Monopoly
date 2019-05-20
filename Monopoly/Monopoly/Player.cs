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
        private int position;
        private int start_roll;
        private int turns_jailed;
        private List<Property> properties_owned;

        public Player(string input_name, string input_character, int start_money)
        {
            name = input_name;
            character = input_character;
            money = start_money;
            position = 0; // go
            properties_owned = new List<Property>();
        }

        public string get_name()
        {
            return name;
        }

        public string get_char()
        {
            return character;
        }

        public int get_start_roll()
        {
            return start_roll;
        }

        public int get_position()
        {
            return position;
        }

        public int jailed()
        {
            return turns_jailed;
        }
               
        public void set_start_roll(int index)
        {
            start_roll = index;
        }

        public void go_to_jail()
        {
            position = 12;
            turns_jailed = 1;
        }

        public void increment_jail()
        {
            turns_jailed++;
        }

        public void release_from_jail()
        {
            position = 12;
            turns_jailed = 0;
        }

        public void pay_for_jail()
        {
            position = 12;
            turns_jailed = 0;
            money -= 50;
        }

        public void advance(int roll, int go_value)
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
