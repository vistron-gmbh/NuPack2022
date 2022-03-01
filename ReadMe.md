# Nupack2022

This is a port of the [Nupack2017](https://github.com/cnsharp/nupack) for VS2022.

# Changes done to run NuPack on VS2022

## Port

Nupack itself was ported to the VS2022 VisualStudio.SDK and VSSDK.BuildTools. As the CNSharp Extensions where also dependend on the old Interop packages I decided to copy classes from the CNSharp Extensions into this project and port them to.

I noticed after the fact, that I pretty much needed everything so it might have been cleaner, to just take the CNShap.Extensions project, remove all dependencies and build it anew with the new ones. But I kept it the way it is.

## Seperation

I seperated the Vsix2022 from the Nupack project itself. Theoretically one could build on that to enable Vsix extensions for 2019 and below. But still there are classes depending on DTE etc. So the seperation is not complete.

## Code/Behavior Differences

### Extension: Add or Publish

At our company we long needed the nuget command executed to deploy the nuget to be `nuget add` . Now we also need `nuget publish`. NuPack17 always used publish.

A checkbox or something different will be added in the next version to support both.

Currenly `Add` is used.

### BeforeQueryStatus

As the BeforeQueryStatus event did never fire if the commands hat the `DefaultInvisible` and `DynamicVisibility` CommandFlag set, I removed both flags. This means that at least on startup the commands are always visible. In the Nupack17 version this was decided by code depending if the `BeforeQueryStatus` is now fired but be aware.

### "Websites" are not supported

At several occasions Nupack17 checked if the project is a valid project format or if it is a "website". See this code:

```csharp
 VSWebSite vsWebSite = project.Object as VSWebSite;
```

But the `VSWebSite` interface does not longer exist or I cannot found it. I even do not know what this is. My gues is, it refers to ASP.NET Projects. If so, you can no longer deploy them.

A fix is appreciated if you know the name/place of the current `VSWebSite` interface.
