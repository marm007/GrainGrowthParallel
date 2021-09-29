using System;
using System.Diagnostics;
using static Config;

namespace GrainGrowthServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Config config = new Config();
            File file = new File();
            if (args.Length > 0)
            {
                config.XMLPath = args[0];
            }

            bool isParallelSimulation = false;

            if(args.Length == 2)
            {
                isParallelSimulation = args[1] == "p" ? true : false;
            }

            file.Deserialize(config);

            string preparingTime = "";
            string simulationTime = "";
            string writingToFileTime = "";

            if (config.Simulation == ESimulation.CA)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                CA cA = new CA(config);

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                preparingTime = ts.TotalSeconds.ToString();
                //preparingTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                //ts.Hours, ts.Minutes, ts.Seconds,
                //ts.Milliseconds / 10);

                stopwatch.Reset();
                stopwatch.Start();

                if (isParallelSimulation)
                    cA.SimulateP();
                else
                    cA.Simulate();

                stopwatch.Stop();
                ts = stopwatch.Elapsed;
                simulationTime = ts.TotalSeconds.ToString();

                stopwatch.Reset();
                stopwatch.Start();

               string filePath = file.Serialize(cA.grains, "ca", config);

                stopwatch.Stop();
                ts = stopwatch.Elapsed;
                writingToFileTime = ts.TotalSeconds.ToString();

                Console.WriteLine(filePath);
            } else if(config.Simulation == ESimulation.MonteCarlo)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();


                MonteCarlo monteCarlo = new MonteCarlo(config, isParallelSimulation);

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                preparingTime = ts.TotalSeconds.ToString();


                stopwatch.Reset();
                stopwatch.Start();

                if(isParallelSimulation)
                    monteCarlo.SimulateP();
                else
                    monteCarlo.Simulate();

                stopwatch.Stop();
                ts = stopwatch.Elapsed;
                simulationTime = ts.TotalSeconds.ToString();


                stopwatch.Reset();
                stopwatch.Start();

                string filePath = file.Serialize(monteCarlo.grains, "mc", config);

                stopwatch.Stop();
                ts = stopwatch.Elapsed;
                writingToFileTime = ts.TotalSeconds.ToString();

                Console.WriteLine(filePath);
            }

            Console.WriteLine(preparingTime);
            Console.WriteLine(simulationTime);
            Console.WriteLine(writingToFileTime);
            if (args.Length == 0)
                Console.ReadKey();
        }

    }
}
