#!/usr/bin/python

import sys
import shutil
import os
import fileinput

from mod_pbxproj import XcodeProject

def exitWithError( message ):
	print 'Error: ' + message
	sys.exit('Failed automatic integration')

if len(sys.argv) < 6:
    exitWithError('Argument list does not contain enough arguments')

pluginPath = sys.argv[1]
xcodePath = sys.argv[2]
projectPath = sys.argv[3]
dataPath = sys.argv[4]
unityApiLevel = int(sys.argv[5].strip())


# Unity API levels
# 0 - UNSUPPORTED
# 1 - UNITY_2_6,
# 2 - UNITY_2_6_1,
# 3 - UNITY_3_0,
# 4 - UNITY_3_0_0,
# 5 - UNITY_3_1,
# 6 - UNITY_3_2,
# 7 - UNITY_3_3,
# 8 - UNITY_3_4,
# 9 - UNITY_3_5,
# 10 - UNITY_4_0,
# 11 - UNITY_4_0_1,
# 12 - UNITY_4_1,
# 13 - UNITY_4_2,
# 14 - UNITY_4_3,
# 15 - UNITY_4_5

# only supporting Unity 3.5 and up
if unityApiLevel < 15:
	exitWithError('Unsupported Unity version')

def checkPath(path, libname):
	if path == None:
		exitWithError('Unable to find ' + libname + ' which is required for Propeller integration. Add it manually or modify PropellerBuild.py to find the correct folder.')

print 'Adding Propeller dependencies to project'

systemConfigurationFrameworkPath = None
adSupportFrameworkPath = None
accountsFrameworkPath = None
socialFrameworkPath = None
securityFrameworkPath = None
libsqlite3Path = None
libicucorePath = None

for directory, dirnames, filenames in os.walk( xcodePath + '/Platforms/iPhoneOS.platform/Developer/SDKs' ):
	if os.path.basename( directory ) == 'SystemConfiguration.framework':
		systemConfigurationFrameworkPath = directory
	elif os.path.basename( directory ) == 'AdSupport.framework':
		adSupportFrameworkPath = directory
	elif os.path.basename( directory ) == 'Social.framework':
		socialFrameworkPath = directory
	elif os.path.basename( directory ) == 'Security.framework':
		securityFrameworkPath = directory
	else:
		if 'libsqlite3.dylib' in filenames:
			libsqlite3Path = directory + '/libsqlite3.dylib'
		if 'libicucore.dylib' in filenames:
			libicucorePath = directory + '/libicucore.dylib'

print 'Locating frameworks and resources required by Propeller'

checkPath( systemConfigurationFrameworkPath, 'SystemConfiguration.framework' )
checkPath( adSupportFrameworkPath, 'AdSupport.framework' )
checkPath( socialFrameworkPath, 'Social.framework' )
checkPath( securityFrameworkPath, 'Security.framework' )
checkPath( libsqlite3Path, 'libsqlite3.dylib' )
checkPath( libicucorePath, 'libicucore.dylib' )

project = XcodeProject.Load( projectPath + '/Unity-iPhone.xcodeproj/project.pbxproj' )

print 'Adding frameworks required by Propeller'

frameworkGroup = project.get_or_create_group('Frameworks')

project.add_file_if_doesnt_exist( systemConfigurationFrameworkPath, tree='SDKROOT', parent=frameworkGroup, weak=True )
project.add_file_if_doesnt_exist( adSupportFrameworkPath, tree='SDKROOT', parent=frameworkGroup, weak=True )
project.add_file_if_doesnt_exist( socialFrameworkPath, tree='SDKROOT', parent=frameworkGroup, weak=True )
project.add_file_if_doesnt_exist( securityFrameworkPath, tree='SDKROOT', parent=frameworkGroup, weak=True )
project.add_file_if_doesnt_exist( libsqlite3Path, tree='SDKROOT', parent=frameworkGroup, weak=True )
project.add_file_if_doesnt_exist( libicucorePath, tree='SDKROOT', parent=frameworkGroup, weak=True )

print 'Adding resources required by Propeller'

resourceGroup = project.get_or_create_group('Resources')

