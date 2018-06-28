//
// Copyright (c) 2016 deltaDNA Ltd. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

#if DDNA_SMARTADS

#import "DDNASmartAdsUnityPlugin.h"
#import <DeltaDNAAds/SmartAds/DDNASmartAdFactory.h>
#import <DeltaDNAAds/SmartAds/DDNASmartAdService.h>
#import <DeltaDNAAds/SmartAds/DDNADebugListener.h>
#import <DeltaDNA/DDNALog.h>

// Unity callback support
void UnitySendMessage(const char *gameObjectName, const char *methodName, const char *message);

#if UNITY_VERSION < 500
void UnityPause( bool pause );
#else
void UnityPause( int pause );
#endif

// Converts C style string to NSString
#define GetStringParam( _x_ ) ( _x_ != NULL ) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]

// Functions called from Unity
void _registerForAds(const char * config, bool userConsent, bool ageRestricted)
{
    [[DDNASmartAdsUnityPlugin sharedPlugin] registerForAds:GetStringParam(config) userConsent:userConsent ageRestricted:ageRestricted];
}

int _isInterstitialAdAllowed(const char * decisionPoint, const char * engageParams, bool checkTime)
{
    return [[DDNASmartAdsUnityPlugin sharedPlugin] isInterstitialAdAllowed:GetStringParam(decisionPoint) engageParams:GetStringParam(engageParams) checkTime:checkTime];
}

int _hasLoadedInterstitialAd()
{
    return [[DDNASmartAdsUnityPlugin sharedPlugin] hasLoadedInterstitialAd];
}

void _showInterstitialAd(const char * decisionPoint, const char * engageParams)
{
    [[DDNASmartAdsUnityPlugin sharedPlugin] showInterstitialAd:GetStringParam(decisionPoint) engageParams:GetStringParam(engageParams)];
}

int _isRewardedAdAllowed(const char * decsionPoint, const char * engageParams, bool checkTime)
{
    return [[DDNASmartAdsUnityPlugin sharedPlugin] isRewardedAdAllowed:GetStringParam(decsionPoint) engageParams:GetStringParam(engageParams) checkTime:checkTime];
}

long _timeUntilRewardedAdAllowed(const char * decisionPoint, const char * engageParams)
{
    return [[DDNASmartAdsUnityPlugin sharedPlugin] timeUntilRewardedAdAllowed:GetStringParam(decisionPoint) engageParams:GetStringParam(engageParams)];
}

int _hasLoadedRewardedAd()
{
    return [[DDNASmartAdsUnityPlugin sharedPlugin] hasLoadedRewardedAd];
}

void _showRewardedAd(const char * decisionPoint, const char * engageParams)
{
    [[DDNASmartAdsUnityPlugin sharedPlugin] showRewardedAd:GetStringParam(decisionPoint) engageParams:GetStringParam(engageParams)];
}

long _getLastShown(const char * decisionPoint)
{
    return [[DDNASmartAdsUnityPlugin sharedPlugin] getLastShown:GetStringParam(decisionPoint)];
}

long _getSessionCount(const char * decisionPoint)
{
    return [[DDNASmartAdsUnityPlugin sharedPlugin] getSessionCount:GetStringParam(decisionPoint)];
}

long _getDailyCount(const char * decisionPoint)
{
    return [[DDNASmartAdsUnityPlugin sharedPlugin] getDailyCount:GetStringParam(decisionPoint)];
}

void _pause()
{
    [[DDNASmartAdsUnityPlugin sharedPlugin] pause];
}

void _resume()
{
    [[DDNASmartAdsUnityPlugin sharedPlugin] resume];
}

void _destroy()
{
    [[DDNASmartAdsUnityPlugin sharedPlugin] destroy];
}

void _setLoggingLevel(int level)
{
    DDNALogLevel logLevel = DDNALogLevelNone;
    switch (level) {
        case 0: logLevel = DDNALogLevelDebug; break;
        case 1: logLevel = DDNALogLevelInfo; break;
        case 2: logLevel = DDNALogLevelWarn; break;
        case 3: logLevel = DDNALogLevelError; break;
        default: logLevel = DDNALogLevelNone; break;
    }
    [DDNALog setLogLevel:logLevel];
}

void _fireEventNewSession()
{
    NSLog(@"Posting NewSessionEvent to NSNotificationCenter");
    [[NSNotificationCenter defaultCenter] postNotificationName:@"DDNASDKNewSession" object:nil];
}

const char * SmartAdsObject = "DeltaDNA.SmartAds";

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

@implementation NSDictionary (DeltaDNA)

+ (NSDictionary *) dictionaryWithJSONString: (NSString *) jsonString
{
    if (jsonString == nil || jsonString.length == 0)
    {
        return [NSDictionary dictionary];
    }

    NSData * data = [jsonString dataUsingEncoding:NSUTF8StringEncoding];
    NSError * error = nil;
    NSDictionary * result = [NSJSONSerialization JSONObjectWithData:data
                                                            options:kNilOptions
                                                              error:&error];
    if (error != 0)
    {
        return [NSDictionary dictionary];
    }

    return result;
}

@end

@interface DDNASmartAdsUnityPlugin () <DDNASmartAdServiceDelegate>
{

}

@property (nonatomic, strong) DDNASmartAdFactory *factory;
@property (nonatomic, strong) DDNASmartAdService *adService;
@property (nonatomic, strong) DDNADebugListener *debugListener;
@property (nonatomic, copy) NSString *config;
@property (nonatomic, assign) BOOL userConsent;
@property (nonatomic, assign) BOOL ageRestricted;

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
        self.debugListener = [DDNADebugListener sharedInstance];
        #if !DDNA_DEBUG_NOTIFICATIONS
        [self.debugListener disableNotifications];
        #endif
        [self.debugListener registerListeners];
    }
    return self;
}

