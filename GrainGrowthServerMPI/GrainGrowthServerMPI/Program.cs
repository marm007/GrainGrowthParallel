using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using static Config;

namespace GrainGrowthServer
{
    [Serializable]
    public struct SimulationParams
    {
        public SimulationParams(Config config, int xStart, int xEnd)
        {
            this.XStart = xStart;
            this.XEnd = xEnd;

            this.Neighbourhood = config.Neighbourhood.ToString();
            this.BoundaryCondition = config.BoundaryCondition.ToString();
            this.AllSizeX = config.SizeX;
            this.AllSizeY = config.SizeY;
            this.AllSizeZ = config.SizeZ;
            this.KT = config.KT;
            this.J = config.J;
            this.Simulation = config.Simulation.ToString();
            this.NumberOfNucleons = config.NumberOfNucleons;
            this.NumberOfIterations = config.NumberOfIterations;
        }

        public string Simulation;

        public int NumberOfNucleons;
        public int NumberOfIterations;
        public int XStart;
        public int XEnd;

        public string Neighbourhood;
        public string BoundaryCondition;

        public int AllSizeX;
        public int AllSizeY;
        public int AllSizeZ;

        public float KT;
        public float J;
    }

    class Program
    {
        
        static void Main(string[] args)
        {

            MPI.Environment.Run(ref args, comm =>
            {

                if (comm.Rank == 0)
                {

                    string preparingTime = "";
                    string simulationTime = "";
                    string writingToFileTime = "";
                    string filePath = "";

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    Config config = new Config();
                    config.NumberOfProcesses = comm.Size - 1;

                    File file = new File();

                    if (args.Length > 0)
                    {
                        config.XMLPath = args[0];
                    }

                    file.Deserialize(config);

                    if (config.Simulation == ESimulation.CA)
                    {

                        int number = config.NumberOfNucleons;

                        List<SGrain> nucleons = Nucleation.Random(number, config);

                        int maxSize = config.SizeX / config.NumberOfProcesses;

                        for (int dest = 1, index = 0; dest < comm.Size; ++dest, ++index)
                        {
                            List<SGrain> grains = new List<SGrain>();
                            foreach (SGrain g in nucleons)
                            {
                                if (dest == comm.Size - 1)
                                {
                                    if (g.X >= maxSize * index && g.X < config.SizeX)
                                    {
                                        grains.Add(g);
                                    }
                                }
                                else
                                {
                                    if (g.X >= maxSize * index && g.X < maxSize * dest)
                                    {
                                        grains.Add(g);
                                    }
                                }

                            }

                            if (dest == comm.Size - 1)
                            {
                                SimulationParams simulationParams = new SimulationParams(config, maxSize * index, config.SizeX);
                                comm.Send(simulationParams, dest, SIMULATION_PARAMS_TAG);

                            }
                            else
                            {
                                SimulationParams simulationParams = new SimulationParams(config, maxSize * index, maxSize * dest);
                                comm.Send(simulationParams, dest, SIMULATION_PARAMS_TAG);

                            }

                            comm.Send(grains, dest, NUCLEONS_TAG);
                        }
                    }
                    else
                    {
                        int maxSize = config.SizeX / config.NumberOfProcesses;

                        for (int dest = 1, index = 0; dest < comm.Size; ++dest, ++index)
                        {

                            if (dest == comm.Size - 1)
                            {
                                SimulationParams simulationParams = new SimulationParams(config, maxSize * index, config.SizeX);
                                comm.Send(simulationParams, dest, SIMULATION_PARAMS_TAG);

                            }
                            else
                            {
                                SimulationParams simulationParams = new SimulationParams(config, maxSize * index, maxSize * dest);
                                comm.Send(simulationParams, dest, SIMULATION_PARAMS_TAG);

                            }
                        }
                    }

                    comm.Barrier();


                    stopwatch.Stop();
                    TimeSpan ts = stopwatch.Elapsed;
                    preparingTime = ts.TotalSeconds.ToString();

                    stopwatch.Reset();
                    stopwatch.Start();

                    int finishedCounter = 0;
                    while (finishedCounter != comm.Size - 1)
                    {
                        for (int dest = 1; dest < comm.Size; ++dest)
                        {
                            if (comm.ImmediateProbe(dest, END_SIMULATION_TAG) != null)
                            {
                                string message = comm.Receive<string>(dest, END_SIMULATION_TAG);
                                finishedCounter++;
                            }

                        }


                        for (int dest = 1; dest < comm.Size; ++dest)
                        {
                            if (finishedCounter == comm.Size - 1)
                            {
                                comm.Send("Finish", dest, END_MESSAGE_TAG);
                            }
                        }
                    }

                    stopwatch.Stop();
                    ts = stopwatch.Elapsed;
                    simulationTime = ts.TotalSeconds.ToString();


                    stopwatch.Reset();
                    stopwatch.Start();

                    List<System.IO.StreamReader> fileNames = new List<System.IO.StreamReader>();

                    for (int dest = 1; dest < comm.Size; ++dest)
                    {
                        fileNames.Add(new System.IO.StreamReader(comm.Receive<string>(dest, THREAD_FILE_NAME_TAG)));
                    }

                    filePath = file.Serialize(fileNames, config);

                    stopwatch.Stop();
                    ts = stopwatch.Elapsed;
                    writingToFileTime = ts.TotalSeconds.ToString();

                    Console.WriteLine(filePath);
                    Console.WriteLine(preparingTime);
                    Console.WriteLine(simulationTime);
                    Console.WriteLine(writingToFileTime);
                }
                else
                {
                    SimulationParams simulationParams = comm.Receive<SimulationParams>(0, SIMULATION_PARAMS_TAG);
                    Config config = new Config();
                    config.SetSizes(simulationParams.XEnd - simulationParams.XStart + 2, simulationParams.AllSizeY, simulationParams.AllSizeZ);
                    config.Neighbourhood = simulationParams.Neighbourhood == "Moore" ? ENeighbourhood.Moore : ENeighbourhood.vonNeumann;
                    config.BoundaryCondition = simulationParams.BoundaryCondition == "Periodic" ? EBoundaryCondition.Periodic : EBoundaryCondition.Nonperiodic;
                    config.IsMPI = true;
                    config.Simulation = simulationParams.Simulation == "CA" ? ESimulation.CA : ESimulation.MonteCarlo;
                    config.NumberOfNucleons = simulationParams.NumberOfNucleons;
                    config.NumberOfIterations = simulationParams.NumberOfIterations;
                    config.KT = simulationParams.KT;
                    config.J = simulationParams.J;


                    MPIFactory factory = new MPIFactory();

                    switch(config.Simulation)
                    {
                        case ESimulation.CA:
                            factory.PerformCA(comm, config, simulationParams);
                            break;
                        case ESimulation.MonteCarlo:
                            factory.PerformMC(comm, config, simulationParams);
                            break;
                    }
                }
            });

        }

    }
}
