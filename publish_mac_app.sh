#!/bin/bash

APP_NAME="AttendanceRecord"
PROJECT_NAME="AttendanceRecord.Presentation"
BIN_PATH="./$PROJECT_NAME/bin/Release/net10.0/osx-arm64/publish"
APP_BUNDLE="$APP_NAME.app"

dotnet publish -c Release "$PROJECT_NAME"

mkdir -p "$APP_BUNDLE/Contents/MacOS"
mkdir -p "$APP_BUNDLE/Contents/Resources"

cp $BIN_PATH/$APP_NAME "$APP_BUNDLE/Contents/MacOS"
cp $BIN_PATH/*.dylib "$APP_BUNDLE/Contents/MacOS"

cp "./$PROJECT_NAME/Info.plist" "$APP_BUNDLE/Contents/"

chmod +x "$APP_BUNDLE/Contents/MacOS/$APP_NAME"
xattr -dr com.apple.quarantine "$APP_BUNDLE"
