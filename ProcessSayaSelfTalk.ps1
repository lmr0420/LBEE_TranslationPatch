$Source = ConvertFrom-Json -InputObject (Get-Content "C:\Users\JackMyth\Documents\Source\LBEE_TranslationPatch\TextMapping\SEEN2830.json" -Raw)
$NewMessages = @()
$Messages =  $Source.MESSAGE
for($i = 0;$i -lt $Messages.Count;$i++)
{
    $NewItem = [ordered]@{
        "JP"=$Messages[$i].JP
        "EN"=$Messages[$i].EN
        "Translation"=$Messages[$i].Translation
    }
    if($NewItem.JP.StartsWith("`$d"))
    {
        $NewItem.Translation = "`$d　" + $NewItem.Translation
    }
    $NewMessages+=$NewItem
}
$OutJson = @{
    "MESSAGE"=$NewMessages
}

Set-Content -Path "C:\Users\JackMyth\Documents\Source\LBEE_TranslationPatch\TextMapping\SEEN2830.out.json" -Value (ConvertTo-Json -InputObject $OutJson)

echo ",\n.*Translation.*" > nul