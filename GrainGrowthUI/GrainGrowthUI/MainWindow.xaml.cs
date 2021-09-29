using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Serialization;
using static GrainGrowthUI.Simulation;

namespace GrainGrowthUI
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int counter = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Export_To_CSV(object sender, RoutedEventArgs e)
        {
            var listView1 = SimulationListView;
            var output = new StringBuilder();
            output.Append("Id,");
            output.Append("SizeX ,");
            output.Append("SizeY ,");
            output.Append("SizeZ ,");
            output.Append("Neighbourhood ,");
            output.Append("BC ,");
            output.Append("Nucleons ,");
            output.Append("Simulation ,");
            output.Append("NumberOfIterations ,");
            output.Append("KT ,");
            output.Append("J ,");
            output.Append("PreparationTime ,");
            output.Append("SimulationTime ,");
            output.Append("WriteToFileTime ,");
            output.AppendLine();
  
            foreach (SimulationListItem item in listView1.Items)
            {
                output.AppendFormat("{0},", item.ID);
                output.AppendFormat("{0},", item.SizeX);
                output.AppendFormat("{0},", item.SizeY);
                output.AppendFormat("{0},", item.SizeZ);
                output.AppendFormat("{0},", item.Neighbourhood);
                output.AppendFormat("{0},", item.BC);
                output.AppendFormat("{0},", item.Nucleons);
                output.AppendFormat("{0},", item.Simulation);
                output.AppendFormat("{0},", item.NumberOfIterations);
                output.AppendFormat("{0},", item.KT);
                output.AppendFormat("{0},", item.J);
                output.AppendFormat("{0},", item.PreparationTime.Replace(',', '.'));
                output.AppendFormat("{0},", item.SimulationTime.Replace(',', '.'));
                output.AppendFormat("{0},", item.WriteToFileTime.Replace(',', '.'));
                output.AppendLine();
            }

            string pVersion = NormalRadioButton.IsChecked == true ? "NORMAL" : OpenMPRadioButton.IsChecked == true ? "OPENMP" : "MPI";

            long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            File.WriteAllText("wyniki_" + milliseconds.ToString() + "_" +  pVersion +  ".csv", output.ToString());

        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {

            if (FileNameTextBox.Text == "" || NumberOfNucleonsTextBox.Text == "" ||
                SizeXTextBox.Text == "" || SizeZTextBox.Text == "" || SizeYTextBox.Text == "")
                return;

            if (MonteCarloRadioButton.IsChecked == true && MonteCarloTextBox.Text == ""
                && KTTextBox.Text == "" && JTextBox.Text == "")
                return;

            string fileName = FileNameTextBox.Text;
            string sizeX = SizeXTextBox.Text;
            string sizeY = SizeYTextBox.Text;
            string sizeZ = SizeZTextBox.Text;
            string numberOfNucleons = NumberOfNucleonsTextBox.Text;
            string neighbourhood = VonNeumannRadioButton.IsChecked == true ? "VonNeumann" : "Moore";
            string bc = PeriodicRadioButton.IsChecked == true ? "Periodic" : "Nonperiodic";
            string simulation = CARadioButton.IsChecked == true ? "CA" : "MC";
            string numberOfIterations = CARadioButton.IsChecked == true ? "0" : MonteCarloTextBox.Text;
            string kt = CARadioButton.IsChecked == true ? "0" : KTTextBox.Text;
            string j = CARadioButton.IsChecked == true ? "0" : JTextBox.Text;

            counter++;


            Simulation mySimulation = new Simulation(counter.ToString(), fileName, sizeX, sizeY, sizeZ, neighbourhood,
                                                     bc, numberOfNucleons, simulation, numberOfIterations, kt, j, SimulationPanel, SimulationListView);

            string pVersion = NormalRadioButton.IsChecked == true ? "NORMAL" : OpenMPRadioButton.IsChecked == true ? "OPENMP" : "MPI";
            
            switch(pVersion)
            {
                case "NORMAL":
                    mySimulation.Run(Config.SERVER_PATH, MyTabControl, SimulationListView);
                    break;

                case "OPENMP":
                    mySimulation.Run(Config.SERVER_PATH, MyTabControl, SimulationListView, "p");
                    break;

                case "MPI":
                    string numberOfProcesses = NumberOfProcessesTextBox.Text;
                    if (numberOfProcesses == "")
                        return;
                    string mpiPath = Config.MPI_PATH;
                    string programPath = Config.MPI_SERVER_PATH;
                    mySimulation.Run("\"" + mpiPath + "\"" + " -n " + numberOfProcesses + " \"" + programPath + "\"",
                                    MyTabControl, SimulationListView);
                    break;
            }
           
          
          
        }
    }
}
