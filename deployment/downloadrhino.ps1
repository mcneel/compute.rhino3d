$url = "http://files.mcneel.com/dujour/exe/20180301/rhino_en-us_6.2.18060.16521.exe"
$output = "rhino6install.exe"
(New-Object System.Net.WebClient).DownloadFile($url, $output)