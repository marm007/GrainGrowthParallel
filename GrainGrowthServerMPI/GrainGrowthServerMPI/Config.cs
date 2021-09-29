using System;
using System.Net.Sockets;

public class Config
{
    public static readonly int SIMULATION_PARAMS_TAG = 1;
    public static readonly int NUCLEONS_TAG = 0;
    public static readonly int END_MESSAGE_TAG = 2;
    public static readonly int END_SIMULATION_TAG = 3;
    public static readonly int EXCHANGE_BORDER_LEFT_CELLS_TAG = 23;
    public static readonly int EXCHANGE_BORDER_RIGHT_CELLS_TAG = 223;
    public static readonly int THREAD_FILE_NAME_TAG = 55;


    private int sizeX = 0;
    private int sizeY = 0;
    private int sizeZ = 0;

    public int NumberOfProcesses { get; set; }

    public ENeighbourhood Neighbourhood = ENeighbourhood.Moore;
    public ENucleation Nucleation = ENucleation.Homogeneus;
    public EBoundaryCondition BoundaryCondition = EBoundaryCondition.Nonperiodic;
    public ESimulation Simulation = ESimulation.CA;
    public bool IsMPI = false;

    public int SizeX { get { return sizeX; } }
    public int SizeY { get { return sizeY; } }
    public int SizeZ { get { return sizeZ; } }

    public bool BreakSimulation = false;

    public string FileName = "#0000.txt";
    public string FilePath = @"C:\Users\Marcin\Source\Repos\KISiM-AGH\projekt-zaliczeniowy-marm007\GrainGrowthServerMPI\GrainGrowthServerMPI\data\";
    public string XMLPath = @"C:\Users\Marcin\Source\Repos\KISiM-AGH\projekt-zaliczeniowy-marm007\GrainGrowthServerMPI\GrainGrowthServerMPI\Data.xml";

    public int NumberOfIterations = 0;
    public int NumberOfNucleons = 1;

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
