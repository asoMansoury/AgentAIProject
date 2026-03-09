using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Text;

namespace Workflow.AiAssisted.PizzaSample
{
    public class AgentFactory(string chatDeployment,string azureOpenAiEndpoint,string azureApiKey)
    {
        public AIAgent CreateLorderTakerAgent()
        {
            return CreateAzureOpenAiClient()
                .GetChatClient(chatDeployment)
                .AsAIAgent("You are a Pizza Order Taker, parsing the customer order")
                .AsBuilder()
                .Build();
        }

        public AIAgent CreateWarningToCustomerAgent()
        {
            return CreateAzureOpenAiClient()
                .GetChatClient(chatDeployment)
                .AsAIAgent("You are a Pizza Confirmer. That need to explain to a user if pizza order can't be met")
                .AsBuilder()
                .Build();
        }

        private AzureOpenAIClient CreateAzureOpenAiClient()
        {
            return new AzureOpenAIClient(new Uri(azureOpenAiEndpoint), new System.ClientModel.ApiKeyCredential(azureApiKey));
        }
    }

  
}
