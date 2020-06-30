using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Globalization;


namespace Parking
{
    class DBConnection
    {
        string connectionString;
        string query;
        string timeDif="";
        //creates DBConnection string
        public DBConnection()
        {
            connectionString = @"Data Source=localhost\SQLEXPRESS;Initial Catalog=Parking;Integrated Security=True";
        }
        //searches for vehicle in DB
        public bool FindVehicle( string RegNum ,out string output)
        {
            string query = "exec dbo.pFindVehicle @phRegNum;";
            output="";
            using (SqlConnection connection =
            new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.Add("@phRegNum", SqlDbType.Char);
                command.Parameters["@phRegNum"].Value = RegNum;
                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        output += reader[0].ToString();
                    }
                    reader.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
        }
        //returns current DB time
        public bool GetDBTime(out DateTime currDateTime)
        {
            string output = "";
            query = "SELECT GETDATE() as SMALLDATETIME;";
            using (SqlConnection connection =
            new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                try
                {
                    connection.Open();
                    output = command.ExecuteScalar().ToString();
                    currDateTime = DateTime.Parse(output);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    currDateTime = DateTime.Now;
                    return false;
                }
            }
        }
        //returns a list with all free spots, if list.count()==0, there is no free spots left
        public bool GetFreeSpot(int vehicleType, out List <string> list)
        {
            string query = "exec dbo.pFreeSpots @typeID";
            list = new List<string>(100);
            using (SqlConnection connection =
            new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.Add("@typeID", SqlDbType.Int);
                command.Parameters["@typeID"].Value = vehicleType;

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    string cell = "";
                    while (reader.Read())
                    {
                        cell = reader[0].ToString();
                        list.Add(cell);
                    }
                    reader.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
        }
        //inserts a Vehicle object into DB, updates current cell size
        public bool ParkVehicle(Vehicle vehicle)
        {
            string query = "dbo.transaction_ParkVehicle @phRegNum, @phTypeID, @phCellID, @phCheckInDT, @phFreeParking ";;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.Add("@phRegNum", SqlDbType.Char);
                command.Parameters["@phRegNum"].Value = vehicle.RegNumber();

                command.Parameters.Add("@phTypeID", SqlDbType.Int);
                command.Parameters["@phTypeID"].Value = vehicle.VehicleType();

                command.Parameters.Add("@phCellID", SqlDbType.Int);
                command.Parameters["@phCellID"].Value = vehicle.Cell();

                command.Parameters.Add("@phCheckInDT", SqlDbType.SmallDateTime);
                command.Parameters["@phCheckInDT"].Value = vehicle.ParkedTime();

                command.Parameters.Add("@phFreeParking", SqlDbType.Int);
                command.Parameters["@phFreeParking"].Value = vehicle.ParkForFree();

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
        }
        //delets vehicle from DB and adds it into records, updates current cell size
        //unfinished -> cast minutes to days/hours/minutes
        public bool DeleteVehicle(string regNum, out string cost)
        {
            string query = "dbo.t_Delete_Vehicle @phRegNum ";
            string output = "";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.Add("@phRegNum", SqlDbType.Char);
                command.Parameters["@phRegNum"].Value = regNum;
                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if(reader.Read())
                    {
                        output=reader["Cost"].ToString();
                        timeDif=reader["TimeDif"].ToString();
                        Console.WriteLine("Vehicle was parked for total of " + timeDif + " time units(minutes)");
                    }
                    cost = output;
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    cost = "error";
                    return false;
                }
            }
        }
        //moves vehicle
        public bool MoveVehicle(string regNum, int reply, out int output)
        {
            output = 0;
            string query = "exec dbo.t_Move_Vehicle  @phPerfectSpot,@phRegNum ";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.Add("@phRegNum", SqlDbType.Char);
                command.Parameters["@phRegNum"].Value = regNum;

                command.Parameters.Add("@phPerfectSpot", SqlDbType.Int);
                command.Parameters["@phPerfectSpot"].Value = reply;
                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        output = Int32.Parse(reader[0].ToString());
                        Console.WriteLine("MoveVehicle " + output);
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
        }
        //sorts vehicles with size < 100 and calls function ShowOrder after each sort
        public bool sortVehicles(out string output)
        {
            bool ok=false;
            output = "";
            string query = "exec dbo.t_sort";
            bool sorted=false;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
               
                    SqlCommand command = new SqlCommand(query, connection);
                    try
                    {
                        connection.Open();
                         while(!sorted){
                            SqlDataReader reader = command.ExecuteReader();
                            if (reader.HasRows)
                            {
                                showOrder(out string orderstr);
                                output=orderstr;
                                ok=true;
                            }else
                            {
                               Console.WriteLine("Already Sorted");
                                sorted=true;
                                ok=false;
                            } 
                           reader.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return false;
                    }
              return ok;
            }
        }
        //prints out latest order from function Sort
        public bool showOrder(out string output){
            output = "";
            string query = "select TOP(1) * from OrderHistory order by OrderID Desc";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if(reader.Read()){
                       Console.WriteLine("\t{0} from cell {1} to cell {2}, move vehicle {3}", reader["OrderID"].ToString(),reader["MoveFromCell"].ToString(),reader["MoveToCell"].ToString(),reader["MoveRegNum"].ToString());
                    }
                   output="";
                   return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
        }
        //get overview over parking situation
        public bool CastRevealSpellOnEmpty(out string[] ArrOut)
        {
            string query = "select CellNum, IsEmpty, size from ParkingCells where IsEmpty=0";
            ArrOut = new string[101];
            ArrOut[0]="Start";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int i=Int32.Parse(reader["CellNum"].ToString());
                       // ArrOut[i] = reader["CellNum"].ToString() +","+reader["size"].ToString() ;
                        ArrOut[i] = "Free Parking Cell";
                    }
                   return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }  
        }  
        //get overview over parking situation
        public bool CastRevealSpellOnNotEmpty(string[] ArrIn,out string[] ArrOut)
        {
            ArrOut=new string[101];
            Array.Copy(ArrIn,ArrOut,101);
            string query = "SELECT ParkingCells.CellNum, ParkingCells.Size, vehicles.RegNum, VehicleTypes.TypeNum FROM Vehicles "
			+"join VehicleTypes "
			+"on Vehicles.TypeID=VehicleTypes.TypeID "
			+"join ParkingCells "
			+"on Vehicles.CellID=ParkingCells.CellID";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int i=Int32.Parse(reader["CellNum"].ToString());
                        if(ArrOut[i]!=null)
                        {
                            ArrOut[i] += " & "+ reader["RegNum"].ToString().Trim();
                        }
                        else
                        {
                             ArrOut[i] = reader["Size"].ToString() +"%full, "+reader["RegNum"].ToString().Trim();
                        }
                    }
                   return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }  
        }
        //remove without paying
        public bool SetFree(string setFreeRegNum){
        query = "exec psetFree @phRegNum";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.Add("@phRegNum", SqlDbType.Char);
                command.Parameters["@phRegNum"].Value = setFreeRegNum;
 
                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
        }
        //showes vehicles that have been parked for x minutes 
        public bool parkedOver(int minutes, out List<string> list){
        query = "exec ParkedOverTwoDays @userQuery";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                list = new List<string>(100);
                command.Parameters.Add("@userQuery", SqlDbType.Char);
                command.Parameters["@userQuery"].Value = minutes;
                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string row= reader["regnum"].ToString().Trim() +" in cell "+reader["CellID"].ToString().Trim()+" was parked since "+reader["CheckInDT"].ToString();
                        list.Add(row); 
                    }
                   return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
        }
        //sum and avg, not finished 
        public bool incomeControl(DateTime startdate, DateTime enddate, out List <string>list){
            query = "exec pIncomeOverview @from,@untill";
            CultureInfo provider = CultureInfo.InvariantCulture;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                list = new List<string>(100);
                command.Parameters.Add("@from", SqlDbType.SmallDateTime);
                command.Parameters["@from"].Value = startdate;
                command.Parameters.Add("@untill", SqlDbType.SmallDateTime);
                command.Parameters["@untill"].Value = enddate;
                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        //string row= reader["Summ"].ToString() +"  "+reader["Avgg"].ToString();
                        string row=reader["summ"].ToString(); 
                        //row= row + reader["avgg"].ToString(); 
                        Console.WriteLine(row);
                        list.Add(row); 
                    }
                   return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
        }
    }
}
