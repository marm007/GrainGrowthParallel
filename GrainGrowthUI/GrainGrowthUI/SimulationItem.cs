using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrainGrowthUI
{
    public class SimulationListItem : INotifyPropertyChanged
    {
        public string ID { get; set; }

        public string SizeX { get; set; }

        public string SizeY { get; set; }

        public string SizeZ { get; set; }

        public string Neighbourhood { get; set; }

        public string BC { get; set; }

        public string Nucleons { get; set; }

        public string Simulation { get; set; }

        public string NumberOfIterations { get; set; }

        public string KT { get; set; }

        public string J { get; set; }

        public string PreparationTime
        {
            get { return preparationTime; }
            set
            {
                if (preparationTime != value)
                {
                    preparationTime = value;
                    OnPropertyChanged("PreparationTime");
                }
            }
        }

        public string SimulationTime
        {
            get { return simulationTime; }
            set
            {
                if (simulationTime != value)
                {
                    simulationTime = value;
                    OnPropertyChanged("SimulationTime");
                }
            }
        }

        public string WriteToFileTime
        {
            get { return writeToFileTime; }
            set
            {
                if (writeToFileTime != value)
                {
                    writeToFileTime = value;
                    OnPropertyChanged("WriteToFileTime");
                }
            }
        }

        public int ProgressValue
        {
            get { return progressValue; }
            set
            {
                if (progressValue != value)
                {
                    progressValue = value;
                    OnPropertyChanged("ProgressValue");
                }
            }
        }

        public bool ProgressBool
        {
            get { return progressBool; }
            set
            {
                if (progressBool != value)
                {
                    progressBool = value;
                    OnPropertyChanged("ProgressBool");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        string preparationTime = "0";
        string simulationTime = "0";
        string writeToFileTime = "0";
        int progressValue;
        bool progressBool;
    }
}