- (void)registerForAds:(NSString *)config userConsent:(BOOL)userConsent ageRestricted:(BOOL)ageRestricted
{
    @synchronized(self) {
        self.config = config;
        self.userConsent = userConsent;
        self.ageRestricted = ageRestricted;   
        self.adService = [self.factory buildSmartAdServiceWithDelegate:self];

        if (self.adService) {
            [self.adService beginSessionWithConfig:[NSDictionary dictionaryWithJSONString:config] userConsent:userConsent ageRestricted:ageRestricted];
        } else {
            UnitySendMessage(SmartAdsObject, "DidFailToRegisterForAds", "Failed to build ad service");
        }
    }
}

- (BOOL)isInterstitialAdAllowed:(NSString *)decisionPoint engageParams:(NSString *)engageParams checkTime:(BOOL)checkTime
{
    @synchronized (self) {
        if (self.adService) {
            NSDictionary *engageParamsDict = [NSDictionary dictionaryWithJSONString:engageParams];
            return [self.adService isInterstitialAdAllowedForDecisionPoint:decisionPoint parameters:engageParamsDict checkTime:checkTime];
        } else {
            return NO;
        }
    }
}

- (BOOL)hasLoadedInterstitialAd
{
    @synchronized(self) {
        if (self.adService) {
            return [self.adService hasLoadedInterstitialAd];
        } else {
            return NO;
        }
    }
}

- (void)showInterstitialAd:(NSString *)decisionPoint engageParams:(NSString *)engageParams
{
    @synchronized(self) {
        NSDictionary *engageParamsDict = [NSDictionary dictionaryWithJSONString:engageParams];
        [self.adService showInterstitialAdFromRootViewController:UnityGetGLViewController() decisionPoint:decisionPoint parameters:engageParamsDict];
    }
}

- (BOOL)isRewardedAdAllowed:(NSString *)decisionPoint engageParams:(NSString *)engageParams checkTime:(BOOL)checkTime
{
    @synchronized (self) {
        if (self.adService) {
            NSDictionary *engageParamsDict = [NSDictionary dictionaryWithJSONString:engageParams];
            return [self.adService isRewardedAdAllowedForDecisionPoint:decisionPoint parameters:engageParamsDict checkTime:checkTime];
        } else {
            return NO;
        }
    }
}

- (long)timeUntilRewardedAdAllowed:(NSString *)decisionPoint engageParams:(NSString *)engageParams
{
    @synchronized(self) {
        if (self.adService) {
            NSDictionary *engageParamsDict = [NSDictionary dictionaryWithJSONString:engageParams];
            return [self.adService timeUntilRewardedAdAllowedForDecisionPoint:decisionPoint parameters:engageParamsDict];
        } else {
            return 0;
        }
    }
}

- (BOOL)hasLoadedRewardedAd
{
    @synchronized(self) {
        if (self.adService) {
            return [self.adService hasLoadedRewardedAd];
        } else {
            return NO;
        }
    }
}

- (void)showRewardedAd:(NSString *)decisionPoint engageParams:(NSString *)engageParams
{
    @synchronized(self) {
        NSDictionary *engageParamsDict = [NSDictionary dictionaryWithJSONString:engageParams];
        [self.adService showRewardedAdFromRootViewController:UnityGetGLViewController() decisionPoint:decisionPoint parameters:engageParamsDict];
    }
}

- (long)getLastShown:(NSString *)decisionPoint
{
    @synchronized(self) {
        if (self.adService) {
            NSDate *lastShown = [self.adService lastShownForDecisionPoint:decisionPoint];
            if (lastShown != nil) {
                return [lastShown timeIntervalSince1970];
            } else {
                return 0;
            }
        } else {
            return 0;
        }
    }
}

- (long)getSessionCount:(NSString *)decisionPoint
{
    @synchronized(self) {
        if (self.adService) {
            return [self.adService sessionCountForDecisionPoint:decisionPoint];
        } else {
            return 0;
        }
    }
}

- (long)getDailyCount:(NSString *)decisionPoint
{
    @synchronized(self) {
        if (self.adService) {
            return [self.adService dailyCountForDecisionPoint:decisionPoint];
        } else {
            return 0;
        }
    }
}

- (void)pause
{
    [self.adService pause];
}

- (void)resume
{
    [self.adService resume];
}

- (void)destroy
{
    self.adService = nil;
}

- (void)reRegisterForAds
{
    [self registerForAds:self.config userConsent:self.userConsent ageRestricted:self.ageRestricted];
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
    UnityPause(YES);
}

- (void)didFailToOpenInterstitialAdWithReason:(NSString *)reason
{
    UnitySendMessage(SmartAdsObject, "DidFailToOpenInterstitialAd", [reason UTF8String]);
}

- (void)didCloseInterstitialAd
{
    UnityPause(NO);
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

- (void)didLoadRewardedAd
{
    UnitySendMessage(SmartAdsObject, "DidLoadRewardedAd", "");
}

- (void)didOpenRewardedAdForDecisionPoint:(NSString *)decisionPoint
{
    UnitySendMessage(SmartAdsObject, "DidOpenRewardedAd", [decisionPoint UTF8String]);
    UnityPause(YES);
}

- (void)didFailToOpenRewardedAdWithReason:(NSString *)reason
{
    UnitySendMessage(SmartAdsObject, "DidFailToOpenRewardedAd", [reason UTF8String]);
}

- (void)didCloseRewardedAdWithReward:(BOOL)reward
{
    UnityPause(NO);
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

@end

#endif // DDNA_SMARTADS
