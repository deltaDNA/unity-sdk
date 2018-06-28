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

#import <Foundation/Foundation.h>

// Unity Binding
void _registerForAds(const char * config, bool userConsent, bool ageRestricted);
int _isInterstitialAdAllowed(const char * decisionPoint, const char * engageParams, bool checkTime);
int _hasLoadedInterstitialAd();
void _showInterstitialAd(const char * decisionPoint, const char * engageParams);
int _isRewardedAdAllowed(const char * decisionPoint, const char * engageParams, bool checkTime);
long _timeUntilRewardedAdAllowed(const char * decisionPoint, const char * engageParams);
int _hasLoadedRewardedAd();
void _showRewardedAd(const char * decisionPoint, const char * engageParams);
long _getLastShown(const char * decisionPoint);
long _getSessionCount(const char * decisionPoint);
long _getDailyCount(const char * decisionPoint);
void _pause();
void _resume();
void _destroy();
void _setLoggingLevel(int level);
void _fireEventNewSession();


@interface DDNASmartAdsUnityPlugin : NSObject

+ (instancetype)sharedPlugin;

- (void)registerForAds:(NSString *)config userConsent:(BOOL)userConsent ageRestricted:(BOOL)ageRestricted;
- (BOOL)isInterstitialAdAllowed:(NSString *)decisionPoint engageParams:(NSString *)engageParams checkTime:(BOOL)checkTime;
- (BOOL)hasLoadedInterstitialAd;
- (void)showInterstitialAd:(NSString *)decisionPoint engageParams:(NSString *)engageParams;
- (BOOL)isRewardedAdAllowed:(NSString *)decisionPoint engageParams:(NSString *)engageParams checkTime:(BOOL)checkTime;
- (long)timeUntilRewardedAdAllowed:(NSString *)decisionPoint engageParams:(NSString *)engageParams;
- (BOOL)hasLoadedRewardedAd;
- (void)showRewardedAd:(NSString *)decisionPoint engageParams:(NSString *)engageParams;
- (long)getLastShown:(NSString *)decisionPoint;
- (long)getSessionCount:(NSString *)decisionPoint;
- (long)getDailyCount:(NSString *)decisionPoint;
- (void)pause;
- (void)resume;
- (void)destroy;

@end

#endif // DDNA_SMARTADS
