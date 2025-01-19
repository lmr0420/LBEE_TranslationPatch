$x = Get-Content -Path "C:\Users\JackMyth\Desktop\LBEEProgramText.txt"
$out = @()
foreach($i in $x)
{
    $PendingOut = $i.Replace("`$__d","\n")  
    $out += @{
        "Source"=$PendingOut
        "Target"=$PendingOut
    }
}
Set-Content -Path "C:\Users\JackMyth\Desktop\LBEEProgramText.json" -Value (ConvertTo-Json $out)