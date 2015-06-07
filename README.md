# SpyderTallyController
Tally controller for Spyder video processor using Windows 10 IoT Core on a Raspberry Pi 2.  Written as a Windows 10 Univsersal 
application, the tally controller is intended to run as an embedded, headless device.  It leverages the Windows IoT Extensions 
SDK to access GPIO pins, as well as the [Spyder Client Library](https://www.nuget.org/packages/SpyderClientLibrary/) to interact 
with Spyder servers.

Key Features
---------------
* Works with all versions of Spyder Server, from version 2.10.8 and up
* Works with both Spyder 200/300 and Spyder X20 systems
* Works by listening to existing Spyder status update messages on the network - no added CPU or network bandwidth overhead
* Gives us tallies for the Vista Spyder video processor!

Usage
---------------
This application was written using Visual Studio 2015, and is written in C#.  Take a look at the docs folder for a wiring diagram, 
as well as a PowerPoint deck that provides an overview of both the hardware and software solution.  For a full video walk-thru,
check out the [YouTube video](https://youtu.be/mBM5LXhSECg) created by the project's author.

Future Work
----------------
The initial implementation of this application hard-codes the Spyder server IP address and the list of sources monitored by the 
individual tallies, and in a subsequent revision I plan to add an additional desktop application which will view/edit this 
configuration information remotely.

Also planned is to add a user interface to the Raspberry Pi itself, to allow a user to monitor and diagnose issues by connecting
an HDMI display to the device directly.
