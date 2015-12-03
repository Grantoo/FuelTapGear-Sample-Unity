#!/usr/bin/python

import sys
import shutil
import os
import fileinput
import time


if len(sys.argv) < 3:
    print 'arg 1 = Unity folder name'
    print 'arg 2 = Unity project path'
    print 'arg 3 = Unity package path'
    print 'Example:'
    print 'python buildTarget.py Unity <TapGearProjectPath> <UnityPackagePath>'
    sys.exit(1)

# the import will only succeed if there are no compilation errors when the Unity project is opened, prior to the import

unityFolderName = sys.argv[1] 	    #name of the unity folder in applications
unityProjectPath = sys.argv[2] 	    #this is our project that we are building from
unityPackagePath = sys.argv[3]      #this is the path to the unity package to import

print 'Unity Folder Name : ' + unityFolderName
print 'Unity Project Path : ' + unityProjectPath
print 'Unity Package Path : ' + unityPackagePath

print 'importing...'

os.system("\"/Applications/" + unityFolderName + "/Unity.app/Contents/MacOS/Unity\" -quit -batchmode -projectPath \"" + unityProjectPath + "\" -importPackage \"" + unityPackagePath + "\"")

print 'done.'

