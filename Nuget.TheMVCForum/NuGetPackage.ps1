Param (
	[switch]$Publish
)

$ErrorActionPreference = "Stop"
$global:ExitCode = 1
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition #set working directory to directory this script is in
function Write-Log {

	#region Parameters
	
		[cmdletbinding()]
		Param(
			[Parameter(ValueFromPipeline=$true)]
			[array] $Messages,

			[Parameter()] [ValidateSet("Error", "Warn", "Info")]
			[string] $Level = "Info",
			
			[Parameter()]
			[Switch] $NoConsoleOut = $false,
			
			[Parameter()]
			[String] $ForegroundColor = 'White',
			
			[Parameter()] [ValidateRange(1,30)]
			[Int16] $Indent = 0,

			[Parameter()]
			[IO.FileInfo] $Path = ".\NuGet.log",
			
			[Parameter()]
			[Switch] $Clobber,
			
			[Parameter()]
			[String] $EventLogName,
			
			[Parameter()]
			[String] $EventSource,
			
			[Parameter()]
			[Int32] $EventID = 1
			
		)
		
	#endregion

	Begin {}

	Process {
		
		$ErrorActionPreference = "Continue"

		if ($Messages.Length -gt 0) {
			try {			
				foreach($m in $Messages) {			
					if ($NoConsoleOut -eq $false) {
						switch ($Level) {
							'Error' { 
								Write-Error $m -ErrorAction SilentlyContinue
								Write-Host ('{0}{1}' -f (" " * $Indent), $m) -ForegroundColor Red
							}
							'Warn' { 
								Write-Warning $m 
							}
							'Info' { 
								Write-Host ('{0}{1}' -f (" " * $Indent), $m) -ForegroundColor $ForegroundColor
							}
						}
					}

					if ($m.Trim().Length -gt 0) {
						$msg = '{0}{1} [{2}] : {3}' -f (" " * $Indent), (Get-Date -Format "yyyy-MM-dd HH:mm:ss"), $Level.ToUpper(), $m
	
						if ($Clobber) {
							$msg | Out-File -FilePath $Path -Force
						} else {
							$msg | Out-File -FilePath $Path -Append
						}
					}
			
					if ($EventLogName) {
			
						if (-not $EventSource) {
							$EventSource = ([IO.FileInfo] $MyInvocation.ScriptName).Name
						}
			
						if(-not [Diagnostics.EventLog]::SourceExists($EventSource)) { 
							[Diagnostics.EventLog]::CreateEventSource($EventSource, $EventLogName) 
						} 

						$log = New-Object System.Diagnostics.EventLog  
						$log.set_log($EventLogName)  
						$log.set_source($EventSource) 
				
						switch ($Level) {
							"Error" { $log.WriteEntry($Message, 'Error', $EventID) }
							"Warn"  { $log.WriteEntry($Message, 'Warning', $EventID) }
							"Info"  { $log.WriteEntry($Message, 'Information', $EventID) }
						}
					}
				}
			} 
			catch {
				throw "Failed to create log entry in: '$Path'. The error was: '$_'."
			}
		}
	}

	End {}

	<#
		.SYNOPSIS
			Writes logging information to screen and log file simultaneously.

		.DESCRIPTION
			Writes logging information to screen and log file simultaneously. Supports multiple log levels.

		.PARAMETER Messages
			The messages to be logged.

		.PARAMETER Level
			The type of message to be logged.
			
		.PARAMETER NoConsoleOut
			Specifies to not display the message to the console.
			
		.PARAMETER ConsoleForeground
			Specifies what color the text should be be displayed on the console. Ignored when switch 'NoConsoleOut' is specified.
		
		.PARAMETER Indent
			The number of spaces to indent the line in the log file.

		.PARAMETER Path
			The log file path.
		
		.PARAMETER Clobber
			Existing log file is deleted when this is specified.
		
		.PARAMETER EventLogName
			The name of the system event log, e.g. 'Application'.
		
		.PARAMETER EventSource
			The name to appear as the source attribute for the system event log entry. This is ignored unless 'EventLogName' is specified.
		
		.PARAMETER EventID
			The ID to appear as the event ID attribute for the system event log entry. This is ignored unless 'EventLogName' is specified.

		.EXAMPLE
			PS C:\> Write-Log -Message "It's all good!" -Path C:\MyLog.log -Clobber -EventLogName 'Application'

		.EXAMPLE
			PS C:\> Write-Log -Message "Oops, not so good!" -Level Error -EventID 3 -Indent 2 -EventLogName 'Application' -EventSource "My Script"

		.INPUTS
			System.String

		.OUTPUTS
			No output.
			
		.NOTESD:\GITHUB\mvcforum\Nuget.TheMVCForum\content\dummy.txt
			Revision History:
				2011-03-10 : Andy Arismendi - Created.
	#>
}

function Create-Process() {
	param([string] $fileName, [string] $arguments)
	Write-Host "Creating NuGet Process.." -ForegroundColor Green
	Write-Host "  File: $fileName" -ForegroundColor Green
	Write-Host "  File: $arguments" -ForegroundColor Green
	$pinfo = New-Object System.Diagnostics.ProcessStartInfo
	$pinfo.FileName = $fileName
	$pinfo.RedirectStandardError = $true
	$pinfo.RedirectStandardOutput = $true
	$pinfo.UseShellExecute = $false
	
	$pinfo.Arguments = $arguments
	$p = New-Object System.Diagnostics.Process
	$p.StartInfo = $pinfo
	return $p
}

