#!/bin/bash
sed -i '' -e '/<\/dict>/i\
	<key>NSAppTransportSecurity</key>\
	<dict>\
		<key>NSAllowsArbitraryLoads</key>\
		<true/>\
	</dict>\
' Platforms/MacCatalyst/Info.plist

sed -i '' -e '/<\/dict>/i\
	<key>NSAppTransportSecurity</key>\
	<dict>\
		<key>NSAllowsArbitraryLoads</key>\
		<true/>\
	</dict>\
' Platforms/iOS/Info.plist
