using System.Collections.Generic;
using System.Linq;

namespace GeneticSignalGame
{
    public class Population
    {
        public List<Chromosome> chromosomes;

        public int populationSize = 500;

        public int elite = 2;

        public double mutationRate = 0.5;

        public double mutationRepeats = 2;

        public double crossoverRate = 0.7;

        public double selectionPressure = 0.8;

        public int iteration = 0;

        public Population()
        {
            chromosomes = new List<Chromosome>();
        }


        public void InitPopulation()
        {
            List<Chromosome> initialChromosomes = new List<Chromosome>();
            int chromosomesCount = populationSize;
            if (Program.config.goodInitialStrategies)
                chromosomesCount = 10 * populationSize;

            for (int i = 0; i < chromosomesCount; i++)
            {
                Chromosome newChromosome = new Chromosome();
                newChromosome.Init();
                newChromosome.Evaluate();
                initialChromosomes.Add(newChromosome);
            }

            initialChromosomes.Sort((c1, c2) => c2.fittingFunction.CompareTo(c1.fittingFunction));

            chromosomes = new List<Chromosome>();
            for (int i = 0; i < populationSize; i++)
                chromosomes.Add(initialChromosomes[i]);
        }

        public void MakeNewPopulation()
        {
            iteration++;
            List<Chromosome> newChromosomes = new List<Chromosome>();

            //---ELITE---
            for (int i = 0; i < elite; i++)
            {
                newChromosomes.Add(chromosomes[i].MakeCopy());
                newChromosomes.Last().Evaluate();
            }

            //---CROSSOVER---
            if (!Program.config.warmup || iteration > Program.warmupIterations)
            {

                List<Chromosome> listToCrossover = new List<Chromosome>(); 
                List<Chromosome> listAfterCrossover = new List<Chromosome>();

                for (int i = 0; i < chromosomes.Count; i++)
                {
                    if (Program.rand.NextDouble() < crossoverRate)
                        listToCrossover.Add(chromosomes[i]);
                    listAfterCrossover.Add(chromosomes[i]);
                }

                listToCrossover.Permutate();

                for (int i = 0; i < listToCrossover.Count - 1; i += 2)
                    listAfterCrossover.Add(listToCrossover[i].Crossover(listToCrossover[i], listToCrossover[i + 1]));

                chromosomes = listAfterCrossover;
            }


            //---MUTATION---
            for (int i = 0; i < chromosomes.Count; i++)
            {
                if (Program.rand.NextDouble() < mutationRate || (Program.config.warmup && iteration < Program.warmupIterations))
                    for (int j = 0; j < mutationRepeats; j++)
                        chromosomes[i].Mutate();
            }

            //--LOCAL OPTIMIZATION--
            for (int i = 0; i < chromosomes.Count; i++)
                chromosomes[i].LocalOptimize();

            //---EVALUATION---
            foreach (Chromosome c in chromosomes)
                c.Evaluate();

            //---SELECTION---
            while (newChromosomes.Count < populationSize)
            {
                int c1 = Program.rand.Next(chromosomes.Count);
                int c2 = Program.rand.Next(chromosomes.Count);

                if ((chromosomes[c1].fittingFunction > chromosomes[c2].fittingFunction) && Program.rand.NextDouble() < selectionPressure)
                    newChromosomes.Add(chromosomes[c1].MakeCopy());
                else
                    newChromosomes.Add(chromosomes[c2].MakeCopy());
            }

            chromosomes = newChromosomes;

            chromosomes.Sort((c1, c2) => (c2.fittingFunction.CompareTo(c1.fittingFunction)));
        }
    }
}
