# Runs every time a package is installed in a project

param($installPath, $toolsPath, $package, $project)

# $installPath is the path to the folder where the package is installed.
# $toolsPath is the path to the tools directory in the folder where the package is installed.
# $package is a reference to the package object.
# $project is a reference to the project the package was installed to.


if ($project) {
	#$dateTime = Get-Date -Format yyyyMMdd-HHmmss
	#$backupPath = Join-Path (Split-Path $project.FullName -Parent) "\App_Data\NuGetBackup\$dateTime"
	#$copyLogsPath = Join-Path $backupPath "CopyLogs"
	#$projectDestinationPath = Split-Path $project.FullName -Parent

	# Create backup folder and logs folder if it doesn't exist yet
	#New-Item -ItemType Directory -Force -Path $backupPath
	#New-Item -ItemType Directory -Force -Path $copyLogsPath
	
	# Create a backup of original web.config
	#$webConfigSource = Join-Path $projectDestinationPath "Web.config"
	#Copy-Item $webConfigSource $backupPath -Force
	
	#$copyWebconfig = $true
	#$destinationWebConfig = Join-Path $projectDestinationPath "Web.config"

	#if(Test-Path $destinationWebConfig) 
	#{
	#	Try 
	#	{
	#		[xml]$config = Get-Content $destinationWebConfig
	#		
	#		$config.configuration.appSettings.ChildNodes | ForEach-Object { 
	#			if($_.key -eq "umbracoConfigurationStatus") 
	#			{
	#				# The web.config has an umbraco-specific appSetting in it
	#				# don't overwrite it and let config transforms do their thing
	#				$copyWebconfig = $false 
	#			}
	#		}
	#	} 
	#	Catch { }
	#}
	
	#$installFolder = Join-Path $projectDestinationPath "Install"
	#if(Test-Path $installFolder) {
	#	Remove-Item $installFolder -Force -Recurse -Confirm:$false
	#}
	
	# Open appropriate readme
	#$DTE.ItemOperations.OpenFile($toolsPath + '\Readme.txt')

}