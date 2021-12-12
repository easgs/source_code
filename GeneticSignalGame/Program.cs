using GeneticSignalGame.Struct;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace GeneticSignalGame
{
    public class Program
    {
        public static SignalGame game;

        public static Random rand = new Random(777);

        public static double EPS = 0.00001;

        static int iterations = 10000;

        public static Config config;

        public static int mutationsRepeat = 20;

        public static int warmupIterations = 200;

        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: GeneticSignalGame.exe games_path result_file_path");
                return;
            }
            string gamesPath = args[1];
            string resultFilePath = args[2];
            RunComputations(gamesPath, resultFilePath);
        }

        public static void RunComputations(string gamesPath, string resultFilePath)
        {
            StreamWriter w = new StreamWriter(resultFilePath);
            w.WriteLine("config;game_id;game;defenderPayoff;attackrPayoff;trainingTime");

            string[] gameTypes = new string[] { "sparse", "moderate", "dense" };

            List<string> games = new List<string>();

            for (int nodes = 10; nodes <= 10; nodes += 10)
                foreach (string gameType in gameTypes)
                    games.AddRange(Directory.GetFiles(gamesPath + "\\" + gameType + "\\" + nodes + "\\", "*.siggame"));

            List<Config> configs = generateConfigs();

            for (int j = 0; j <games.Count; j++)
            {
                int runId = rand.Next(int.MaxValue);
                config = configs[rand.Next(configs.Count)];
                int gameIndx = j;
                string gameName = Path.GetFileNameWithoutExtension(games[gameIndx]);
                game = LoadGameDefinition(games[gameIndx]);

                Population population = new Population();
                population.populationSize = 200;
                population.mutationRate = 0.8;
                population.mutationRepeats = 10;
                population.crossoverRate = 0.5;
                population.selectionPressure = 0.8;

                DateTime startTime = DateTime.Now;
                population.InitPopulation();

                double previousFittingFunction = double.MinValue;
                int lastIterationFittingFunctionChange = -1;

                int it = 0;
                for (it = 0; it < iterations; it++)
                {
                    population.MakeNewPopulation();
                    if (it % 10 == 0)
                    {
                        Console.WriteLine(Path.GetFileNameWithoutExtension(games[gameIndx]) + " it: " + it);
                        Console.WriteLine(string.Format("Iteration {0}. Attack on target: {1}. Attacker payoff: {2}. Defender payoff: {3}",
                            it, population.chromosomes[0].attackerResponse.target, population.chromosomes[0].attackerResult, population.chromosomes[0].fittingFunction));
                    }

                    if (Math.Abs(population.chromosomes[0].fittingFunction - previousFittingFunction) > EPS)
                    {
                        previousFittingFunction = population.chromosomes[0].fittingFunction;
                        lastIterationFittingFunctionChange = it;
                    }

                    if (it - lastIterationFittingFunctionChange > 200)
                        break;
                }

                int trainingTime = (int)DateTime.Now.Subtract(startTime).TotalSeconds;

                Console.WriteLine(game + ";" + population.chromosomes[0].fittingFunction + ";" + population.chromosomes[0].attackerResult + ";" + trainingTime + ";" + it);

                w.WriteLine("EASG" + ";" + game.id + ";" + Path.GetFileNameWithoutExtension(games[gameIndx]) + ";" + population.chromosomes[0].fittingFunction + ";" + population.chromosomes[0].attackerResult + ";" + trainingTime
                    + ";" + it + ";" + population.populationSize + ";" + population.mutationRate + ";" + population.crossoverRate + ";"
                    + population.mutationRepeats + ";" + population.selectionPressure);
                w.Flush();
            }

            w.Close();
            Console.WriteLine("---- THE END ----");
        }

        static void Serialize(Chromosome c, string path)
        {
            File.WriteAllText(path, new JavaScriptSerializer().Serialize(c));
        }

        static Chromosome Deserialize(string path)
        {
            string json = File.ReadAllText(path);
            return (Chromosome)(new JavaScriptSerializer().Deserialize<Chromosome>(json));
        }

        static SignalGame LoadGameDefinition(string filePath)
        {
            SignalGame gameDefinition = (new JavaScriptSerializer().Deserialize(File.ReadAllText(filePath), typeof(SignalGame))) as SignalGame;
            gameDefinition.graphConfig.MakeAdjacencyList();
            gameDefinition.GenerateAllAttackerStrategies();
            return gameDefinition;
        }

        static List<Config> generateConfigs()
        {
            Config c0 = new Config();
            c0.configName = "baseline";

            Config c1 = new Config();
            c1.goodInitialStrategies = true;
            c1.configName = "goodInitialStrategies";

            Config c2 = new Config();
            c2.newIndividualsInGeneration = true;
            c2.configName = "newIndividualsInGeneration";

            Config c3 = new Config();
            c3.mutationAddPureStrategy = true;
            c3.configName = "mutationAddPureStrategy";

            Config c4 = new Config();
            c4.mutationDeletePureStrategy = true;
            c4.configName = "mutationDeletePureStrategy";

            Config c5 = new Config();
            c5.mutationChangeProbability = true;
            c5.configName = "mutationChangeProbability";

            Config c6 = new Config();
            c6.mutationSwitchProbability = true;
            c6.configName = "mutationSwitchProbability";

            Config c7 = new Config();
            c7.mutationWeakestPureStrategy = true;
            c7.configName = "mutationWeakestPureStrategy";

            Config c8 = new Config();
            c8.mutationWeakestPureStrategyProportional = true;
            c8.configName = "mutationWeakestPureStrategyProportional";

            Config c9 = new Config();
            c9.crossoverWithPayoff = true;
            c9.configName = "crossoverWithPayoff";

            Config c10 = new Config();
            c10.mutationAddPureStrategy = true;
            c10.mutationBetterPayoff = true;
            c10.configName = "betterPayoff_mutationAddPureStrategy";

            Config c11 = new Config();
            c11.mutationDeletePureStrategy = true;
            c11.mutationBetterPayoff = true;
            c11.configName = "betterPayoff_mutationDeletePureStrategy";

            Config c12 = new Config();
            c12.mutationChangeProbability = true;
            c12.mutationBetterPayoff = true;
            c12.configName = "betterPayoff_mutationChangeProbability";

            Config c13 = new Config();
            c13.mutationSwitchProbability = true;
            c13.mutationBetterPayoff = true;
            c13.configName = "betterPayoff_mutationSwitchProbability";

            Config c15 = new Config();
            c15.mutationDeletePureStrategy = true;
            c15.mutationWeakestPureStrategy = true;
            c15.configName = "weakest_mutationDeletePureStrategy";

            Config c16 = new Config();
            c16.mutationDeletePureStrategy = true;
            c16.mutationWeakestPureStrategyProportional = true;
            c16.configName = "weakestProportional_mutationDeletePureStrategy";

            Config c17 = new Config();
            c17.warmup = true;
            c17.configName = "pureWarmup";

            Config c18 = new Config();
            c18.mutationDefendAttackerTarget = true;
            c18.configName = "mutationDefendAttackerTarget";

            Config c19 = new Config();
            c19.mutationDefendAttackerTarget = true;
            c19.warmup = true;
            c19.configName = "mutationDefendAttackerTargetWithWarmup";

            Config c20 = new Config();
            c20.checkBaseSignalingStrategy = true;
            c20.configName = "checkBaseSignalingStrategy";

            Config c21 = new Config();
            c21.checkBaseSignalingStrategy = true;
            c21.mutationChangeProbability = true;
            c21.mutationBetterPayoff = true;
            c21.mutationDefendAttackerTarget = true;
            c21.configName = "mutationChangeProbabilityBetterPayoffDefendTarget";

            Config c22 = new Config();
            c22.checkBaseSignalingStrategy = true;
            c22.mutationChangeProbability = true;
            c22.mutationBetterPayoff = true;
            c22.configName = "mutationChangeProbabilityBetterPayoff";

            List<Config> configs = new List<Config> { c0 };
            return configs;
        }
    }
}
