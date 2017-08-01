# Xabe.AutoUpdater  [![Build Status](https://travis-ci.org/tomaszzmuda/Xabe.AutoUpdater.svg?branch=master)](https://travis-ci.org/tomaszzmuda/Xabe.AutoUpdater)

Simple .net core library providing automatically update.

## Using ##

Install the [Xabe.AutoUpdater NuGet package](https://www.nuget.org/packages/Xabe.AutoUpdater) via nuget:

	PM> Install-Package Xabe.AutoUpdater
	
Creating file lock:

	var GitHubUpdater = new GitHubUpdater();
	var updater = new Updater(GitHubUpdater);
	
GitHubUpdater is your own implementation of Xabe.AutoUpdater.IUpdate.

## Recommended using ##

	var GitHubUpdater = new GitHubUpdater();
	var updater = new Updater(GitHubUpdater);
	if(await updater.CheckForUpdate())
	{
		updater.Update();
	}
	
## Lincence ## 

Xabe.AutoUpdater is licensed under MIT - see [License](License.md) for details.
