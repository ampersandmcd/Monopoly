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
        private bool is_jailed;
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

        public void set_start_roll(int index)
        {
            start_roll = index;
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
