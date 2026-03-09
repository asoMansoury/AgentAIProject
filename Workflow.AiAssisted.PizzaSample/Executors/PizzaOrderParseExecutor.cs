using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Workflow.AiAssisted.PizzaSample.Models;

namespace Workflow.AiAssisted.PizzaSample.Executors
{
    public class PizzaOrderParseExecutor : Executor<string, PizzaOrder>
    {
        private readonly AIAgent _agent;

        public PizzaOrderParseExecutor(AIAgent agent)
            : base("PizzaOrderTaker")
        {
            _agent = agent;
        }

        public async override ValueTask<PizzaOrder> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            Utils.WriteLineYellow($"Parsing customer order: {message}");
            var response = await _agent.RunAsync(new Microsoft.Extensions.AI.ChatMessage(Microsoft.Extensions.AI.ChatRole.User, message));


            // Fix: Use response.Text and System.Text.Json.JsonSerializer.Deserialize
            PizzaOrder pizzaOrder = System.Text.Json.JsonSerializer.Deserialize<PizzaOrder>(
                response.Text,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
                    Converters = { new JsonStringEnumConverter() }
                }
            );
            // In a real implementation, you would want to add error handling and validation here
            return pizzaOrder;
        }
    }
}
