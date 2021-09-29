
using GrainGrowthServer;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Config;

public class CA
{

    public Grain[,,] grains { get; private set; }

    public List<Grain> mGrains;

    private int SizeX;
    private int SizeY;
    private int SizeZ;

    public CA(Config config)
    {
        this.SizeX = config.SizeX;
        this.SizeY = config.SizeY;
        this.SizeZ = config.SizeZ;

        this.mGrains = new List<Grain>();
        this.grains = new Grain[SizeX, SizeY, SizeZ];

        for (int i = 0; i < SizeX; i++)
        {
            for (int j = 0; j < SizeY; j++)
            {
                for (int k = 0; k < SizeZ; k++)
                {
                    grains[i, j, k] = new Grain(i, j, k, 0);
                }
            }
        }

        int number = config.NumberOfNucleons;


        Nucleation.Random(this.grains, number, config);

        for (int i = 0; i < SizeX; i++)
        {
            for (int j = 0; j < SizeY; j++)
            {
                for (int k = 0; k < SizeZ; k++)
                {
                    Grain grain = this.grains[i, j, k];

                    this.grains[i, j, k].Neighbours = NeighbourhoodFactory.GetNeighboursGrains(grains, grain, config);
                    this.mGrains.Add(this.grains[i, j, k]);
                }
            }
        }
    }

    public CA(SimulationParams simulationParams, List<SGrain> sGrains, Config config)
    {
        this.SizeX = simulationParams.XEnd - simulationParams.XStart + 2;
        this.SizeY = simulationParams.AllSizeY;
        this.SizeZ = simulationParams.AllSizeZ;

        this.mGrains = new List<Grain>();
        this.grains = new Grain[SizeX, SizeY, SizeZ];
        
        for (int i = 0, xI = simulationParams.XStart; i < this.SizeX; i++)
        {
            for (int j = 0; j < SizeY ; j++)
            {
                for (int k = 0; k < SizeZ; k++)
                {
                    if(i == 0)
                    {
                        if(simulationParams.XStart == 0)
                        {
                            grains[i, j, k] = new Grain(simulationParams.AllSizeX - 1, j, k, 0);
                        }
                        else
                        {
                            grains[i, j, k] = new Grain(simulationParams.XStart - 1, j, k, 0);
                        }
                    }
                    else if(i == this.SizeX - 1)
                    {
                        if(simulationParams.XEnd == simulationParams.AllSizeX)
                        {
                            grains[i, j, k] = new Grain(0, j, k, 0);
                        }else
                        {
                            grains[i, j, k] = new Grain(simulationParams.XEnd, j, k, 0);
                        }
                    }
                    else
                    {
                        grains[i, j, k] = new Grain(xI, j, k, 0);
                        if (xI == simulationParams.XStart || xI == simulationParams.XEnd - 1)
                            grains[i, j, k].IsOnBorder = true;
                    }
                }
            }

            if(i > 0)
                xI++;
        }



        for (int i = 0; i < SizeX; i++)
        {
            for (int j = 0; j < SizeY; j++)
            {
                for (int k = 0; k < SizeZ; k++)
                {
                    Grain grain = this.grains[i, j, k];

                    SGrain foundSGrain = sGrains.Find(delegate(SGrain sg)
                    {
                        return sg.X == grain.X && sg.Y == grain.Y && sg.Z == grain.Z;
                    });

                    if (foundSGrain.State != 0)
                    {
                        this.grains[i, j, k].State = foundSGrain.State;
                    }
                    this.grains[i, j, k].Neighbours = NeighbourhoodFactory.GetNeighboursGrains(grains, i, j, k, config);
                    if (i != 0 && i != config.SizeX - 1)
                        this.mGrains.Add(this.grains[i, j, k]);
                }
            }
        }
    }


    public void Simulate()
    {
        bool simulationEnded = false;

        List<Grain> list = new List<Grain>();

        while (!simulationEnded)
        {
            simulationEnded = true;

            list.Clear();

            for (int i = 0; i < SizeX; i++)
            {
                for (int j = 0; j < SizeY; j++)
                {
                    for (int k = 0; k < SizeZ; k++)
                    {

                        if (grains[i, j, k].State == 0)
                        {
                            simulationEnded = false;

                            int cellEnd = NeighbourhoodFactory.GetState(this.grains[i, j, k]);

                            if (cellEnd != grains[i, j, k].State)
                            {
                                this.grains[i, j, k].PrevState = cellEnd;
                                list.Add(this.grains[i, j, k]);
                            }
                        }
                    }
                }
            }

            foreach (Grain grain in list)
            {
                grain.State = grain.PrevState;
            }
        }

    }

    public void SimulateParallel()
    {
        ConcurrentBag<Grain> list = new ConcurrentBag<Grain>();
        bool simulationEnded = false;

        while (simulationEnded)
        {
            list.Clear();

            simulationEnded = true;

            Parallel.ForEach(mGrains,
                g =>
                {
                    if (g.State == 0)
                    {
                        simulationEnded = false;

                        int cellEnd = NeighbourhoodFactory.GetState(g);

                        if (cellEnd != g.State)
                        {
                            g.PrevState = cellEnd;
                            list.Add(g);
                        }
                    }
                }
            );

            foreach (Grain grain in list)
            {
                grain.State = grain.PrevState;
            }
        }

    }

    public List<List<SGrain>> SimulateMPI(out bool simulationEnded)
    {
        simulationEnded = true;

        List<SGrain> grainsLeft = new List<SGrain>();
        List<SGrain> grainsRight = new List<SGrain>();

        List<Grain> list = new List<Grain>();

        for (int i = 1; i < SizeX - 1; i++)
        {
            for (int j = 0; j < SizeY; j++)
            {
                for (int k = 0; k < SizeZ; k++)
                {

                    if (grains[i, j, k].State == 0)
                    {
                        simulationEnded = false;
                        int cellEnd = NeighbourhoodFactory.GetState(this.grains[i, j, k]);

                        if (cellEnd != grains[i, j, k].State)
                        {
                            this.grains[i, j, k].PrevState = cellEnd;
                            if (this.grains[i, j, k].IsOnBorder)
                            {
                                if (i == 1)
                                {
                                    grainsLeft.Add(new SGrain
                                    {
                                        X = this.grains[i, j, k].X,
                                        Y = this.grains[i, j, k].Y,
                                        Z = this.grains[i, j, k].Z,
                                        State = this.grains[i, j, k].PrevState,
                                    });
                                }
                                else if (i == SizeX - 2)
                                {
                                    grainsRight.Add(new SGrain
                                    {
                                        X = this.grains[i, j, k].X,
                                        Y = this.grains[i, j, k].Y,
                                        Z = this.grains[i, j, k].Z,
                                        State = this.grains[i, j, k].PrevState,
                                    });
                                }
                            }
                            list.Add(this.grains[i, j, k]);
                        }
                    }
                }
            }
        }

        foreach (Grain grain in list)
        {
            grain.State = grain.PrevState;
        }

        return new List<List<SGrain>>() { grainsLeft, grainsRight };
    }
}
