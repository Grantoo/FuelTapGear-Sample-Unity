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
xcodeVersion = sys.argv[6]

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
# 15 - UNITY_4_5,
# 16 - UNITY_4_6,
# 17 - UNITY_5_0,
# 18 - UNITY_5_1

# only supporting Unity 3.5 and up
if unityApiLevel < 9:
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
cfNetworkFrameworkPath = None
audioToolboxFrameworkPath = None
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
	elif os.path.basename( directory ) == 'CFNetwork.framework':
		cfNetworkFrameworkPath = directory
	elif os.path.basename( directory ) == 'AudioToolbox.framework':
		audioToolboxFrameworkPath = directory
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
checkPath( cfNetworkFrameworkPath, 'CFNetwork.framework' )
checkPath( audioToolboxFrameworkPath, 'AudioToolbox.framework' )
checkPath( libsqlite3Path, 'libsqlite3.dylib' )
checkPath( libicucorePath, 'libicucore.dylib' )

project = XcodeProject.Load( projectPath + '/Unity-iPhone.xcodeproj/project.pbxproj' )

print 'Adding frameworks required by Propeller'

frameworkGroup = project.get_or_create_group('Frameworks')

project.add_file_if_doesnt_exist( systemConfigurationFrameworkPath, tree='SDKROOT', parent=frameworkGroup )
project.add_file_if_doesnt_exist( adSupportFrameworkPath, tree='SDKROOT', parent=frameworkGroup )
project.add_file_if_doesnt_exist( socialFrameworkPath, tree='SDKROOT', parent=frameworkGroup )
project.add_file_if_doesnt_exist( securityFrameworkPath, tree='SDKROOT', parent=frameworkGroup )
project.add_file_if_doesnt_exist( cfNetworkFrameworkPath, tree='SDKROOT', parent=frameworkGroup )
project.add_file_if_doesnt_exist( audioToolboxFrameworkPath, tree='SDKROOT', parent=frameworkGroup )
project.add_file_if_doesnt_exist( libsqlite3Path, tree='SDKROOT', parent=frameworkGroup )
project.add_file_if_doesnt_exist( libicucorePath, tree='SDKROOT', parent=frameworkGroup )

print 'Inserting Propeller libraries'

classesGroup = project.get_or_create_group( 'Classes' )
librariesGroup = project.get_or_create_group( 'Libraries' )

if unityApiLevel < 17:
	shutil.copy( pluginPath + '/PropellerSDK.h' , projectPath + '/Classes/PropellerSDK.h' )
	shutil.copy( pluginPath + '/libPropellerSDK.a', projectPath + '/Libraries/libPropellerSDK.a' )
	shutil.copy( pluginPath + '/PropellerSDKUnityWrapper.mm', projectPath + '/Libraries/PropellerSDKUnityWrapper.mm' )
	project.add_file_if_doesnt_exist( projectPath + '/Classes/PropellerSDK.h', parent=classesGroup )
	project.add_file_if_doesnt_exist( projectPath + '/Libraries/libPropellerSDK.a', parent=librariesGroup )
	project.add_file_if_doesnt_exist( projectPath + '/Libraries/PropellerSDKUnityWrapper.mm', parent=librariesGroup )

project.saveFormat3_2()

# Unity 4.2 changed the name of the AppController.mm file to UnityAppController.mm
controllerFilename = ''

if unityApiLevel < 13:
	controllerFilename = 'AppController.mm'
else:
	controllerFilename = 'UnityAppController.mm'

controllerFile = projectPath + '/Classes/' + controllerFilename

print 'Injecting Propeller script into ' + controllerFilename

# inject code into AppController.mm or UnityAppController.m

injectionPrefix = '// *** INSERTED BY PROPELLER BUILD SCRIPT ***'
injectionSuffix = '// *** END PROPELLER BUILD SCRIPT INSERTION ***'

def addInjectionPrefix(indents=0):
	print '\t' * indents + injectionPrefix
	print ''

def addInjectionSuffix(indents=0):
	print ''
	print '\t' * indents + injectionSuffix
	print ''

