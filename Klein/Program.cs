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
            model.AddConstraint("capaciteit", ruw1 + ruw2 + ruw3 + ruw4 <= 100000);
            model.AddConstraint("min_A_in_1", 0.98 * olie1A >= 0.6 * (olie1A + olie1B + olie1C));
            model.AddConstraint("max_C_in_1", 0.98 * olie1C <= 0.35 * (olie1A + olie1B + olie1C));
            model.AddConstraint("max_C_in_2", 0.98 * olie2C <= 0.3 * (olie2A + olie2B + olie2C));
        }
    }
}
