using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace Parking
{
    class Menu
    {
        public void ShowMenu()
        {
            DBConnection dbcon = new DBConnection();
            List<string> list = Messages.ShowOptions();
            while (true) 
            {
                Console.WriteLine(Messages.EnterParams("first"));
                foreach (string str in list)
                {
                    if(str.Length!=0)
                    {
                        Console.WriteLine(str);
                    }
                }
                string userAction = Console.ReadLine();
                bool intInput = Int32.TryParse(userAction,out int menuChoice);
                if (intInput)
                {
                    Console.Clear();
                    switch (menuChoice)
                    {
                        case 1:
                            ParkVehicle(dbcon,0);
                            break;
                        case 2:
                            Find(dbcon, "", out string parkedAt);
                            break;
                        case 3:
                            Move(dbcon);
                            break;
                        case 4:
                            Remove(dbcon,"");
                            break;
                        case 5:
                            Sort(dbcon);
                            break;
                        case 6:
                            SetVehicleFree(dbcon);
                            break;
                        case 7:
                            CastReveal(dbcon);
                            break;
                        case 8:
                            ParkedOverTwoDays(dbcon);
                            break;
                        case 9:
                            incomePerday(dbcon);
                            break;
                        case 10:
                            CreateTestVehicles(dbcon);
                            break;
                        default:
                            break;
                    }
                   
                }
            }
        }
        //find first best spot for a vehicle of given size
        bool suitableSpot(DBConnection dbcon, int vehicleType, out int freeCell)
        {
            freeCell = 0;
            if (dbcon.GetFreeSpot(vehicleType, out List<string> emptyCells))
            {
                if (emptyCells.Count > 0)
                {
                    freeCell = Int32.Parse(emptyCells[0]);
                    return true;
                }
                else
                {
                    Console.WriteLine("Parking is full!");
                }
            }
            return false;
        }
        //validates input and returns a list with regNum and vehicleType
        bool ProvideVehicleInfo(out List<string> list)
        {
            bool ok = false;
            list = new List<string>();
            string provideParams = "";
            string error = "";
            provideParams = Messages.EnterParams("regNum");
            Console.WriteLine(provideParams);
            string regNum = Console.ReadLine();
            bool isAllowedRegNum = InputControl.RegNumCheck(regNum, out error);
            if (!isAllowedRegNum)
            {
                Console.WriteLine(error);
                return ok;
            }

            provideParams = Messages.EnterParams("Type");
            Console.WriteLine(provideParams);
            string vehicleType = Console.ReadLine();
            bool isDigitInput = InputControl.DigitInput(vehicleType, out error);
            if (!isDigitInput)
            {
                Console.WriteLine(error);
                return ok;
            }
            bool allowedType = InputControl.AllowedVehicleType(vehicleType, out error);
            if (!allowedType)
            {
                Console.WriteLine(error);
                return ok;
            }

            if (isAllowedRegNum && isDigitInput && allowedType)
            {
                Console.WriteLine(regNum + " " + vehicleType);
                list.Add(regNum);
                list.Add(vehicleType);
                ok = true;
            }
            return ok;
        }
        //searches if any vehicle with given regNum is currently parked, out parkedAt 
        bool Find(DBConnection dbcon, string uiRegNum, out string parkedAt)
        {
            bool found = false;
            if (uiRegNum == "") 
            { 
                Console.WriteLine(Messages.EnterParams("regNum"));
                uiRegNum = Console.ReadLine();
                bool isAllowedRegNum = InputControl.RegNumCheck(uiRegNum, out string error);
                if (!isAllowedRegNum)
                {
                    Console.WriteLine(error);
                    parkedAt = "";
                    return found;
                }
            }
            if(dbcon.FindVehicle(uiRegNum, out string output))
            {
                if(output!="")
                {
                    found = true;
                    Console.WriteLine(uiRegNum + " was found at parking cell " + output);
                }
                else
                {
                    found = false;
                    Console.WriteLine(uiRegNum + " was not found in our parking ");
                }         
            }
            else
            {
                Console.WriteLine(output);
            }
            parkedAt = output;
            return found;
        }
        //creates object of type Vehicle and inserts it into database if there is a free spot left
        bool ParkVehicle(DBConnection dbcon, int free)
        {
            bool parked = false;
            if (ProvideVehicleInfo(out List<string> listVehicleParams))
            {
                string regNum = listVehicleParams[0];
                int vehicleType = Int32.Parse(listVehicleParams[1]);
                int perfectSpot = 0;
                if (suitableSpot(dbcon, vehicleType, out int getFreeCell))
                {
                    perfectSpot = getFreeCell;
                }
                else
                {
                    Console.WriteLine("Parking is full!");
                    parked = false;
                }
                if (Find(dbcon, regNum, out string parkedAt) == false)
                {
                    if (dbcon.GetDBTime(out DateTime DBDateTime) && perfectSpot != 0)
                    {
                        DateTime currDateTime = DBDateTime;
                        Console.WriteLine(currDateTime);
                        Vehicle vehicle = new Vehicle(regNum, vehicleType, perfectSpot, currDateTime, free);
                        dbcon.ParkVehicle(vehicle);
                        parked = true;
                    }
                }
            }
            return parked;
        }
        //moves the vehicle with provided RegNum if it is currently in the parked
        bool Move(DBConnection dbcon)
        {
            {
                Console.WriteLine(Messages.EnterParams("regNum"));
                string uiRegNum = Console.ReadLine();
                if(!InputControl.RegNumCheck(uiRegNum,out string error)){
                    Console.WriteLine(error);
                    return false;
                }
                Console.WriteLine(Messages.EnterParams("Where"));
                bool ok= Int32.TryParse(Console.ReadLine(), out int reply);
                if (ok)
                {
                    if (Find(dbcon, uiRegNum, out string parkedAt))
                    {
                            if (dbcon.MoveVehicle(uiRegNum, reply, out int output))
                            {
                                if(output==1)
                                {
                                    Console.WriteLine("Vehicle has been moved");
                                    Console.WriteLine(uiRegNum + " is now parked at " + reply);
                                    return true;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Can't move there, cell nr " + reply + " seems to be full");
                            }
                    }
                }
                else
                {
                    Console.WriteLine(Messages.EnterParams(""));
                }
            }
            return false;
        }
        //removes a vehicle with provided regNum
        bool Remove(DBConnection dbcon, string uiRegNum)
        {
            if(uiRegNum=="")
            {
                Console.WriteLine(Messages.EnterParams("regNum"));
                uiRegNum = Console.ReadLine();
            }
            if(Find(dbcon, uiRegNum,out string parkedAt))
            {
                if(dbcon.DeleteVehicle(uiRegNum, out string cost))
                {
                    Console.WriteLine(uiRegNum+ " total cost will be " + cost.Substring(0,cost.Length-5) + " schmeckles");
                    return true;
                }
            }
            return false;
                
        }
        //sorts all vehicles with size less than 100
        bool Sort(DBConnection dbcon)
       {
           if(dbcon.sortVehicles(out string output))
           {
                Console.WriteLine(output);
                return true;
           }
           return false;
        }
        //writes out every cell and vehicle in it
        bool CastReveal(DBConnection dbcon)
        {
           if(dbcon.CastRevealSpellOnEmpty(out string[] PakringOverview))
           {
                if(dbcon.CastRevealSpellOnNotEmpty(PakringOverview, out PakringOverview))
                {
                    //Console.BackgroundColor = ConsoleColor.DarkGray;
                    //Console.ForegroundColor = ConsoleColor.Green;
                    for(int i=1; i<PakringOverview.Length;i++){
                        if(PakringOverview[i]=="Free Parking Cell"){
                            Console.ForegroundColor = ConsoleColor.Green;
                        }else if(PakringOverview[i].Substring(0,3)=="100"){
                          Console.ForegroundColor = ConsoleColor.DarkRed;

                        }
                        else{
                            Console.ForegroundColor = ConsoleColor.Yellow;
                        }
                        Console.WriteLine(i +" "+ PakringOverview[i]);
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    return true;
                }
                return true;
           }
           
           return true;
           
        }
        //sets FreeParking in DB to 1 and after deletes the vehile with total cost 0
        bool SetVehicleFree(DBConnection dbcon){
                Console.WriteLine(Messages.EnterParams("regNum"));
                string uiRegNum = Console.ReadLine();
            bool ok=false;
                bool isAllowedRegNum = InputControl.RegNumCheck(uiRegNum, out string error);
                if (!isAllowedRegNum)
                {
                    Console.WriteLine(error);
                }
            if(Find(dbcon, uiRegNum,out string parkedAt)){
                dbcon.SetFree(uiRegNum);
                Remove(dbcon,uiRegNum);
                ok= true;
            }
            return ok;
        }
        //show all vehicles that have been parked since x hours, finns en vy för längre än två dagar med i db
        bool ParkedOverTwoDays(DBConnection dbcon){
            bool ok = false;
            string duration = "";
                Console.WriteLine(Messages.EnterParams("hours"));
                duration = Console.ReadLine();
            int minutes=0;
            if(Int32.TryParse(duration, out int hours))
            {
                minutes=hours*60;
                dbcon.parkedOver(minutes, out List <string>list);
                foreach(string str in list){
                    Console.WriteLine(str);
                }
                ok= true;
            }
            else
            {
                Console.WriteLine(Messages.EnterParams(""));
            }
            return ok;
        }
        //sum and avg, not finished 
        bool incomePerday(DBConnection dbcon){
        CultureInfo provider = CultureInfo.CurrentUICulture;

         bool ok = false;
            string startdate = "";
            string enddate="";
                Console.WriteLine("enter start date(xx-xx-xx)");
                startdate = Console.ReadLine();
                DateTime startDateTime=DateTime.ParseExact(startdate,"yy-MM-dd",provider);
                Console.WriteLine(startDateTime);
                Console.WriteLine("enter end date(xx-xx-xx)");
                enddate = Console.ReadLine();
                DateTime enfDateTime=DateTime.ParseExact(startdate,"yy-MM-dd",provider);
                dbcon.incomeControl(startDateTime, enfDateTime, out List <string>list);
                foreach(string str in list){
                    Console.WriteLine(str);
                }
                ok= true;
            return ok;
        }
        //test function for parking 10 vehicles, deleting them and parking them again
        void CreateTestVehicles(DBConnection dbcon)
        {
            int vehicleType = 2;
            for(int i = 11; i < 22; i++) {
                string regNum = "test" + i;
                  if (dbcon.GetDBTime(out DateTime DBDateTime))
                  {
                      dbcon.GetFreeSpot(vehicleType, out List<string> emptyCells);
                      int perfectSpot = int.Parse(emptyCells[0]);
                      DateTime currDateTime = DBDateTime;
                      Console.WriteLine(currDateTime);
                      Vehicle vehicle = new Vehicle(regNum, vehicleType, perfectSpot, currDateTime, 0);
                      dbcon.ParkVehicle(vehicle);
                  }

            }
            for(int i = 11; i < 22; i++) {
                string regNum = "test" + i;
                Remove(dbcon, regNum);
            }
            for(int i = 11; i < 22; i++) {
                string regNum = "test" + i;
                  if (dbcon.GetDBTime(out DateTime DBDateTime))
                  {
                      dbcon.GetFreeSpot(vehicleType, out List<string> emptyCells);
                      int perfectSpot = int.Parse(emptyCells[0]);
                      DateTime currDateTime = DBDateTime;
                      Console.WriteLine(currDateTime);
                      Vehicle vehicle = new Vehicle(regNum, vehicleType, perfectSpot, currDateTime, 0);
                      dbcon.ParkVehicle(vehicle);
                  }
            }
        }

    }
}
