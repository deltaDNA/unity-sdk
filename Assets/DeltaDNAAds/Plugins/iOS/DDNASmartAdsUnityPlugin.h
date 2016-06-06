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

#import <Foundation/Foundation.h>

// Unity Binding
void _registerForAds(const char * decisionPoint);
int _isInterstitialAdAllowed(const char * decisionPoint, const char * engageParams);
int _isInterstitialAdAvailable();
void _showInterstitialAd(const char * decisionPoint);
int _isRewardedAdAllowed(const char * decisionPoint, const char * engageParams);
int _isRewardedAdAvailable();
void _showRewardedAd(const char * decisionPoint);
void _engageResponse(const char * engagementId, const char * response, int statusCode, const char * error);
void _pause();
void _resume();
void _destroy();


@interface DDNASmartAdsUnityPlugin : NSObject

+ (instancetype)sharedPlugin;

- (void)registerForAds:(NSString *)decisionPoint;
- (BOOL)isInterstitialAdAllowed:(NSString *)decisionPoint engageParams:(NSString *)engageParams;
- (BOOL)isInterstitialAdAvailable;
- (void)showInterstitialAdWithDecisionPoint:(NSString *)decisionPoint;
- (BOOL)isRewardedAdAllowed:(NSString *)decisionPoint engageParams:(NSString *)engageParams;
- (BOOL)isRewardedAdAvailable;
- (void)showRewardedAdWithDecisionPoint:(NSString *)decisionPoint;
- (void)engageResponseForId:(NSString *)engagementId
                   response:(NSString *)response
                 statusCode:(NSInteger)statusCode
                      error:(NSString *)error;
- (void)pause;
- (void)resume;
- (void)destroy;

@end
