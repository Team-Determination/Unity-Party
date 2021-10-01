#! /bin/sh

# Example install script for Unity3D project. See the entire example: https://github.com/JonathanPorta/ci-build

# This link changes from time to time. I haven't found a reliable hosted installer package for doing regular
# installs like this. You will probably need to grab a current link from: http://unity3d.com/get-unity/download/archive
echo 'Downloading from https://download.unity3d.com/download_unity/1381962e9d08/UnityDownloadAssistant.dmg'
curl -o Unity.dmg https://download.unity3d.com/download_unity/1381962e9d08/UnityDownloadAssistant.dmg

echo 'Installing Unity.dmg'
sudo installer -dumplog -package Unity.dmg -target /
