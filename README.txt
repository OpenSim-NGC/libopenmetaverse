libOpenMetaverse Library Fork for OpenSimulator (version >= 0.9.1.0)


Finding Help
------------

If you need any help we have a couple of resources, the primary one being 
the #opensim-dev IRC channel on Freenode.


Getting started on Windows
====================================================================================


Prerequisites (all Freely Available)
--------------------------------------

Microsoft .NET Framework 4.6
Visual Visual Studio (community) 2017

Compiling
---------
1. Open Explorer and browse to the directory you extracted the source distribution to
2. Double click the runprebuild.bat file, this will create the necessary solution and project files
3. open the solution OpenMetaverse.sln from within Visual Studio
4. From the Build Menu choose Build Solution (or press the F6 Key)

The library, example applications and tools will be in the bin directory


Getting started on Linux
====================================================================================

Prerequisites Needed
--------------------

mono 5.x - https://www.mono-project.com/download/stable/#download-lin
(install mono.complete)

Compiling
---------
1. Change to the directory you extracted the source distribution to
2. run the prebuild file: % sh runprebuild.sh - This will generate the solution files for xbuild

using xbuild
3. Compile the solution with the command: % xbuild OpenMetaverse.sln
using msbuild
3. Compile the solution with the command: % msbuild OpenMetaverse.sln

The library, example applications and tools will be in the bin directory


Happy fiddling,
-- OpenMetaverse Ninjas 

