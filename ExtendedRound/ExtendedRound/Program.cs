using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            CarTime += r.TimeOfDrive;
            Rides.Add(r._Ride.ID);
            Program.Atlas.RemoveRide(r._Ride.StartY, r._Ride.StartX, r._Ride);
            Program._Points.Remove(r._point);
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
        public Points _point;
        public int waitTime;
        public double TimeProportion;

        public ResContainer(int ApproachDistance, Ride _Ride, bool Bonus, Points Point, int waitTime)
        {
            this.waitTime = waitTime;
            this.ApproachDistance = ApproachDistance;
            this._Ride = _Ride;
            this.Bonus = Bonus;
            this.TimeOfDrive = ApproachDistance + _Ride.Distance + this.waitTime;
            this.PointsEarned = _Ride.Distance;
            this._point = Point;

            if (Bonus)
                this.PointsEarned += Program.B;

            TimeProportion = (double)this.TimeOfDrive / (double)(ApproachDistance + waitTime);

        }

        public ResContainer(ResContainer rold, int minretdist)
        {
            waitTime = rold.waitTime;
            ApproachDistance = rold.ApproachDistance;
            _Ride = rold._Ride;
            Bonus = rold.Bonus;
            TimeOfDrive = ApproachDistance + _Ride.Distance + waitTime;
            PointsEarned = _Ride.Distance;
            _point = rold._point;

            if (Bonus)
                this.PointsEarned += Program.B;

            TimeProportion = (double)this.TimeOfDrive / (double)(ApproachDistance + waitTime + minretdist);
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

        List<Points> GetNextPoints(int ys, int xs)
        {
            List<Points> points = new List<Points>();
            int a, b;
            
            for (int c = 0; c < Program.Atlas.R + Program.Atlas.C; c++)
            {
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
                                if (Program.Atlas._Map[a, b].Count != 0)
                                {
                                    points.Add(new Points(ys + i, xs + j));
                                    if (points.Count == 10)
                                        return points;
                                }
                            }

                        }
                    }
                }
            }

            return points;
        }

        public List<ResContainer> FindBestRidesFromPos(int Y, int X, int CurrentTime, int TimeToEnd, Ride SkipRide, out int MinApproachDistance) 
        {
            List<ResContainer> Results = new List<ResContainer>();
            int ApproachTime = 0, RidesChecked = 0, temp, waitTime;
            MinApproachDistance = Program.Atlas.R + Program.Atlas.C;

            foreach (Points Point in Program._Points)
            {
                if (_Map[Point.Y, Point.X].Count != 0)
                {
                    foreach (Ride _ride in _Map[Point.Y, Point.X])
                    {
                        RidesChecked++;

                        if (_ride.Equals(SkipRide))
                            continue;

                        ApproachTime = Math.Abs(Y - Point.Y) + Math.Abs(X - Point.X);
                        temp = ApproachTime + CurrentTime;
                        waitTime = _ride.EarliestStart - temp;

                        if (ApproachTime < MinApproachDistance)
                            MinApproachDistance = ApproachTime;

                        if (waitTime < 0)
                            waitTime = 0;

                        if (_ride.LatestFinish <= CurrentTime + ApproachTime + _ride.Distance + waitTime)
                            continue;

                        if (ApproachTime >= TimeToEnd)
                            goto AfterLoop;

                        if (TimeToEnd >= ApproachTime + _ride.Distance)
                        {
                            
                            if (temp == _ride.EarliestStart)
                                Results.Add(new ResContainer(ApproachTime, _ride, true, Point, 0));
                            else if(temp < _ride.EarliestStart && _ride.EarliestStart - Program.B >= temp)
                                Results.Add(new ResContainer(ApproachTime, _ride, true, Point, waitTime));
                            else
                                Results.Add(new ResContainer(ApproachTime, _ride, false, Point, 0));
                        }
                            
                        if(RidesChecked >= Program.N)
                        {
                            goto AfterLoop;
                        }
                    }
                }
            }

            AfterLoop:

            Results.Sort((a, b) => b.TimeProportion.CompareTo(a.TimeProportion));

            if (Results.Count < 6)
                return Results;

            return new List<ResContainer> { Results[0], Results[1], Results[2], Results[3], Results[4] };

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
            int MinimumReturnDistance;

            Program._Points.Sort((a, b) => (a.Y + a.X).CompareTo(b.Y + b.X));

            foreach (Car c in Program.Cars)
            {
                //Console.WriteLine("##### Debug #####\nFinding {0} route to car{1}\n", c.Rides.Count + 1, c.ID);

                PossibleRides = FindBestRidesFromPos(c.Y, c.X, c.CarTime, Program.T - c.CarTime, null, out MinimumReturnDistance);

                if (PossibleRides.Count == 0)
                {
                    c.EndRun();

                    continue;
                }

                BestPossibleRides = new List<ResContainer>();

                foreach (ResContainer _ResCont in PossibleRides)
                {
                    tmp = c.CarTime + _ResCont.TimeOfDrive;
                    PossibleRidesFromAnotherPossibleRide = FindBestRidesFromPos(_ResCont._Ride.EndY, _ResCont._Ride.EndX, tmp, Program.T - tmp, _ResCont._Ride, out MinimumReturnDistance);

                    if (PossibleRidesFromAnotherPossibleRide.Count != 0)
                    {
                        BestPossibleRides.Add(new ResContainer(_ResCont, MinimumReturnDistance));
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
            //Stopwatch watch = new Stopwatch();
            GenerateRouteFirstTime();

            List<ResContainer> PossibleRides, PossibleRidesFromAnotherPossibleRide, BestPossibleRides;
            int tmp, MinimumReturnDistance; 
            
            while(Program.CheckIfAnyCar() && !CheckIfEmpty())
            {
                if (GetNextCar(out Car c))
                {
                    //Console.WriteLine("##### Debug #####\nFinding {0} route to car{1}\n", c.Rides.Count + 1, c.ID);

                    Program._Points.Sort((a, b) => (Math.Abs(c.Y - a.Y) + Math.Abs(c.X - a.X)).CompareTo(Math.Abs(c.Y - b.Y) + Math.Abs(c.X - b.X)));

                    PossibleRides = FindBestRidesFromPos(c.Y, c.X, c.CarTime, Program.T - c.CarTime, null, out MinimumReturnDistance);
                        
                    if(PossibleRides.Count == 0)
                    {
                        c.EndRun();
                        
                        continue;
                    }

                    BestPossibleRides = new List<ResContainer>();

                    foreach (ResContainer _ResCont in PossibleRides)
                    {
                        tmp = c.CarTime + _ResCont.TimeOfDrive;
                        PossibleRidesFromAnotherPossibleRide = FindBestRidesFromPos(_ResCont._Ride.EndY, _ResCont._Ride.EndX, tmp, Program.T - tmp, _ResCont._Ride, out MinimumReturnDistance);

                        if(PossibleRidesFromAnotherPossibleRide.Count != 0)
                        {
                            BestPossibleRides.Add(new ResContainer(_ResCont, MinimumReturnDistance));
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

                //watch.Start();
                Program.Cars.Sort((a, b) => a.CarTime.CompareTo(b.CarTime));
                //watch.Stop();

                //Console.WriteLine("Czas sortowania: {0}ms", watch.ElapsedMilliseconds);
                //Console.ReadKey();
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
        public static List<Points> _Points = new List<Points>();

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

                _Points.Add(new Points(y, x));
                Atlas._Map[y, x].Add(new Ride(y, x, Int32.Parse(parameters[2]), Int32.Parse(parameters[3]), Int32.Parse(parameters[4]), Int32.Parse(parameters[5])));
            }

            return true;
        }
    }
}
