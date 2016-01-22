#import "DDNASmartAdsUnityPlugin.h"
#import <DeltaDNAAds/SmartAds/DDNASmartAdFactory.h>
#import <DeltaDNAAds/SmartAds/DDNASmartAdService.h>

// Unity callback support
void UnitySendMessage(const char *gameObjectName, const char *methodName, const char *message);

// Converts C style string to NSString
#define GetStringParam( _x_ ) ( _x_ != NULL ) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]

// Functions called from Unity
void _registerForAds(const char * decisionPoint)
{
    [[DDNASmartAdsUnityPlugin sharedPlugin] registerForAds:GetStringParam(decisionPoint)];
}

BOOL _isInterstitialAdAvailable()
{
    return [[DDNASmartAdsUnityPlugin sharedPlugin] isInterstitialAdAvailable];
}

void _showInterstitialAd(const char * adPoint)
{
    [[DDNASmartAdsUnityPlugin sharedPlugin] showInterstitialAdWithAdPoint:GetStringParam(adPoint)];
}

BOOL _isRewardedAdAvailable()
{
    return [[DDNASmartAdsUnityPlugin sharedPlugin] isRewardedAdAvailable];
}

void _showRewardedAd(const char * adPoint)
{
    [[DDNASmartAdsUnityPlugin sharedPlugin] showRewardedAdWithAdPoint:GetStringParam(adPoint)];
}

void _engageResponse(const char * engagementId, const char * response, int statusCode, const char * error)
{
    [[DDNASmartAdsUnityPlugin sharedPlugin] engageResponseForId:GetStringParam(engagementId)
                                                       response:GetStringParam(response)
                                                     statusCode:statusCode
                                                          error:GetStringParam(error)];
}

const char * SmartAdsObject = "DeltaDNAAds.DDNASmartAds";

UIViewController *UnityGetGLViewController();

@implementation NSString (DeltaDNAAds)

+ (NSString *) stringWithContentsOfDictionary: (NSDictionary *) aDictionary
{
    NSString * result = nil;
    NSError * error = nil;
    NSData * data = [NSJSONSerialization dataWithJSONObject:aDictionary
                                                    options:kNilOptions
                                                      error:&error];
    if (error == 0)
    {
        result = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
    }
    return result;
}

@end

@interface DDNASmartAdsUnityPlugin () <DDNASmartAdServiceDelegate>
{

}

@property (nonatomic, strong) DDNASmartAdFactory *factory;
@property (nonatomic, strong) DDNASmartAdService *adService;
@property (nonatomic, strong) NSMutableDictionary *engagements;

@end

@implementation DDNASmartAdsUnityPlugin

+ (instancetype)sharedPlugin
{
    static dispatch_once_t pred = 0;
    __strong static id _sharedObject = nil;
    dispatch_once(&pred, ^{
        _sharedObject = [[self alloc] init];
    });
    return _sharedObject;
}

- (instancetype)init
{
    if ((self = [super init])) {
        self.factory = [[DDNASmartAdFactory alloc] init];
        self.engagements = [NSMutableDictionary dictionary];
    }
    return self;
}

- (void)registerForAds:(NSString *)decisionPoint
{
    @synchronized(self) {
        self.adService = [self.factory buildSmartAdServiceWithDelegate:self];

        if (self.adService) {
            [self.adService beginSessionWithDecisionPoint:decisionPoint];
        } else {
            UnitySendMessage(SmartAdsObject, "DidFailToRegisterForAds", "Failed to build ad service");
        }
    }
}

- (BOOL)isInterstitialAdAvailable
{
    @synchronized(self) {
        if (self.adService) {
            return [self.adService isInterstitialAdAvailable];
        } else {
            return NO;
        }
    }
}

- (void)showInterstitialAdWithAdPoint:(NSString *)adPoint
{
    @synchronized(self) {
        [self.adService showInterstitialAdFromRootViewController:UnityGetGLViewController() adPoint:adPoint];
    }
}

- (BOOL)isRewardedAdAvailable
{
    @synchronized(self) {
        if (self.adService) {
            return [self.adService isRewardedAdAvailable];
        } else {
            return NO;
        }
    }
}

