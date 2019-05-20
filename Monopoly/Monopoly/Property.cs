using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{
    public class Property
    {
        public string name;
        public string space_type; // go, street, blank, jail, free_parking, income_tax, luxury_tax, go_to_jail, utility, railroad
        public string color;
        public int position;
        public int price;
        public int price_build;
        public int[] rent = new int[6]; //0th index = no houses, 1st index = 1 house,..., 5th index = hotel
        public int num_houses; //0 = no houses, 1 = 1 house, ..., 5 = hotel
        public bool is_owned;
        public Player owner;

        // parameter constructor for use with beginner_board.csv
        // takes in string array of row of table and assigns attributes from that row
        public Property(string[] attributes)
        {
            name = attributes[0];
            space_type = attributes[1];
            color = attributes[2];
            position = Convert.ToInt32(attributes[3]);
            price = Convert.ToInt32(attributes[4]);
            price_build = Convert.ToInt32(attributes[5]);
            for (int i = 6; i < 12; i++)
            {
                rent[i - 6] = Convert.ToInt32(attributes[i]);
            }

            num_houses = 0;
            is_owned = false;
            owner = null;
        }

        public override string ToString()
        {
            return string.Format(@"     Name: {0}
                Type: {1}
                Color: {2}
                Position: {3}
                Price: {4}
                Price to Build: {5}
                Rent: {6} / {7} / {8} / {9} / {10} / {11}
                Owner: {12}", name, space_type, color, position, price, price_build,
                rent[0], rent[1], rent[2], rent[3], rent[4], rent[5], owner != null ? owner.ToString() : "null");
        }
    }
}
