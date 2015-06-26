#!/usr/bin/env bash

## Build Android Plugin
( cd ../android-sdk ; gradle makeJar )

## Copy into Assets/Plugins/Android
cp ../android-sdk/ddnasdk/build/libs/ddnasdk.jar Assets/Plugins/Android/

## Copy Adhancr libs in Assets/Plugins/Android
cp ../android-sdk/adhancr/libs/adhancr.jar Assets/Plugins/Android/
cp -r ../android-sdk/adhancr/libs/armeabi-v7a Assets/Plugins/Android/libs/
cp -r ../android-sdk/adhancr/libs/x86 Assets/Plugins/Android/libs/

## Build Android Eclipse Project
rm -r Build/Android
mkdir Build/Android
/Applications/Unity/Unity.app/Contents/MacOS/Unity -batchmode -projectPath ~/Documents/sandbox/unity-sdk-dev -executeMethod BuildScript.BuildAndroidEclipseProject -quit
