
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

    public void SimulateP()
    {
        ConcurrentBag<Grain> list = new ConcurrentBag<Grain>();
        bool simulationEnded = false;

        while (!simulationEnded)
        {
            simulationEnded = true;
            list.Clear();
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

           // if (list.Count == 0)
           //     break;

            foreach (Grain grain in list)
            {
                grain.State = grain.PrevState;
            }
        }

    }
}
