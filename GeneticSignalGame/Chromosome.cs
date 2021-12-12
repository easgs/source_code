using GeneticSignalGame.Struct;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticSignalGame
{
    public class Chromosome
    {
        public double fittingFunction;

        public double attackerResult;
        
        public AttackerStrategy attackerResponse;

        public List<DefenderStrategy> defenderStrategies;

        public List<double> probabilities;

        int[] signalingUnitNeighbourhoodPerNode;
        int[] signalingNoUnitNeighbourhoodPerNode;

        public void Evaluate()
        {
            //FixStrategy();

            Payoff bestResult = new Payoff();

            AttackerStrategy bestAttackStrategy = new AttackerStrategy();
            bestAttackStrategy.target = -1;

            foreach (AttackerStrategy attackerStrategy in Program.game.attackerStrategies)
            {
                Payoff payoff = new Payoff();
                for (int i = 0; i < defenderStrategies.Count; i++)
                {
                    Payoff partialPayoff = EvaluatePureStrategy(defenderStrategies[i], attackerStrategy);
                    defenderStrategies[i].partialPayoff = partialPayoff;
                    payoff += EvaluatePureStrategy(defenderStrategies[i], attackerStrategy) * probabilities[i];
                }

                if (payoff.attacker > bestResult.attacker + Program.EPS 
                    || Math.Abs(payoff.attacker - bestResult.attacker) < Program.EPS && payoff.defender > bestResult.defender)
                {
                    bestResult = payoff;
                    bestAttackStrategy = attackerStrategy;
                }
            }

            fittingFunction = bestResult.defender;
            attackerResult = bestResult.attacker;
            attackerResponse = bestAttackStrategy;
        }


        private Payoff EvaluatePureStrategy(DefenderStrategy strategy, AttackerStrategy attackerStrategy)
        {
            Payoff result = new Payoff();

            if (strategy.patrollerAllocation.Contains(attackerStrategy.target)) 
            {
                result.defender = Program.game.defenderReward[attackerStrategy.target];
                result.attacker = Program.game.attackerPenalty[attackerStrategy.target];
                return result;
            }

            bool defenderInNeighborhood = Program.game.graphConfig.adjacencyList[attackerStrategy.target]
                                .Intersect(strategy.patrollerAllocation.ToList()).Any();

            Signal sendSignal = Signal.NoSignal;
            if (strategy.droneAllocation.Contains(attackerStrategy.target))
            {
                sendSignal = Signal.WeakSignal;
                if (defenderInNeighborhood && strategy.signalingUnitNeighbourhood[strategy.droneAllocation.ToList().IndexOf(attackerStrategy.target)] == 1)
                    sendSignal = Signal.StrongSignal;
                if (!defenderInNeighborhood && strategy.signalingNoUnitNeighbourhood[strategy.droneAllocation.ToList().IndexOf(attackerStrategy.target)] == 1)
                    sendSignal = Signal.StrongSignal;
            }

            double attackerNoticedNoSignalProbability = 1.0;
            double attackerNoticedWeakSignalProbability = 0.0;
            double attackerNoticedStrongSignalProbability = 0.0;
            if (sendSignal == Signal.WeakSignal)
            {
                attackerNoticedNoSignalProbability = Program.game.kappa;
                attackerNoticedWeakSignalProbability = 1 - Program.game.kappa;
                attackerNoticedStrongSignalProbability = 0.0;
            }
            else if (sendSignal == Signal.StrongSignal)
            {
                attackerNoticedNoSignalProbability = Program.game.lambda;
                attackerNoticedWeakSignalProbability = Program.game.mu;
                attackerNoticedStrongSignalProbability = 1 - Program.game.lambda - Program.game.mu;
            }


            double detectionProbability = 0.0;
            if (strategy.droneAllocation.Contains(attackerStrategy.target))
                detectionProbability = 1 - Program.game.gamma;

            double attackerRunAwayProbability = attackerStrategy.signalReactions[0] * attackerNoticedNoSignalProbability +
                attackerStrategy.signalReactions[1] * attackerNoticedWeakSignalProbability +
                attackerStrategy.signalReactions[2] * attackerNoticedStrongSignalProbability;

            if (strategy.patrollerReallocation.Contains(attackerStrategy.target))
            {
                result.defender += (1 - detectionProbability) * (1 - attackerRunAwayProbability) * Program.game.defenderReward[attackerStrategy.target];
                result.attacker += (1 - detectionProbability) * (1 - attackerRunAwayProbability) * Program.game.attackerPenalty[attackerStrategy.target];
            }
            else
            {
                result.defender += (1 - detectionProbability) * (1 - attackerRunAwayProbability) * Program.game.defenderPenalty[attackerStrategy.target];
                result.attacker += (1 - detectionProbability) * (1 - attackerRunAwayProbability) * Program.game.attackerReward[attackerStrategy.target];
            }

            if (defenderInNeighborhood)
            {
                result.defender += detectionProbability * (1 - attackerRunAwayProbability) * Program.game.defenderReward[attackerStrategy.target];
                result.attacker += detectionProbability * (1 - attackerRunAwayProbability) * Program.game.attackerPenalty[attackerStrategy.target];
            }
            else
            {
                result.defender += detectionProbability * (1 - attackerRunAwayProbability) * Program.game.defenderPenalty[attackerStrategy.target];
                result.attacker += detectionProbability * (1 - attackerRunAwayProbability) * Program.game.attackerReward[attackerStrategy.target];
            }

            return result;
        }


        public void Mutate()
        {
            double payoffBeforeMutation = fittingFunction;
            double payoffAfterMutation = fittingFunction;

            Chromosome original = this.MakeCopy();

            int i = 0;
            do
            {
                Restore(original);

                if (Program.config.mutateStandard)
                    MutateStandard();

                if (Program.config.mutationAddPureStrategy)
                    MutateAddPureStrategy();

                if (Program.config.mutationChangeProbability)
                    MutateChangeProbability();

                if (Program.config.mutationDefendAttackerTarget)
                    MutateDefendAttackerTarget();

                Evaluate();
                payoffAfterMutation = fittingFunction;
                i++;
            }
            while (Program.config.mutationBetterPayoff && i < Program.mutationsRepeat && payoffBeforeMutation >= payoffAfterMutation);
        }

        void Restore(Chromosome c)
        {
            fittingFunction = c.fittingFunction;
            attackerResult = c.attackerResult;
            attackerResponse = c.attackerResponse == null ? null : c.attackerResponse.MakeCopy();
            defenderStrategies = c.defenderStrategies.Select(x => x.MakeCopy()).ToList();

            probabilities = c.probabilities.Select(x => x).ToList();

            signalingUnitNeighbourhoodPerNode = c.signalingUnitNeighbourhoodPerNode.Select(x => x).ToArray();
            signalingNoUnitNeighbourhoodPerNode = c.signalingNoUnitNeighbourhoodPerNode.Select(x => x).ToArray();
        }

        public void MutateStandard()
        {
            int mutatedElement = Program.rand.Next(5);

            DefenderStrategy strategy = defenderStrategies[Program.rand.Next(defenderStrategies.Count)];
            if (mutatedElement == 0) //patrollerAllocation
            {
                List<int> candidateNodes = Enumerable.Range(0, Program.game.graphConfig.vertexCount)
                    .Except(strategy.patrollerAllocation.ToList()).ToList();
                int indx = Program.rand.Next(strategy.patrollerAllocation.Length);
                strategy.patrollerAllocation[indx] = candidateNodes[Program.rand.Next(candidateNodes.Count)];
                List<int> neighbours = Program.game.graphConfig.adjacencyList[strategy.patrollerAllocation[indx]];
                strategy.patrollerReallocation[indx] = neighbours[Program.rand.Next(neighbours.Count)];
            }
            else if (mutatedElement == 1) //droneAllocation
            {
                List<int> candidateNodes = Enumerable.Range(0, Program.game.graphConfig.vertexCount)
                    .Except(strategy.droneAllocation.ToList()).ToList();
                if (strategy.droneAllocation.Length > 0)
                    strategy.droneAllocation[Program.rand.Next(strategy.droneAllocation.Length)]
                        = candidateNodes[Program.rand.Next(candidateNodes.Count)];
            }
            else if (mutatedElement == 2) //patrollerReallocation
            {
                int indx = Program.rand.Next(strategy.patrollerAllocation.Length);
                List<int> neighbours = Program.game.graphConfig.adjacencyList[strategy.patrollerAllocation[indx]];
                strategy.patrollerReallocation[indx] = neighbours[Program.rand.Next(neighbours.Count)];
            }
            else if (mutatedElement == 3) //signalingUnitNeighbourhood
            {
                int indx = Program.rand.Next(signalingUnitNeighbourhoodPerNode.Length);
                signalingUnitNeighbourhoodPerNode[indx] = 1 - signalingUnitNeighbourhoodPerNode[indx];
            }
            else if (mutatedElement == 4) //signalingNoUnitNeighbourhood
            {
                int indx = Program.rand.Next(signalingNoUnitNeighbourhoodPerNode.Length);
                signalingNoUnitNeighbourhoodPerNode[indx] = 1 - signalingNoUnitNeighbourhoodPerNode[indx];
            }
        }

        public void MutateAddPureStrategy()
        {
            defenderStrategies.Add(GenerateRandomDefenderStrategy());
            probabilities.Add(Program.rand.NextDouble());
            NormalizeProbabilities();
        }

        public void MutateChangeProbability()
        {
            int indx = Program.rand.Next(probabilities.Count);
            probabilities[indx] = Program.rand.NextDouble();
            NormalizeProbabilities();
        }

        public void MutateDefendAttackerTarget()
        {
            if (attackerResponse == null) return;
            List<DefenderStrategy> strategiesToMutate =  defenderStrategies.Where(x => !x.droneAllocation.Contains(attackerResponse.target) && !x.patrollerAllocation.Contains(attackerResponse.target)).ToList();
            if (strategiesToMutate.Count == 0) return;
            DefenderStrategy strategyToMutate = strategiesToMutate[Program.rand.Next(strategiesToMutate.Count)];
            if (Program.rand.Next(3) == 0) //droneAllocation
                strategyToMutate.droneAllocation[Program.rand.Next(strategyToMutate.droneAllocation.Length)] = attackerResponse.target;
            else
            {
                int indx = Program.rand.Next(Program.rand.Next(strategyToMutate.patrollerAllocation.Length));
                strategyToMutate.patrollerAllocation[indx] = attackerResponse.target;
                List<int> neighbours = Program.game.graphConfig.adjacencyList[strategyToMutate.patrollerAllocation[indx]];
                strategyToMutate.patrollerReallocation[indx] = neighbours[Program.rand.Next(neighbours.Count)];
            }
        }

        public Chromosome Crossover(Chromosome c1, Chromosome c2)
        {
            Chromosome result = new Chromosome();
            result.defenderStrategies = new List<DefenderStrategy>();
            result.probabilities = new List<double>();
            for (int i = 0; i < c1.defenderStrategies.Count; i++)
            {
                result.defenderStrategies.Add(c1.defenderStrategies[i].MakeCopy());
                result.probabilities.Add(c1.probabilities[i]);
            }

            for (int i = 0; i < c2.defenderStrategies.Count; i++)
            {
                result.defenderStrategies.Add(c2.defenderStrategies[i].MakeCopy());
                result.probabilities.Add(c2.probabilities[i]);
            }

            List<int> strategiesToRemove = new List<int>();
            int maxIndx = probabilities.IndexOf(probabilities.Max());
            for (int i = 0; i < result.defenderStrategies.Count; i++)
            {
                if (i == maxIndx) continue;
                double randomNumber = Program.rand.NextDouble() * Program.rand.NextDouble();
                if (randomNumber > result.probabilities[i])
                    strategiesToRemove.Add(i);
            }
            result.RemoveStrategies(strategiesToRemove);

            result.signalingUnitNeighbourhoodPerNode = new int[c1.signalingUnitNeighbourhoodPerNode.Length];
            result.signalingNoUnitNeighbourhoodPerNode = new int[c1.signalingNoUnitNeighbourhoodPerNode.Length];

            for (int i=0; i< result.signalingUnitNeighbourhoodPerNode.Length; i++)
                if (i < result.signalingUnitNeighbourhoodPerNode.Length/2)
                {
                    result.signalingUnitNeighbourhoodPerNode[i] = c1.signalingUnitNeighbourhoodPerNode[i];
                    result.signalingNoUnitNeighbourhoodPerNode[i] = c1.signalingNoUnitNeighbourhoodPerNode[i];
                }
                else
                {
                    result.signalingUnitNeighbourhoodPerNode[i] = c2.signalingUnitNeighbourhoodPerNode[i];
                    result.signalingNoUnitNeighbourhoodPerNode[i] = c2.signalingNoUnitNeighbourhoodPerNode[i];
                }


            return result;
        }


        public void RemoveStrategies(List<int> strategiesToRemove)
        {
            List<DefenderStrategy> newStrategies = new List<DefenderStrategy>();
            List<double> newProbabilities = new List<double>();
            for (int i = 0; i < defenderStrategies.Count; i++)
                if (!strategiesToRemove.Contains(i))
                {
                    newStrategies.Add(defenderStrategies[i]);
                    newProbabilities.Add(probabilities[i]);
                }

            defenderStrategies = newStrategies;
            probabilities = newProbabilities;

            double probabilitiesSum = probabilities.Sum();  
            probabilities = probabilities.Select(x => x * (1 / probabilitiesSum)).ToList();
        }

        public void FixStrategy()
        {
            foreach (DefenderStrategy strategy in defenderStrategies)
            {
                for (int i=0; i<strategy.droneAllocation.Length; i++)
                {
                    strategy.signalingUnitNeighbourhood[i] = signalingUnitNeighbourhoodPerNode[strategy.droneAllocation[i]];
                    strategy.signalingNoUnitNeighbourhood[i] = signalingNoUnitNeighbourhoodPerNode[strategy.droneAllocation[i]];
                }

            }
        }

        void NormalizeProbabilities()
        {
            double probabilitiesSum = probabilities.Sum();
            probabilities = probabilities.Select(x => x * (1 / probabilitiesSum)).ToList();
        }

        public void LocalOptimize()
        {
            foreach (DefenderStrategy strategy in defenderStrategies)
            {
                HashSet<int> patrollerAllocationSet = new HashSet<int>();
                for (int i = 0; i < strategy.patrollerAllocation.Length; i++)
                {
                    int v = strategy.patrollerAllocation[i];
                    int initV = v;
                    while (patrollerAllocationSet.Contains(v))
                        v++;
                    if (v < Program.game.graphConfig.vertexCount)
                    {
                        strategy.patrollerAllocation[i] = v;
                        patrollerAllocationSet.Add(v);
                    }

                    List<int> neighbours = Program.game.graphConfig.adjacencyList[v];
                    if (initV != v && !neighbours.Contains(strategy.patrollerReallocation[i]))
                        strategy.patrollerReallocation[i] = neighbours[Program.rand.Next(neighbours.Count)];
                }

                HashSet<int> droneAllocationSet = new HashSet<int>();
                for (int i = 0; i < strategy.droneAllocation.Length; i++)
                {
                    int v = strategy.droneAllocation[i];
                    while ((patrollerAllocationSet.Contains(v) || droneAllocationSet.Contains(v)) && v < Program.game.graphConfig.vertexCount)
                        v++;
                    if (v < Program.game.graphConfig.vertexCount)
                    {
                        strategy.droneAllocation[i] = v;
                        droneAllocationSet.Add(v);
                    }
                }

            }

            if (Program.config.checkBaseSignalingStrategy)
            {
                Chromosome c1 = this.MakeCopy();
                c1.defenderStrategies.ForEach(x => x.signalingUnitNeighbourhood.Fill(0));
                c1.defenderStrategies.ForEach(x => x.signalingNoUnitNeighbourhood.Fill(0));
                c1.signalingUnitNeighbourhoodPerNode.Fill(0);
                c1.signalingNoUnitNeighbourhoodPerNode.Fill(0);
                c1.Evaluate();

                Chromosome c2 = this.MakeCopy();
                c2.defenderStrategies.ForEach(x => x.signalingUnitNeighbourhood.Fill(0));
                c2.defenderStrategies.ForEach(x => x.signalingNoUnitNeighbourhood.Fill(1));
                c2.signalingUnitNeighbourhoodPerNode.Fill(0);
                c2.signalingNoUnitNeighbourhoodPerNode.Fill(1);
                c2.Evaluate();

                Chromosome c3 = this.MakeCopy();
                c3.defenderStrategies.ForEach(x => x.signalingUnitNeighbourhood.Fill(1));
                c3.defenderStrategies.ForEach(x => x.signalingNoUnitNeighbourhood.Fill(0));
                c3.signalingUnitNeighbourhoodPerNode.Fill(1);
                c3.signalingNoUnitNeighbourhoodPerNode.Fill(0);
                c3.Evaluate();

                Chromosome c4 = this.MakeCopy();
                c4.defenderStrategies.ForEach(x => x.signalingUnitNeighbourhood.Fill(1));
                c4.defenderStrategies.ForEach(x => x.signalingNoUnitNeighbourhood.Fill(1));
                c4.signalingUnitNeighbourhoodPerNode.Fill(1);
                c4.signalingNoUnitNeighbourhoodPerNode.Fill(1);
                c4.Evaluate();

                Evaluate();
                double maxFittingFunction = new Chromosome[] { c1, c2, c3, c4 }.Max(x => x.fittingFunction);
                if (fittingFunction > maxFittingFunction)
                {
                    if (c1.fittingFunction == maxFittingFunction)
                        Restore(c1);
                    else if (c2.fittingFunction == maxFittingFunction)
                        Restore(c2);
                    else if (c3.fittingFunction == maxFittingFunction)
                        Restore(c3);
                    else if (c4.fittingFunction == maxFittingFunction)
                        Restore(c4);
                }
            }

        }

        public void Init()
        {
            signalingUnitNeighbourhoodPerNode = new int[Program.game.graphConfig.vertexCount];
            signalingNoUnitNeighbourhoodPerNode = new int[Program.game.graphConfig.vertexCount];

            for (int i = 0; i < signalingUnitNeighbourhoodPerNode.Length; i++)
            {
                signalingUnitNeighbourhoodPerNode[i] = Program.rand.Next(2);
                signalingNoUnitNeighbourhoodPerNode[i] = Program.rand.Next(2);
            }

            defenderStrategies = new List<DefenderStrategy>() { GenerateRandomDefenderStrategy() };
            probabilities = new List<double>() { 1.0 };
        }

        public DefenderStrategy GenerateRandomDefenderStrategy()
        {
            DefenderStrategy strategy = new DefenderStrategy();

            //patrollerAllocation
            strategy.patrollerAllocation = new int[(int)Program.game.patrollerCount];
            List<int> randomizedNodes = Enumerable.Range(0, Program.game.graphConfig.vertexCount).ToList();
            randomizedNodes.Permutate();
            for (int i = 0; i < strategy.patrollerAllocation.Length; i++)
                strategy.patrollerAllocation[i] = randomizedNodes[i % randomizedNodes.Count];

            //droneAllocation
            strategy.droneAllocation = new int[(int)Program.game.droneCount];
            randomizedNodes.Permutate();
            for (int i = 0; i < strategy.droneAllocation.Length; i++)
                strategy.droneAllocation[i] = randomizedNodes[i % randomizedNodes.Count];

            //patrollerReallocation
            strategy.patrollerReallocation = new int[(int)Program.game.patrollerCount];
            for (int i = 0; i < strategy.patrollerReallocation.Length; i++)
            {
                List<int> neighbours = Program.game.graphConfig.adjacencyList[strategy.patrollerAllocation[i]];
                strategy.patrollerReallocation[i] = neighbours[Program.rand.Next(neighbours.Count)];
            }

            //signalingUnitNeighbourhood
            strategy.signalingUnitNeighbourhood = new int[(int)Program.game.droneCount];
            for (int i = 0; i < strategy.signalingUnitNeighbourhood.Length; i++)
                strategy.signalingUnitNeighbourhood[i] = Program.rand.Next(2);

            //signalingNoUnitNeighbourhood
            strategy.signalingNoUnitNeighbourhood = new int[(int)Program.game.droneCount];
            for (int i = 0; i < strategy.signalingNoUnitNeighbourhood.Length; i++)
                strategy.signalingNoUnitNeighbourhood[i] = Program.rand.Next(2);

            return strategy;
        }


        public Chromosome MakeCopy()
        {
            Chromosome c = new Chromosome();
            c.fittingFunction = fittingFunction;
            c.attackerResult = attackerResult;
            c.attackerResponse = attackerResponse == null ? null : attackerResponse.MakeCopy();
            c.defenderStrategies = defenderStrategies.Select(x => x.MakeCopy()).ToList();

            c.probabilities = probabilities.Select(x => x).ToList();

            c.signalingUnitNeighbourhoodPerNode = signalingUnitNeighbourhoodPerNode.Select(x => x).ToArray();
            c.signalingNoUnitNeighbourhoodPerNode = signalingNoUnitNeighbourhoodPerNode.Select(x => x).ToArray();

            return c;
        }

    }
}
