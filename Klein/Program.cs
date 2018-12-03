using System;
using Microsoft.SolverFoundation;
using Microsoft.SolverFoundation.Common;
using Microsoft.SolverFoundation.Services;

namespace Klein
{
    class Program
    {
        static void Main(string[] args)
        {
            SolverContext context = SolverContext.GetContext();
            Model model = context.CreateModel();

            // DECISIONS
            Decision olie1A = new Decision(Domain.RealNonnegative, "productie_olie1_A");
            Decision olie1B = new Decision(Domain.RealNonnegative, "productie_olie1_B");
            Decision olie1C = new Decision(Domain.RealNonnegative, "productie_olie1_C");
            Decision olie2A = new Decision(Domain.RealNonnegative, "productie_olie2_A");
            Decision olie2B = new Decision(Domain.RealNonnegative, "productie_olie2_B");
            Decision olie2C = new Decision(Domain.RealNonnegative, "productie_olie2_C");
            Decision ruw1 = new Decision(Domain.RealNonnegative, "verwerking_ruw1");
            Decision ruw2 = new Decision(Domain.RealNonnegative, "verwerking_ruw2");
            Decision ruw3 = new Decision(Domain.RealNonnegative, "verwerking_ruw3");
            Decision ruw4 = new Decision(Domain.RealNonnegative, "verwerking_ruw4");

            model.AddDecisions(olie1A, olie1B, olie1C, olie2A, olie2B, olie2C, ruw1, ruw2, ruw3, ruw4);

            // CONSTRAINTS
            model.AddConstraint("capaciteit_Totaal", ruw1 + ruw2 + ruw3 + ruw4 <= 100000);

            model.AddConstraint("min_A_in_1", (0.98 * olie1A) >= (0.6 * (olie1A * 0.98 + olie1B * 0.99 + olie1C * 0.99)));
            model.AddConstraint("max_C_in_1", (0.99 * olie1C) <= (0.35 * (olie1A * 0.98 + olie1B * 0.99 + olie1C * 0.99)));
            model.AddConstraint("max_C_in_2", (0.99 * olie2C) <= (0.3 * (olie2A * 0.98 + olie2B * 0.99 + olie2C * 0.99)));

            model.AddConstraint("capaciteit_A", (olie1A + olie2A) <= (ruw1 * 0.8 + ruw2 * 0.3 + ruw3 * 0.7 + ruw4 * 0.4));
            model.AddConstraint("capaciteit_B", (olie1B + olie2B) <= (ruw1 * 0.1 + ruw2 * 0.3 + ruw3 * 0.1 + ruw4 * 0.5));
            model.AddConstraint("capaciteit_C", (olie1C + olie2C) <= (ruw1 * 0.1 + ruw2 * 0.4 + ruw3 * 0.2 + ruw4 * 0.1));

            // GOAL
            model.AddGoal("winst", GoalKind.Maximize, 0.21 * (olie1A * 0.98 + olie1B * 0.99 + olie1C * 0.99) +
                                                        0.18 * (olie2A * 0.98 + olie2B * 0.99 + olie2C * 0.99) -
                                                        (0.14 * ruw1 + 0.10 * ruw2 + 0.15 * ruw3 + 0.12 * ruw4));

            // OUTPUT
            Solution solution = context.Solve(new SimplexDirective());
            Console.Write(solution.GetReport());
            Console.Read();
        }
    }
}
