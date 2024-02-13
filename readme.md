# Project hierarchy overview
  
### Converters: 
Classes prefixed with Json are Json value converters, others are binding converters used in the UI
### Extensions: 
Helper Class extensions 
### Helpers:
- CLHelper (static) (WIP) - Intended to be used for cross language support of the ReferenceAdded event
- ConversionHelper (static) - Converts things, (DebrickedAPI reply models to internal models and more)
- DebrickedAPIHelper (not static) - any debricked api calls are handled here, instantiate to use, implements IDisposable
- DebrickedCLIHelper (static) - Helper for installing/updating the debricked CLI
- EncryptionHelper (static) - Encrypt/Decrypt stringdata using DPAPI current user context
- ErrorListHelper (singleton) - Provides ErrorListProvider instance for writing to error list
- HierarchyEventsHelper (non-static) - Helper for handling VsHierarchyEvents in the MasterEventHandler
- HttpHelper (static) - helper class used by the DebrickedAPIHelper to make http calls
- MasterEventHandlerHelper (singleton) (WIP) - central event handler, instantiated on package launch, handles all events
- ProcessHelper (static) - helper methods for running processes (used scanhelper to run debricked commands)
- ScanHelper (non-static) - helper for running scans (prepares & runs scan, fetches results using DebrickedAPIHelper and returns results)
- ScanResultStorageHelper (static) - helper for storing and loading scan results to/from disk
- ScrollviewerHelper (static) - dont worry about it, dont touch it ;=)
- SettingsHelper (static) - helper for storing and loading some file base settings (general settings are handled through the Options>General.cs class
### Models:
Constants: Constant values (enums, regex patterns, api endpoints etc)  
DebrickedApi: models that reflect the data structure returned by the debricked api  
Github: models that reflect data returned by the github api  
	
files in the root of this folder reflect the internal models used by the extension for UI bindings and storage


### Options:  
option page(s)  
### Selectors:   
template selectors used in the UI  
### toolwindows:  
Dialogs:   
contains UI xaml and code-behind for credential and repo prompt  
main:  
Controls:  
UI xaml and code-behind of the toolwindow itself and the two datagrids used in it  
DebrickedPackage.cs: entry point for the extension
	

# Behaviours:

The package is loaded in the Background when Visual Studio starts.

It subscribes to the OnAfterBackgroundSolutionLoadComplete event which on trigger 
- loads existing data for the project that was opened
- writes policy violations to the error list
- populates the toolwindow with results if it is loaded
- subscribes to other event handlers based on settings (onAfterBuild, onReferenceAdded etc)  

When VS is opened with a solution already open the same logic is executed.

The toolwindow can be opened through View->other windows->Debricked  
The toolwindow shows 2 tabs (Vulnerabilities/Dependencies) of data in a filterable and sortable grid format, a click on a row shows details  
It also has a "Rescan" button that triggers the following actions:  
- check for debricked cli update (calls github api)
- update cli if newer version is available
- run resolve
- run fingerprint (if option enabled)
- create temporary repo or map to existing repo depending on mapping strategy options
- run scan
- fetch results from the api (depending on settings and refresh timeout either a full or partial refresh is done)
- update toolwindow ui and errorlist

Depending on the provided options (credentials, mapping strategy) the toolwindow will open further dialogs
during this process to request user input
Depending on options (triggers) the above process will also be triggered by:
- SolutionBuild (except when followed by debug session)
- SolutionBuild (always)
- RefrenceAdded
	
The options can be found in Options->General.cs or in the UI under Tools->Options->Debricked
each option contains a description that hopefully explains what it does

# How to Build:
Install the Visual Studio extension development workload through the Visual Studio installer
Load Solution in Visual Studio 2022, right click solution in solution explorer -> build :)

# WIPs:
- temp repo creation and rule copying -> complete implementation and test
- finish the MasterEventHandler handlers for refAdded/removed 
- support ReferenceAdded event for other languages (python/js etc)-> this should be possible by subscribing to the reference hierarchy nodes in the solution explorer, an example for python is already started in CLHelper
- error handling

## Ideas for what to do next:
- investigate option for on-commit trigger (including option to abort commit)
- investigate if its worthwile to implement an "onLockFileEdited" trigger

## Tip:
If you ever need to reset your Experimental Instance and the extension doesnt show up when debugging do the following:
- close all vs instances
- reset experimental instance
- open your project
- clean project
- rebuild solution
- switch to release mode and launch the debugger
- when prompted select "continue debugging"
- once the experimental instance is loaded close it and switch back to debug mode
