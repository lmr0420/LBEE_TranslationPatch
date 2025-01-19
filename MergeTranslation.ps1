$Source = ConvertFrom-Json -InputObject (Get-Content "C:\Users\JackMyth\Documents\Source\LBEE_TranslationPatch\Source.json" -Raw)
$Processed = ConvertFrom-Json -InputObject (Get-Content "C:\Users\JackMyth\Documents\Source\LBEE_TranslationPatch\Processed.json"  -Raw)
if($Source.Count -ne $Processed.Count)
{
    Write-Output "数量不匹配!";
    exit
}
$Merged = @()

for($i = 0;$i -lt $Source.Count;$i++)
{
    $NewItem = [ordered]@{
        "JP"=$Source[$i].JP
        "EN"=$Source[$i].EN
        "Translation"=$Processed[$i]
    }
    $Merged+=$NewItem
}

Set-Content -Path "C:\Users\JackMyth\Documents\Source\LBEE_TranslationPatch\Merged.json" -Value (ConvertTo-Json -InputObject $Merged)

echo ",\n.*Translation.*" > nul