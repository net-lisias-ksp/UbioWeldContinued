# Welding Ltd. Continuum :: Changes

* 2019-0525: 2.6.0.5 (Lisias) for 1.4.1 <= KSP <= 1.4.5
	+ After almost a Year, Continuum goes gold! :-)
		- Finally fully supporting KSP 1.4.5... =P
		- KSP >= 1.5 depends on implementing proper support for `ModulePartVariant`, something that will be tricky to say the least.
	+ Using KSPe Facilities (hard dependency):
		- File facilities (user configurable files are not on <KSP_ROOT>/PluginData)
		- UI facilities (transparent proxy to ClickTroughBlocker)
		- Log facilities
	+ Bug fixes
		- Toolbar registering fix
		- Log level select now works
		- Preventing empty `.cfg` files to be created 

