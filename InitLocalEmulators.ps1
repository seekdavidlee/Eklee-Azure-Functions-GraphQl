Import-Module "$env:ProgramFiles\Azure Cosmos DB Emulator\PSModules\Microsoft.Azure.CosmosDB.Emulator"
Start-CosmosDbEmulator

$storageEmulatorPath = (${env:ProgramFiles(x86)} + "\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe") 

& $storageEmulatorPath init /server "(localdb)\MsSqlLocalDb"
& $storageEmulatorPath start