# WebP Converter Tool for RPG Maker MV

## Intro
This small command line cwebp wrapper app converts image files to WebP files so RPG Maker MV can load them (It works with the same idea that the [Opus converter tool](https://github.com/acemod13/RMMVOpusConverter) works). WebP can provide smaller file size while having the same image quality. The tool implements both the lossless and lossy conversions as well.

## System Requirements
- Windows 7 Service Pack 1 or newer/Ubunutu 18.04 or newer.
- Google's WebP [binaries](https://developers.google.com/speed/webp/download). The cwebp app is required.

## Compiling

You can either use Visual Studio 2019 (with .NET Core's workload) or use the command line/terminal and run the following (make sure to install .NET Core 3.1 first):
```
dotnet build -c Release
dotnet run --project "RMMVWebPConverter"
```
