using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Config;

namespace GrainGrowthServer
{
    class MPIFactory
    {

        public void PerformMC(MPI.Communicator comm, Config config, SimulationParams simulationParams)
        {
        
            MonteCarlo monteCarlo = new MonteCarlo(simulationParams, config);

            comm.Barrier();

            bool simulationEnded = false;

            int currentIteration = config.Simulation == ESimulation.CA ? -100 : 0;

            // exchange border cells at the beggining

            {
                List<SGrain> grainsLeft = new List<SGrain>();
                List<SGrain> grainsRight = new List<SGrain>();

                foreach (Grain grain in monteCarlo.grainsStack)
                {
                    if (grain.IsOnBorder)
                    {
                        if (grain.X == simulationParams.XStart)
                        {
                            grainsLeft.Add(new SGrain
                            {
                                X = grain.X,
                                Y = grain.Y,
                                Z = grain.Z,
                                State = grain.State,
                            });
                        }
                        else if (grain.X == simulationParams.XEnd - 1)
                        {
                            grainsRight.Add(new SGrain
                            {
                                X = grain.X,
                                Y = grain.Y,
                                Z = grain.Z,
                                State = grain.State,
                            });
                        }
                    }
                }

                MPI.Request left = null;
                MPI.Request right = null;

                if (comm.Rank != 1 && comm.Rank != comm.Size - 1)
                {
                    left = comm.ImmediateSend<List<SGrain>>(grainsLeft, comm.Rank - 1, EXCHANGE_BORDER_LEFT_CELLS_TAG);
                    right = comm.ImmediateSend<List<SGrain>>(grainsRight, comm.Rank + 1, EXCHANGE_BORDER_RIGHT_CELLS_TAG);

                }
                else if (comm.Rank == 1)
                {
                    left = comm.ImmediateSend<List<SGrain>>(grainsLeft, comm.Size - 1, EXCHANGE_BORDER_LEFT_CELLS_TAG);
                    right = comm.ImmediateSend<List<SGrain>>(grainsRight, comm.Rank + 1, EXCHANGE_BORDER_RIGHT_CELLS_TAG);
                }
                else if (comm.Rank == comm.Size - 1)
                {
                    left = comm.ImmediateSend<List<SGrain>>(grainsLeft, comm.Rank - 1, EXCHANGE_BORDER_LEFT_CELLS_TAG);
                    right = comm.ImmediateSend<List<SGrain>>(grainsRight, 1, EXCHANGE_BORDER_RIGHT_CELLS_TAG);
                }

                List<SGrain> grainsOnBorderLeft = null;
                List<SGrain> grainsOnBorderRight = null;

                if (comm.Rank != 1 && comm.Rank != comm.Size - 1)
                {
                    grainsOnBorderLeft = comm.Receive<List<SGrain>>(comm.Rank - 1, EXCHANGE_BORDER_RIGHT_CELLS_TAG);
                    grainsOnBorderRight = comm.Receive<List<SGrain>>(comm.Rank + 1, EXCHANGE_BORDER_LEFT_CELLS_TAG);

                }
                else if (comm.Rank == 1)
                {
                    grainsOnBorderLeft = comm.Receive<List<SGrain>>(comm.Size - 1, EXCHANGE_BORDER_RIGHT_CELLS_TAG);
                    grainsOnBorderRight = comm.Receive<List<SGrain>>(comm.Rank + 1, EXCHANGE_BORDER_LEFT_CELLS_TAG);
                }
                else if (comm.Rank == comm.Size - 1)
                {
                    grainsOnBorderLeft = comm.Receive<List<SGrain>>(comm.Rank - 1, EXCHANGE_BORDER_RIGHT_CELLS_TAG);
                    grainsOnBorderRight = comm.Receive<List<SGrain>>(1, EXCHANGE_BORDER_LEFT_CELLS_TAG);
                }

                left.Wait();
                right.Wait();
                int cc = 0;

                if (grainsOnBorderLeft != null && grainsOnBorderRight != null)
                {
                    for (int j = 0; j < config.SizeY; j++)
                    {
                        for (int k = 0; k < config.SizeZ; k++)
                        {
                            Grain grainLeft = monteCarlo.grains[0, j, k];
                            Grain grainRight = monteCarlo.grains[config.SizeX - 1, j, k];

                            SGrain foundSGrainL = grainsOnBorderLeft.Find(delegate (SGrain sg)
                            {
                                return sg.X == grainLeft.X && sg.Y == grainLeft.Y && sg.Z == grainLeft.Z;
                            });

                            SGrain foundSGrainR = grainsOnBorderRight.Find(delegate (SGrain sg)
                            {
                                return sg.X == grainRight.X && sg.Y == grainRight.Y && sg.Z == grainRight.Z;
                            });


                            if (foundSGrainL.State != 0 && foundSGrainR.State != 0 && foundSGrainL.X == foundSGrainR.X && foundSGrainL.Y == foundSGrainR.Y && foundSGrainL.Z == foundSGrainR.Z)
                            {
                                Console.WriteLine("Blad");
                            }
                            else 
                            {
                                if (foundSGrainL.State != 0)
                                {
                                    cc++;
                                    monteCarlo.grains[0, j, k].State = foundSGrainL.State;
                                }

                                if (foundSGrainR.State != 0)
                                {
                                    monteCarlo.grains[config.SizeX - 1, j, k].State = foundSGrainR.State;
                                }
                            }
                        }
                    }

                }
               // Console.WriteLine(cc);

            }

            while (!simulationEnded)
            {
                List<List<SGrain>> list = null;

                list = monteCarlo.SimulateMPI(simulationParams);
                currentIteration++;


                MPI.Request left = null;
                MPI.Request right = null;

                if (comm.Rank != 1 && comm.Rank != comm.Size - 1)
                {
                    left = comm.ImmediateSend<List<SGrain>>(list[0], comm.Rank - 1, EXCHANGE_BORDER_LEFT_CELLS_TAG);
                    right = comm.ImmediateSend<List<SGrain>>(list[1], comm.Rank + 1, EXCHANGE_BORDER_RIGHT_CELLS_TAG);

                }
                else if (comm.Rank == 1)
                {
                    left = comm.ImmediateSend<List<SGrain>>(list[0], comm.Size - 1, EXCHANGE_BORDER_LEFT_CELLS_TAG);
                    right = comm.ImmediateSend<List<SGrain>>(list[1], comm.Rank + 1, EXCHANGE_BORDER_RIGHT_CELLS_TAG);
                }
                else if (comm.Rank == comm.Size - 1)
                {
                    left = comm.ImmediateSend<List<SGrain>>(list[0], comm.Rank - 1, EXCHANGE_BORDER_LEFT_CELLS_TAG);
                    right = comm.ImmediateSend<List<SGrain>>(list[1], 1, EXCHANGE_BORDER_RIGHT_CELLS_TAG);
                }

                List<SGrain> grainsOnBorderLeft = null;
                List<SGrain> grainsOnBorderRight = null;

               
                if (comm.Rank != 1 && comm.Rank != comm.Size - 1)
                {
                    grainsOnBorderLeft = comm.Receive<List<SGrain>>(comm.Rank - 1, EXCHANGE_BORDER_RIGHT_CELLS_TAG);
                    grainsOnBorderRight = comm.Receive<List<SGrain>>(comm.Rank + 1, EXCHANGE_BORDER_LEFT_CELLS_TAG);

                }
                else if (comm.Rank == 1)
                {
                    grainsOnBorderLeft = comm.Receive<List<SGrain>>(comm.Size - 1, EXCHANGE_BORDER_RIGHT_CELLS_TAG);
                    grainsOnBorderRight = comm.Receive<List<SGrain>>(comm.Rank + 1, EXCHANGE_BORDER_LEFT_CELLS_TAG);
                }
                else if (comm.Rank == comm.Size - 1)
                {
                    grainsOnBorderLeft = comm.Receive<List<SGrain>>(comm.Rank - 1, EXCHANGE_BORDER_RIGHT_CELLS_TAG);
                    grainsOnBorderRight = comm.Receive<List<SGrain>>(1, EXCHANGE_BORDER_LEFT_CELLS_TAG);
                }

                left.Wait();
                right.Wait();

                if (grainsOnBorderLeft != null)
                    foreach (SGrain foundSGrainL in grainsOnBorderLeft)
                    {
                        Grain grainLeft = monteCarlo.grains[0, foundSGrainL.Y, foundSGrainL.Z];
                        grainLeft.State = foundSGrainL.State;
                    }

                if (grainsOnBorderRight != null)
                    foreach (SGrain foundSGrainR in grainsOnBorderRight)
                    {
                        Grain grainRight = monteCarlo.grains[config.SizeX - 1, foundSGrainR.Y, foundSGrainR.Z];
                        grainRight.State = foundSGrainR.State;

                    }

                if (currentIteration >= config.NumberOfIterations)
                    simulationEnded = true;

                if (simulationEnded)
                {
                    comm.Send("Simulation ended", 0, END_SIMULATION_TAG);
                }
            }

            File file = new File();
            String path = file.Serialize(monteCarlo.grains, simulationParams.Simulation.ToString(), config, simulationParams, comm.Rank, comm.Size);
            comm.Send(path, 0, THREAD_FILE_NAME_TAG);

        }

