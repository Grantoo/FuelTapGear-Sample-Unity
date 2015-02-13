#import "PropellerSDK.h"

extern "C"
{
    #define encodeString(rawString) CFBridgingRelease(CFURLCreateStringByAddingPercentEscapes(NULL, (CFStringRef)rawString, NULL, (CFStringRef)@"!*'();:@&=+$,/?%#[]",kCFStringEncodingUTF8))
    
    @interface PropellerUnityListener : NSObject<PropellerSDKDelegate>
    {
        NSString* tournamentID;
        NSString* matchID;
    }

    - (NSString*) tournamentID;
    - (NSString*) matchID;
    - (void)sdkCompletedWithExit;
    - (void)sdkFailed:(NSDictionary *)result;
    - (void)sdkCompletedWithMatch:(NSDictionary *)match;
    - (BOOL)sdkSocialLogin:(BOOL)allowCache;
    - (BOOL)sdkSocialInvite:(NSDictionary*)inviteDetail;
    - (BOOL)sdkSocialShare:(NSDictionary*)shareDetail;
    @end

    @implementation PropellerUnityListener

        - (NSString*) tournamentID
        {
            return tournamentID;
        }

        - (NSString*) matchID
        {
            return matchID;
        }

        -(void)sdkCompletedWithExit
        {
            UnitySendMessage("PropellerSDK", "PropellerOnSdkCompletedWithExit", "");
        }


        -(void)sdkFailed:(NSDictionary *)result
        {
            const char* messageUTF8String = "";
            
            if (result) {
                NSString* messageString = [result objectForKey:@"message"];
                
                if (messageString) {
                    messageUTF8String = [messageString UTF8String];
                }
            }
            
            
            // sdk failed (alert box will have been displayed)
            UnitySendMessage("PropellerSDK", "PropellerOnSdkFailed", messageUTF8String);
        }


        -(void)sdkCompletedWithMatch:(NSDictionary *)match
        {
            NSMutableArray *paramList = [[NSMutableArray alloc] init];

            tournamentID =  [match objectForKey:PSDK_MATCH_RESULT_TOURNAMENT_KEY];

            if ((tournamentID != nil) && ([tournamentID length] > 0)) {
                [paramList addObject:tournamentID];
            }

            matchID = [match objectForKey:PSDK_MATCH_RESULT_MATCH_KEY];

            if ((matchID != nil) && ([matchID length] > 0)) {
                [paramList addObject:matchID];
            }

            NSError *error = nil;
            NSData* jsonData = [NSJSONSerialization dataWithJSONObject:[match objectForKey:PSDK_MATCH_RESULT_PARAMS_KEY] options:kNilOptions error:&error];
            NSString *paramsString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];

            if ((paramsString != nil) && ([paramsString length] > 0)) {
                [paramList addObject:encodeString(paramsString)];
            }

            NSString *resultString = [paramList componentsJoinedByString:@"&"];
            UnitySendMessage("PropellerSDK", "PropellerOnSdkCompletedWithMatch", [resultString UTF8String]);

            [paramsString release];
            [paramList release];
        }

        - (BOOL)sdkSocialLogin:(BOOL)allowCache
        {
                NSString *resultString = allowCache ? @"true" : @"false";
                UnitySendMessage("PropellerSDK", "PropellerOnSdkSocialLogin", [resultString UTF8String]);
                return YES;
        }

        - (BOOL)sdkSocialInvite:(NSDictionary*)inviteDetail
        {
            NSMutableString* stringBuilder = [[NSMutableString alloc] init];
            
            BOOL first = YES;
            
            for (NSString* key in inviteDetail) {
                NSString* value = [inviteDetail objectForKey:key];
                
                if (first)
                {
                    first = NO;
                }
                else
                {
                    [stringBuilder appendString:@"&"];
                }
                
                [stringBuilder appendString:encodeString(key)];
                [stringBuilder appendString:@"="];
                [stringBuilder appendString:encodeString(value)];
            }
            
            UnitySendMessage("PropellerSDK", "PropellerOnSdkSocialInvite", [stringBuilder UTF8String]);
            return YES;
        }

        - (BOOL)sdkSocialShare:(NSDictionary*)shareDetail
        {
            NSMutableString* stringBuilder = [[NSMutableString alloc] init];
            
            BOOL first = YES;
            
            for (NSString* key in shareDetail) {
                NSString* value = [shareDetail objectForKey:key];
                
                if (first)
                {
                    first = NO;
                }
                else
                {
                    [stringBuilder appendString:@"&"];
                }
                
                [stringBuilder appendString:encodeString(key)];
                [stringBuilder appendString:@"="];
                [stringBuilder appendString:encodeString(value)];
            }
            
            UnitySendMessage("PropellerSDK", "PropellerOnSdkSocialShare", [stringBuilder UTF8String]);
            return YES;
        }

    @end

    @interface PropellerUnityNotificationListener : NSObject<PropellerSDKNotificationDelegate>
    {
    }

    - (void)sdkOnNotificationEnabled:(PropellerSDKNotificationType)notificationType;
    - (void)sdkOnNotificationDisabled:(PropellerSDKNotificationType)notificationType;

    @end

    @implementation PropellerUnityNotificationListener

        - (void)sdkOnNotificationEnabled:(PropellerSDKNotificationType)notificationType
        {
            NSString *notificationTypeString = [NSString stringWithFormat:@"%d", (int)notificationType];
            UnitySendMessage("PropellerSDK", "PropellerOnSdkOnNotificationEnabled", [notificationTypeString UTF8String]);
        }

        - (void)sdkOnNotificationDisabled:(PropellerSDKNotificationType)notificationType
        {
            NSString *notificationTypeString = [NSString stringWithFormat:@"%d", (int)notificationType];
            UnitySendMessage("PropellerSDK", "PropellerOnSdkOnNotificationDisabled", [notificationTypeString UTF8String]);
        }

    @end

    static PropellerUnityListener* gsPropellerUnityListener;
    static BOOL autoRotate;
    
    static void validateOrientation();

