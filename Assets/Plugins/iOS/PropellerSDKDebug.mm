#import "PropellerSDK.h"

extern "C"
{
    @interface PropellerSDK (Private)

        + (void)useDebugServers:(NSString *)sdkURL fuelAPIURL:(NSString *)fuelAPIURL tournamentAPIURL:(NSString *)tournamentAPIURL challengeAPIURL:(NSString *)challengeAPIURL cdnAPIURL:(NSString *)cdnAPIURL transactionAPIURL:(NSString*)transactionAPIURL dynamicsAPIURL:(NSString*)dynamicsAPIURL;

    @end
    
    void iOSUseDebugServers(const char* sdkHost, const char* apiHost, const char* tournamentHost, const char* challengeHost, const char* cdnHost, const char* transactionHost, const char* dynamicsHost)
    {
        NSString* sdkURL = [NSString stringWithFormat:@"%s", sdkHost];
        NSString* fuelAPIURL = [NSString stringWithFormat:@"%s", apiHost];
        NSString* tournamentAPIURL = [NSString stringWithFormat:@"%s", tournamentHost];
        NSString* challengeAPIURL = [NSString stringWithFormat:@"%s", challengeHost];
        NSString* cdnAPIURL = [NSString stringWithFormat:@"%s", cdnHost];
        NSString* transactionAPIURL = [NSString stringWithFormat:@"%s", transactionHost];
        NSString* dynamicsAPIURL = [NSString stringWithFormat:@"%s", dynamicsHost];
        
        [PropellerSDK useDebugServers:sdkURL
                           fuelAPIURL:fuelAPIURL
                     tournamentAPIURL:tournamentAPIURL
                      challengeAPIURL:challengeAPIURL
                            cdnAPIURL:cdnAPIURL
                    transactionAPIURL:transactionAPIURL
                       dynamicsAPIURL:dynamicsAPIURL];
    }
    
}