# HornSide Changelog

## Current Branch
### Changes in Milestone Athens RC2 (v2.2.****.*****)
* Fixes
	* Issue #9 has been solved.
	* Shadow is now topmost whenever the topmost property is enabled
* Options
	* Removed Position tab and placed the items back to General tab
	* Removed UITheme from the options menu, will only be used at startup and will match the OS running.
	* Added Report Issue/Bug button to the About tab
* Localization
	* Changed few localization references
	* Added TileLibraryTitle
* Code
	* Removed the "Restart" button from the error dialog
	* Removed some commented sections in the code
	* Removed Send Feedback (the old one) since we already have the issue tracker
	* Completed implementation of the XML system
	
### Changes in Milestone Athens RC1 (v2.2.2000.21174)
* Options
	* Recategorized the items (General, Position, Display, About)
	* Added uninstall all tiles and view tile list (half-implemented)
	* Added UI Theme option. HornSide wouldn't look like Win95/2000 anymore (half-implemented)
	* Background when DWM is enabled is now glass
	* New easter egg*
* Tiles
	* Tile developers can now add a custom description, developer name, and version
	* Added Tile Availability to the details pane in Tile Library (to identify which tile is online and which is locally available)
	* Local tiles can now be seen in the Tile Library
	* Get more tiles is now Tile Library
	* Made the UI a bit Longhorn-like (half-implemented)
* Unification\Branding\Code Clarity
	* Changed solution target framework to .NET 3.5
	* Removing old registry-settings leftovers. (To maintain code clarity)
	* The program now has the SharedIV tag removed from the file description
	* Added BranchProdStatus
	* Implemented a shared assembly version system for easier version changing
	* Added GitInfo in the report to identify from what branch/milestone the build came
	* Replaced some methods used (ShellExecute with Process.Start)
	* Applied the option (showErrors) to the new static void HandleError()
* Behavior\UI
	* The shadow on the sidebar is now on top when the topmost property in the options is enabled
	* Reorganized the sidebar's context menu (now contains: Lock the sidebar, Properties (old options), Add a tile, Remove all tiles, Tile Library, Minimize. Moved the close button to the tray icon)
	* Removed AeroClear theme (a/r)
* Localization
	* There are a lot of new strings added to the localization file
	* Removed all other localization files except for English
* Performance and Error
	* HornSide is now registered in WER (restart is now also available as an option when the program crashes)
	* Binded error reporting service values in the code to the new settings system.
	* Re-basing the settings system (from INI to XML) - It's already XML in the past, there are traces.
	* Disabled bug reporting service for the mean-time. The "Send Feedback" button will direct to the issue tracker here.
	* Added Restart button to the error dialog. Some people with low-end computers that use some tiles that has problems make the computer's CPU usage rise to 100%.

### Localization:
* It is recommended to update your localization files and include the following localization that have been added, listed below:
	* TileListFail - string to show when failed retriving tile list from online library.
	* OfflineResolveFind - string to find when to show tile list fail.
	* FeedbackReqTitle - the title of the dialog when sending feedback reports.
	* FeedbackEmail - caption above the textbox on where to put email to contact.
	* FeedbackComment - caption above the textbox where the comment is entered.
	* UpdateDlgCaption - Downloading update dialog title.
	* UpdateDlgInstruct - Please wait while the update is being downloaded.
	* UpdateDlgText - HornSide will restart automatically.
	* LegacyUpdateCaption - HornSide build 1.0 is available. Visit http://www.example.com/ to download the latest version.
	* MaintenanceGroup
	* ViewTileList
	* EnableShadow
	* UninstallAllTiles
	* HSPnalization																			
	* You can check the English localization file since it's the only one that contains the updated keys.

## Older releases:
### Changes in v2.2.0000.00000 (initial release, minimal changes to original commit):
* Bumped assembly versions to 2.2.0000.00000.
* Bumped all copyright notices to 2016.
* Minor support for Windows 8 and higher. Glass will be disabled on higher editions.
* Added references to different windows styles supported by the PresentationFramework. This allows HornSide's UI to adjust to the OS running, however, this excludes Windows 8.
* Allow localization of the feedback tab and few changes to the default English strings.* To suppress the error when entering the tile library offline. Only applies to people using English version of the .NET framework. The localizable string to search for can be found at the English local file.