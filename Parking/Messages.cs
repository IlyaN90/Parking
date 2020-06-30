using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking
{
     static class Messages
    {
        //not finished, should take care of all Console.WriteLine() for easier alteration
        public static string EnterParams(string str)
        {
            if (str == "first")
            {
                str = "Please choose what you want to do:";
                return str;
            }
            else if (str == "regNum")
            {
                str = "Please, enter registreation number for your vehicle: ";
                return str;
            }
            else if (str == "Type")
            {
                str = "Please, enter 1(Car) or 2(MC): ";
            }
            else if (str == "Where")
            {
                str = "Where do you want to park it?: ";
            }
            else if(str == "hours")
            {
                 str = "Provide how many hours: ";
            }
            else
            {
                str = "Try again";
            }
            return str;
        }
        //returns string list with all menu options
        public static List<string> ShowOptions()
        {
            List<string> list=new List <string>();
            list.Add("1. Park a vehicle");
            list.Add("2. Find Vehicle");
            list.Add("3. Move a vehicle");
            list.Add("4. Remove a vehicle");
            list.Add("5. Sort");
            list.Add("6. Remove vehicle for free");
            list.Add("7. Display overview");
            list.Add("8. Vehicles paked longer than: ");
            list.Add("9. Income/day (not finished)");
            list.Add("10. Create 10, delete 10 and create 10 again");

            return list;
        }

    }
}
