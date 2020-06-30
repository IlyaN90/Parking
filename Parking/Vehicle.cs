using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking
{
    class Vehicle
    {
        string regNumber;
        int vehicleType;
        int cell;
        DateTime parkedTime;
        int parkForFree;

        public Vehicle(string regNumber, int vehicleType,int cell, DateTime parkedTime, int parkForFree)
        {
            this.parkForFree=parkForFree;
            this.regNumber = regNumber;
            this.vehicleType = vehicleType;
            this.cell = cell;
            this.parkedTime = parkedTime;
        }
        public string RegNumber()
        {
            return this.regNumber;
        }

        public int VehicleType()
        {
            return this.vehicleType;
        }

        public int ParkForFree()
        {
              return this.parkForFree;
        }

        public int Cell()
        {
            return this.cell;
        }

        public DateTime ParkedTime()
        {
            return this.parkedTime;
        }
    }
}