def addFacebookDependencies(dataPath, frameworkGroup, resourceGroup):
	if os.path.exists(dataPath + '/Facebook/Scripts/FB.cs'):
		print 'Found Unity Facebook SDK'
		return True

	iOSFacebookSDKFrameworkPath = None

	for directory, dirnames, filenames in os.walk( os.path.expanduser('~') + '/Documents/FacebookSDK' ):
		if os.path.basename( directory ) == 'FacebookSDK.framework':
			iOSFacebookSDKFrameworkPath = directory
			break;

	if iOSFacebookSDKFrameworkPath == None:
		return False

	print 'Found iOS Facebook SDK'

	project.add_file_if_doesnt_exist( iOSFacebookSDKFrameworkPath, tree='SDKROOT', parent=frameworkGroup )

	iOSFacebookSDKResourcesBundlePath = iOSFacebookSDKFrameworkPath + '/Resources/FacebookSDKResources.bundle'

	if os.path.exists(iOSFacebookSDKResourcesBundlePath):
		print 'iOS Facebook SDK resources bundle found'
		project.add_file_if_doesnt_exist( iOSFacebookSDKResourcesBundlePath, tree='SDKROOT', parent=resourceGroup )

	project.add_framework_search_paths([iOSFacebookSDKFrameworkPath + '/' + os.pardir])

	return True

print 'Adding Facebook dependencies required by Propeller'

if not addFacebookDependencies(dataPath, frameworkGroup, resourceGroup):
    exitWithError('No Facebook SDK found')

print 'Inserting Propeller libraries'

classesGroup = project.get_or_create_group( 'Classes' )
librariesGroup = project.get_or_create_group( 'Libraries' )
shutil.copy( pluginPath + '/PropellerSDK.h' , projectPath + '/Classes/PropellerSDK.h' )
shutil.copy( pluginPath + '/libPropellerSDK.a', projectPath + '/Libraries/libPropellerSDK.a' )
shutil.copy( pluginPath + '/PropellerSDKUnityWrapper.mm', projectPath + '/Libraries/PropellerSDKUnityWrapper.mm' )
project.add_file_if_doesnt_exist( projectPath + '/Classes/PropellerSDK.h', parent=classesGroup )
project.add_file_if_doesnt_exist( projectPath + '/Libraries/libPropellerSDK.a', parent=librariesGroup )
project.add_file_if_doesnt_exist( projectPath + '/Libraries/PropellerSDKUnityWrapper.mm', parent=librariesGroup )

project.saveFormat3_2()

print 'Injecting Propeller script into AppController'

# Unity 4.2 changed the name of the AppController.mm file to UnityAppController.mm
controllerFile = projectPath + '/Classes/'
if unityApiLevel < 13:
    controllerFile += 'AppController.mm'
else:
    controllerFile += 'UnityAppController.mm'

# inject code into AppController.mm or UnityAppController.m

def addHeader():
	print '// *** INSERTED BY PROPELLER BUILD SCRIPT ***'
	print ''
	print '#include "PropellerSDK.h"'
	print ''
	print '// *** END PROPELLER BUILD SCRIPT INSERTION ***'
	print ''

def addReceiveLocalNotification():
	print '\t// *** INSERTED BY PROPELLER BUILD SCRIPT ***'
	print ''
	print '\t[PropellerSDK handleLocalNotification:notification newLaunch:NO];'
	print ''
	print '\t// *** END PROPELLER BUILD SCRIPT INSERTION ***'
	print ''

def addReceiveRemoteNotification():
	print '\t// *** INSERTED BY PROPELLER BUILD SCRIPT ***'
	print ''
	print '\t[PropellerSDK handleRemoteNotification:userInfo newLaunch:NO];'
	print ''
	print '\t// *** END PROPELLER BUILD SCRIPT INSERTION ***'
	print ''

def addRegisterRemoteNotifications():
	print '\t// *** INSERTED BY PROPELLER BUILD SCRIPT ***'
	print ''
	print '\tNSString *devToken = [[deviceToken description] stringByTrimmingCharactersInSet:[NSCharacterSet characterSetWithCharactersInString:@"<>"]];'
	print '\tdevToken = [devToken stringByReplacingOccurrencesOfString:@" " withString:@""];'
	print '\t[PropellerSDK setNotificationToken:devToken];'
	print ''
	print '\t// *** END PROPELLER BUILD SCRIPT INSERTION ***'
	print ''

