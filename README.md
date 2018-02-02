# Final Fantasy XIV - Actor
### Based on https://github.com/eai04191/Actor

TL;DR; A C# Application to install ACT, and multiple plugins, for Final Fantasy XIV.

### Why another Actor? Do we really need this?
To start, I did this *rework* for the Actor, just to make it accessible to everyone;

The real intent was, infact, to just translate the original Actor so that everyone could understand what was going on.
I ended up with this because I saw too many limitations in the original Actor; like the fact that it would install ACT exactly where you runned the script or the fact that I had to install all the plugins... even the ones I didn't wanted to!

So, to be completely honest, no you don't need this. 

You can install ACT manually, download all the things you need and do all the work on your own; I'm happy with that :)
I'll, anyway, use this little app, as it is simple and quick!

## Todo
Before we get to the juicy and ready-to-go ActorConsole, here are some of the goals I would like to achieve:
- ~~Better organize the output dir for compiled source~~
- ~~Create a configuration file, versioned on github, with all the needed links (now hardcoded)~~
  - ~~Handle the configuration and remove hardcoded strings~~
- (Not in this repository) Refactor DFAssist plugin (it seems to be a little tricky to use it as it is now)
- Add Custom Triggers xml support
  - Check if can be imported from URL
  - Version the xml files directly on github, when possible
  - Include Triggernometry plugin
- Add Commandline parameter to skip all the iterations
  - **/path="installPath"** to specify a different install path when using commandline parameters
  - **/y** to silently install everything and run ACT in the end
  - **/n** to silently install only ACT and the FFXIV Parsing Plugin
  - Handle Commandline parameters in Actor.Core (to use them also with ActorGui)
- Load and Configure ACT before the first start (based on [this](https://gist.github.com/TomRichter/e044a3dff5c50024cf514ffb20a201a9))
  - Fast implementation for ActorConsole
  - Refactoring to use the same implementation for ActorGui
- Edit the Advanced Combat Tracker.exe properties
  - Set to "Run as Administrator" to true
  - for Windows 10 (and 4k resolutions), Set "DPI override" to false
- Add ACT to the Windows Firewall exceptions (low priority)
- ~~Ask to start ACT at the end of the installation~~
- ~~Update an already existing ACT installation (now simply overwrites everything)~~
  - ~~Check the versions and ask/download/install only the needed plugins/prerequisites~~
- Create the ActorGui (a more user-friendly interface for the Actor)

And everything that comes up to my mind while I code.
I'm also open to suggestions, so feel free to write me if you have an idea that may fit in the project ;)

## ActorConsole
<p align="center">
  <img src="https://user-images.githubusercontent.com/3910202/35669724-56cde7ea-0736-11e8-80ac-6f7a5f0c33a1.png" width="90%" />
</p>
This is the *console* version of the application.

The main goal was to have the program ready as soon as possible and then create a common library to use also for a future GUI version.
You can see the flow of how it came as it is now just by watching at the commits.

This app will just download all the necessary file you need to use ACT without problems.
- At first will ask for the path where you want to install ACT
- Then will ask if you need to install the prerequisites:
  - Microsoft Visual C++ Redistributable
  - Microsoft .NET Framework 4.7
  - Win10Pcap
- At this point ACT will be downloaded and installed
- Then will ask, for every plugin, if you want to install it or not

The plugins actually available are:
 - [FFXIV Parsing Plugin](https://github.com/ravahn/FFXIV_ACT_Plugin) (Required)
 - [Hojoring Plugin](https://github.com/anoyetta/ACT.Hojoring)
 - [Overlay Plugin](https://github.com/hibiyasleep/OverlayPlugin)
 - [DFAssist Plugin](https://github.com/wanaff14/ACTFate)

---

### If you like my work
<a href="https://www.paypal.me/ruggierocarlo">
  <img src="https://user-images.githubusercontent.com/3910202/35670996-5fb27278-073a-11e8-9a0a-7f951bbf04ff.png" width="25%" alt="Support with PayPal" />
</a>
