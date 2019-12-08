# AvalonBar
AvalonBar is a sidebar with the aim of reimplementing and expanding upon the features and concept of the Longhorn Sidebar.

This project is a fork of [LongBar from CodePlex](http://longbar.codeplex.com/).

# Features
- The sidebar comes with 9 default themes
- Add tiles from a selection available at the Tile Gallery
- The ability to extend the sidebar across the taskbar
- Run and show the sidebar at startup
- Blur behind the sidebar (Windows 7 and Windows 10)
- Multi-monitor support (choose on which monitor the sidebar will be displayed)

# Download
Check the [Releases](https://github.com/AvalonBar/AvalonBar/releases) page.

# Building from source
Clone the repository using `git`, checkout the `master` branch, and hit compile.
## Requirements
- Visual Studio 2015 or higher
- .NET Framework 4.6 SDK
## Dependencies
- Microsoft.Windows.APICodePack-Core
- ICSharpCode.SharpZipLib
- WPF Toolkit (optional, some tiles may not work without this)

# Contributing
You can help out by:
- Filing an issue (bugs, suggestions, questions)
- Becoming a tile creator/developer
  - The `template-src` folder contains a tile that you can use as a base
- Localization
- Sending a pull request
  - Please make sure that your PR has an associated issue with it;
  - Follows the surrounding code style (we don't have an established code style rules yet); and
  - Your name added to the list of [contributors](CONTRIBUTORS.md)

## License
This project is licensed under the [Microsoft Public License (MS-PL)](LICENSE).