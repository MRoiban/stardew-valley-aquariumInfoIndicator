# Stardew Valley Modding

## Tools

- SMAPI
- Content patcher
  - Use Content Patcher for Asset Changes
    - For modifying game assets like sprites, maps, or data files, consider using Content Patcher.
    - This framework allows you to create content packs using JSON files without writing C# code.
- .NET6
  - You can create a content pack for static data and use a C# mod to add dynamic behavior. This approach separates concerns and simplifies maintenance.

## Directory Structure

./
|-- ./framework/ -> the c# code is written here
|-- ./handlers/ -> handlers code
|-- ./docs/ -> any documentation/manual on how things work
|-- ./[CP] $MOD_NAME$/ -> if the mod needs to use content patcher mod then have the CP code here
|-- ./i18n/ -> translation files
|-- ./ModEntry.cs
|-- ./$MOD_NAME$.csproj
|-- ./manifest.json

## Create a basic mod

### Quick start

If you're experienced enough to skip the tutorial, here's a quick summary of this section:

|  |
| - |

| **expand for quick start** |
| -------------------------------- |

### Create the project

A SMAPI mod is a compiled library (DLL) with an entry method that gets called by SMAPI, so let's set that up.

1. Open Visual Studio or MonoDevelop.
2. Create a solution with a _Class Library_ project (see [how to create a project](https://stardewvalleywiki.com/Modding:IDE_reference#create-project "Modding:IDE reference")). (Don't select _Class Library (.NET Framework)_! That's a separate thing with a similar name.)
3. Target .NET 6 (see [how to change target framework](https://stardewvalleywiki.com/Modding:IDE_reference#set-target-framework "Modding:IDE reference")).You may need to [install the SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).That's the version installed and used by the game. Newer versions may not be installed for players, and SMAPI may not be able to load them. [Yes we know it&#39;s EOL](https://stardewvalleywiki.com/Modding:Modder_Guide/Get_Started#What_does_.22target_NET_6.0.22_mean.3F).
4. Reference the [Pathoschild.Stardew.ModBuildConfig NuGet package](https://www.nuget.org/packages/Pathoschild.Stardew.ModBuildConfig) (see [how to add the package](https://stardewvalleywiki.com/Modding:IDE_reference#add-nuget "Modding:IDE reference")).
   - If you are getting an error stating _The type or namespace name "StardewModdingAPI" could not be found_, then it's possible that your game path is not being detected. You will need to set the GamePath property to the game's executable directory. This can be done by adding a _GamePath_ property to the _PropertyGroup_ in your _.csproj_ settings.
5. Restart Visual Studio/MonoDevelop after installing the package.

### Add the code

Next let's add some code SMAPI will run.

1. Delete the Class1.cs or MyClass.cs file (see [how to delete a file](https://stardewvalleywiki.com/Modding:IDE_reference#delete-file "Modding:IDE reference")).
2. Add a C# class file called ModEntry.cs to your project (see [how to add a file](https://stardewvalleywiki.com/Modding:IDE_reference#Add_a_file "Modding:IDE reference")).
3. Put this code in the file (replace YourProjectName with the name of your project):

   using System;
   using Microsoft.Xna.Framework;
   using StardewModdingAPI;
   using StardewModdingAPI.Events;
   using StardewModdingAPI.Utilities;
   using StardewValley;

   namespace YourProjectName
   {
   /// `<summary>`The mod entry point.`</summary>`
   internal sealed class ModEntry : Mod
   {
   /*********
   ** Public methods
   *********/
   /// `<summary>`The mod entry point, called after the mod is first loaded.`</summary>`
   /// `<param name="helper">`Provides simplified APIs for writing mods.`</param>`
   public override void Entry(IModHelper helper)
   {
   helper.Events.Input.ButtonPressed += this.OnButtonPressed;
   }

   /*********
   ** Private methods
   *********/
   /// `<summary>`Raised after the player presses a button on the keyboard, controller, or mouse.`</summary>`
   /// `<param name="sender">`The event sender.`</param>`
   /// `<param name="e">`The event data.`</param>`
   private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
   {
   // ignore if player hasn't loaded a save yet
   if (!Context.IsWorldReady)
   return;

   // print button presses to the console window
   this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", LogLevel.Debug);
   }
   }
   }

Here's a breakdown of what that code is doing:

1. `using X;` (see [using directive](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/using-directive)) makes classes in that namespace available in your code.
2. `namespace YourProjectName` (see [namespace keyword](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/namespace)) defines the scope for your mod code. Don't worry about this when you're starting out, Visual Studio or MonoDevelop will add it automatically when you add a file.
3. `public class ModEntry : Mod` (see [class keyword](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/class)) creates your mod's main class, and subclasses SMAPI's Mod class. SMAPI will detect your Mod subclass automatically, and Mod gives you access to SMAPI's APIs.
4. `public override void Entry(IModHelper helper)` is the method SMAPI will call when your mod is loaded into the game. The `helper` provides convenient access to many of SMAPI's APIs.
5. `helper.Events.Input.ButtonPressed += this.OnButtonPressed;` adds an 'event handler' (_i.e.,_ a method to call) when the button-pressed event happens. In other words, when a button is pressed (the helper.Events.Input.ButtonPressed event), SMAPI will call your this.OnButtonPressed method. See [events in the SMAPI reference](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Events "Modding:Modder Guide/APIs/Events") for more info.

Note: if you get compile errors along the lines of "Feature XX is not available in C# `<number>`. Please use language version `<number>` or greater", add `<LangVersion>`Latest`</LangVersion>` to your .csproj.

### Add your manifest

The mod manifest tells SMAPI about your mod.

1. Add a file named manifest.json to your project.
2. Paste this code into the file:

   {
   "Name": "`<your project name>`",
   "Author": "`<your name>`",
   "Version": "1.0.0",
   "Description": "`<One or two sentences about the mod>`",
   "UniqueID": "`<your name>`.`<your project name>`",
   "EntryDll": "`<your project name>`.dll",
   "MinimumApiVersion": "4.0.0",
   "UpdateKeys": []
   }
3. Replace the <...> placeholders with the correct info. Don't leave any <> symbols!