function HandlePublishError {
	param([string] $ErrorMessage)

	# Run NuGet Setup
	$encodedMessage = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($ErrorMessage))
	$setupTask = Start-Process PowerShell.exe "-ExecutionPolicy Unrestricted -File .\NuGetSetup.ps1 -Url $url -Base64EncodedMessage $encodedMessage" -Wait -PassThru

	#Write-Log ("NuGet Setup Task Exit Code: " + $setupTask.ExitCode)

	if ($setupTask.ExitCode -eq 0) {
		# Try to push package again
		$publishTask = Create-Process .\NuGet.exe ("push " + $_.Name + " -Source " + $url)
		$publishTask.Start() | Out-Null
		$publishTask.WaitForExit()
			
		$output = ($publishTask.StandardOutput.ReadToEnd() -Split '[\r\n]') |? {$_}
		$error = (($publishTask.StandardError.ReadToEnd() -Split '[\r\n]') |? {$_}) 
		Write-Log $output
		Write-Log $error Error

		if ($publishTask.ExitCode -eq 0) {
			$global:ExitCode = 0
		}
	}
	elseif ($setupTask.ExitCode -eq 2) {
		$global:ExitCode = 2
	}
	else {
		$global:ExitCode = 0
	}
}

function Publish {

	Write-Log " "
	Write-Log "Publishing package..." -ForegroundColor Green

	# Get nuget config
	[xml]$nugetConfig = Get-Content .\NuGet.Config
	
	$nugetConfig.configuration.packageSources.add | ForEach-Object {
		$url = $_.value

		Write-Log "Repository Url: $url"
		Write-Log " "

		Get-ChildItem *.nupkg | Where-Object { $_.Name.EndsWith(".symbols.nupkg") -eq $false } | ForEach-Object { 

			# Try to push package
			$task = Create-Process .\NuGet.exe ("push " + $_.Name + " -Source " + $url)
			$task.Start() | Out-Null
			$task.WaitForExit()
			
			$output = ($task.StandardOutput.ReadToEnd() -Split '[\r\n]') |? { $_ }
			$error = ($task.StandardError.ReadToEnd() -Split '[\r\n]') |? { $_ }
			Write-Log $output
			Write-Log $error Error
		   
			if ($task.ExitCode -gt 0) {
				HandlePublishError -ErrorMessage $error
				#Write-Log ("HandlePublishError() Exit Code: " + $global:ExitCode)
			}
			else {
				$global:ExitCode = 0
			}                
		}
	}
}

Write-Log " "
Write-Log "NuGet Packager 2.0.3" -ForegroundColor Yellow

# Make sure the nuget executable is writable
Set-ItemProperty NuGet.exe -Name IsReadOnly -Value $false

# Make sure the nupkg files are writeable and create backup
if (Test-Path *.nupkg) {
	Set-ItemProperty *.nupkg -Name IsReadOnly -Value $false

	Write-Log " "
	Write-Log "Creating backup..." -ForegroundColor Green

	Get-ChildItem *.nupkg | ForEach-Object { 
		Move-Item $_.Name ($_.Name + ".bak") -Force
		Write-Log ("Renamed " + $_.Name + " to " + $_.Name + ".bak")
	}
}

Write-Log " "
Write-Log "Updating NuGet..." -ForegroundColor Green
Write-Log (Invoke-Command {.\NuGet.exe update -Self} -ErrorAction Stop)

Write-Log " "
Write-Log "Creating package..." -ForegroundColor Green

#Ryios: I modified the way nuget was being called here because redirecting standard output with ProcessStartInfo was hanging often
#do to the fact that if the buffer fills up during WaitToExit() it will hang, and even more so if the REadToEnd() buffer isn't disposed, and it wasn't being disposed
#it's a lot easier to just do it inline to get the success ouput, and a try catch to get an error if one occurrs..
#
#I also got rid of the redunant if block for generating pdbs and moved it into the same try catch
$packOutput = $null
try
{
	# Create symbols package if any .pdb files are located in the lib folder	
	If ((Get-ChildItem *.pdb -Path .\lib -Recurse).Count -gt 0) {
		$packOutput = .\nuget pack $scriptPath\Package.nuspec -Symbol -Verbosity Detailed | Out-String #this is so much simpler.. and doesn't hang
	}else {
		$packOutput = .\nuget pack $scriptPath\Package.nuspec -Verbosity Detailed | Out-String #this is so much simpler.. and doesn't hang
	}
	$global:ExitCode = 0 #retained this so publish will run if the nuget package project is in release mode
	Write-Log PackageResult-Success
}
catch [Exception] {
	$packOutput = $_.Exception.ToString()
	$global:ExitCode = -1
	Write-Log PackageResult-Error
}
finally {
	Write-Log ------------------------------------
	Write-Log $packOutput
}
#Old Code (old code, left the process pieces here for reference)
#$packageTask = Create-Process NuGet.exe ("pack $scriptPath\Package.nuspec -Verbosity Detailed")
#$output = ($packageTask.StandardOutput.ReadToEnd() -Split '[\r\n]') |? {$_}
#$error = (($packageTask.StandardError.ReadToEnd() -Split '[\r\n]') |? {$_}) 
#Write-Log $output
#Write-Log $error Error
#$global:ExitCode = $packageTask.ExitCode

# Check if package should be published
if ($Publish -and $global:ExitCode -eq 0) {
	Write-Log ""
	Publish
}

Write-Log " "
Write-Log "Exit Code: $global:ExitCode" -ForegroundColor Gray

$host.SetShouldExit($global:ExitCode)
Exit $global:ExitCode