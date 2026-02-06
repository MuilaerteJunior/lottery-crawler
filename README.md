# LotteryCrawler

Developer plan (pseudocode)

## What is this repository?

This project tries to guess results from lottery draws and can also generate random bets for you to try your luck.

There are two application projects:
- `LotteryCrawler.App` — primary CLI application for generating bets and attempting to guess results (equivalent to previous `Bet` behavior).
- `LotteryCrawler.Study` — supporting application focused on studying historical results and producing analysis used by the guessing logic.

## Build (requires .NET 10)

- From solution root:
  - `dotnet build`
- Or build a specific project:
  - `dotnet build src/LotteryCrawler.App`
  - `dotnet build src/LotteryCrawler.Study`

## How to run

You can run the apps either by using the built executable (on Windows) or `dotnet run`.

Examples using built executables:
- Generate a bet (default parameters)
  - `LotteryCrawler.App.exe g`
- Generate a specific 6 bets
  - `LotteryCrawler.App.exe g 6`

## How to view help

Both applications use `Displayer.ShowHelp()` to present usage information. To display help and see exact supported commands and options, use any of the common help invocations:

- `LotteryCrawler.App.exe h`
- 
Same applies for `LotteryCrawler.Study`:
- `LotteryCrawler.Study.exe h`

Follow the printed help to discover additional commands and options implemented by each application.

## Notes, feedback and contributions

Critics, suggestions, and improvements are welcome.

Contact: https://www.linkedin.com/in/muilaerte-junior/