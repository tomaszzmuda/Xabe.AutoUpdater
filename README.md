# Xabe.AutoUpdater  [![Build Status](https://travis-ci.org/tomaszzmuda/Xabe.AutoUpdater.svg?branch=master)](https://travis-ci.org/tomaszzmuda/Xabe.AutoUpdater)

Dotnet core library providing automatically update.

## Using ##

Install the [Xabe.AutoUpdater NuGet package](https://www.nuget.org/packages/Xabe.AutoUpdater) via nuget:

	PM> Install-Package Xabe.AutoUpdater
	
Update application:

	IUpdater updater = new AutoUpdater.Updater(new AssemblyVersionChecker(), new GithubProvider("Xabe.VideoConverter", "tomaszzmuda", "Xabe.VideoConverter"));
	if(await updater.IsUpdateAvaiable())
		updater.Update();
	
Updater needs two parameter to create it.

	Updater(IVersionChecker versionChecker, IReleaseProvider releaseProvider)

At this time only GitHub Releases provider is emplemented, but you can make providers by your own implementing those interfaces.

## Example using ##

[Xabe.VideoConverter](https://github.com/tomaszzmuda/Xabe.VideoConverter/blob/master/Xabe.VideoConverter/Updater.cs)

## Lincence ## 

Xabe.AutoUpdater is licensed under MIT - see [License](LICENSE.md) for details.
