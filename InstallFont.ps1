$FONTS = 0x14
$FromPath=".\fonts"

$objShell = New-Object -ComObject Shell.Application
$objFolder = $objShell.Namespace($FONTS)

$CopyOptions = 4 + 16
$CopyFlag = [String]::Format("{0:x}", $CopyOptions)

foreach($File in $(Ls $Frompath)) {
  If (test-path "c:\windows\fonts\$($file.name)") 
     {"Font already exists - not copying"}  #Useful for testing
  Else 
     {
      $copyFlag = [String]::Format("{0:x}", $CopyOptions)
      "copying $($file.fullname)"           # Useful for debugging
      $objFolder.CopyHere($File.fullname, $CopyOptions)
     }
}
