using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Shared;
using System;
using System.Collections.Generic;
using System.Text;
using Workflow.AiAssisted.PizzaSample.Models;

namespace Workflow.AiAssisted.PizzaSample.Executors
{
    public class PizzaWarningExecutor:Executor<PizzaOrder>
    {
        private readonly AIAgent warningToCustomerAgent;
        public PizzaWarningExecutor(AIAgent aIAgent): base("PizzaWarning")
        {
            warningToCustomerAgent = aIAgent;
        }


        public async override ValueTask HandleAsync(PizzaOrder message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            Utils.WriteLineRed("Can't create the pizza in full");
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<WarningType, string> warning in message.Warnings)
            {
                sb.Append($" - {warning.Key}: {warning.Value}");
            }

            AgentResponse response = await warningToCustomerAgent.RunAsync($"Explain to the user can't we can't for-fill their order to the following: {sb}");
            Console.WriteLine("Send as email: " + response);
        }
    }
}
