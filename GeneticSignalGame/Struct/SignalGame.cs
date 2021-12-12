using System.Collections.Generic;

namespace GeneticSignalGame.Struct
{
    public class SignalGame
    {
        public string id;

        public double gamma;

        public double kappa;

        public double lambda;

        public double mu;

        public double patrollerCount;

        public double droneCount;

        public Graph graphConfig;

        public List<double> defenderReward;

        public List<double> defenderPenalty;

        public List<double> attackerPenalty;

        public List<double> attackerReward;

        public List<AttackerStrategy> attackerStrategies;

        public void GenerateAllAttackerStrategies()
        {
            attackerStrategies = new List<AttackerStrategy>();
            for (int i=0; i<graphConfig.vertexCount; i++)
                for (int weakSignalReaction = 0; weakSignalReaction <= 1; weakSignalReaction++)
                    for (int strongSignalReaction = 0; strongSignalReaction <= 1; strongSignalReaction++)
                    {
                        AttackerStrategy attackerStrategy = new AttackerStrategy();
                        attackerStrategy.target = i;
                        attackerStrategy.signalReactions = new int[] { 0, weakSignalReaction, strongSignalReaction };
                        attackerStrategies.Add(attackerStrategy);
                    }
        }
    }
}
