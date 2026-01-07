/* Steps : 
 * 1: Create an 'Azure AI Foundry' Resource + Deploy Model
 * 2: Add Nuget Packages (Azure.AI.Agents.Persisten, Azure.Identity, Microsoft.Agents.AI.OpenAI)
 * 3: Create an PersistentAgentsClient (Azure Identity)
 * 4 : use the client's Administration to create a new agent
 * 5: use client to get an AIAgent from the persistentAgent's Id
 * 6: Create a new Thread
 * 7: Call like normal
 * 8: (Optional) Clean up - delete the agent from the PersistentAgentsClient
 */

// See https://aka.ms/new-console-template for more information

using Microsoft.Agents.AI;

const string endpoint = "https://projectdemoaso-resource.services.ai.azure.com/api/projects/ProjectDemoAso";
const string model = "gpt-4.1";
const string apiSecret = "";

Azure.AI.Agents.Persistent.PersistentAgentsClient client = 
    new Azure.AI.Agents.Persistent.PersistentAgentsClient(endpoint, new Azure.Identity.DefaultAzureCredential());

// 4: Create a new Agent
var aiFoundryAgent = await client.Administration.CreateAgentAsync(model, "MyFirstAgent", "Some description","You are a nice AI");
// With this line to get the agent's ID from the creation response:

var threadResponse = await client.Threads.CreateThreadAsync();
var threadId = threadResponse.Value.Id;
var agentId = aiFoundryAgent.Value.Id;
// 5: Get the AIAgent from the PersistentAgent's Id
var runResponse = await client.Runs.CreateRunAsync(threadId, agentId);


// 5️⃣ Run the agent
var runId = runResponse.Value.Id;

var run = await client.Runs.GetRunAsync(threadId, runId);
