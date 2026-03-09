using Microsoft.Agents.AI.Workflows;
using OpenAI.Graders;
using Shared;
using System;
using System.Collections.Generic;
using System.Text;
using Workflow.AiAssisted.PizzaSample.Models;

namespace Workflow.AiAssisted.PizzaSample.Executors
{
    public class PizzaStockCheckerExecutor : Executor<PizzaOrder, PizzaOrder>
    {
        public PizzaStockCheckerExecutor() : base("StockChecker")
        {
        }

        public override ValueTask<PizzaOrder> HandleAsync(PizzaOrder message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            foreach (var topping in message.Toppings)
            {
                if (topping == "Mushrooms")
                {
                    Utils.WriteLineDarkGray("Checking stock for Mushrooms...");
                    message.Warnings.Add(WarningType.OutOfIngredient, "Mushrooms are running low. Consider removing them or substituting with another topping.");
                }
                else
                {
                    Utils.WriteLineYellow($"Checking stock for {topping}...");
                }
            }

            return ValueTask.FromResult(message);
        }
    }
}