def addFailToRegisterNotifications():
	print '\t// *** INSERTED BY PROPELLER BUILD SCRIPT ***'
	print ''
	print '\t[PropellerSDK setNotificationToken:nil];'
	print ''
	print '\t// *** END PROPELLER BUILD SCRIPT INSERTION ***'
	print ''

def addLaunchOptionLines():
	print '\t// *** INSERTED BY PROPELLER BUILD SCRIPT ***'
	print ''
	print '\tNSDictionary *remoteNotificationDict = [launchOptions objectForKey:UIApplicationLaunchOptionsRemoteNotificationKey];'
	print ''
	print '\tif (remoteNotificationDict)'
	print '\t{'
	print '\t\tif (![PropellerSDK handleRemoteNotification:remoteNotificationDict newLaunch:YES])'
	print '\t\t{'
	print '\t\t\t// This is not a Grantoo remote notification, I should handle it'
	print '\t\t}'
	print '\t}'
	print ''
	print '\tUILocalNotification *localNotification = [launchOptions objectForKey:UIApplicationLaunchOptionsLocalNotificationKey];'
	print ''
	print '\tif (localNotification)'
	print '\t{'
	print '\t\tif (![PropellerSDK handleLocalNotification:localNotification newLaunch:YES])'
	print '\t\t{'
	print '\t\t\t// This is not a Grantoo local notification, I should handle it'
	print '\t\t}'
	print '\t}'
	print ''
	print '\t// we want to register this device with the APNS'
	print '#if __IPHONE_OS_VERSION_MAX_ALLOWED >= 80000'
	print '\tif ([[[UIDevice currentDevice] systemVersion] compare:@"8.0" options:NSNumericSearch] != NSOrderedAscending) {'
	print '\t\tUIUserNotificationType userNotificationTypes = (UIUserNotificationTypeAlert |'
	print '                                                        UIUserNotificationTypeBadge |'
	print '                                                        UIUserNotificationTypeSound);'
	print '\t\tUIUserNotificationSettings *userNotificationSettings = [UIUserNotificationSettings'
	print '                                                                settingsForTypes:userNotificationTypes'
	print '                                                                      categories:nil];'
	print '\t\t[[UIApplication sharedApplication] registerUserNotificationSettings:userNotificationSettings];'
	print '\t} else {'
	print '\t\tUIRemoteNotificationType remoteNotificationTypes = (UIRemoteNotificationTypeAlert |'
	print '                                                            UIRemoteNotificationTypeBadge |'
	print '                                                            UIRemoteNotificationTypeSound);'
	print '\t\t[[UIApplication sharedApplication] registerForRemoteNotificationTypes:remoteNotificationTypes];'
	print '\t}'
	print '#else'
	print '\tUIRemoteNotificationType remoteNotificationTypes = (UIRemoteNotificationTypeAlert |'
	print '                                                        UIRemoteNotificationTypeBadge |'
	print '                                                        UIRemoteNotificationTypeSound);'
	print '\t[[UIApplication sharedApplication] registerForRemoteNotificationTypes:remoteNotificationTypes];'
	print '#endif'
	print ''
	print '\t// *** END PROPELLER BUILD SCRIPT INSERTION ***'
	print ''

def addEnterBackground():
	print '\t// *** INSERTED BY PROPELLER BUILD SCRIPT ***'
	print ''
	print '\t[[UIApplication sharedApplication] setApplicationIconBadgeNumber:1];'
	print '\t[[UIApplication sharedApplication] setApplicationIconBadgeNumber:0];'
	print '\t[[UIApplication sharedApplication] cancelAllLocalNotifications];'
	print '\t[PropellerSDK restoreAllLocalNotifications];'
	print ''
	print '\t// *** END PROPELLER BUILD SCRIPT INSERTION ***'
	print ''

def addEnterForeground():
	print '\t// *** INSERTED BY PROPELLER BUILD SCRIPT ***'
	print ''
	print '\t[PropellerSDK handleApplicationWillEnterForeground:application];'
	print ''
	print '\t// *** END PROPELLER BUILD SCRIPT INSERTION ***'
	print ''
	
def addDealloc():
	print ''
	print '\t// *** INSERTED BY PROPELLER BUILD SCRIPT ***'
	print ''
	print '\t[[NSNotificationCenter defaultCenter] removeObserver:self];'
	print ''
	print '\t// *** END PROPELLER BUILD SCRIPT INSERTION ***'
	print ''

