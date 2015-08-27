#ifdef PSDK_DEBUG

#import <objc/runtime.h>

#import "PropellerSDK.h"

#define NSStringize_helper(x) #x
#define NSStringize(x) @NSStringize_helper(x)

extern "C"
{
    static void swizzleClassMethod(Class cls, SEL sourceSelector, SEL targetSelector);
   
    @interface PropellerSDK (Private)

        + (void)useDebugServers:(NSString *)sdkURL fuelAPIURL:(NSString *)fuelAPIURL tournamentAPIURL:(NSString *)tournamentAPIURL challengeAPIURL:(NSString *)challengeAPIURL cdnAPIURL:(NSString *)cdnAPIURL transactionAPIURL:(NSString*)transactionAPIURL dynamicsAPIURL:(NSString*)dynamicsAPIURL;

    @end
    
    @implementation PropellerSDK (Private)

    + (void)load
    {
        static dispatch_once_t onceToken;
        dispatch_once(&onceToken, ^{
            swizzleClassMethod(
                               [PropellerSDK class],
                               @selector(initialize:gameSecret:gameHasLogin:gameHasInvite:gameHasShare:),
                               @selector(initializeDebug:gameSecret:gameHasLogin:gameHasInvite:gameHasShare:));
        });
    }

    + (void) initializeDebug:(NSString*)gameKey gameSecret:(NSString*)gameSecret gameHasLogin:(BOOL)hasLogin gameHasInvite:(BOOL)hasInvite gameHasShare:(BOOL)hasShare
    {
        NSString *gameID = nil;
        NSString *gameKey = nil;
        NSString *sdkUrl = nil;
        NSString *fuelHost = nil;
        NSString *tournamentHost = nil;
        NSString *challengeHost = nil;
        NSString *cdnHost = nil;
        NSString *transactionHost = nil;
        NSString *dynamicsHost = nil;
        
        NSString *env = NSStringize(PSDK_ENV);
        
        if ([env isEqualToString:@"Internal"]) {
            gameID = @"5522ed09663330000b090000";
            gameKey = @"62363bb2-cecc-a2bc-694c-5e5e532a214f";
            sdkUrl = @"https://api-internal.fuelpowered.com/sdk/";
            fuelHost = @"https://api-internal.fuelpowered.com/api/v1";
            tournamentHost = @"https://api-internal.fuelpowered.com/api/v1";
            challengeHost = @"https://challenge-internal.fuelpowered.com/v1";
            cdnHost = @"http://cdn-internal.fuelpowered.com/api/v1";
            transactionHost = @"https://transaction-internal.fuelpowered.com/api";
            dynamicsHost = @"http://apiv2-internal.fuelpowered.com/api/v2";
        } else if ([env isEqualToString:@"Sandbox"]) {
            gameID = @"5477a6c17061701423190000";
            gameKey = @"bc8939af-ac67-de48-7791-a4dc76ac3cfe";
            sdkUrl = @"https://api-sandbox.fuelpowered.com/sdk/";
            fuelHost = @"https://api-sandbox.fuelpowered.com/api/v1";
            tournamentHost = @"https://api-sandbox.fuelpowered.com/api/v1";
            challengeHost = @"https://challenge-sandbox.fuelpowered.com/v1";
            cdnHost = @"http://cdn-sandbox.fuelpowered.com/api/v1";
            transactionHost = @"https://transaction-sandbox.fuelpowered.com/api";
            dynamicsHost = @"https://api-sandbox.fuelpowered.com/api/v2";
        } else {
            gameID = @"542b3bec636f62427de7ac00";
            gameKey = @"7944a1a9-fc41-8789-4c67-dc98bb4c1743";
            sdkUrl = @"https://api.fuelpowered.com/sdk/";
            fuelHost = @"https://api.fuelpowered.com/api/v1";
            tournamentHost = @"https://api.fuelpowered.com/api/v1";
            challengeHost = @"https://challenge.fuelpowered.com/v1";
            cdnHost = @"http://cdn.fuelpowered.com/api/v1";
            transactionHost = @"https://transaction.fuelpowered.com/api";
            dynamicsHost = @"https://api.fuelpowered.com/api/v2";
        }

        [PropellerSDK useDebugServers:sdkUrl
                           fuelAPIURL:fuelHost
                     tournamentAPIURL:tournamentHost
                      challengeAPIURL:challengeHost
                            cdnAPIURL:cdnHost
                    transactionAPIURL:transactionHost
                       dynamicsAPIURL:dynamicsHost];
        
        [PropellerSDK initializeDebug:gameKey
                           gameSecret:gameSecret
                         gameHasLogin:hasLogin
                        gameHasInvite:hasInvite
                         gameHasShare:hasShare];
    }

    @end
    
    void swizzleClassMethod(Class cls, SEL sourceSelector, SEL targetSelector)
    {
        Method sourceMethod = class_getClassMethod(cls, sourceSelector);
        Method targetMethod = class_getClassMethod(cls, targetSelector);
        
        cls = object_getClass((id)cls);
        
        if (class_addMethod(cls, sourceSelector, method_getImplementation(targetMethod), method_getTypeEncoding(targetMethod))) {
            class_replaceMethod(cls, targetSelector, method_getImplementation(sourceMethod), method_getTypeEncoding(sourceMethod));
        } else {
            method_exchangeImplementations(sourceMethod, targetMethod);
        }
    }
}
#endif
