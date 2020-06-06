#!/usr/bin/env bash

source ./CONFIG.inc

clean() {
	rm $FILE
	if [ ! -d Archive ] ; then
		rm -f Archive
		mkdir Archive
	fi
}

FILE=$PACKNAME-$VERSION.zip
echo $FILE
clean
zip -r $FILE ./GameData/* -x ".*"
zip -r $FILE ./PluginData/* -x ".*"
sip $FILE INSTALL.md
zip -d $FILE __MACOSX "**/.DS_Store"
mv $FILE ./Archive