def addExtraFunctions():
	print ''
	print '// *** INSERTED BY PROPELLER BUILD SCRIPT ***'
	print ''
	print '-(id)init'
	print '{'
	print '\tif (self = [super init])'
	print '\t{'
	print '\t\t[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(receiveLocalNotification:) name:@"PropellerSDKChallengeCountChanged" object:nil];'
	print '\t\t[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(receiveLocalNotification:) name:@"PropellerSDKTournamentInfo" object:nil];'
	print '\t\t[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(receiveLocalNotification:) name:@"PropellerSDKVirtualGoodList" object:nil];'
	print '\t\t[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(receiveLocalNotification:) name:@"PropellerSDKVirtualGoodRollback" object:nil];'
	print '\t\t[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(receiveLocalNotification:) name:@"FuelDynamicsUserValues" object:nil];'
	print '\t}'
	print ''
	print '\treturn self;'
	print '}'
	print ''
	print '-(NSString *)urlEncode:(NSString *)rawString'
	print '{'
	print '\treturn CFBridgingRelease(CFURLCreateStringByAddingPercentEscapes(NULL, (CFStringRef)rawString, NULL, (CFStringRef)@"!*\'();:@&=+$,/?%#[]", kCFStringEncodingUTF8));'
	print '}'	
	print ''
	print '-(void)receiveLocalNotification:(NSNotification *) notification'
	print '{'
	print '\tNSDictionary *data = notification.userInfo;'
	print '\tNSString *type = [notification name];'
	print ''
	print '\tif ([type isEqualToString:@"PropellerSDKChallengeCountChanged"])'
	print '\t{'
	print '\t\tNSString *message = nil;'
	print ''
	print '\t\tif (data == nil) {'
	print '\t\t\tmessage = @"";'
	print '\t\t} else {'
	print '\t\t\tNSString *count = @"";'
	print '\t\t\tNSObject *countObject = [data objectForKey:@"count"];'
	print ''
	print '\t\t\tif ((countObject != nil) && [countObject isKindOfClass:[NSNumber class]]) {'
	print '\t\t\t\tint countValue = [((NSNumber*) countObject) integerValue];'
	print '\t\t\t\tcount = [NSString stringWithFormat:@"%i", countValue];'
	print '\t\t\t\t[[UIApplication sharedApplication] setApplicationIconBadgeNumber:countValue];'
	print '\t\t\t}'
	print ''
	print '\t\t\tmessage = count;'
	print '\t\t}'
	print ''
	print '\t\tUnitySendMessage("PropellerSDK", "PropellerOnChallengeCountChanged", [message UTF8String]);'
	print '\t} else if ([type isEqualToString:@"PropellerSDKTournamentInfo"]) {'
	print '\t\tNSString *message = nil;'
	print ''
	print '\t\tif (data == nil) {'
	print '\t\t\tmessage = @"";'
	print '\t\t} else {'
	print '\t\t\tNSMutableArray *paramList = [[NSMutableArray alloc] init];'
	print '\t\t\tNSString *name = @"";'
	print '\t\t\tNSObject *nameObject = [data objectForKey:@"name"];'
	print ''
	print '\t\t\tif ((nameObject != nil) && [nameObject isKindOfClass:[NSString class]]) {'
	print '\t\t\t\tname = [self urlEncode:((NSString*) nameObject)];'
	print '\t\t\t}'
	print ''
	print '\t\t\t[paramList addObject:name];'
	print ''
	print '\t\t\tNSString *campaignName = @"";'
	print '\t\t\tNSObject *campaignNameObject = [data objectForKey:@"campaignName"];'
	print ''
	print '\t\t\tif ((campaignNameObject != nil) && [campaignNameObject isKindOfClass:[NSString class]]) {'
	print '\t\t\t\tcampaignName = [self urlEncode:((NSString*) campaignNameObject)];'
	print '\t\t\t}'
	print ''
	print '\t\t\t[paramList addObject:campaignName];'
	print ''
	print '\t\t\tNSString *sponsorName = @"";'
	print '\t\t\tNSObject *sponsorNameObject = [data objectForKey:@"sponsorName"];'
	print ''
	print '\t\t\tif ((sponsorNameObject != nil) && [sponsorNameObject isKindOfClass:[NSString class]]) {'
	print '\t\t\t\tsponsorName = [self urlEncode:((NSString*) sponsorNameObject)];'
	print '\t\t\t}'
	print ''
	print '\t\t\t[paramList addObject:sponsorName];'
	print ''
	print '\t\t\tNSString *startDate = @"";'
	print '\t\t\tNSObject *startDateObject = [data objectForKey:@"startDate"];'
	print ''
	print '\t\t\tif ((startDateObject != nil) && [startDateObject isKindOfClass:[NSNumber class]]) {'
	print '\t\t\t\tstartDate = [self urlEncode:[((NSNumber*) startDateObject) stringValue]];'
	print '\t\t\t}'
	print ''
	print '\t\t\t[paramList addObject:startDate];'
	print ''
	print '\t\t\tNSString *endDate = @"";'
	print '\t\t\tNSObject *endDateObject = [data objectForKey:@"endDate"];'
	print ''
	print '\t\t\tif ((endDateObject != nil) && [endDateObject isKindOfClass:[NSNumber class]]) {'
	print '\t\t\t\tendDate = [self urlEncode:[((NSNumber*) endDateObject) stringValue]];'
	print '\t\t\t}'
	print ''
	print '\t\t\t[paramList addObject:endDate];'
	print ''
	print '\t\t\tNSString *logo = @"";'
	print '\t\t\tNSObject *logoObject = [data objectForKey:@"logo"];'
	print ''
	print '\t\t\tif ((logoObject != nil) && [logoObject isKindOfClass:[NSString class]]) {'
	print '\t\t\t\tlogo = [self urlEncode:((NSString*) logoObject)];'
	print '\t\t\t}'
	print ''
	print '\t\t\t[paramList addObject:logo];'
	print ''
	print '\t\t\tmessage = [paramList componentsJoinedByString:@"&"];'
	print '\t\t}'
	print ''
	print '\t\tUnitySendMessage("PropellerSDK", "PropellerOnTournamentInfo", [message UTF8String]);'
	print '\t} else if ([type isEqualToString:@"PropellerSDKVirtualGoodList"]) {'
	print '\t\tNSString *message = nil;'
	print ''
	print '\t\tif (data == nil) {'
	print '\t\t\tmessage = @"";'
	print '\t\t} else {'
	print '\t\t\tNSMutableArray *paramList = [[NSMutableArray alloc] init];'
	print '\t\t\tNSString *transactionId = @"";'
	print '\t\t\tNSObject *transactionIdObject = [data objectForKey:@"transactionID"];'
	print ''
	print '\t\t\tif ((transactionIdObject != nil) && [transactionIdObject isKindOfClass:[NSString class]]) {'
	print '\t\t\t\ttransactionId = (NSString*) transactionIdObject;'
	print '\t\t\t}'
	print '\t\t\t[paramList addObject:transactionId];'
	print ''
	print '\t\t\tNSObject *virtualGoodsObject = [data objectForKey:@"virtualGoods"];'
	print ''
	print '\t\t\tif ((virtualGoodsObject != nil) && [virtualGoodsObject isKindOfClass:[NSArray class]]) {'
	print '\t\t\t\tNSArray *virtualGoods = (NSArray*) virtualGoodsObject;'
	print ''
	print '\t\t\t\tfor (NSObject* virtualGoodObject in virtualGoods) {'
	print '\t\t\t\t\tif ((virtualGoodObject == nil) || ![virtualGoodObject isKindOfClass:[NSDictionary class]]) {'
	print '\t\t\t\t\t\tcontinue;'
	print '\t\t\t\t\t}'
	print ''
	print '\t\t\t\t\tNSDictionary *virtualGood = (NSDictionary*) virtualGoodObject;'
	print '\t\t\t\t\tNSObject *virtualGoodIdObject = [virtualGood objectForKey:@"goodId"];'
	print ''
	print '\t\t\t\t\tif ((virtualGoodIdObject == nil) || ![virtualGoodIdObject isKindOfClass:[NSString class]]) {'
	print '\t\t\t\t\t\tcontinue;'
	print '\t\t\t\t\t}'
	print ''
	print '\t\t\t\t\t[paramList addObject:(NSString*) virtualGoodIdObject];'
	print '\t\t\t\t}'
	print '\t\t\t}'
	print ''
	print '\t\t\tmessage = [paramList componentsJoinedByString:@"&"];'
	print '\t\t}'
	print ''
	print '\t\tUnitySendMessage("PropellerSDK", "PropellerOnVirtualGoodList", [message UTF8String]);'
	print '\t} else if ([type isEqualToString:@"PropellerSDKVirtualGoodRollback"]) {'
	print '\t\tNSString *message = nil;'
	print ''
	print '\t\tif (data == nil) {'
	print '\t\t\tmessage = @"";'
	print '\t\t} else {'
	print '\t\t\tNSString *transactionId = @"";'
	print '\t\t\tNSObject *transactionIdObject = [data objectForKey:@"transactionID"];'
	print ''
	print '\t\t\tif ((transactionIdObject != nil) && [transactionIdObject isKindOfClass:[NSString class]]) {'
	print '\t\t\t\ttransactionId = (NSString*) transactionIdObject;'
	print '\t\t\t}'
	print ''
	print '\t\t\tmessage = transactionId;'
	print '\t\t}'
	print ''
	print '\t\tUnitySendMessage("PropellerSDK", "PropellerOnVirtualGoodRollback", [message UTF8String]);'
	print '\t} else if ([type isEqualToString:@"FuelDynamicsUserValues"]) {'
	print '\t\tNSString *message = nil;'
	print ''
	print '\t\tif (data == nil) {'
	print '\t\t\tmessage = @"";'
	print '\t\t} else {'
	print '\t\t\tNSMutableArray *paramList = [[NSMutableArray alloc] init];'
	print ''
	print '\t\t\tfor (id item in data){'
	print '\t\t\t\t[paramList addObject:item];'
	print '\t\t\t\t[paramList addObject:[data objectForKey:item]];'
	print '\t\t\t}'
	print ''
	print '\t\t\tmessage = [paramList componentsJoinedByString:@"&"];'
	print '\t\t}'
	print ''
	print '\t\tbool hasCompete = [PropellerSDK getHasCompete];'
	print '\t\tif(hasCompete == true) {'
	print '\t\t\tUnitySendMessage("PropellerSDK", "FuelDynamicsUserValues", [message UTF8String]);'
	print '\t\t} else {'
	print '\t\t\tUnitySendMessage("FuelDynamics", "FuelDynamicsUserValues", [message UTF8String]);'
	print '\t\t}'
	print '\t}'
	print '}'
	print ''
	print '// *** END PROPELLER BUILD SCRIPT INSERTION ***'
	print ''