def addFunctionInjectionPrefix(signature, contentOnly=False, contentIndents=1):
	if not contentOnly:
		addInjectionPrefix()
		print signature
		print '{'
	else:
		addInjectionPrefix(contentIndents)

def addFunctionInjectionSuffix(contentOnly=False, contentIndents=1):
	if not contentOnly:
		print '}'
		addInjectionSuffix()
	else:
		addInjectionSuffix(contentIndents)

def addHeader():
	addInjectionPrefix()
	print '#include "PropellerSDK.h"'
	addInjectionSuffix()

def addInit(contentOnly=False):
	addFunctionInjectionPrefix('- (id)init', contentOnly, 2)
	if not contentOnly:
		print '\tif( (self = [super init]) )'
		print '\t{'
	print '\t\t[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(receiveLocalNotification:) name:@"PropellerSDKChallengeCountChanged" object:nil];'
	print '\t\t[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(receiveLocalNotification:) name:@"PropellerSDKTournamentInfo" object:nil];'
	print '\t\t[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(receiveLocalNotification:) name:@"PropellerSDKVirtualGoodList" object:nil];'
	print '\t\t[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(receiveLocalNotification:) name:@"PropellerSDKVirtualGoodRollback" object:nil];'
	print '\t\t[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(receiveLocalNotification:) name:@"PropellerSDKUserValues" object:nil];'
	print '\t\t[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(receiveLocalNotification:) name:@"PropellerSDKNotification" object:nil];'
	if not contentOnly:
		print '\t}'
		print '\treturn self;'
	addFunctionInjectionSuffix(contentOnly, 2)

def addDealloc(contentOnly=False):
	addFunctionInjectionPrefix('- (void) dealloc', contentOnly)
	print '\t[[NSNotificationCenter defaultCenter] removeObserver:self];'
	if not contentOnly:
		print '#if !__has_feature(objc_arc)'
		print '\t[super dealloc];'
		print '#endif'
	addFunctionInjectionSuffix(contentOnly)

def addReceiveLocalNotification(contentOnly=False):
	addFunctionInjectionPrefix('- (void)application:(UIApplication*)application didReceiveLocalNotification:(UILocalNotification*)notification', contentOnly)
	print '\tif ([PropellerSDK handleLocalNotification:notification newLaunch:NO]) {'
	print '\t\t[self broadcastLocalNotification:NO];'
	print '\t}'
	addFunctionInjectionSuffix(contentOnly)

def addRegisterRemoteNotifications(contentOnly=False):
	addFunctionInjectionPrefix('- (void)application:(UIApplication*)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData*)deviceToken', contentOnly)
	print '\tNSString *devToken = [[deviceToken description] stringByTrimmingCharactersInSet:[NSCharacterSet characterSetWithCharactersInString:@"<>"]];'
	print '\tdevToken = [devToken stringByReplacingOccurrencesOfString:@" " withString:@""];'
	print '\t[PropellerSDK setNotificationToken:devToken];'
	addFunctionInjectionSuffix(contentOnly)

def addFailToRegisterRemoteNotifications(contentOnly=False):
	addFunctionInjectionPrefix('- (void)application:(UIApplication*)application didFailToRegisterForRemoteNotificationsWithError:(NSError*)error', contentOnly)
	print '\t[PropellerSDK setNotificationToken:nil];'
	addFunctionInjectionSuffix(contentOnly)

def addReceiveRemoteNotification(contentOnly=False):
	addFunctionInjectionPrefix('- (void)application:(UIApplication*)application didReceiveRemoteNotification:(NSDictionary*)userInfo', contentOnly)
	print '\tif ([PropellerSDK handleRemoteNotification:userInfo newLaunch:NO]) {'
	print '\t\t[self broadcastLocalNotification:NO];'
	print '\t}'
	addFunctionInjectionSuffix(contentOnly)

def addRegisterUserNotificationSettings(contentOnly=False):
	print '#ifdef __IPHONE_8_0'
	print ''
	addFunctionInjectionPrefix('- (void)application:(UIApplication *)application didRegisterUserNotificationSettings:(UIUserNotificationSettings *)notificationSettings', contentOnly)
	print '\t[application registerForRemoteNotifications];'
	addFunctionInjectionSuffix(contentOnly)
	print '#endif'
	print ''

