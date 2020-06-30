using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking
{
    static class InputControl
    {
        public static bool RegNumCheck(string userInput, out string error)
        {
            if (userInput.Length > 10)
            {
                error = "Regestration number is too long (max 10).";
                return false;
            }
           for (int i = 0; i < userInput.Length; i++)
            {
                 if (!Char.IsLetterOrDigit(userInput[i]))
                 {
                    error = "Can only include letters and digits.";
                    return false;
                 }
            }
            error = "";
            return true;
        }

        public static bool DigitInput(string userInput, out string error)
        {
            for (int i = 0; i < userInput.Length; i++)
            {
                if (!Char.IsDigit(userInput[i]))
                {
                    error = "Please use digits only."; 
                    return false;
                }
            }
            error = "";
            return true;
        }

        public static bool AllowedVehicleType(string userInput, out string error)
        {
            if (userInput != "1" && userInput != "2")
            {
                error = "Please choose from available options.";
                return false;
            }
            error = "";
            return true;
        }
    }
}
