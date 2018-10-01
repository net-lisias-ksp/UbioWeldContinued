# Welding Ltd. Continuum (Archive)

* 2016-0403: 2.3.0_Beta (Alewx) for KSP 1.1.0 PRE-RELEASE
	+ UbioweldingLtd 2.3.0
		- Adapted for KSP 1.1.0 Beta
* 2015-1111: 2.2.2 (Alewx) for KSP 1.0.5
	+ Recompiled for KSP 1.0.5
* 2015-1011: 2.2.1 (Alewx) for KSP 1.0.4
	+ added new config field for file simplification that caused trouble for some Players
	+ slight improvements for the Setting changes at runtime.
* 2015-0827: 2.2.0_release (Alewx) for KSP 1.0.4
	+ New weldedMeshSwitch works like an interstellarMeshSwitch is currently limited to InterstellarMeshSwitch FSMeshSwitch is ignored
	+ Added some modules and attributes for the moduleAttributeList config
	+ Fixed a major bug that could cause freeze of UI and welding
	+ added experimental nodes in the weldment file for:
		- stackSymmetry
		- CoLOffset
		- CoPOffset
		- explosionPotential
		- thermalMassModifier
		- heatConductivity
		- emissiveConstant
		- radiatorHeadroom
		- bulkheadProfiles
	+ Textfield for stackSymmetry in the info window
	+ improved scaleing readout
	+ removed needless vectors from the written file
	+ WeldedMeshSwitch contains an option to destroy unused objects to save tiny performance
	+ imropved the rotation or models to be not set to 360 degrees
	+ settings contains a precisionDigits value that will round the values of the new part to this amount of digits after the dot
	+ saving the settings out of the game now makes them also the actual ones used
	+ added safety check into the destruction of unused objects at the WeldedMeshSwitch
	+ final Version of scaling for models implemented to work now with every damn configuration that is possible out there
* 2015-0701: 2.2.0 (Alewx) for KSP 1.0.4 PRE-RELEASE
	+ New weldedMeshSwitch works like an interstellarMeshSwitch is currently limited to InterstellarMeshSwitch FSMeshSwitch is ignored
	+ Added some modules and attributes for the moduleAttributeList config
	+ Fixed a major bug that could cause freeze of UI and welding
	+ added experimental nodes in the weldment file for:
		- stackSymmetry
		- CoLOffset
		- CoPOffset
		- explosionPotential
		- thermalMassModifier
		- heatConductivity
		- emissiveConstant
		- radiatorHeadroom
		- bulkheadProfiles
	+ Textfield for stackSymmetry in the info window
	+ improved scaleing readout
	+ removed needless vectors from the written file
	+ WeldedMeshSwitch contains an option to destroy unused objects to save tiny performance
	+ imropved the rotation or models to be not set to 360 degrees
	+ settings contains a precisionDigits value that will round the values of the new part to this amount of digits after the dot
	+ saving the settings out of the game now makes them also the actual ones used
	+ added safety check into the destruction of unused objects at the WeldedMeshSwitch
	+ final Version of scaling for models implemented to work now with every damn configuration that is possible out there
* 2015-0527: 2.1.3 (Alewx) for KSP 1.0
	+ Fixed a Major bug that prevented the info window from working an also then from saving the weldment
* 2015-0527: 2.1.2 (Alewx) for KSP 1.0
	+ changed the compatibility for KSP 1.0
* 2015-0114: 2.1.1 (girka2k) for KSP 0.90
	+ Compatibility with Linux and MacOS
	+ AVC version check integration
	+ Fixed problem with with the welding of parts without category
* 2014-1203: 05 (girka2k) for KSP 0.25 PRE-RELEASE
	+ Unofficial-v.05 changes
		- Fixed wrong rescaling of some models in KSP 0.25
		- Improved database reloading with ModuleManager installed
		- Updated <ModulesToIgnore> list
		- Dropped compatibility with KSP 0.24.2
* 2014-1101: 04 (girka2k) for KSP 0.24 PRE-RELEASE
	+ Unofficial-v.04 changes:
		- Fixed wrong rotation of models in some cases (especially in SHP)
		- Now topmost and lowest nodes are placed to the end of list for fix impossibility to attach welded part to other in VAB/SHP. (It is assumed that the topmost and lowest nodes, most likely, are stack nodes)
		- Most floating-point values (excluding amount of resources) of welded part are rounded to 5 fractional digits
		- Some other minor fixes
	+ This is a joint release of girka2k and Alewx
	+ previous changes: https://github.com/girka2k/unofficailUbioWeld/blob/dev/changelog.txt