def addFinishLaunching(contentOnly=False):
	addFunctionInjectionPrefix('- (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions', contentOnly)
	print '\tNSDictionary *remoteNotificationDict = [launchOptions objectForKey:UIApplicationLaunchOptionsRemoteNotificationKey];'
	print ''
	print '\tif (remoteNotificationDict)'
	print '\t{'
	print '\t\tif ([PropellerSDK handleRemoteNotification:remoteNotificationDict newLaunch:YES])'
	print '\t\t{'
	print '\t\t\t[self broadcastLocalNotification:YES];'
	print '\t\t} else {'
	print '\t\t\t// This is not a Fuel remote notification, I should handle it'
	print '\t\t}'
	print '\t}'
	print ''
	print '\tUILocalNotification *localNotification = [launchOptions objectForKey:UIApplicationLaunchOptionsLocalNotificationKey];'
	print ''
	print '\tif (localNotification)'
	print '\t{'
	print '\t\tif ([PropellerSDK handleLocalNotification:localNotification newLaunch:YES])'
	print '\t\t{'
	print '\t\t\t[self broadcastLocalNotification:YES];'
	print '\t\t} else {'
	print '\t\t\t// This is not a Fuel local notification, I should handle it'
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
	addFunctionInjectionSuffix(contentOnly)

def addEnterBackground(contentOnly=False):
	addFunctionInjectionPrefix('- (void)applicationDidEnterBackground:(UIApplication*)application', contentOnly)
	print '\t[[UIApplication sharedApplication] setApplicationIconBadgeNumber:1];'
	print '\t[[UIApplication sharedApplication] setApplicationIconBadgeNumber:0];'
	print '\t[[UIApplication sharedApplication] cancelAllLocalNotifications];'
	print '\t[PropellerSDK restoreAllLocalNotifications];'
	addFunctionInjectionSuffix(contentOnly)

def addExtraFunctions():
	addInjectionPrefix()
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
	print '\t} else if ([type isEqualToString:@"PropellerSDKUserValues"]) {'
	print '\t\tNSString *message = nil;'
	print ''
	print '\t\tif (data == nil) {'
	print '\t\t\tmessage = @"";'
	print '\t\t} else {'
	print '\t\t\tNSMutableArray *paramList = [[NSMutableArray alloc] init];'
	print ''
	print '\t\t\tNSDictionary *variablesDictionary = [data objectForKey:@"variables"];'
	print ''
	print '\t\t\tfor (id item in variablesDictionary){'
	print '\t\t\t\t[paramList addObject:item];'
	print '\t\t\t\t[paramList addObject:[variablesDictionary objectForKey:item]];'
	print '\t\t\t}'
	print ''
	print '\t\t\tNSDictionary *conditionsDictionary = [data objectForKey:@"dynamicConditions"];'
	print ''
	print '\t\t\tfor (id item in conditionsDictionary){'
	print '\t\t\t\t[paramList addObject:item];'
	print '\t\t\t\t[paramList addObject:[conditionsDictionary objectForKey:item]];'
	print '\t\t\t}'
	print ''
	print '\t\t\tmessage = [paramList componentsJoinedByString:@"&"];'
	print '\t\t}'
	print ''
	print '\t\tUnitySendMessage("PropellerCommon", "PropellerOnUserValues", [message UTF8String]);'
	print '\t} else if ([type isEqualToString:@"PropellerSDKNotification"]) {'
	print '\t\tNSString *message = nil;'
	print ''
	print '\t\tif (data != nil) {'
	print '\t\t\tmessage = [data objectForKey:@"applicationState"];'
	print '\t\t}'
	print ''
	print '\t\tif (message == nil) {'
	print '\t\t\tmessage = @"";'
	print '\t\t}'
	print ''
	print '\t\tUnitySendMessage("PropellerSDK", "PropellerOnNotification", [message UTF8String]);'
	print '\t}'
	print '}'
	print ''
	print '- (void) broadcastLocalNotification:(BOOL)newLaunch'
	print '{'
	print '\tNSString *applicationState = nil;'
	print ''
	print '\tif (newLaunch) {'
	print '\t\tapplicationState = @"inactive";'
	print '\t} else {'
	print '\t\tUIApplication *application = [UIApplication sharedApplication];'
	print ''
	print '\t\tif ([application applicationState] == UIApplicationStateActive) {'
	print '\t\t\tapplicationState = @"active";'
	print '\t\t} else {'
	print '\t\t\tapplicationState = @"background";'
	print '\t\t}'
	print '\t}'
	print ''
	print '\tNSDictionary *userInfo = [NSDictionary dictionaryWithObject:applicationState forKey:@"applicationState"];'
	print ''
	print '\t[[NSNotificationCenter defaultCenter] postNotificationName:@"PropellerSDKNotification" object:nil userInfo:userInfo];'
	print '}'
	addInjectionSuffix()

