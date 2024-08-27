This is a Visual Studio 2017 project I have setup to assist in validating user defined mods. I use it to validate my own modded scripts prior to publishing on Steam.

It also contains all of my Mods as subfolders in the solution, as well as a snapshot of MoM scripts and XML files.

All of my Master of Magic (2022) Mods are located here:

https://github.com/DorianGrayII/MOM-Script-Validation-Framework/tree/master/MOM/PlayerScripts

if you are familiar with visual studio at all, let me point out that there is a Class Library build target, but it is dummy output and can't really be used.

The purpose of the build is to validate the user mod scripts against the existing MOM Scripts and the underlying CSharp-Assembly.dll.

Also, when writing your own Script Code for your mod, it handles auto-complete for you.
