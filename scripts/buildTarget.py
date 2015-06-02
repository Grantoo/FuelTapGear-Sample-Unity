#!/usr/bin/python

import sys
import shutil
import os
import fileinput
import time


if len(sys.argv) < 4:
    print 'arg 1 = Target Build Sku'
    print 'arg 2 = Unity folder name'
    print 'arg 3 = Output File Name'
    print 'arg 4 = Unity Project Path'
    print 'Example:'
    print 'python buildTarget.py Android Unity TapGear_v1.apk TapGearProjectOath'
    sys.exit(1)


#This system only works with Unity 5.x.x - 4.x.x does not permit building from the command line
targetBuildSku = sys.argv[1] 	    #build target - iOS, Android
unityFolderName = sys.argv[2] 	    #name of the unity folder in applications
outputFileName = sys.argv[3] 	    #name of the output file
unityProjectPath = sys.argv[4] 	    #this is our project that we are building from
buildNumber = sys.argv[5]


print 'Target Build Sku : ' + targetBuildSku
print 'Unity Folder Name : ' + unityFolderName
print 'Output File Name : ' + outputFileName
print 'Unity Project Path : ' + unityProjectPath

if targetBuildSku == 'Android':
    keystorePass = sys.argv[6]

    print 'building Android...'
    os.system("\"/Applications/" + unityFolderName + "/Unity.app/Contents/MacOS/Unity\" -quit -batchmode -projectPath \"" + unityProjectPath + "\" -executeMethod AutoBuilder.PerformAndroidBuild \"" + outputFileName + "\" " + buildNumber + " " + keystorePass)
    print 'Build Complete'
elif targetBuildSku == 'iOS':
    print 'building iOS...'
    os.system("\"/Applications/" + unityFolderName + "/Unity.app/Contents/MacOS/Unity\" -quit -batchmode -projectPath \"" + unityProjectPath + "\" -executeMethod AutoBuilder.PerformiOSBuild \"" + outputFileName + "\" " + buildNumber)
    print 'Build Complete'
else:
    print targetBuildSku + ' not defined'


print 'cleaning up...'
print 'done.'