injectHeader = True
injectInit = False
injectReceiveLocalNotification = False
injectRegisterRemoteNotifications = False
injectFailToRegisterRemoteNotifications = False
injectReceiveRemoteNotification = False
injectRegisterUserNotificationSettings = False
injectFinishLaunching = False
injectEnterBackground = False

injectedHeader = False
injectedInit = False
injectedDealloc = False
injectedReceiveLocalNotification = False
injectedRegisterRemoteNotifications = False
injectedFailToRegisterRemoteNotifications = False
injectedReceiveRemoteNotification = False
injectedRegisterUserNotificationSettings = False
injectedFinishLaunching = False
injectedEnterBackground = False

lastNonBlankLine = ''

for line in fileinput.input( controllerFile, inplace=1 ):
	if len(line.strip()) == 0:
		print line,
		continue

	if '[super init]' in line:
		injectInit = True
	elif '[super dealloc]' in line:
		if injectionSuffix not in lastNonBlankLine:
			addDealloc(True)
			injectedDealloc = True
	elif 'didReceiveLocalNotification:(' in line:
		injectReceiveLocalNotification = True
	elif 'didRegisterForRemoteNotificationsWithDeviceToken:(' in line:
		injectRegisterRemoteNotifications = True
	elif 'didFailToRegisterForRemoteNotificationsWithError:(' in line:
		injectFailToRegisterRemoteNotifications = True
	elif 'didReceiveRemoteNotification:(' in line:
		injectReceiveRemoteNotification = True
	elif 'didRegisterUserNotificationSettings:(' in line:
		injectRegisterUserNotificationSettings = True
	elif 'didFinishLaunchingWithOptions:(' in line:
		injectFinishLaunching = True
	elif ')applicationDidEnterBackground:(' in line:
		injectEnterBackground = True
	else:
		if injectHeader:
			injectHeader = False
			if injectionPrefix not in line:
				addHeader()
				injectedHeader = True
		if injectInit and '{' not in line:
			injectInit = False
			if injectionPrefix not in line:
				addInit(True)
				injectedInit = True
		if injectReceiveLocalNotification and '{' not in line:
			injectReceiveLocalNotification = False
			if injectionPrefix not in line:
				addReceiveLocalNotification(True)
				injectedReceiveLocalNotification = True
		if injectRegisterRemoteNotifications and '{' not in line:
			injectRegisterRemoteNotifications = False
			if injectionPrefix not in line:
				addRegisterRemoteNotifications(True)
				injectedRegisterRemoteNotifications = True
		if injectFailToRegisterRemoteNotifications and '{' not in line:
			injectFailToRegisterRemoteNotifications = False
			if injectionPrefix not in line:
				addFailToRegisterRemoteNotifications(True)
				injectedFailToRegisterRemoteNotifications = True
		if injectReceiveRemoteNotification and '{' not in line:
			injectReceiveRemoteNotification = False
			if injectionPrefix not in line:
				addReceiveRemoteNotification(True)
				injectedReceiveRemoteNotification = True
		if injectRegisterUserNotificationSettings and '{' not in line:
			injectRegisterUserNotificationSettings = False
			if injectionPrefix not in line:
				addRegisterUserNotificationSettings(True)
				injectedRegisterUserNotificationSettings = True
		if injectFinishLaunching and '{' not in line:
			injectFinishLaunching = False;
			if injectionPrefix not in line:
				addFinishLaunching(True)
				injectedFinishLaunching = True
		if injectEnterBackground and '{' not in line:
			injectEnterBackground = False
			if injectionPrefix not in line:
				addEnterBackground(True)
				injectedEnterBackground = True

	lastNonBlankLine = line

	print line,

