$AZURE_OPENAI_API_KEY = "5a587061c4f544b8a2af61f8b72042ec"

curl https://rdc-openai-sweden.openai.azure.com/openai/assistants?api-version=2024-02-15-preview -H "api-key: $AZURE_OPENAI_API_KEY" 

curl https://rdc-openai-sweden.openai.azure.com/openai/assistants/asst_sNae8qHZdwyes9i03SUCTbtx?api-version=2024-02-15-preview -H "api-key: $AZURE_OPENAI_API_KEY" -H 'Content-Type: application/json' -X DELETE

