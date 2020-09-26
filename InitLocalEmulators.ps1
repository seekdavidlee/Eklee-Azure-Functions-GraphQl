Import-Module "$env:ProgramFiles\Azure Cosmos DB Emulator\PSModules\Microsoft.Azure.CosmosDB.Emulator"
Start-CosmosDbEmulator

"%ProgramFiles(x86)%\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe" init /server "(localdb)\MsSqlLocalDb"
"%ProgramFiles(x86)%\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe" start