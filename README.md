# Welcome to the N.I.N.A. Plugin Template Repository

This repository contains examples and guidelines on how to develop a plugin for the astrophotography imaging suite [N.I.N.A. - Nighttime Imaging 'N' Astronomy](https://nighttime-imaging.eu/).

## General

The plugins for N.I.N.A. are C# class libraries, that expose certain classes to be imported by the application using the [Managed Extensibility Framework (MEF)](https://docs.microsoft.com/en-us/dotnet/framework/mef/).
Currently plugins are capable to extend the advanced sequencer's functionality by creating new instructions, instruction sets, triggers or conditions.

## Plugin Meta Data

Each plugin must implement a set of assembly attributes inside the AssemblyInfo.cs to expose the necessary meta data for a plugin to be correctly identified by N.I.N.A.

`[AssemblyTitle]` - **Required**

The name of your plugin. This name will be used by the N.I.N.A. plugin manager to show inside the list of plugins as well as using the name as a folder name for putting the plugin content inside the general plugin folder

`[Guid]` - **Required**

This is a unique identifier - using a GUID - of your plugin and must not be changed throughout the lifetime of your plugin for version increases. It is used to identify your assembly during the installation and deinstallation process.

`[AssemblyVersion] & [AssemblyFileVersion]` - **Required**

It consists of a string following "Major.Minor.Patch.Build" describing the plugin version.  

`[AssemblyMetadata(ShortDescription)]` - **Required**

A quick summary of your plugin's capabilities and features

`[AssemblyCompany]` - *Recommended*

The author (you) of the plugin

`[AssemblyMetadata(License)]` -  *Recommended*

A short name of the license in use (e.g.  MPL-2.0, MS-PL, MIT)

`[AssemblyMetadata(LicenseURL)]` -  *Recommended*

Link leading to the license text

`[AssemblyMetadata(Repository)]` -  *Recommended*

A link to the remote repository, where the source code of the plugin is available

`[AssemblyMetadata(MinimumApplicationVersion)]` -  *Recommended*

This field describes the minimum version of N.I.N.A. that this plugin is compatible with. Similar to the plugin version it consists of Major, Minor, Patch and Build.  
If multiple versions of a plugin are available, the plugin manager inside the application will serve the plugin manifest with the highest version that is compatible with the currently running application using the minimum application version.

`[AssemblyMetadata(ChangelogURL)]`

If you want to maintain a list of detailed changelogs you can add a url to your manifest that leads to the list of changes

`[AssemblyMetadata(Tags)]`

Some quick search terms to enable users to quickly search for

`[AssemblyMetadata(Homepage)]`

Homepage of the plugin creator where the plugin and more is found  

`[AssemblyMetadata(LongDescription)]`

An in-depth description of your plugin, with all the content description that is part of the plugin

`[AssemblyMetadata(FeaturedImageURL)]`

URL to a logo for the plugin. This image will be shown prominently in the app next to the name 

`[AssemblyMetadata(ScreenshotURL)]`

An image URL showing the plugin in action

`[AssemblyMetadata(AltScreenshotURL)]`

An alternative image URL showing the plugin in action from a different angle compared to the ScreenshotURL

## Exportable Interfaces

The following interfaces are available to export via MEF.

### IPluginManifest

**Mandatory to be exported once!**

The interface that defines the *plugin meta data*. Each plugin requires an export of this interface to be able to be displayed inside N.I.N.A. and for the users to see basic info about the plugin.

### ISequenceItem

*Defines an instruction for the advanced sequencer*

### ISequenceTrigger

*Defines a trigger for the advanced sequencer*

### ISequenceCondition

*Defines a condition for the advanced sequencer*

### ISequenceContainer

*Defines an instruction set for the advanced sequencer*

## Available Base Classes

The N.I.N.A. packages provide a set of base classes that can be inherited from, that will already handle most of the boilerplate required for the exportable interfaces.  
Each base class can provide a set of overridable methods as well as some methods that still are mandatory to be implemented in the child class.

### NINA.Plugin.PluginManifest

*Implements IPluginManifest*  
This base class can be used to grab all required plugin meta data automatically.  
All required properties from the interface IPluginManifest will then be automatically populated out of the assembly meta data defined in AssemblyInfo.cs

### NINA.Sequencer.SequenceItem

*Implements ISequenceItem*

### NINA.Sequencer.SequenceTrigger

*Implements ISequenceTrigger*

### NINA.Sequencer.SequenceCondition

*Implements ISequenceCondition*

### NINA.Sequencer.SequenceContainer

*Implements ISequenceContainer*

## Constructor Injection

Exports using entities for the advanced sequencer have the ability to inject various instances from the N.I.N.A. application to be able to interact with the main application.
To inject an instance, a sequence entity just has to add the corresponding interface to be injected into the constructor. When an instance is then created in the advanced sequencer, the requested instances that correspond to the interface are injected.

The following interfaces can be injected:  
        - *IProfileService*: Get or set profile specific values  
        - *ICameraMediator*: Get camera specific info and interact with the camera  
        - *ITelescopeMediator*: Get telescope specific info and interact with the telescope  
        - *IFocuserMediator*: Get focuser specific info and interact with the focuser    
        - *IFilterWheelMediator*: Get filter wheel specific info and interact with the filter wheel    
        - *IGuiderMediator*: Get guider specific info and interact with the guider    
        - *IRotatorMediator*: Get rotator specific info and interact with the rotator    
        - *IFlatDeviceMediator*: Get flat device specific info and interact with the flat device    
        - *IWeatherDataMediator*: Get weather data specific info and interact with the weather data device  
        - *IDomeMediator*: Get dome specific info and interact with the dome      
        - *ISwitchMediator*: Get switch specific info and interact with the switch    
        - *ISafetyMonitorMediator*: Get safety monitor specific info and interact with the safety monitor  
        - *IImagingMediator*: Capture images using a capture sequence  
        - *IApplicationStatusMediator*: Notify the application of status updates, that will be displayed in the bottom status bar  
        - *INighttimeCalculator*: Retrieve nighttime data, like start of dusk, dawn etc.  
        - *IPlanetariumFactory*: Retrieve the currently selected planetarium interaction and interact with the planetarium app  
        - *IImageHistoryVM*: An object holding all captured images and their meta data  
        - *IDeepSkyObjectSearchVM*: An object to search the database for deep sky objects  
        - *IImageSaveMediator*: Save images by pushing image data to this object    
        - *IApplicationMediator*: Interact with the general application, like switching tabs  
        - *IApplicationResourceDictionary*: Retrieve application resources with this dictionary  
        - *IFramingAssistantVM*: Interact with the framing assistant using this instance  
        - *IList&lt;IDateTimeProvider&gt;*: A list of providers to get DateTimes for various astronomical events like dusk/dawn/meridian etc.  

Example:

```csharp
[Exports(ISequenceItem)]
public class MyPluginItem : SequenceItem {

    IProfileService profileService;
    ICameraMediator cameraMediator;
    MyPluginItem(IProfileService profileService, ICameraMediator cameraMediator) {
        this.profileService = profileService;
        this.cameraMediator = cameraMediator;
    }
}
```

## Plugin DataTemplate / User Interface

### Main Page Template

Inside N.I.N.A. each plugin will have a dedicated page containing information about the plugin as well as showing available global customizations when available.
To retrieve the datatemplate for these global plugin customization options, the application will search for a datatemplate with a specific naming pattern of `<IPluginManifest.Name>_Options`. If your plugin manifest name for example is "MyAwesomePlugin" then the Datatemplate must have the key `MyAwesomePlugion_Options`  
Furthermore to be imported correctly by the application the ResourceDictionary where this DataTemplate is defined must add the correct export in the code behind using the MEF attribute `[Export(typeof(ResourceDictionary))]`.

```xml
<DataTemplate x:Key="<IPluginManifest.Name>_Options">
    <StackPanel DataContext="{Binding}" Orientation="Vertical">
        <!-- Your plugin specific options or general controls -->
    </StackPanel>
</DataTemplate>
```

### Instruction Detail Template

Each advanced sequence entity can define its own look and feel on the advanced sequencer main page.   
For ease of use a base implementation for these entities is availabe using the `SequenceBlockView` which already handles most of the layout. Custom controls can then be added into the `SequenceBlockView.SequenceItemContent`.  
Furthermore to be imported correctly by the application the ResourceDictionary where this DataTemplate is defined must add the correct export in the code behind using the MEF attribute `[Export(typeof(ResourceDictionary))]`.

```xml
<DataTemplate DataType="{x:Type local:<EntityDataType>}">
        <nina:SequenceBlockView DataContext="{Binding}">
            <nina:SequenceBlockView.SequenceItemContent>
                <StackPanel Orientation="Horizontal">                    
                    <!-- Your entity specific settings and controls -->
                </StackPanel>
            </nina:SequenceBlockView.SequenceItemContent>
        </nina:SequenceBlockView>
    </DataTemplate>
```

### Instruction Mini Template

Inside the imaging tab, there is a compact version of the advanced sequencer. Each sequence entity can define its minified version in a special datatemplate. This datatemplate has to follow a specific naming pattern `<Fully Qualified EntityDataType TypeName>_Mini`. For example if your fully qualified entity is called "MyAwesomePluginNamespace.MyAwesomeInstruction" the datatemplate key should be `MyAwesomePluginNamespace.MyAwesomeInstruction_Mini`.  
Furthermore to be imported correctly by the application the ResourceDictionary where this DataTemplate is defined must add the correct export in the code behind using the MEF attribute `[Export(typeof(ResourceDictionary))]`.

```xml
    <DataTemplate x:Key="<Fully Qualified EntityDataType TypeName>_Mini">
        <mini:MiniSequenceItem>
            <mini:MiniSequenceItem.SequenceItemContent>
                <StackPanel Orientation="Horizontal">
                    <!-- Your entity specific details in compact form -->
                </StackPanel>
            </mini:MiniSequenceItem.SequenceItemContent>
        </mini:MiniSequenceItem>
    </DataTemplate>
```

## Plugin Distribution

### Official Plugin Repository

N.I.N.A. has the capability to download plugins inside the application using a plugin manager. To be able to show your plugin inside the app, a manifest has to be created and uploaded to the official manifest repository.  
Please refer to the guide at the [official community plugin manifest repository](https://bitbucket.org/Isbeorn/nina.plugin.manifests/) that will describe in detail how it is done.

### Manual File Distribution

In addition to the offical distribution, you can also simply distribute your plugin by sharing the compiled file(s). To use the plugin the user has to copy the files into the folder at `%localappdata%\NINA\Plugins`