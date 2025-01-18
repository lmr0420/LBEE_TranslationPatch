
Set-Location $PSScriptRoot

$JsonFiles = Get-ChildItem ./TextMapping
$Charset = (Get-Content ./Files/Charset.txt -Raw).ToCharArray()
foreach($JsonFile in $JsonFiles)
{
    $Source = ConvertFrom-Json -InputObject (Get-Content $JsonFile.FullName -Raw)
    if($null -ne $Source.SELECT)
    {
        foreach($SelectItem in $Source.SELECT)
        {
            if($SelectItem -eq $null)
            {
                break;
            }
            $TranslationCharArray = $SelectItem.Translation.ToCharArray()
            foreach($TranslationChar in $TranslationCharArray)
            {
                if($Charset.Contains($TranslationChar) -eq $false)
                {
                    Write-Output "Invalid Char in $($JsonFile.Name): $($SelectItem.Translation) ($TranslationChar)"
                }
            }
        }
    }
    
}