injectHeader = False
injectReceiveLocalNotification = False
injectReceiveRemoteNotification = False
injectRegisterNotifications = False
injectFailToRegisterNotifications = False
injectLaunch = False
injectEnterBackground = False
injectEnterForeground = False
injectExtraFunctions = False

hasRegisterUserNotificationSettings = False

lastNonBlankLine = ''

for line in fileinput.input( controllerFile, inplace=1 ):
	if len(line.strip()) == 0:
		print line,
		continue

	if '#include <mach/mach_time.h>' in line:
		injectHeader = True
	elif 'didReceiveLocalNotification:' in line:
		injectReceiveLocalNotification = True
	elif 'didRegisterForRemoteNotificationsWithDeviceToken:' in line:
		injectRegisterNotifications = True
	elif 'didFailToRegisterForRemoteNotificationsWithError:' in line:
		injectFailToRegisterNotifications = True
	elif 'didReceiveRemoteNotification:' in line:
		injectReceiveRemoteNotification = True
	elif 'didFinishLaunchingWithOptions:' in line:
		injectLaunch = True
	elif 'applicationDidEnterBackground:' in line:
		injectEnterBackground = True
	elif 'applicationWillEnterForeground:' in line:
		injectEnterForeground = True
	elif '[super dealloc]' in line:
		if '// *** END PROPELLER BUILD SCRIPT INSERTION ***' not in lastNonBlankLine:
			addDealloc()
		injectDealloc = False
		injectExtraFunctions = True
	elif 'didRegisterUserNotificationSettings:' in line:
		hasRegisterUserNotificationSettings = True
	else:
		if injectHeader:
			if '// *** INSERTED BY PROPELLER BUILD SCRIPT ***' not in line:
				addHeader()
			injectHeader = False
		elif injectReceiveLocalNotification and '{' not in line:
			if '// *** INSERTED BY PROPELLER BUILD SCRIPT ***' not in line:
				addReceiveLocalNotification()
			injectReceiveLocalNotification = False
		elif injectReceiveRemoteNotification and '{' not in line:
			if '// *** INSERTED BY PROPELLER BUILD SCRIPT ***' not in line:
				addReceiveRemoteNotification()
			injectReceiveRemoteNotification = False
		elif injectRegisterNotifications and '{' not in line:
			if '// *** INSERTED BY PROPELLER BUILD SCRIPT ***' not in line:
				addRegisterRemoteNotifications()
			injectRegisterNotifications = False
		elif injectFailToRegisterNotifications and '{' not in line:
			if '// *** INSERTED BY PROPELLER BUILD SCRIPT ***' not in line:
				addFailToRegisterNotifications()
			injectFailToRegisterNotifications = False
		elif injectLaunch and 'return ' in line:
			if '// *** END PROPELLER BUILD SCRIPT INSERTION ***' not in lastNonBlankLine:
				addLaunchOptionLines()
			injectLaunch = False
		elif injectEnterBackground and '{' not in line:
			if '// *** INSERTED BY PROPELLER BUILD SCRIPT ***' not in line:
				addEnterBackground()
			injectEnterBackground = False
		elif injectEnterForeground and '{' not in line:
			if '// *** INSERTED BY PROPELLER BUILD SCRIPT ***' not in line:
				addEnterForeground()
			injectEnterForeground = False
		elif injectExtraFunctions and '}' not in line:
			if '// *** INSERTED BY PROPELLER BUILD SCRIPT ***' not in line:
				addExtraFunctions()
				
			injectExtraFunctions = False

	lastNonBlankLine = line

	print line,

