#import <Foundation/Foundation.h>

// Unity Binding
void _registerForAds(const char * decisionPoint);
BOOL _isInterstitialAdAvailable();
void _showInterstitialAd(const char * adPoint);
BOOL _isRewardedAdAvailable();
void _showRewardedAd(const char * adPoint);
void _engageResponse(const char * engagementId, const char * response, int statusCode, const char * error);
void _pause();
void _resume();
void _destroy();


@interface DDNASmartAdsUnityPlugin : NSObject

+ (instancetype)sharedPlugin;

- (void)registerForAds:(NSString *)decisionPoint;
- (BOOL)isInterstitialAdAvailable;
- (void)showInterstitialAdWithAdPoint:(NSString *)adPoint;
- (BOOL)isRewardedAdAvailable;
- (void)showRewardedAdWithAdPoint:(NSString *)adPoint;
- (void)engageResponseForId:(NSString *)engagementId
                   response:(NSString *)response
                 statusCode:(NSInteger)statusCode
                      error:(NSString *)error;
- (void)pause;
- (void)resume;
- (void)destroy;

@end