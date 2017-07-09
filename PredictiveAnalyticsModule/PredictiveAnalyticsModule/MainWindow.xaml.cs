using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PredictiveAnalyticsModule
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<List<Chromosome>> evolution = new List<List<Chromosome>>();
        List<Chromosome> chromosomes = new List<Chromosome>();
        int generation = 0;
        int numberOfGenerations = 20;
        double lambda;
        double miu;
        int clients;
        string path = @"d:\history.txt";
        public static int populationSize = 10;
        public static int crossoverProbability = 30;
        public static int mutationProbability = 20;
        public static int currentChromosome = new int();
        public static int bestSolutionIndex = new int();
        public static int parentIndex1 = new int();
        public static int parentIndex2 = new int();
        public static double[] selectionProbability = new double[populationSize];
        public static double[,] probabilityDiapason = new double[populationSize, 2];

        public string Evaluation(List<Chromosome> chromosomes)
        {
            evolution.Add(new List<Chromosome>(chromosomes)); //saving the current generation
            output.DataContext = evolution[generation];
            chromosomes.Sort((x, y) => x.Ws.CompareTo(y.Ws));
            int mark = 10;
            foreach (Chromosome c in chromosomes)
            {
                c.ranking = mark;
                mark--;
            }
            chromosomes.Sort((x, y) => x.Pk.CompareTo(y.Pk));
            mark = 10;
            foreach (Chromosome c in chromosomes)
            {
                c.ranking = (c.ranking + mark) / 2;
                mark--;
            }
            chromosomes.Sort((x, y) => x.ranking.CompareTo(y.ranking));
            chromosomes.Reverse();
            bestSolutionIndex = chromosomes[0].index - 1;
            if (!File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine(generation + ": chromosome " + bestSolutionIndex + ", " + chromosomes[0].S + " " + chromosomes[0].K + " " + chromosomes[0].Pk + " " + chromosomes[0].Ws);
                }
            }
            else using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(generation + ": chromosome " + bestSolutionIndex + ", " + chromosomes[0].S + " " + chromosomes[0].K + " " + chromosomes[0].Pk + " " + chromosomes[0].Ws);
            }
            string response = (bestSolutionIndex + 1) + " chromosome, Max.number of connections: " + chromosomes[0].K + ", Max.number of transactions: "+ chromosomes[0].S + ", Total mark: " + chromosomes[0].ranking;
            chromosomes.Clear();
            return response;
        }

        void probabilityCalculation(List<Chromosome> chromosomes)
        {
            for (int i = 0; i < populationSize; i++)
            {
                selectionProbability[i] = 0;
                for (int j = 0; j < 2; j++)
                {
                    probabilityDiapason[i, j] = 0; //With this value the probability diapason of this chromosome will contain only 1 number of 100 (1%)
                }
            }
            double summaryMark = 0;
            for (int i = 0; i < populationSize; i++)
            {
                summaryMark = summaryMark + chromosomes[i].ranking;
            }
            for (int i = 0; i < populationSize; i++)
                selectionProbability[i] = 100 * chromosomes[i].ranking / summaryMark; //Calculating the percentage of current fitness function in sum of all the fitness functions
            probabilityDiapason[0, 0] = 0;
            probabilityDiapason[0, 1] = selectionProbability[0];
            for (int i = 1; i < populationSize; i++)
            {
                probabilityDiapason[i, 0] = probabilityDiapason[i - 1, 1] + 1;
                probabilityDiapason[i, 1] = probabilityDiapason[i, 0] + selectionProbability[i];
            }
        }

        void parentsSeletion()
        {
            parentIndex1 = 0;
            parentIndex2 = 0;
            Random rnd = new Random();
            int roulette = rnd.Next(0, 100);
            for (int i = 1; i < populationSize; i++)
            {
                if (roulette >= probabilityDiapason[i, 0] && roulette < probabilityDiapason[i, 1])
                {
                    parentIndex1 = i;
                    break;
                }
            }
            roulette = 0;
            bool rouletteValid = false;
            while (!rouletteValid)
            {
                roulette = rnd.Next(0, 100);
                if (roulette < probabilityDiapason[parentIndex1, 0] || roulette >= probabilityDiapason[parentIndex1, 1])
                    rouletteValid = true;
            }
            for (int i = 1; i < populationSize; i++)
            {
                if (roulette >= probabilityDiapason[i, 0] && roulette < probabilityDiapason[i, 1])
                {
                    parentIndex2 = i;
                    break;
                }
            }
        }

        void crossover(List<Chromosome> oldGeneration, List<Chromosome> newGeneration)
        {
            //crossing over the first half of genotype (which stands for number of transactions)
            Random rnd = new Random();
            int division = rnd.Next(1, 3);
            bool firstParent = new bool();
            if (rnd.NextDouble() > 0.5)
                firstParent = true;
            else
                firstParent = false;
            newGeneration.Add(new Chromosome());
            chromosomes[currentChromosome].index = (byte)(currentChromosome + 1);
            newGeneration[currentChromosome].genotype = new bool[8];
            for (int i = 0; i < division; i++)
            {
                if (firstParent)
                    newGeneration[currentChromosome].genotype[i] = oldGeneration[parentIndex1].genotype[i];
                else
                    newGeneration[currentChromosome].genotype[i] = oldGeneration[parentIndex2].genotype[i];
            }
            for (int i = division; i < 4; i++)
            {
                if (!firstParent)
                    newGeneration[currentChromosome].genotype[i] = oldGeneration[parentIndex1].genotype[i];
                else
                    newGeneration[currentChromosome].genotype[i] = oldGeneration[parentIndex2].genotype[i];
            }

            //crossing over the second half of genotype (which stands for number of connections)
            rnd = new Random();
            division = rnd.Next(4, 6);
            if (rnd.NextDouble() > 0.5)
                firstParent = true;
            else
                firstParent = false;
            for (int i = 3; i < division; i++)
            {
                if (firstParent)
                    newGeneration[currentChromosome].genotype[i] = oldGeneration[parentIndex1].genotype[i];
                else
                    newGeneration[currentChromosome].genotype[i] = oldGeneration[parentIndex2].genotype[i];
            }
            for (int i = division; i < 8; i++)
            {
                if (!firstParent)
                    newGeneration[currentChromosome].genotype[i] = oldGeneration[parentIndex1].genotype[i];
                else
                    newGeneration[currentChromosome].genotype[i] = oldGeneration[parentIndex2].genotype[i];
            }
            currentChromosome++;
        }

        void mutation(List<Chromosome> newGeneration)
        {
            Random rnd = new Random();
            int mutate = new int();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < populationSize; j++)
                {
                    mutate = rnd.Next(0, 100);
                    if (mutate <= mutationProbability)
                        newGeneration[j].genotype[i] = !newGeneration[j].genotype[i];
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            buttonStart.Visibility = Visibility.Hidden;
            buttonStep.Visibility = Visibility.Visible;
            buttonFinal.Visibility = Visibility.Visible;
            comboBoxClientNumber.IsEnabled = false;
            txtBoxInputFlow.IsEnabled = false;
            txtBoxServiceTime.IsEnabled = false;

            miu = 1 / Convert.ToDouble(txtBoxServiceTime.Text);
            clients = Convert.ToInt32(comboBoxClientNumber.SelectedIndex) + 1;
            lambda = (clients * Convert.ToDouble(txtBoxInputFlow.Text) / 1000);

            //Generate initial population
            Random rnd = new Random();
            for (int i = 0; i < populationSize; i++)
            {
                Chromosome c = new Chromosome();
                c.index = (byte)(i + 1);
                c.genotype = new bool[8];
                for (int j = 0; j < 8; j++)
                {
                    if (rnd.NextDouble() > 0.5)
                        c.genotype[j] = true;
                    else
                        c.genotype[j] = false;
                }
                c.calculateFitness(lambda, miu);
                chromosomes.Add(c);
            }
            txtBlockResult.Text += "Current generation: " + (generation + 1) + ". Best solution:" + Environment.NewLine;
            string result = Evaluation(chromosomes);
            txtBlockResult.Text += result;

        }

        private void buttonStep_Click(object sender, RoutedEventArgs e)
        {
            currentChromosome = 0;
            //Elitism - copy of the best chromosome of the previous generation
            chromosomes.Add(new Chromosome());
            chromosomes[currentChromosome].index = (byte)(currentChromosome + 1);
            chromosomes[currentChromosome].genotype = new bool[8];
            chromosomes[currentChromosome].genotype = evolution[generation][bestSolutionIndex].genotype;
            currentChromosome++;

            probabilityCalculation(evolution[generation]);
            Random rnd = new Random();
            while (currentChromosome < populationSize)
            {
                parentsSeletion();
                int breed = rnd.Next(0, 100);
                if (breed <= crossoverProbability)
                    crossover(evolution[generation], chromosomes);
                else
                {
                    chromosomes.Add(new Chromosome());
                    chromosomes[currentChromosome].index = (byte)(currentChromosome + 1);
                    chromosomes[currentChromosome].genotype = new bool[8];
                    chromosomes[currentChromosome].genotype = evolution[generation][parentIndex1].genotype;
                    currentChromosome++;
                    if (currentChromosome < populationSize)
                    {
                        chromosomes.Add(new Chromosome());
                        chromosomes[currentChromosome].index = (byte)(currentChromosome + 1);
                        chromosomes[currentChromosome].genotype = new bool[8];
                        chromosomes[currentChromosome].genotype = evolution[generation][parentIndex2].genotype;
                        currentChromosome++;
                    }
                }
            }
            mutation(chromosomes);
            foreach (Chromosome c in chromosomes)
                c.calculateFitness(lambda, miu);
            generation++;
            txtBlockResult.Text += Environment.NewLine;
            txtBlockResult.Text += Environment.NewLine;
            txtBlockResult.Text += "Current generation: " + (generation + 1) + ". Best solution:" + Environment.NewLine;
            string result = Evaluation(chromosomes);
            txtBlockResult.Text += result;
            output.DataContext = evolution[generation];

            if (generation == numberOfGenerations - 1)
            {
                txtBlockResult.Text += Environment.NewLine;
                txtBlockResult.Text += Environment.NewLine;
                txtBlockResult.Text += "Final generation. Simulation complete.";
            }
        }

        private void buttonFinal_Click(object sender, RoutedEventArgs e)
        {
            while (generation < numberOfGenerations - 1)
            {
                currentChromosome = 0;
                //Elitism - copy of the best chromosome of the previous generation
                chromosomes.Add(new Chromosome());
                chromosomes[currentChromosome].index = (byte)(currentChromosome + 1);
                chromosomes[currentChromosome].genotype = new bool[8];
                chromosomes[currentChromosome].genotype = evolution[generation][bestSolutionIndex].genotype;
                currentChromosome++;

                probabilityCalculation(evolution[generation]);
                Random rnd = new Random();
                while (currentChromosome < populationSize)
                {
                    parentsSeletion();
                    int breed = rnd.Next(0, 100);
                    if (breed <= crossoverProbability)
                        crossover(evolution[generation], chromosomes);
                    else
                    {
                        chromosomes.Add(new Chromosome());
                        chromosomes[currentChromosome].index = (byte)(currentChromosome + 1);
                        chromosomes[currentChromosome].genotype = new bool[8];
                        chromosomes[currentChromosome].genotype = evolution[generation][parentIndex1].genotype;
                        currentChromosome++;
                        if (currentChromosome < populationSize)
                        {
                            chromosomes.Add(new Chromosome());
                            chromosomes[currentChromosome].index = (byte)(currentChromosome + 1);
                            chromosomes[currentChromosome].genotype = new bool[8];
                            chromosomes[currentChromosome].genotype = evolution[generation][parentIndex2].genotype;
                            currentChromosome++;
                        }
                    }
                }
                mutation(chromosomes);
                foreach (Chromosome c in chromosomes)
                    c.calculateFitness(lambda, miu);
                generation++;
                txtBlockResult.Text += Environment.NewLine;
                txtBlockResult.Text += Environment.NewLine;
                txtBlockResult.Text += "Current generation: " + (generation + 1) + ". Best solution:" + Environment.NewLine;
                txtBlockResult.Text += Evaluation(chromosomes);
                output.DataContext = evolution[generation];

                if (generation == numberOfGenerations - 1)
                {
                    txtBlockResult.Text += Environment.NewLine;
                    txtBlockResult.Text += Environment.NewLine;
                    txtBlockResult.Text += "Final generation. Simulation complete.";
                    buttonStep.Visibility = Visibility.Hidden;
                    buttonFinal.Visibility = Visibility.Hidden;
                    buttonNewSimulation.Visibility = Visibility.Visible;
                }
            }
        }

        private void buttonNewSimulation_Click(object sender, RoutedEventArgs e)
        {
            output.DataContext = null;
            evolution.Clear();
            chromosomes.Clear();
            generation = 0;
            comboBoxClientNumber.IsEnabled = true;
            txtBoxInputFlow.IsEnabled = true;
            txtBoxServiceTime.IsEnabled = true;
            buttonStart.Visibility = Visibility.Visible;
            buttonNewSimulation.Visibility = Visibility.Hidden;
        }
    }

    public class Chromosome
    {
        public byte index { get; set; }
        public bool[] genotype { get; set; }
        public double ranking { get; set; }
        public byte S { get; set; } //max number of transactions
        public byte K { get; set; } //max number of connections
        public double Ws { get; set; } //average time spent in the system
        public double Pk { get; set; } //loss probability
        public double P0 { get; set; } //probability of system being idle
        public void calculateFitness(double lambda, double miu)
        {
            //Get phenotype from genotype
            BitArray bits;
            byte[] bytes = new byte[1];
            bool[] temp = new bool[8];
            Buffer.BlockCopy(genotype, 0, temp, 4, 4);
            Array.Reverse(temp);
            bits = new BitArray(temp);
            bits.CopyTo(bytes, 0);
            S = bytes[0];
            if (S == 0) S = 16;
            Array.Reverse(temp);
            Buffer.BlockCopy(genotype, 4, temp, 4, 4);
            Array.Reverse(temp);
            bits = new BitArray(temp);
            bits.CopyTo(bytes, 0);
            K = bytes[0];
            if (K == 0) K = 16;

            double ro = lambda / miu;
            int factorN = 1;

            //Calculate probability of system's being idle
            for (int n = 1; n < S; n++)
            {
                factorN = factorN * n;
                P0 = P0 + (Math.Pow(ro, n) / factorN);
            }

            int factorS = factorN * S;
            if (K > S) //there is a queue in the system
            {
                for (int n = S; n < K; n++)
                    P0 = P0 + (Math.Pow(ro, n) / (factorS * Math.Pow(S, n - S)));
                P0 = 1 / (P0 + (Math.Pow(ro, K) / factorS)); 
            }
            else //system has no queue
            {
                P0 = 1 / (P0 + (Math.Pow(ro, S) / factorS));
            }

            //Calculate loss probability
            if (K > S)
                Pk = (Math.Pow(ro, K) / (factorS * Math.Pow(S, K - S))) * P0;
            else
                Pk = (Math.Pow(ro, S) / factorS) * P0;

            //Calculate average time spent in the system
            double EAR = lambda * (1 - Pk); //effective arrival rate
            double Ls = 0; //average number of clients in the system
            
            for (int n = 1; n < S; n++)
            {
                Ls = Ls + n * Math.Pow(ro, n);
            }

            if (K > S)
            {
                for (int n = S; n < K; n++)
                    Ls = Ls + n * (Math.Pow(ro, n) / Math.Pow(S, n - S));
                Ls = (P0 / factorS) * (Ls + K * (Math.Pow(ro, K) / Math.Pow(S, K - S)));
            }
            else
                Ls = (P0 / factorS) * (Ls + S * Math.Pow(ro, S));

            Ws = Ls / EAR;
        }
    }

    [ValueConversion(typeof(bool[]), typeof(string))]
    public class BoolArrayConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool[] bits = (bool[])value;
            string output = null;
            foreach (bool b in bits)
            {
                if (b) output += "1";
                else output += "0";
            }
            return output;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
