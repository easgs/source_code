namespace GeneticSignalGame
{
    public class Config
    {
        public string configName;

        public bool mutateStandard = true;

        public bool goodInitialStrategies;

        public bool newIndividualsInGeneration;

        public bool mutationAddPureStrategy;

        public bool mutationDeletePureStrategy;

        public bool mutationChangeProbability;

        public bool mutationSwitchProbability;

        public bool mutationBetterPayoff;

        public bool mutationWeakestPureStrategy;

        public bool mutationWeakestPureStrategyProportional;

        public bool crossoverWithPayoff;

        public bool mutationDefendAttackerTarget;

        public bool crossoverSwap;

        public bool warmup;

        public bool checkBaseSignalingStrategy;
    }
}
