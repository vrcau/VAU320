$fileContent = [System.IO.File]::ReadAllText($args[2])
$fileContent -replace '  serializedUdonProgramAsset: .*\r?\n.*\r?\n', ''