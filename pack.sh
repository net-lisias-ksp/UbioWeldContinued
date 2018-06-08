#!/usr/bin/env bash

VERSION=$(cat ./UbioWeldingLtd/Version.cs | grep -Po 'public const string Number = "(.+)";' | sed 's/public const string Number = "//' | sed 's/";//')
echo $VERSION
FILE=UbioWeldContinuum-$VERSION.zip
rm $FILE
zip -r $FILE ./GameData/* -x .DS_Store
mv $FILE ./Archive
