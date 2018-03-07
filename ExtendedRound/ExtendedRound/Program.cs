using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtendedRound
{

    class Car
    {
        public int X, Y, CarTime;
        public bool EndARun;
        public int ID;
        public static int NumOfCars = 0;
        public static int CarID = 0;

        public List<int> Rides;

        public Car()
        {
            X = 0;
            Y = 0;
            CarTime = 0;
            ID = CarID++;
            EndARun = false;
            Rides = new List<int>();
        }

        public void EndRun()
        {
            EndARun = true;
            NumOfCars--;

            Console.WriteLine("##### Debug #####\nOne of the cars end a run\nCars left: {0}\n", NumOfCars);
        }

        public void Drive(ResContainer r)
        {
            Y = r._Ride.EndY;
            X = r._Ride.EndX;
            CarTime += r.ApproachDistance + r._Ride.Distance;
            Rides.Add(r._Ride.ID);
            Program.Atlas.RemoveRide(r._Ride.StartY, r._Ride.StartX, r._Ride);
            if(CarTime >= Program.T)
                EndRun();
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

    class ResContainer
    {
        public int ApproachDistance;
        public int TimeOfDrive;
        public int PointsEarned;
        // public int TimeWasted; przy sprawdzaniu sie sprzyda, ale w klasie Map !!!!!!!!!!!!!!!!!!!!!!!
        public Ride _Ride;
        public bool Bonus;

        public ResContainer(int ApproachDistance, Ride _Ride, bool Bonus)
        {
            this.ApproachDistance = ApproachDistance;
            this._Ride = _Ride;
            this.Bonus = Bonus;
            this.TimeOfDrive = ApproachDistance + _Ride.Distance;
            this.PointsEarned = _Ride.Distance;
            if (Bonus)
                this.PointsEarned += Program.B;
        }
    }

    class Points
    {
        public int X, Y;
        public Points(int y, int x)
        {
            X = x;
            Y = y;
        }
    }

    class Map
    {
        public List<Ride>[,] _Map;
        public int R, C;

        public Map(int R, int C)
        {
            this._Map = new List<Ride>[R, C];

            for (int i = 0; i < R; i++) 
            {
                for (int j = 0; j < C; j++) 
                {
                    this._Map[i, j] = new List<Ride>();
                }
            }

            this.R = R;
            this.C = C;
        }

        public void RemoveRide(int a, int b, Ride r)
        {
            _Map[a, b].Remove(r);
        }

        public bool CheckIfEmpty()
        {
            for (int i = 0; i < R; i++)
            {
                for (int j = 0; j < C; j++)
                {
                    if (_Map[i, j].Count != 0)
                        return false;
                }
            }

            return true;
        }

        List<Points> GetNextPoints(int ys, int xs, int c)
        {
            List<Points> points = new List<Points>();
            int a, b;

            for (int i = -c; i <= c; i++) 
            {
                for (int j = -c; j <= c; j++)
                {
                    if (Math.Abs(i) + Math.Abs(j) == c)
                    {
                        a = ys + i;
                        b = xs + j;
                        if (a >= 0 && b >= 0 && a < Program.Atlas.R && b < Program.Atlas.C)
                        {
                            if(Program.Atlas._Map[a, b].Count != 0)
                                points.Add(new Points(ys + i, xs + j));
                        }
                            
                    }
                }
            }

            return points;
        }

        public List<ResContainer> FindBestRidesFromPos(int Y, int X, int CurrentTime, int TimeToEnd, Ride SkipRide) 
        {
            List<ResContainer> Results = new List<ResContainer>();
            int ApproachTime = 0, RidesChecked = 0;
            List<Points> _Points;

            while (true)
            {
                _Points = GetNextPoints(Y, X, ApproachTime);

                if (TimeToEnd <= ApproachTime)
                    break;

                if (_Points.Count == 0)
                {
                    ApproachTime++;
                    continue;
                }

                if (RidesChecked >= Program.N)
                {
                    goto AfterLoop;
                }

                foreach (Points Point in _Points)
                {
                    if (_Map[Point.Y, Point.X].Count != 0)
                    {
                        foreach (Ride _ride in _Map[Point.Y, Point.X])
                        {
                            RidesChecked++;

                            if (_ride.Equals(SkipRide))
                                continue;

                            if (TimeToEnd >= ApproachTime + _ride.Distance)
                            {
                                if (ApproachTime + CurrentTime == _ride.EarliestStart)
                                    Results.Add(new ResContainer(ApproachTime, _ride, true));
                                else
                                    Results.Add(new ResContainer(ApproachTime, _ride, false));
                            }
                            
                            if(RidesChecked >= Program.N)
                            {
                                goto AfterLoop;
                            }
                        }
                    }
                }
                
                if (Results.Count >= 5)
                    break;

                ApproachTime++;
            }

            AfterLoop:

            Results.Sort((a, b) => a.PointsEarned.CompareTo(b.PointsEarned)); 

            return Results;

        }

        bool GetNextCar(out Car car)
        {
            car = new Car();
            foreach(Car c in Program.Cars)
            {
                if(!c.EndARun)
                {
                    car = c;
                    return true;
                }
            }

            return false;
        }

        void GenerateRouteFirstTime()
        {
            List<ResContainer> PossibleRides, PossibleRidesFromAnotherPossibleRide, BestPossibleRides;
            int tmp;

            foreach(Car c in Program.Cars)
            {
                Console.WriteLine("##### Debug #####\nFinding {0} route to car{1}\n", c.Rides.Count + 1, c.ID);
                PossibleRides = FindBestRidesFromPos(c.Y, c.X, c.CarTime, Program.T - c.CarTime, null);

                if (PossibleRides.Count == 0)
                {
                    c.EndRun();

                    continue;
                }

                BestPossibleRides = new List<ResContainer>();

                foreach (ResContainer _ResCont in PossibleRides)
                {
                    tmp = c.CarTime + _ResCont.TimeOfDrive;
                    PossibleRidesFromAnotherPossibleRide = FindBestRidesFromPos(_ResCont._Ride.EndY, _ResCont._Ride.EndX, tmp, Program.T - tmp, _ResCont._Ride);

                    if (PossibleRidesFromAnotherPossibleRide.Count != 0)
                    {
                        BestPossibleRides.Add(_ResCont);
                    }
                }

                if (BestPossibleRides.Count == 0)
                {
                    c.Drive(PossibleRides[0]);
                }
                else
                {
                    BestPossibleRides.Sort((a, b) => a.PointsEarned.CompareTo(b.PointsEarned));
                    c.Drive(BestPossibleRides[0]);
                }

            }

            Program.Cars.Sort((a, b) => a.CarTime.CompareTo(b.CarTime));
            
        }

        public void GenerateRoute()
        {

            GenerateRouteFirstTime();

            List<ResContainer> PossibleRides, PossibleRidesFromAnotherPossibleRide, BestPossibleRides;
            int tmp; 
            
            while(Program.CheckIfAnyCar() && !CheckIfEmpty())
            {
                //Console.WriteLine("qwec");
                if (GetNextCar(out Car c))
                {
                    Console.WriteLine("##### Debug #####\nFinding {0} route to car{1}\n", c.Rides.Count + 1, c.ID);
                    PossibleRides = FindBestRidesFromPos(c.Y, c.X, c.CarTime, Program.T - c.CarTime, null);
                        
                    if(PossibleRides.Count == 0)
                    {
                        c.EndRun();
                        
                        continue;
                    }

                    BestPossibleRides = new List<ResContainer>();

                    foreach (ResContainer _ResCont in PossibleRides)
                    {
                        tmp = c.CarTime + _ResCont.TimeOfDrive;
                        PossibleRidesFromAnotherPossibleRide = FindBestRidesFromPos(_ResCont._Ride.EndY, _ResCont._Ride.EndX, tmp, Program.T - tmp, _ResCont._Ride);

                        if(PossibleRidesFromAnotherPossibleRide.Count != 0)
                        {
                            BestPossibleRides.Add(_ResCont);
                        }
                    }

                    if(BestPossibleRides.Count == 0)
                    {
                        c.Drive(PossibleRides[0]);
                    }
                    else
                    {
                        BestPossibleRides.Sort((a, b) => a.PointsEarned.CompareTo(b.PointsEarned));
                        c.Drive(BestPossibleRides[0]);
                    }
                    
                }

                Program.Cars.Sort((a, b) => a.CarTime.CompareTo(b.CarTime));
            }
        }
    }

    class Program
    {

        public static int CurrentID = 0;
        public static int CurrentTime = 0;
        public static int B = 0, T = 0, N = 0;
        public static List<Car> Cars = new List<Car>();
        public static Map Atlas;

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

            Atlas.GenerateRoute();

            SaveData(args[1]);

            //Console.ReadKey();

        }

        public static bool CheckIfAnyCar()
        {
            foreach(Car c in Cars)
            {
                if (!c.EndARun)
                    return true;
            }

            return false;
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
            //Console.WriteLine("Output: \n{0}", Data);
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
                F = Int32.Parse(parameters[2]);

            N = Int32.Parse(parameters[3]);
            B = Int32.Parse(parameters[4]);
            T = Int32.Parse(parameters[5]);

            Atlas = new Map(R, C);

            for (int i = 0; i < F; i++)
            {
                Cars.Add(new Car());
            }

            Car.NumOfCars = Cars.Count;

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