        public void PerformCA(MPI.Communicator comm, Config config, SimulationParams simulationParams)
        {

            List<SGrain> lGrains = comm.Receive<List<SGrain>>(0, 0);
            CA cA = new CA(simulationParams, lGrains, config);

            comm.Barrier();

            bool simulationEnded = false;


            while (!simulationEnded)
            {
                List<List<SGrain>> list = cA.SimulateMPI(out simulationEnded);

                if (comm.Size > 2)
                {
                    if (comm.Rank != 1 && comm.Rank != comm.Size - 1)
                    {
                        comm.Send<List<SGrain>>(list[0], comm.Rank - 1, EXCHANGE_BORDER_LEFT_CELLS_TAG);
                        comm.Send<List<SGrain>>(list[1], comm.Rank + 1, EXCHANGE_BORDER_RIGHT_CELLS_TAG);

                    }
                    else if (comm.Rank == 1)
                    {
                        comm.Send<List<SGrain>>(list[0], comm.Size - 1, EXCHANGE_BORDER_LEFT_CELLS_TAG);
                        comm.Send<List<SGrain>>(list[1], comm.Rank + 1, EXCHANGE_BORDER_RIGHT_CELLS_TAG);
                    }
                    else if (comm.Rank == comm.Size - 1)
                    {
                        comm.Send<List<SGrain>>(list[0], comm.Rank - 1, EXCHANGE_BORDER_LEFT_CELLS_TAG);
                        comm.Send<List<SGrain>>(list[1], 1, EXCHANGE_BORDER_RIGHT_CELLS_TAG);
                    }


                    List<SGrain> grainsOnBorderLeft = null;
                    List<SGrain> grainsOnBorderRight = null;

                    if (comm.Rank != 1 && comm.Rank != comm.Size - 1)
                    {
                        if (comm.ImmediateProbe(comm.Rank - 1, EXCHANGE_BORDER_RIGHT_CELLS_TAG) != null)
                            grainsOnBorderLeft = comm.Receive<List<SGrain>>(comm.Rank - 1, EXCHANGE_BORDER_RIGHT_CELLS_TAG);
                        if (comm.ImmediateProbe(comm.Rank + 1, EXCHANGE_BORDER_LEFT_CELLS_TAG) != null)
                            grainsOnBorderRight = comm.Receive<List<SGrain>>(comm.Rank + 1, EXCHANGE_BORDER_LEFT_CELLS_TAG);

                    }
                    else if (comm.Rank == 1)
                    {
                        if (comm.ImmediateProbe(comm.Size - 1, EXCHANGE_BORDER_RIGHT_CELLS_TAG) != null)
                            grainsOnBorderLeft = comm.Receive<List<SGrain>>(comm.Size - 1, EXCHANGE_BORDER_RIGHT_CELLS_TAG);
                        if (comm.ImmediateProbe(comm.Rank + 1, EXCHANGE_BORDER_LEFT_CELLS_TAG) != null)
                            grainsOnBorderRight = comm.Receive<List<SGrain>>(comm.Rank + 1, EXCHANGE_BORDER_LEFT_CELLS_TAG);
                    }
                    else if (comm.Rank == comm.Size - 1)
                    {
                        if (comm.ImmediateProbe(comm.Rank - 1, EXCHANGE_BORDER_RIGHT_CELLS_TAG) != null)
                            grainsOnBorderLeft = comm.Receive<List<SGrain>>(comm.Rank - 1, EXCHANGE_BORDER_RIGHT_CELLS_TAG);
                        if (comm.ImmediateProbe(1, EXCHANGE_BORDER_LEFT_CELLS_TAG) != null)
                            grainsOnBorderRight = comm.Receive<List<SGrain>>(1, EXCHANGE_BORDER_LEFT_CELLS_TAG);
                    }

                    if (grainsOnBorderLeft != null)
                        foreach (SGrain foundSGrainL in grainsOnBorderLeft)
                        {
                            Grain grainLeft = cA.grains[0, foundSGrainL.Y, foundSGrainL.Z];
                            grainLeft.State = foundSGrainL.State;
                        }

                    if (grainsOnBorderRight != null)
                        foreach (SGrain foundSGrainR in grainsOnBorderRight)
                        {
                            Grain grainRight = cA.grains[config.SizeX - 1, foundSGrainR.Y, foundSGrainR.Z];
                            grainRight.State = foundSGrainR.State;

                        }
                }

                if (simulationEnded)
                {
                    // File file1 = new File();
                    // file1.Serialize(cA.grains, simulationParams.Simulation.ToString(), config, simulationParams, comm.Rank, comm.Size, "test");
                    comm.Send("Simulation ended", 0, END_SIMULATION_TAG);
                }
            }


            File file = new File();
            String path = file.Serialize(cA.grains, simulationParams.Simulation.ToString(), config, simulationParams, comm.Rank, comm.Size);
            comm.Send(path, 0, THREAD_FILE_NAME_TAG);
        }
    }
}
