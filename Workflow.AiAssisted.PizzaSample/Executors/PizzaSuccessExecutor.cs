using Microsoft.Agents.AI.Workflows;
using System;
using System.Collections.Generic;
using System.Text;
using Workflow.AiAssisted.PizzaSample.Models;

namespace Workflow.AiAssisted.PizzaSample.Executors
{
    public class PizzaSuccessExecutor : Executor<PizzaOrder>
    {
        public PizzaSuccessExecutor() : base("PizzaSuccessExecutor")
        {
        }

        public override ValueTask HandleAsync(PizzaOrder message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"Order for {message.Size} {message.Size} crust pizza with toppings: {string.Join(", ", message.Toppings)} has been placed successfully!");
            return ValueTask.CompletedTask;
        }
    }
}
