# Semantic Kernel Console App Example Playground

In this Solution / Repo I have several Console Apps that I demo various SK examples.  I often leverage console apps to learn new concepts and to play with different patterns as it's a quick way to get up an running.  I hope you find this repo useful.

## Console-SK-DeFi-Assistant
In this example, I create a custom SK plugin for the Uniswap V3 Subgraph. I also make use of ToolCallBehavior.AutoInvokeKernelFunctions setting so the Kernel will automatically determine if there is a Native Function it needs to invoke.  

When you run the example, you can ask general questions like you would with any ChatBot, but if you ask a question that aligns to the description of the SK Function the Kernel will invoke the function passing in a GraphQL query.  What is interesting about this, is the the AI will general the GraphQL query for you, or you can actually provide a GraphQL query.

In this example, I am not streaming the Chat Completion so you have to wait for the AI to finish.  If you are interesting in seeing a Streaming example, take a look at the next example.

## Console-SK-DeFi-Assistant-Streaming
This is the same example as the above, but I have implementing Streaming logic, which results in a better Chat experience, and would be the recommended approach for a production solution.

## Notes
This concept could be used with any GraphQL endpoint.  I will explore with other GraphQL endpoints when I have time, but the UniSwap V3 GraphQL endpoint is very well know in the Cryto space.


## Requirements for this example

1. Rename the App.config.bak to App.config
2. Modify the App.config with your LLM/AI details.

    ~~~
			<?xml version="1.0" encoding="utf-8" ?>
			<configuration>
				<appSettings>
					<add key="AzureOpenAIEndpoint" value="<AzureOpenAI Endpoint URI>" />
					<add key="AzureOpenAIKey" value="AzureOpenAI KEY" />  
					<add key="AzureOpenAIModel" value="AzureOpenAI Model Name" />
				</appSettings>
			</configuration>
    ~~~
 