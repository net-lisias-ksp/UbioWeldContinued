# Welding Ltd. Continuum

This is a welding tool to allow you to merge KSP parts together in order to reduce your part count and increase performance.  This is a fork by Lisias from the Continued fork from Ubiozor's "UbioWeldingLtd".


## Installation Instructions

To install, place the GameData folder inside your Kerbal Space Program folder. Optionally, you can also do the same for the PluginData (be careful to do not overwrite your custom settings):

* **REMOVE ANY OLD VERSIONS OF THE PRODUCT BEFORE INSTALLING**, including any other fork:
	+ Delete `<KSP_ROOT>/GameData/net.lisias.ksp/UbioWeldingLtd`
* Extract the package's `GameData` folder into your KSP's root:
	+ `<PACKAGE>/GameData` --> `<KSP_ROOT>/GameData`
* Extract the package's `PluginData` folder (if available) into your KSP's root, taking precautions to do not overwrite your custom settings if this is not what you want to.
	+ `<PACKAGE>/PluginData` --> `<KSP_ROOT>/PluginData`
	+ You can safely ignore this step if you already had installed it previously and didn't deleted your custom configurable files.
*. Make a **backup copy** from `GameData/__LOCAL/UbioWeldingLtd` (just in case)

The following file layout must be present after installation:

```
<KSP_ROOT>
	[GameData]
		[net.lisias.ksp]
			[UbioWeldingLtd]
				[Textures]
					...
				CHANGE_LOG.md
				LICENSE
				NOTICE
				README.md
				UbioWeldContinuum.dll
				UbioWeldContinuum.version
		000_KSPe.dll
		ModuleManager.dll
		...
	[PluginData]
		[net.lisias.ksp]
			[UbioWeldingLtd]
				moduleAttributeList.xml
				new-config.xml
	KSP.log
	PastDatabase.cfg
	...
```


## Dependencies
* Hard Dependencies
	* [KSP API Extensions/L](https://github.com/net-lisias-ksp/KSPAPIExtensions) 2.0 or newer
	* [Toolbar Control](https://github.com/net-lisias-kspu/ToolbarControl) 0.1.6.15 or newer
* Soft Dependencies
	* [Module Manager](https://github.com/net-lisias-kspu/ModuleManager) 3.0.4 or newer
