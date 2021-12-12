using GeneticSignalGame.Struct;
using System.Linq;

namespace GeneticSignalGame
{
    public class DefenderStrategy
    {
        public int[] patrollerAllocation;

        public int[] droneAllocation;

        public int[] patrollerReallocation;

        public int[] signalingUnitNeighbourhood;

        public int[] signalingNoUnitNeighbourhood;

        public Payoff partialPayoff;

        public DefenderStrategy MakeCopy()
        {
            DefenderStrategy ds = new DefenderStrategy();
            ds.patrollerAllocation = patrollerAllocation.Select(x => x).ToArray();
            ds.droneAllocation = droneAllocation.Select(x => x).ToArray();
            ds.patrollerReallocation = patrollerReallocation.Select(x => x).ToArray();
            ds.signalingUnitNeighbourhood = signalingUnitNeighbourhood.Select(x => x).ToArray();
            ds.signalingNoUnitNeighbourhood = signalingNoUnitNeighbourhood.Select(x => x).ToArray();
            if (partialPayoff != null)
                ds.partialPayoff = new Payoff(partialPayoff.defender, partialPayoff.attacker);

            return ds;
        }

    }
}
