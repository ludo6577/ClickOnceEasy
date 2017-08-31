######################################################################################################
# More info here: https://msdn.microsoft.com/en-us/library/xc3tc5xx.aspx
# This answer in SO can be usefull too: https://stackoverflow.com/a/8192093/2576706
# 
######################################################################################################

###### Variables ######
$Version = "1.0.0.0"
$AppName = "SoftFluentApp"  	#  do not call the Unity project the same as $AppName in deploy script
$Publisher = "SoftFluent"
$ServerUrl = "\\sfsrv03\Temp\lfe\Deployment"

# Directory
$BinDirectory = "Bin" 			# Unity compiled application (everything in this folder will be compressed!)
$OutputDirectory = "Output"		# Output (manifests and compressed archive)
$ToolsDirectory = "Tools"		# Launcher.exe (a simple console application that launch the Unity Project and unzip it if necessary)


########################################################################
# 1) Prepare the application in the Temp folder
# Clear Output directory (create if doesn't exists)
New-Item -ItemType Directory -Force -Path $OutputDirectory
Remove-Item $OutputDirectory\* -recurse

# Compress the Unity application from the bin directory and put it in the Output directory
Add-Type -Assembly System.IO.Compression.FileSystem
$compressionLevel = [System.IO.Compression.CompressionLevel]::Optimal
[System.IO.Compression.ZipFile]::CreateFromDirectory($BinDirectory, "$OutputDirectory\App.zip", $compressionLevel, $false)

# Then add the Launcher.exe in Output directory (and rename it to the name of the project)
Copy-Item -Path $ToolsDirectory\Launcher.exe $OutputDirectory
Rename-Item $OutputDirectory\Launcher.exe "$AppName.exe"


########################################################################
# 2) Manifest Creation
.\mage -New Application -name "$AppName" -Version $Version -FromDirectory $OutputDirectory\ -ToFile "$OutputDirectory\$AppName.exe.manifest"

# TODO: sign the manifest with Authenticode certificate (Replace mycert.pfx with the path to your certificate file. Replace passwd with the password for your certificate file.)
#mage -Sign $AppName.exe.manifest -CertFile mycert.pfx -Password passwd  


########################################################################
# 3) Installer Generation
.\mage -New Deployment -name "$AppName" -Version $Version -Install true -Publisher $Publisher -ProviderUrl "$ServerUrl\$AppName.application" -AppManifest "$OutputDirectory\$AppName.exe.manifest" -ToFile "$OutputDirectory\$AppName.application"  

# TODO: Sign the deployment manifest
# mage -Sign $AppName.application -CertFile mycert.pfx -Password passwd  


########################################################################
# 4) Deployment
# Copy all of the files in the deployment directory to the deployment destination or media.
Remove-Item $ServerUrl\* -recurse
Copy-Item -Path $OutputDirectory\* -Destination $ServerUrl\ -Container;




