using System.Linq;

namespace GeneticSignalGame
{
    public class AttackerStrategy
    {
        public int target;

        public int[] signalReactions;

        public AttackerStrategy MakeCopy()
        {
            AttackerStrategy at = new AttackerStrategy();
            at.target = target;
            at.signalReactions = signalReactions == null ? null : signalReactions.Select(x => x).ToArray();
            return at;
        }

    }
}
