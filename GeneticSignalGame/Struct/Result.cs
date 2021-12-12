namespace GeneticSignalGame.Struct
{
    public class Result
    {
        public double trainingTime;

        public Payoff payoff;
    }

    public class Payoff
    {
        public Payoff()
        {
            defender = attacker = 0;
        }

        public Payoff(double _defender, double _attacker)
        {
            defender = _defender;
            attacker = _attacker;
        }

        public double defender = 0;

        public double attacker = 0;

        public static Payoff operator +(Payoff a, Payoff b)
        {
            Payoff result = new Payoff();
            result.defender = a.defender + b.defender;
            result.attacker = a.attacker + b.attacker;
            return result;
        }

        public static Payoff operator *(Payoff p, double d)
        {
            Payoff result = new Payoff();
            result.defender = p.defender * d;
            result.attacker = p.attacker * d;
            return result;
        }
    }
}
