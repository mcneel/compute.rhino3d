$url = "http://files.mcneel.com/dujour/exe/20171215/rhino_en-us_6.1.17349.22391.exe"
$output = "rhino6install.exe"
(New-Object System.Net.WebClient).DownloadFile($url, $output)