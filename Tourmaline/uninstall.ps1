Get-Content $profile | Where-Object {$_ -nomatch "Set-Alias tourmaline ${pwd}\Tourmaline.exe"} | Set-Content $profile;
. $profile;