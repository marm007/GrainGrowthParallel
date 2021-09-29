using GrainGrowthServer;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class MonteCarlo
{

    private static Random random = new Random();

    private float J = 1.0f;
    private float kT = 0.1f;
    private int NumberOfIterations = 0;


    public Grain[,,] grains { get; private set; }
    private List<Stack> grainsStacks;

    private int numberOfThreads = 1;
    static object lockObj = new object();

    public MonteCarlo(Config config, bool isParallelSimulation)
    {
        this.NumberOfIterations = config.NumberOfIterations;
        this.kT = config.KT;
        this.J = config.J;

        this.grainsStacks = new List<Stack>();
        this.grains = new Grain[config.SizeX, config.SizeY, config.SizeZ];

        this.numberOfThreads = config.NumberOfMCThreads;

        
        int max = config.SizeX;
        int start = 0;
        int end = 0;

        if (isParallelSimulation)
        {
            max = config.SizeX / this.numberOfThreads - 1;
            start = 0;
            end = config.SizeX / this.numberOfThreads - 1;
        }


        for (int i = 0, index = 0; i < config.SizeX; i++)
        {
            for (int j = 0; j < config.SizeY; j++)
            {
                for (int k = 0; k < config.SizeZ; k++)
                {
                    grains[i, j, k] = new Grain(i, j, k, 0);

                    if(isParallelSimulation && this.numberOfThreads > 1)
                    {
                        if (i == start || i == end)
                        {
                            grains[i, j, k].IsOnBroder = true;
                           
                        }

                        if (config.BoundaryCondition == Config.EBoundaryCondition.Nonperiodic && (i == 0 || i == config.SizeX - 1))
                        {
                            grains[i, j, k].IsOnBroder = false;
                        } 
                    }
                }
            }

            if (i >= end)
            {

                ++index;
                start = end + 1;
                if (index == this.numberOfThreads)
                    end = config.SizeX - 1;
                else
                    end = (end + 1) * 2 - 1;
            }
        }

        int number = config.NumberOfNucleons;


        Nucleation.RandomMC(this.grains, number, config);

        if(isParallelSimulation)
        {

            for (int i = 0; i < this.numberOfThreads; i++)
                this.grainsStacks.Add(new Stack());
        } else
        {
            this.grainsStacks.Add(new Stack());
        }

        for (int i = 0, index = 0; i < config.SizeX; i++)
        {
            for (int j = 0; j < config.SizeY; j++)
            {
                for (int k = 0; k < config.SizeZ; k++)
                {
                    this.grains[i, j, k].Neighbours = NeighbourhoodFactory.GetNeighboursGrains(grains, this.grains[i, j, k], config);
                    this.grainsStacks[index].Push(this.grains[i, j, k]);
                }
            }

            if (i == (index + 1) * (max ))
            {
                max = (max + 1) * 2 - 1;
                index++;
            }
        }
    }

    private void SimulateTask(Stack all)
    {

        while (all.Count > 0)
        {
            
            Grain grain = (Grain)all.Pop();
            int stateBefore = grain.State;

            List<int> neighbours = grain.Neighbours.Select(g => g.State).ToList();
        
            int energyBefore = 0;
            int energyAfter = 0;
            int stateAfter = 0;

            lock (lockObj)
            {
                energyBefore = CalculateEnergy(neighbours, stateBefore);

                grain.Q = energyBefore;

                stateAfter = (neighbours.ElementAt(random.Next(neighbours.Count)));

                energyAfter = CalculateEnergy(neighbours, stateAfter);
                //energyAfter = (int)(this.J) * neighbours.Where(s => s != stateAfter).ToList().Count;
            }

            int deltaEnergy = energyAfter - energyBefore;
            if (deltaEnergy <= 0)
            {
                grain.State = stateAfter;
                grain.Q = energyAfter;
            }
            else
            {
                float probability = (float)Math.Exp(-(deltaEnergy / this.kT)) * 100;
                float value = (float)random.NextDouble() * 100;

                if (value <= probability)
                {
                    grain.State = stateAfter;
                    grain.Q = energyAfter;
                }
            }
        }
    }

    public void SimulateP()
    {

        int currentIteration = 0;


        while (currentIteration < NumberOfIterations)
        {
            //List<Grain> all = grains.OfType<Grain>().ToList();
            
            //List<int> allIndexes = Enumerable.Range(0, all.Count).ToList();
            // List<Stack> all = this.grainsStack.ShuffleList(); // new Stack<Grain>(grains.OfType<Grain>().ToList().OrderBy(x => Guid.NewGuid()).ToList());

            List<Task> tasks = new List<Task>();

            foreach(Stack s in grainsStacks)
            {
              tasks.Add(Task<Dictionary<Grain, int>>.Run(() => SimulateTask(s.Shuffle())));
            }
           

            Task.WaitAll(tasks.ToArray());
            currentIteration++;
        }
    }

    private int CalculateEnergy(List<int> neighbours, int state)
    {
        return (int)(this.J) * neighbours.Where(s => s != state).ToList().Count;
    }

    public void Simulate()
    {

        int currentIteration = 0;

        while (currentIteration < NumberOfIterations)
        {

            //List<Grain> all = grains.OfType<Grain>().ToList();

            //List<int> allIndexes = Enumerable.Range(0, all.Count).ToList();
            var all = this.grainsStacks[0].Shuffle(); // new Stack<Grain>(grains.OfType<Grain>().ToList().OrderBy(x => Guid.NewGuid()).ToList());

            while (all.Count > 0)
            {
                Grain grain = (Grain)all.Pop();

                //int index = random.Next(allIndexes.Count);
                //Grain grain = all.ElementAt(index);
                //allIndexes.RemoveAt(index);

                int stateBefore = grain.State;

                List<int> neighbours = grain.Neighbours.Select(g => g.State).ToList();

                //List<int> nonZeroNeighbours = new List<int>();

                //for (int i = 0; i < neighbours.Count; i++)
                //    if (neighbours[i].State != 0)
                //        nonZeroNeighbours.Add(neighbours[i].State);

                // if (nonZeroNeighbours.Count == 0)
                //     continue;

                int energyBefore = CalculateEnergy(neighbours, stateBefore);

                grain.Q = energyBefore;

                int stateAfter = (neighbours.ElementAt(random.Next(neighbours.Count)));

                int energyAfter = CalculateEnergy(neighbours, stateAfter);

                int deltaEnergy = energyAfter - energyBefore;
                if (deltaEnergy <= 0)
                {
                    grain.State = stateAfter;
                    grain.Q = energyAfter;

                }
                else
                {
                    float probability = (float)Math.Exp(-(deltaEnergy / this.kT)) * 100;
                    float value = (float)random.NextDouble() * 100;

                    if (value <= probability)
                    {
                        grain.State = stateAfter;
                        grain.Q = energyAfter;
                    }
                }
            }

            currentIteration++;
        }
    }
}