- (void)showRewardedAdWithAdPoint:(NSString *)adPoint
{
    @synchronized(self) {
        [self.adService showRewardedAdFromRootViewController:UnityGetGLViewController() adPoint:adPoint];
    }
}

- (void)engageResponseForId:(NSString *)engagementId response:(NSString *)response statusCode:(NSInteger)statusCode error:(NSString *)error
{
    @synchronized(self) {
        if (self.engagements[engagementId]) {
            void (^handler)(NSString *, NSInteger, NSError *);
            handler = self.engagements[engagementId];
            NSError * nserror = nil;
            if (error != nil && error.length > 0) {
                [NSError errorWithDomain:NSURLErrorDomain code:-57 userInfo:@{NSLocalizedDescriptionKey: error}];
            }
            handler(response, statusCode, nserror);
        } else {
            NSLog(@"Engagement with id %@ not found", engagementId);
        }
    }
}

#pragma mark DDNASmartAdServiceDelegate

- (void)didRegisterForInterstitialAds
{
    UnitySendMessage(SmartAdsObject, "DidRegisterForInterstitialAds", "");
}

- (void)didFailToRegisterForInterstitialAdsWithReason:(NSString *)reason
{
    UnitySendMessage(SmartAdsObject, "DidFailToRegisterForInterstitialAds", [reason UTF8String]);
}

- (void)didOpenInterstitialAd
{
    UnitySendMessage(SmartAdsObject, "DidOpenInterstitialAd", "");
}

- (void)didFailToOpenInterstitialAd
{
    UnitySendMessage(SmartAdsObject, "DidFailToOpenInterstitialAd", "");
}

- (void)didCloseInterstitialAd
{
    UnitySendMessage(SmartAdsObject, "DidCloseInterstitialAd", "");
}

- (void)didRegisterForRewardedAds
{
    UnitySendMessage(SmartAdsObject, "DidRegisterForRewardedAds", "");
}

- (void)didFailToRegisterForRewardedAdsWithReason:(NSString *)reason
{
    UnitySendMessage(SmartAdsObject, "DidFailToRegisterForRewardedAds", [reason UTF8String]);
}

- (void)didOpenRewardedAd
{
    UnitySendMessage(SmartAdsObject, "DidOpenRewardedAd", "");
}

- (void)didFailToOpenRewardedAd
{
    UnitySendMessage(SmartAdsObject, "DidFailToOpenRewardedAd", "");
}

- (void)didCloseRewardedAdWithReward:(BOOL)reward
{
    UnitySendMessage(SmartAdsObject, "DidCloseRewardedAd", [[NSString stringWithContentsOfDictionary:@{@"reward":[NSNumber numberWithBool:reward]}] UTF8String]);
}

- (void)recordEventWithName:(NSString *)eventName parameters:(NSDictionary *)parameters
{
    NSDictionary *message = @{
        @"eventName": eventName,
        @"parameters": parameters
    };
    
    UnitySendMessage(SmartAdsObject, "RecordEvent", [[NSString stringWithContentsOfDictionary:message] UTF8String]);
}

- (void)requestEngagementWithDecisionPoint:(NSString *)decisionPoint
                                   flavour:(NSString *)flavour
                                parameters:(NSDictionary *)parameters
                         completionHandler:(void (^)(NSString *response, NSInteger statusCode, NSError *connectionError))completionHandler
{
    NSString *engagementId = [[NSUUID UUID] UUIDString];

    NSDictionary *engagement = @{
            @"decisionPoint": decisionPoint,
            @"flavour": flavour,
            @"parameters": parameters == nil ? @{} : parameters,
            @"id": engagementId
    };

    void (^handlerCopy)(NSString *, NSInteger, NSError *) = [completionHandler copy];
    [self.engagements setObject:handlerCopy forKey:engagementId];

    UnitySendMessage(SmartAdsObject, "RequestEngagement", [[NSString stringWithContentsOfDictionary:engagement] UTF8String]);
}

@end
