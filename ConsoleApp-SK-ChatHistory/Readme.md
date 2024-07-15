# ConsoleApp-SK-ChatHistory
**Update:** 7/14/2024 - Wrote this code over the weekend.

The goal here is to implement classes that allow us to store and retrieve ChatHistory for users per session, like you see on the ChatGPT website.   I have found a few repos with some logic for this, but I do not necessarily agree with the approach that was used so I decided to roll my own.

I decided to first start by writing a CosmosDbService class and to get this working from a very simple Console App.  Once I am done with this, I will write a ChatService Class that leverages the CosmosDBService Class to store the AI prompts along with Completion and Token metrics.  At some point I will add logic for a semantic cache and the idea there is to cache the prompts and completions using the new **Vector Embeddings** ***preview*** feature of CosmosDB, which has support for TTL (time to live).  What this will allow me to do, is set the expiry for cached items to 1 day and when a user provides a prompt, we can first check the cache and if a result is found returned the cached completion instead of calling the LLM again.

My plan is to implement the Caching feature as a feature so it can be turned on or off. 
 
