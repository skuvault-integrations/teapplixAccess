<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)

.Description
	How to use this script and build the module:

	Get the utility script Invoke-Build.ps1:
	https://github.com/nightroman/Invoke-Build

	Copy it to the path. Set location to this directory. Build:
	PS> Invoke-Build Build

	This command builds the module and installs it to the $ModuleRoot which is
	the working location of the module. The build fails if the module is
	currently in use. Ensure it is not and then repeat.

	The build task Help fails if the help builder Helps is not installed.
	Ignore this or better get and use the script (it is really easy):
	https://github.com/nightroman/Helps
#>

param
(
	$Configuration = 'Release',
	$logfile = $null
)

$project_name = "TeapplixAccess"

# Folder structure:
# \build - Contains all code during the build process
# \build\artifacts - Contains all files during intermidiate bulid process
# \build\output - Contains the final result of the build process
# \release - Contains final release files for upload
# \release\archive - Contains files archived from the previous builds
# \src - Contains all source code
$build_dir = "$BuildRoot\build"
$log_dir = "$BuildRoot\log"
$build_artifacts_dir = "$build_dir\artifacts"
$build_output_dir = "$build_dir\output"
$release_dir = "$BuildRoot\release"
$archive_dir = "$release_dir\archive"

$src_dir = "$BuildRoot\src"
$solution_file = "$src_dir\TeapplixAccess.sln"
	
# Use MSBuild.
use Framework\v4.0.30319 MSBuild

task Clean { 
	exec { MSBuild "$solution_file" /t:Clean /p:Configuration=$configuration /v:quiet } 
	Remove-Item -force -recurse $build_dir -ErrorAction SilentlyContinue | Out-Null
}

task Init Clean, { 
    New-Item $build_dir -itemType directory | Out-Null
    New-Item $build_artifacts_dir -itemType directory | Out-Null
    New-Item $build_output_dir -itemType directory | Out-Null
}

task Build {
	exec { MSBuild "$solution_file" /t:Build /p:Configuration=$configuration /v:minimal /p:OutDir="$build_artifacts_dir\" }
}

task Package  {
	New-Item $build_output_dir\TeapplixAccess\lib\net45 -itemType directory -force | Out-Null
	Copy-Item $build_artifacts_dir\TeapplixAccess.??? $build_output_dir\TeapplixAccess\lib\net45 -PassThru |% { Write-Host "Copied " $_.FullName }
}

# Set $script:Version = assembly version
task Version {
	assert (( Get-Item $build_artifacts_dir\TeapplixAccess.dll ).VersionInfo.FileVersion -match '^(\d+\.\d+\.\d+)')
	$script:Version = $matches[1]
}

task Archive {
	New-Item $release_dir -ItemType directory -Force | Out-Null
	New-Item $archive_dir -ItemType directory -Force | Out-Null
	Move-Item -Path $release_dir\*.* -Destination $archive_dir
}

task Zip Version, {
	$release_zip_file = "$release_dir\$project_name.$Version.zip"
	
	Write-Host "Zipping release to: " $release_zip_file
	
	exec { & 7za.exe a $release_zip_file $build_output_dir\TeapplixAccess\lib\net45\* -mx9 }
}

task NuGet Package, Version, {

	Write-Host ================= Preparing TeapplixAccess Nuget package =================
	$text = "Teapplix webservices API wrapper."
	# nuspec
	Set-Content $build_output_dir\TeapplixAccess\TeapplixAccess.nuspec @"
<?xml version="1.0"?>
<package>
	<metadata>
		<id>TeapplixAccess</id>
		<version>$Version-alpha4</version>
		<authors>Slav Ivanyuk</authors>
		<owners>Slav Ivanyuk</owners>
		<projectUrl>https://github.com/slav/TeapplixAccess</projectUrl>
		<licenseUrl>https://raw.github.com/slav/TeapplixAccess/master/License.txt</licenseUrl>
		<requireLicenseAcceptance>false</requireLicenseAcceptance>
		<copyright>Copyright (C) Agile Harbor, LLC 2013</copyright>
		<summary>$text</summary>
		<description>$text</description>
		<tags>Teapplix</tags>
		<dependencies> 
			<group targetFramework="net45">
				<dependency id="Netco" version="1.1.0" />
				<dependency id="CuttingEdge.Conditions" version="1.2.0.0" />
				<dependency id="LINQtoCSV " version="1.2.0.0" />
			</group>
		</dependencies>
	</metadata>
</package>
"@
	# pack
	exec { NuGet pack $build_output_dir\TeapplixAccess\TeapplixAccess.nuspec -Output $build_dir }
	
	$pushTeapplixAccess = Read-Host 'Push TeapplixAccess ' $Version ' to NuGet? (Y/N)'
	Write-Host $pushTeapplixAccess
	if( $pushTeapplixAccess -eq "y" -or $pushTeapplixAccess -eq "Y" )	{
		Get-ChildItem $build_dir\*.nupkg |% { exec { NuGet push  $_.FullName }}
	}
}

task . Init, Build, Package, Zip, NuGet