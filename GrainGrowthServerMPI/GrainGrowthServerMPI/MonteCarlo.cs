using GrainGrowthServer;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class MonteCarlo
{

    private static Random random = new Random();

    private float J = 1.0f;
    private float kT = 0.1f;
    private int NumberOfIterations = 0;


    public Grain[,,] grains { get; private set; }
    public Stack grainsStack { get; private set; }

    static object lockObj = new object();

    public MonteCarlo(Config config)
    {
        this.NumberOfIterations = config.NumberOfIterations;
        this.kT = config.KT;
        this.J = config.J;

        this.grainsStack = new Stack();
        this.grains = new Grain[config.SizeX, config.SizeY, config.SizeZ];

        for (int i = 0; i < config.SizeX; i++)
        {
            for (int j = 0; j < config.SizeY; j++)
            {
                for (int k = 0; k < config.SizeZ; k++)
                {
                    grains[i, j, k] = new Grain(i, j, k, 0);
                }
            }
        }

        int number = config.NumberOfNucleons;


        Nucleation.RandomMC(this.grains, number, config);

        for (int i = 0; i < config.SizeX; i++)
        {
            for (int j = 0; j < config.SizeY; j++)
            {
                for (int k = 0; k < config.SizeZ; k++)
                {
                    Grain grain = this.grains[i, j, k];

                    this.grains[i, j, k].Neighbours = NeighbourhoodFactory.GetNeighboursGrains(grains, grain, config);
                    this.grainsStack.Push(this.grains[i, j, k]);
                }
            }
        }
    }

    public MonteCarlo(SimulationParams simulationParams, Config config)
    {
        this.NumberOfIterations = config.NumberOfIterations;
        this.kT = config.KT;
        this.J = config.J;
        this.grainsStack = new Stack();
        this.grains = new Grain[config.SizeX, config.SizeY, config.SizeZ];

        for (int i = 0, xI = simulationParams.XStart; i < config.SizeX; i++)
        {
            for (int j = 0; j < config.SizeY; j++)
            {
                for (int k = 0; k < config.SizeZ; k++)
                {
                    if (i == 0)
                    {
                        if (simulationParams.XStart == 0)
                        {
                            grains[i, j, k] = new Grain(simulationParams.AllSizeX - 1, j, k, 0);
                        }
                        else
                        {
                            grains[i, j, k] = new Grain(simulationParams.XStart - 1, j, k, 0);
                        }
                    }
                    else if (i == config.SizeX - 1)
                    {
                        if (simulationParams.XEnd == simulationParams.AllSizeX)
                        {
                            grains[i, j, k] = new Grain(0, j, k, 0);
                        }
                        else
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

            if (i > 0)
                xI++;
        }

        for (int i = 1; i < config.SizeX - 1; i++)
        {
            for (int j = 0; j < config.SizeY; j++)
            {
                for (int k = 0; k < config.SizeZ; k++)
                {
                    this.grains[i, j, k].Neighbours = NeighbourhoodFactory.GetNeighboursGrains(grains, i, j, k, config);
                    this.grainsStack.Push(this.grains[i, j, k]);
                }
            }
        }

        int number = config.NumberOfNucleons;

        Nucleation.RandomMC(this.grainsStack, number, config);
    }

    public void Simulate()
    {

        int currentIteration = 0;

        while (currentIteration < NumberOfIterations)
        {

            //List<Grain> all = grains.OfType<Grain>().ToList();

            //List<int> allIndexes = Enumerable.Range(0, all.Count).ToList();
            var all = this.grainsStack.Shuffle(); // new Stack<Grain>(grains.OfType<Grain>().ToList().OrderBy(x => Guid.NewGuid()).ToList());

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

    public void SimulateParallel()
    {

        int currentIteration = 0;


        while (currentIteration < NumberOfIterations)
        {
            //List<Grain> all = grains.OfType<Grain>().ToList();

            //List<int> allIndexes = Enumerable.Range(0, all.Count).ToList();
            List<Stack> all = this.grainsStack.ShuffleList(4); // new Stack<Grain>(grains.OfType<Grain>().ToList().OrderBy(x => Guid.NewGuid()).ToList());
            List<Task> tasks = new List<Task>();
            foreach(Stack s in all)
            {
                tasks.Add(Task.Run(() => SimulateTask(s)));
            }

            Task.WaitAll(tasks.ToArray());

            currentIteration++;
        }
    }

    public List<List<SGrain>> SimulateMPI(SimulationParams simulationParams)
    {
        var all = this.grainsStack.Shuffle();

        List<SGrain> grainsLeft = new List<SGrain>();
        List<SGrain> grainsRight = new List<SGrain>();

        bool hasChanged = false;

        while (all.Count > 0)
        {
            hasChanged = false;

            Grain grain = (Grain)all.Pop();

            int stateBefore = grain.State;

            List<int> neighbours = grain.Neighbours.Select(g => g.State).ToList();

            int energyBefore = CalculateEnergy(neighbours, stateBefore);

            grain.Q = energyBefore;

            int stateAfter = (neighbours.ElementAt(random.Next(neighbours.Count)));

            int energyAfter = CalculateEnergy(neighbours, stateAfter);

            int deltaEnergy = energyAfter - energyBefore;

            if (deltaEnergy <= 0)
            {
                hasChanged = true;
                grain.State = stateAfter;
                grain.Q = energyAfter;
            }
            else
            {
                float probability = (float)Math.Exp(-(deltaEnergy / this.kT)) * 100;
                float value = (float)random.NextDouble() * 100;

                if (value <= probability)
                {
                    hasChanged = true;
                    grain.State = stateAfter;
                    grain.Q = energyAfter;
                }
            }

            if(grain.IsOnBorder && hasChanged)
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

        return new List<List<SGrain>>() { grainsLeft, grainsRight };
    }

    private int CalculateEnergy(List<int> neighbours, int state)
    {
        return (int)(this.J) * neighbours.Where(s => s != state).ToList().Count;
    }

    private void SimulateTask(Stack all)
    {
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
    }

}
