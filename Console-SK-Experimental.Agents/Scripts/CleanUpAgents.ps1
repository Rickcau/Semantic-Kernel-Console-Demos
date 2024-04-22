## 5a587061c4f544b8a2af61f8b72042ec
## "https://rdc-openai-sweden.openai.azure.com"

# Set your Azure API key
$AZURE_OPENAI_API_KEY = "<YOUR KEY>"  # Replace with your actual API key
$AZURE_OPENAI_ENDPOINT = "https://<YOUR_ENDPOINT>.openai.azure.com"

# Define the URL
$url = "$($AZURE_OPENAI_ENDPOINT)/openai/assistants?api-version=2024-02-15-preview"

# Make the GET request
$response = Invoke-RestMethod -Uri $url -Headers @{
    "api-key" = $AZURE_OPENAI_API_KEY
}

# Store the response in a file
$response | ConvertTo-Json | Out-File -FilePath "response.json"

# Access the data from the response
$assistants = $response.data

# Iterate through each assistant and perform further actions if needed
$count = 0
foreach ($assistant in $assistants) {
    $assistantId = $assistant.id
    Write-Host "Assistant ID: $assistantId"
    $url = "https://rdc-openai-sweden.openai.azure.com/openai/assistants/$($assistantId)?api-version=2024-02-15-preview"
    Write-Host $url
    try {
        # Make the DELETE call
        Invoke-RestMethod -Uri $url -Headers @{
            "api-key" = $AZURE_OPENAI_API_KEY
        } -Method Delete
        Write-Host "Deleted assistant with ID: $assistantId"
        count = ++
    } catch {
        if ($_.Exception.Response.StatusCode -eq 404) {
            Write-Host "Assistnt with ID $assistantId not found.  Skipping..."
        } else {
            Write-Host "Error delete assistant iwht ID $assistantId : $($_.Exception.Message)"
        }
    }
    
}

Write-Host "We are all cleaned up now. We deleted $count Assistants!"