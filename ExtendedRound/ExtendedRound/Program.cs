using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtendedRound
{

    class Car
    {
        public int X, Y;
        int AvailableAt;

        public List<int> Rides;

        public Car()
        {
            X = 0;
            Y = 0;
            AvailableAt = 0;
            Rides = new List<int>();
        }

        public bool IsAvailable()
        {
            return (AvailableAt >= Program.CurrentTime);
        }

        public void Drive(Ride r)
        {
            
        }
    }

    class Ride
    {
        public int ID, StartX, StartY, EndX, EndY, EarliestStart, LatestFinish, Distance;

        public Ride(int StartY, int StartX, int EndY, int EndX, int EarliestStart, int LatestFinish)
        {
            this.ID = Program.CurrentID++;
            this.StartX = StartX;
            this.StartY = StartY;
            this.EndX = EndX;
            this.EndY = EndY;
            this.EarliestStart = EarliestStart;
            this.LatestFinish = LatestFinish;
            this.Distance = Math.Abs(StartX - EndX) + Math.Abs(StartY - EndY);
        }
    }

    class Map
    {
        public List<Ride>[,] _Map;
        public int R, C;

        public Map(int R, int C)
        {
            this._Map = new List<Ride>[R, C];
            this.R = R;
            this.C = C;
        }

        public void RemoveRide(int a, int b, Ride r)
        {
            _Map[a, b].Remove(r);
        }

        public void FindNextRide(int currY, int currX)
        {

        }

        public void GenerateRoute()
        {

        }
    }

    class Program
    {

        public static int CurrentID = 0;
        public static int CurrentTime = 0;
        static int B = 0, T = 0;
        static Map Atlas;
        static List<Car> Cars = new List<Car>();
        

        static void Main(string[] args)
        {

            if(args.Length != 2)
            {
                Console.WriteLine("Usage: app_name <input_file> <output_file>");
                Console.ReadKey();
                return;
            }

            if (!ReadInputData(args[0]))
            {
                Console.ReadKey();
                return;
            }



            SaveData(args[1]);

        }

        static void SaveData(string arg)
        {
            string Data = "";
            for (int i = 0; i < Cars.Count; i++)
            {
                Data += (Cars[i].Rides.Count).ToString() + " ";
                Cars[i].Rides.ForEach(delegate (int RideID)
                {
                    Data += (RideID).ToString() + " ";
                });
                Data += "\n";
            }
            System.IO.StreamWriter sw = new System.IO.StreamWriter(arg);
            sw.Write(Data);
            sw.Close();
        }

        static bool ReadInputData(string arg)
        {

            string[] lines;

            try
            {
                lines = System.IO.File.ReadAllLines(arg);
            }
            catch (System.Exception e)
            {
                Console.WriteLine("Error while reading the input file: ", e.Message);
                return false;
            }

            string[] parameters = lines[0].Split(' ');

            if(parameters.Length != 6)
            {
                Console.WriteLine("Error while converting first line of input file.");
                return false;
            }

            int R = Int32.Parse(parameters[0]),
                C = Int32.Parse(parameters[1]),
                F = Int32.Parse(parameters[2]),
                N = Int32.Parse(parameters[3]);

            B = Int32.Parse(parameters[4]);
            T = Int32.Parse(parameters[5]);

            Atlas = new Map(R, C);

            for (int i = 0; i < F; i++)
            {
                Cars.Add(new Car());
            }

            int x, y;

            for (int i = 1; i <= N; i++)
            {
                parameters = lines[i].Split(' ');

                if (parameters.Length != 6)
                {
                    Console.WriteLine("Error while converting {0} line of input file.", i);
                    return false;
                }

                y = Int32.Parse(parameters[0]);
                x = Int32.Parse(parameters[1]);

                Atlas._Map[y, x].Add(new Ride(y, x, Int32.Parse(parameters[2]), Int32.Parse(parameters[3]), Int32.Parse(parameters[4]), Int32.Parse(parameters[5])));
            }

            return true;
        }
    }
}
