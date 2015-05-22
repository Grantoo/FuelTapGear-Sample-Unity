//
//  PropellerSDK.h
//  libPropellerSDK
//
//  Copyright (c) 2015 Fuel Powered, Inc. All rights reserved.
//
// PropellerSDK is implemented as a singleton that is accessible
// via an static instance factory method. One may use this class
// to setup an easy Fuel integration by simply following these
// ordered steps:
//
// 1.) Call the initialize: method with the appropriate start-up params
// 2.) Call the instance method to get a reference to the singleton instance
//     of this class.
// 3.) Call any of the instance methods for the function needed.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

#define PSDK_MATCH_RESULT_TOURNAMENT_KEY @"tournamentID"
#define PSDK_MATCH_RESULT_MATCH_KEY @"matchID"
#define PSDK_MATCH_RESULT_PARAMS_KEY @"params"
#define PSDK_MATCH_POST_TOURNAMENT_KEY @"tournamentID"
#define PSDK_MATCH_POST_MATCH_KEY @"matchID"
#define PSDK_MATCH_POST_SCORE_KEY @"score"
#define PSDK_MATCH_POST_VISUALSCORE_KEY @"visualScore"
#define PSDK_MATCH_POST_MATCHDATA_KEY @"matchData"
#define PSDK_MATCH_POST_CURRENCIES_KEY @"currencies"
#define PSDK_MATCH_POST_CURRENCYID_KEY @"id"

typedef enum
{
    PSDK_NOTIFICATION_TYPE_NONE = 0x0,
    PSDK_NOTIFICATION_TYPE_ALL = 0x3,
    PSDK_NOTIFICATION_TYPE_PUSH = 1 << 0,
    PSDK_NOTIFICATION_TYPE_LOCAL = 1 << 1
} PropellerSDKNotificationType;

typedef enum
{
    kPropelSDKPortrait,
    kPropelSDKLandscape
} PropelSDKGameOrientation;

@protocol PropellerSDKDelegate <NSObject>

// -----------------------------------------------------------------------------
// PropellerSDKDelegate callback methods
// -----------------------------------------------------------------------------

@required
- (void)sdkCompletedWithExit;
- (void)sdkCompletedWithMatch:(NSDictionary*)match;
- (void)sdkFailed:(NSDictionary*)result;

// -----------------------------------------------------------------------------
// PropellerSDKDelegate social callback methods
// -----------------------------------------------------------------------------

@optional
- (BOOL)sdkSocialLogin:(BOOL)allowCache;
- (BOOL)sdkSocialInvite:(NSDictionary*)inviteDetail;
- (BOOL)sdkSocialShare:(NSDictionary*)shareDetail;

@end

@protocol PropellerSDKNotificationDelegate <NSObject>

// -----------------------------------------------------------------------------
// PropellerSDKNotificationDelegate callback methods
// -----------------------------------------------------------------------------

@required
- (void)sdkOnNotificationEnabled:(PropellerSDKNotificationType)notificationType;
- (void)sdkOnNotificationDisabled:(PropellerSDKNotificationType)notificationType;

@end

#pragma mark -

@interface PropellerSDK : NSObject

// -----------------------------------------------------------------------------
// Propeller SDK initialization methods
// -----------------------------------------------------------------------------

// Used to initialize the singleton instance with some
// kind of parameters.
+ (void)setRootViewController:(UIViewController*)rootViewController;
+ (void)useSandbox;
+ (void)initialize:(NSString*)gameID gameSecret:(NSString*)gameSecret;
+ (void)initialize:(NSString*)gameID gameSecret:(NSString*)gameSecret gameHasLogin:(BOOL)gameHasLogin gameHasInvite:(BOOL)gameHasInvite gameHasShare:(BOOL)gameHasShare;

// -----------------------------------------------------------------------------
// Propeller SDK singleton methods
// -----------------------------------------------------------------------------

+ (PropellerSDK*)instance;

// -----------------------------------------------------------------------------
// Propeller SDK configuration methods
// -----------------------------------------------------------------------------

- (void)setOrientation:(PropelSDKGameOrientation)orientation;
- (void)setLanguageCode:(NSString*)languageCode;

// -----------------------------------------------------------------------------
// Propeller SDK lifecycle methods
// -----------------------------------------------------------------------------

+ (void)handleApplicationWillEnterForeground:(UIApplication*)application;

// -----------------------------------------------------------------------------
// Notification methods
// -----------------------------------------------------------------------------

// Sets the push notification token
+ (void)setNotificationToken:(NSString*)token;

// Handle incoming push notification.
// bNewLaunch = YES if the push notification caused the app to launch
// bNewLaunch = NO if the push notification came in while the app is already active
// Returns YES if the notification was handled, NO otherwise.
+ (BOOL)handleRemoteNotification:(NSDictionary*)userInfo newLaunch:(BOOL)bNewLaunch;

// Handle incoming local notification.
// bNewLaunch = YES if the local notification caused the app to launch
// bNewLaunch = NO if the local notification came in while the app is already active
// Returns YES if the notification was handled, NO otherwise.
+ (BOOL)handleLocalNotification:(UILocalNotification*)notification newLaunch:(BOOL)bNewLaunch;

// Restores scheduled Propeller SDK local notifications
+ (void)restoreAllLocalNotifications;

+ (void)setNotificationDelegate:(id<PropellerSDKNotificationDelegate>)notificationDelegate;
- (BOOL)enableNotification:(PropellerSDKNotificationType)notificationType;
- (BOOL)disableNotification:(PropellerSDKNotificationType)notificationType;
+ (BOOL)isNotificationEnabled:(PropellerSDKNotificationType)notificationType;

// -----------------------------------------------------------------------------
// Launch methods
// -----------------------------------------------------------------------------

// Displays the fuel view by using the root view controller
// and pushing onto the navigation stack, if we have a UINavigationController
// at the root on iPhone or by presenting modally on iPhone or else by presenting
// as a modal form sheet on iPad.
- (BOOL)launch:(id<PropellerSDKDelegate>)delegate;

// -----------------------------------------------------------------------------
// Match result methods
// -----------------------------------------------------------------------------
- (BOOL)submitMatchResult:(NSDictionary*)matchResult;

// -----------------------------------------------------------------------------
// Challenge count methods
// -----------------------------------------------------------------------------

// Retrieves and returns the challenge counts for the current user
- (void)syncChallengeCounts;

// -----------------------------------------------------------------------------
// Tournament info methods
// -----------------------------------------------------------------------------

// Retrieves and returns the tournament information
- (void)syncTournamentInfo;

// -----------------------------------------------------------------------------
// Virtual good methods
// -----------------------------------------------------------------------------

// Request and acknowledge virtual goods
- (BOOL)syncVirtualGoods;
- (BOOL)acknowledgeVirtualGoods:(NSString*)transactionID consumed:(BOOL)consumed;

// -----------------------------------------------------------------------------
// Social callback methods
// -----------------------------------------------------------------------------

// Called by the game to signal that it has completed the login/share/invite process on their end.

// Similar to setSocialLoginData but reloads webview to finish login flow init by online sdk.
- (BOOL)sdkSocialLoginCompleted:(NSDictionary*)loginData;
// Signal that the invite process has completed.
- (BOOL)sdkSocialInviteCompleted;
// Signal that the share process has completed.
- (BOOL)sdkSocialShareCompleted;


// -----------------------------------------------------------------------------
// Dynamic Game Variables methods
// -----------------------------------------------------------------------------

// Request and Dynamic variables
- (BOOL)setUserConditions:(NSDictionary *)conditions;
- (BOOL)syncUserValues;

@end