#define NORMALIZED_JSON_DATATYPE_INT	0
#define NORMALIZED_JSON_DATATYPE_LONG	1
#define NORMALIZED_JSON_DATATYPE_FLOAT	2
#define NORMALIZED_JSON_DATATYPE_DOUBLE	3
#define NORMALIZED_JSON_DATATYPE_BOOL	4
#define NORMALIZED_JSON_DATATYPE_STRING	5

	static NSObject* normalizeJSONDictionary(NSDictionary *dictionary);
	static NSObject* normalizeJSONList(NSArray *array);
	static NSObject* normalizeJSONValue(NSDictionary *valueDictionary);
    static BOOL isNormalizedJSONValue(NSDictionary *dictionary);

    void iOSInitialize(const char* key, const char* secret, const char* screenOrientation, bool useTestServers, bool hasLogin, bool hasInvite, bool hasShare)
    {
        if ( useTestServers )
        {
            [PropellerSDK useSandbox];
        }

        [PropellerSDK setNotificationDelegate:[PropellerUnityNotificationListener alloc]];

        [PropellerSDK initialize:[NSString stringWithUTF8String:key] gameSecret:[NSString stringWithUTF8String:secret] gameHasLogin:(BOOL)hasLogin gameHasInvite:(BOOL)hasInvite gameHasShare:(BOOL)hasShare];

        if (0 == strcmp(screenOrientation, "landscape")) {
            [[PropellerSDK instance] setOrientation:kPropelSDKLandscape];
            autoRotate = NO;
        } else if (0 == strcmp(screenOrientation, "portrait")) {
            [[PropellerSDK instance] setOrientation:kPropelSDKPortrait];
            autoRotate = NO;
        } else {
            autoRotate = YES;
        }
        
        gsPropellerUnityListener = [PropellerUnityListener alloc];
    }

    void iOSInitializeDynamicsOnly(const char* key, const char* secret)
    {
        [PropellerSDK initializeDynamicsOnly:[NSString stringWithUTF8String:key] gameSecret:[NSString stringWithUTF8String:secret]];
   	}


    void iOSSetLanguageCode(const char* languageCode)
    {
        [[PropellerSDK instance] setLanguageCode:[NSString stringWithUTF8String:languageCode]];
    }

    BOOL iOSLaunch()
    {
        validateOrientation();
        
        UIViewController* pUIViewController = UnityGetGLViewController();
        [PropellerSDK setRootViewController:pUIViewController];

        return [[PropellerSDK instance] launch:gsPropellerUnityListener];
    }

    BOOL iOSSubmitMatchResult(const char* data)
    {
        NSError *error = nil;
        NSString* matchResultString = [NSString stringWithFormat:@"%s" , data];
        NSData* matchResultData = [matchResultString dataUsingEncoding:NSUTF8StringEncoding];
        NSDictionary *matchResult = [NSJSONSerialization JSONObjectWithData:matchResultData
                                                                    options:NSJSONReadingMutableContainers
                                                                      error:&error];

        if (error != nil) {
            return false;
        }

        matchResult = (NSDictionary*)normalizeJSONDictionary(matchResult);

        if (matchResult == nil) {
	        return false;
        }

        PropellerSDK *gSDK = [PropellerSDK instance];

        return [gSDK submitMatchResult:matchResult];
    }

    void iOSSyncChallengeCounts()
    {
        [[PropellerSDK instance] syncChallengeCounts];
    }

    void iOSSyncTournamentInfo()
    {
        [[PropellerSDK instance] syncTournamentInfo];
    }

    void iOSSyncVirtualGoods()
    {
        [[PropellerSDK instance] syncVirtualGoods];
    }

    void iOSAcknowledgeVirtualGoods(const char* transactionId, BOOL consumed)
    {
        NSString* transactionIdString = nil;

        if (transactionId) {
            transactionIdString = [NSString stringWithFormat:@"%s", transactionId];
        }

        [[PropellerSDK instance] acknowledgeVirtualGoods:transactionIdString consumed:consumed];
    }

    void iOSEnableNotification(PropellerSDKNotificationType notificationType)
    {
        [[PropellerSDK instance] enableNotification:notificationType];
    }

    void iOSDisableNotification(PropellerSDKNotificationType notificationType)
    {
        [[PropellerSDK instance] disableNotification:notificationType];
    }

    BOOL iOSIsNotificationEnabled(PropellerSDKNotificationType notificationType)
    {
        return [PropellerSDK isNotificationEnabled:notificationType];
    }

    void iOSSdkSocialLoginCompleted(const char* urlEncodedCString)
    {
        NSMutableDictionary *loginInfo = nil;

        if (urlEncodedCString != nil) {
            loginInfo = [[NSMutableDictionary alloc] init];
            
            NSString* urlEncodedString = [NSString stringWithFormat:@"%s" , urlEncodedCString];

            NSArray* keyValuePairs = [urlEncodedString componentsSeparatedByString: @"&"];

            for (NSString* keyValuePairString in keyValuePairs) {
                NSArray* keyValuePair = [keyValuePairString componentsSeparatedByString: @"="];
                
                NSString* key = [keyValuePair[0] stringByReplacingPercentEscapesUsingEncoding:NSUTF8StringEncoding];
                NSString* value = [keyValuePair[1] stringByReplacingPercentEscapesUsingEncoding:NSUTF8StringEncoding];
                                   
                [loginInfo setObject:value forKey:key];
            }
        }

        [[PropellerSDK instance] sdkSocialLoginCompleted:loginInfo];

        if (loginInfo != nil) {
            [loginInfo release];
        }
    }

    void iOSSdkSocialInviteCompleted()
    {
        [[PropellerSDK instance] sdkSocialInviteCompleted];
    }

    void iOSSdkSocialShareCompleted()
    {
        [[PropellerSDK instance] sdkSocialShareCompleted];
    }
    
    void iOSRestoreAllLocalNotifications()
    {
        [PropellerSDK restoreAllLocalNotifications];
    }
    
    void validateOrientation()
    {
        if (!autoRotate) {
            return;
        }
        
        UIInterfaceOrientation orientation = [[UIApplication sharedApplication] statusBarOrientation];
        
        if (UIInterfaceOrientationIsLandscape(orientation)) {
            [[PropellerSDK instance] setOrientation:kPropelSDKLandscape];
        } else if (UIInterfaceOrientationIsPortrait(orientation)) {
            [[PropellerSDK instance] setOrientation:kPropelSDKPortrait];
        }
    }

    BOOL iOSSetUserConditions(const char* data)
    {
        validateOrientation();
        
        NSError *error = nil;
        NSString* conditionsString = [NSString stringWithFormat:@"%s" , data];
        NSData* conditionsData = [conditionsString dataUsingEncoding:NSUTF8StringEncoding];
        NSDictionary *conditions = [NSJSONSerialization JSONObjectWithData:conditionsData
                                                                    options:NSJSONReadingMutableContainers
                                                                      error:&error];

        if (error != nil) {
            return false;
        }

        conditions = (NSDictionary*)normalizeJSONDictionary(conditions);

        if (conditions == nil) {
            return false;
        }

        PropellerSDK *gSDK = [PropellerSDK instance];

        return [gSDK setUserConditions:conditions];
    }

    void iOSGetUserValues()
    {
        [[PropellerSDK instance] getUserValues];
    }
    
	NSObject* normalizeJSONDictionary(NSDictionary *dictionary)
	{
		if (dictionary == nil) {
			return nil;
		}

		if (isNormalizedJSONValue(dictionary)) {		
			return normalizeJSONValue(dictionary);
		}

		NSMutableDictionary *resultDictionary = [[NSMutableDictionary alloc] init];

		for (NSString *key in dictionary) {
			if (key == nil) {
				continue;
			}

			NSObject *value = [dictionary objectForKey:key];

			if (value == nil) {
				continue;
			}

			NSObject *normalizedValue = nil;

			if ([value isKindOfClass:[NSArray class]]) {
				normalizedValue = normalizeJSONList((NSArray*) value);
			} else if ([value isKindOfClass:[NSDictionary class]]) {
				normalizedValue = normalizeJSONDictionary((NSDictionary*) value);
			} else {
				continue;
			}

			if (normalizedValue == nil) {
				continue;
			}

			[resultDictionary setObject:normalizedValue forKey:key];
		}

		return resultDictionary;
	}

	NSObject* normalizeJSONList(NSArray *array)
	{
		if (array == nil) {
			return nil;
		}

		NSMutableArray *resultArray = [[NSMutableArray alloc] init];

		for (NSObject *value in array) {
			if (value == nil) {
				continue;
			}

			NSObject *normalizedValue = nil;

			if ([value isKindOfClass:[NSArray class]]) {
				normalizedValue = normalizeJSONList((NSArray*) value);
			} else if ([value isKindOfClass:[NSDictionary class]]) {
				normalizedValue = normalizeJSONDictionary((NSDictionary*) value);
			} else {
				continue;
			}

			if (normalizedValue == nil) {
				continue;
			}

			[resultArray addObject:normalizedValue];
		}

		return resultArray;
	}

	NSObject* normalizeJSONValue(NSDictionary *valueDictionary)
	{
		if (valueDictionary == nil) {
			return nil;
		}

		NSString *type = (NSString*) [valueDictionary objectForKey:@"type"];
		NSString *value = (NSString*) [valueDictionary objectForKey:@"value"];

		if ((type == nil) || (value == nil)) {
			return nil;
		}

		switch ([type intValue]) {
			case NORMALIZED_JSON_DATATYPE_INT:
			case NORMALIZED_JSON_DATATYPE_LONG:
			case NORMALIZED_JSON_DATATYPE_FLOAT:
			case NORMALIZED_JSON_DATATYPE_DOUBLE: {
				NSNumberFormatter *numberFormatter = [[NSNumberFormatter alloc] init];
				[numberFormatter setNumberStyle:NSNumberFormatterDecimalStyle];
				return [numberFormatter numberFromString:value];
			}
			case NORMALIZED_JSON_DATATYPE_BOOL: {
				BOOL boolValue = [value caseInsensitiveCompare:@"true"] == NSOrderedSame;
				return [NSNumber numberWithBool:boolValue];
			}
			case NORMALIZED_JSON_DATATYPE_STRING: {
				return value;
			}
			default:
				return nil;
		}	
	}

    BOOL isNormalizedJSONValue(NSDictionary *dictionary)
    {
	    if (dictionary == nil) {
		    return NO;
	    }

		NSObject *checksumObject = [dictionary objectForKey:@"checksum"];

		if ((checksumObject == nil) || ![checksumObject isKindOfClass:[NSString class]]) {
			return NO;
		}

		NSString *checksum = (NSString*) checksumObject;

		if (![checksum isEqualToString:@"faddface"]) {
			return NO;
		}

		NSObject *typeObject = [dictionary objectForKey:@"type"];

		if ((typeObject == nil) || ![typeObject isKindOfClass:[NSString class]]) {
			return NO;
		}

		NSString *type = (NSString*) typeObject;

		switch ([type intValue]) {
			case NORMALIZED_JSON_DATATYPE_INT:
			case NORMALIZED_JSON_DATATYPE_LONG:
			case NORMALIZED_JSON_DATATYPE_FLOAT:
			case NORMALIZED_JSON_DATATYPE_DOUBLE:
			case NORMALIZED_JSON_DATATYPE_BOOL:
			case NORMALIZED_JSON_DATATYPE_STRING:
				break;
			default:
				return NO;
		}

		NSObject *valueObject = [dictionary objectForKey:@"value"];

		if ((valueObject == nil) || ![valueObject isKindOfClass:[NSString class]]) {
			return NO;
		}

		return YES;
    }

}
