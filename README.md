A fork of libOpenMetaverse for OpenSimulator (versions >= 0.9.3.0) use

For OpenSimulator versions 0.9.1.x and 0.9.2.x use branch Mono-Net4x

For OpenSimulator older versions use the original Library

 `https://github.com/openmetaversefoundation/libopenmetaverse`
 

# git clone

get or update source from git

 `git clone https://bitbucket.org/opensimulator/libopenmetaverse.git`
 
or

 `git clone https://github.com/opensim/libopenmetaverse.git`
 
or

 `git clone https://github.com/UbitUmarov/libopenmetaverse.git`


# Building on Windows

## Requirements
  To building under Windows, the following is required:

  * [dotnet 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

optionally also

  * [Visual Studio](https://visualstudio.microsoft.com/vs/features/net-development/), version 2022 or later
  

### Building
 To create the project files, run   

  `runprebuild.bat`

run

  `compile.bat`

Or load the generated OpenMetaverse.sln into Visual Studio and build the solution.



# Building on Linux / Mac

## Requirements

 * [dotnet 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
 * libgdiplus 
 
 if you have mono 6.x complete, you already have libgdiplus, otherwise you need to install it
 using a package manager for your operating system, like apt, brew, macports, etc
 for example on debian:
 
 `apt-get update && apt-get install -y apt-utils libgdiplus libc6-dev`

### Building
  To create the project files, run:

  `./runprebuild.sh`

  then run

 `compile.bat`
 
 
### Current building problems
several projects will only compile and run on windows
many of those project also need fixes on their project files to set the framework to netxxx-windows and allow windows forms

For now, runprebuild.(bat/sh) will only create projects for the dlls needed for OpenSimulator.
