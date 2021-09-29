using System;
using System.Net.Sockets;

public class Config
{

    private int sizeX = 0;
    private int sizeY = 0;
    private int sizeZ = 0;


    public ENeighbourhood Neighbourhood = ENeighbourhood.Moore;
    public ENucleation Nucleation = ENucleation.Homogeneus;
    public EBoundaryCondition BoundaryCondition = EBoundaryCondition.Nonperiodic;
    public ESimulation Simulation = ESimulation.CA;


    public int SizeX { get { return sizeX; } }
    public int SizeY { get { return sizeY; } }
    public int SizeZ { get { return sizeZ; } }

    public bool BreakSimulation = false;

    public string FileName = "micro.txt";
    public string FilePath = @"C:\Users\Marcin\Source\Repos\KISiM-AGH\projekt-zaliczeniowy-marm007\GrainGrowthServer\GrainGrowthServer\data\";
    public string XMLPath = @"C:\Users\Marcin\Source\Repos\KISiM-AGH\projekt-zaliczeniowy-marm007\GrainGrowthServer\GrainGrowthServer\Data.xml";

    public int NumberOfIterations = 0;
    public int NumberOfNucleons = 1;
    public readonly int NumberOfMCThreads = 4;
    public float KT = 0.0f;
    public float J = 0.0f;

    public enum ENeighbourhood { vonNeumann, Moore };
    public enum ENucleation { Homogeneus, Radial, Random, Banned};
    public enum EBoundaryCondition { Periodic, Nonperiodic };
    public enum EGridState { Enable = 1, Disable = 0 };
    public enum EEnergyState { Enable, Disable };
    public enum ESimulation { CA, MonteCarlo };
    

    public Config()
    {

    }

    public void SetSizes(int sX, int sY, int sZ)
    {
        sizeX = sX;
        sizeY = sY;
        sizeZ = sZ;
    }


}