fileinput.close()

controllerFilenameIndex = controllerFilename.find('.')
controllerFilenameString = controllerFilename[0:controllerFilenameIndex]
controllerFileImplementation = '@implementation ' + controllerFilenameString

injectExtraFunctions = False

lastNonBlankLine = ''

for line in fileinput.input( controllerFile, inplace=1 ):
	if len(line.strip()) == 0:
		print line,
		continue

	if controllerFileImplementation in line:
		injectExtraFunctions = True
	else:
		if injectExtraFunctions and '@end' in line:
			injectExtraFunctions = False
			if injectionSuffix not in lastNonBlankLine:
				if not injectedInit:
					addInit()
				if not injectedDealloc:
					addDealloc()
				if not injectedReceiveLocalNotification:
					addReceiveLocalNotification()
				if not injectedRegisterRemoteNotifications:
					addRegisterRemoteNotifications()
				if not injectedFailToRegisterRemoteNotifications:
					addFailToRegisterRemoteNotifications()
				if not injectedReceiveRemoteNotification:
					addReceiveRemoteNotification()
				if not injectedRegisterUserNotificationSettings:
					addRegisterUserNotificationSettings()
				if not injectedFinishLaunching:
					addFinishLaunching()
				if not injectedEnterBackground:
					addEnterBackground()
				addExtraFunctions()

	lastNonBlankLine = line

	print line,

fileinput.close()

def getFilePath(fileName):
	files = project.get_files_by_name(fileName)

	if len(files) != 1:
		exitWithError('Unable to locate the PBX file reference for ' + fileName)

	filePath = files[0].get('path')

	if filePath == None:
		exitWithError('PBX file reference for ' + fileName + ' is missing a file path')

	return projectPath + '/' + filePath

def addGLViewControllerExport():
	functionExport = True

	for line in fileinput.input( projectPath + '/Classes/AppController.h', inplace=1 ):
		if functionExport:
			if injectionPrefix not in line:
				addInjectionPrefix()
				print 'UIViewController* UnityGetGLViewController();'
				addInjectionSuffix()
			functionExport = False

		print line,

	fileinput.close()

def addWrapperHeader(headerFile):
	headerCorrect = False
	injectHeader = False

	for line in fileinput.input( getFilePath('PropellerSDKUnityWrapper.mm'), inplace=1 ):
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

def addImport(sourceFile, headerFile):
	headerCorrect = False

	for line in fileinput.input( projectPath + '/' + sourceFile, inplace=1 ):
		if not headerCorrect:
			if injectionPrefix not in line:
				addInjectionPrefix()
				print '#import ' + headerFile
				addInjectionSuffix()
			headerCorrect = True

		print line,

	fileinput.close()

print 'Injecting compatibility shim into PropellerSDKUnityWrapper.mm'

if unityApiLevel >= 15:
	addWrapperHeader('UI/UnityViewControllerBase.h')
elif unityApiLevel >= 10:
	addWrapperHeader('iPhone_View.h')
else:
	addWrapperHeader('AppController.h')

print 'Injecting additional compatibility shims'

if (unityApiLevel == 13) or (unityApiLevel == 14):
	xcodeMajorVersionIndex = xcodeVersion.find('.')
	xcodeMajorVersionString = xcodeVersion[0:xcodeMajorVersionIndex]
	xcodeMajorVersion = int(xcodeMajorVersionString)

	if xcodeMajorVersion >= 6:
		addImport('Classes/Unity/CMVideoSampling.mm', '<OpenGLES/ES2/glext.h>')

if unityApiLevel < 10:
	addGLViewControllerExport()

print 'Propeller SDK code injection completed!'