fileinput.close()

def addRegisterUserNotificationSettings():
	print '#ifdef __IPHONE_8_0'
	print '- (void)application:(UIApplication *)application didRegisterUserNotificationSettings:(UIUserNotificationSettings *)notificationSettings'
	print '{'
	print '\t[application registerForRemoteNotifications];'
	print '}'
	print '#endif'
	print ''

injectMissingFunctions = False

lastNonBlankLine = ''

for line in fileinput.input( controllerFile, inplace=1 ):
	if len(line.strip()) == 0:
		print line,
		continue

	if 'receiveLocalNotification:' in line:
		injectMissingFunctions = True
	elif injectMissingFunctions and '// *** END PROPELLER BUILD SCRIPT INSERTION ***' in line:
		if not hasRegisterUserNotificationSettings:
			addRegisterUserNotificationSettings()
		injectMissingFunctions = False

	lastNonBlankLine = line

	print line,

fileinput.close()

def addGLViewControllerExport():
	functionExport = True

	for line in fileinput.input( projectPath + '/Classes/AppController.h', inplace=1 ):
		if functionExport:
			if '// *** INSERTED BY PROPELLER BUILD SCRIPT ***' not in line:
				print '// *** INSERTED BY PROPELLER BUILD SCRIPT ***'
				print ''
				print 'UIViewController* UnityGetGLViewController();'
				print ''
				print '// *** END PROPELLER BUILD SCRIPT INSERTION ***'
				print ''
			functionExport = False

		print line,

	fileinput.close()

def addWrapperHeader(headerFile):
	headerCorrect = False
	injectHeader = False

	for line in fileinput.input( projectPath + '/Libraries/PropellerSDKUnityWrapper.mm', inplace=1 ):
		if not headerCorrect:
			if injectHeader:
				if '#import "' + headerFile + '"' not in line:
					print '#import "' + headerFile + '"'
				headerCorrect = True
			else:
				if '#import "PropellerSDK.h"' in line:
					injectHeader = True

		print line,

	fileinput.close()

print 'Injecting compatibility shim into PropellerSDKUnityWrapper'

if unityApiLevel >= 15:
	addWrapperHeader('UI/UnityViewControllerBase.h')
elif unityApiLevel >= 10:
	addWrapperHeader('iPhone_View.h')
else:
	addGLViewControllerExport()
	addWrapperHeader('AppController.h')

print 'Propeller SDK code injection completed!'
