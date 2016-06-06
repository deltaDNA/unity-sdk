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

int _isInterstitialAdAllowed(const char * decisionPoint, const char * engageParams)
{
    return [[DDNASmartAdsUnityPlugin sharedPlugin] isInterstitialAdAllowed:GetStringParam(decisionPoint) engageParams:GetStringParam(engageParams)];
}

int _isInterstitialAdAvailable()
{
    return [[DDNASmartAdsUnityPlugin sharedPlugin] isInterstitialAdAvailable];
}

void _showInterstitialAd(const char * decisionPoint)
{
    [[DDNASmartAdsUnityPlugin sharedPlugin] showInterstitialAdWithDecisionPoint:GetStringParam(decisionPoint)];
}

int _isRewardedAdAllowed(const char * decsionPoint, const char * engageParams)
{
    return [[DDNASmartAdsUnityPlugin sharedPlugin] isRewardedAdAllowed:GetStringParam(decsionPoint) engageParams:GetStringParam(engageParams)];
}

int _isRewardedAdAvailable()
{
    return [[DDNASmartAdsUnityPlugin sharedPlugin] isRewardedAdAvailable];
}

void _showRewardedAd(const char * decisionPoint)
{
    [[DDNASmartAdsUnityPlugin sharedPlugin] showRewardedAdWithDecisionPoint:GetStringParam(decisionPoint)];
}

void _engageResponse(const char * engagementId, const char * response, int statusCode, const char * error)
{
    [[DDNASmartAdsUnityPlugin sharedPlugin] engageResponseForId:GetStringParam(engagementId)
                                                       response:GetStringParam(response)
                                                     statusCode:statusCode
                                                          error:GetStringParam(error)];
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

- (BOOL)isInterstitialAdAllowed:(NSString *)decisionPoint engageParams:(NSString *)engageParams
{
    @synchronized (self) {
        if (self.adService) {
            NSDictionary *engageParamsDict = [NSDictionary dictionaryWithJSONString:engageParams];
            return [self.adService isInterstitialAdAllowedForDecisionPoint:decisionPoint engagementParameters:engageParamsDict];
        } else {
            return NO;
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

- (void)showInterstitialAdWithDecisionPoint:(NSString *)decisionPoint
{
    @synchronized(self) {
        [self.adService showInterstitialAdFromRootViewController:UnityGetGLViewController() decisionPoint:decisionPoint];
    }
}

- (BOOL)isRewardedAdAllowed:(NSString *)decisionPoint engageParams:(NSString *)engageParams
{
    @synchronized (self) {
        if (self.adService) {
            NSDictionary *engageParamsDict = [NSDictionary dictionaryWithJSONString:engageParams];
            return [self.adService isRewardedAdAllowedForDecisionPoint:decisionPoint engagementParameters:engageParamsDict];
        } else {
            return NO;
        }
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

- (void)showRewardedAdWithDecisionPoint:(NSString *)decisionPoint
{
    @synchronized(self) {
        [self.adService showRewardedAdFromRootViewController:UnityGetGLViewController() decisionPoint:decisionPoint];
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

- (void)didFailToOpenInterstitialAdWithReason:(NSString *)reason
{
    UnitySendMessage(SmartAdsObject, "DidFailToOpenInterstitialAd", [reason UTF8String]);
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

- (void)didFailToOpenRewardedAdWithReason:(NSString *)reason
{
    UnitySendMessage(SmartAdsObject, "DidFailToOpenRewardedAd", [reason UTF8String]);
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
