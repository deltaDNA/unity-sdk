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

#include "DDNAUnityNotificationsPlugin.h"
#include "NSString+DDNAHelpers.h"


void UnitySendMessage(const char *className, const char *methodName, const char *parameter);

void _markUnityLoaded()
{
    [[DDNAUnityNotificationsPlugin sharedPlugin] unityLoaded];
}

// Unity's internal custom notifications to listen to
extern NSString *kUnityDidReceiveRemoteNotification;
extern NSString *kUnityDidRegisterForRemoteNotificationsWithDeviceToken;
extern NSString *kUnityDidFailToRegisterForRemoteNotificationsWithError;

@interface DDNAUnityNotificationsPlugin ()
{

}

+ (void)applicationDidFinishLaunchingNotification:(NSNotification *)notification;
+ (void)unityDidReceiveRemoteNotification:(NSNotification *)notification;
+ (void)unityDidRegisterForRemoteNotifications:(NSNotification *)notification;
+ (void)unityDidFailToRegisterForRemoteNotifications:(NSNotification *)notification;

@end


@implementation DDNAUnityNotificationsPlugin

static dispatch_queue_t _taskQueue;
static BOOL _sdkLoaded = NO;
static BOOL _coldLaunch = NO;

static BOOL _didLaunchWithNotification = NO;
static NSDictionary *_remoteNotification = nil;

+ (instancetype)sharedPlugin
{
    static id sharedPlugin = nil;

    static dispatch_once_t pred;
    dispatch_once(&pred, ^{
        sharedPlugin = [[self alloc] init];
    });
    return sharedPlugin;
}

+ (void)load
{
    _taskQueue = dispatch_queue_create("com.deltadna.TaskQueue", NULL);
    if (!_sdkLoaded) {
        dispatch_suspend(_taskQueue);
    }

    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(applicationDidFinishLaunchingNotification:)
        name:@"UIApplicationDidFinishLaunchingNotification" object:nil];

    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(unityDidReceiveRemoteNotification:)
        name:kUnityDidReceiveRemoteNotification object:nil];

    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(unityDidRegisterForRemoteNotifications:)
        name:kUnityDidRegisterForRemoteNotificationsWithDeviceToken object:nil];

    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(unityDidFailToRegisterForRemoteNotifications:)
        name:kUnityDidFailToRegisterForRemoteNotificationsWithError object:nil];
}

+ (void)applicationDidFinishLaunchingNotification:(NSNotification *)notification
{
    _coldLaunch = YES;
}

+ (void)unityDidReceiveRemoteNotification:(NSNotification *)notification
{
    if (_sdkLoaded) {
      NSMutableDictionary *payload = [NSMutableDictionary dictionaryWithDictionary:[notification userInfo]];
      if (payload != nil) {
          UIApplication *application = [UIApplication sharedApplication];
          payload[@"_ddLaunch"] = [NSNumber numberWithBool:application.applicationState != UIApplicationStateActive || _coldLaunch];
          UnitySendMessage("DeltaDNA.IosNotifications", "DidReceivePushNotification",
              [[NSString jsonStringWithContentsOfDictionary:payload] UTF8String]);

          _coldLaunch = NO;
      }
    } else {
        __typeof(self) __weak weakSelf = self;
        dispatch_async(_taskQueue, ^{
            [weakSelf unityDidReceiveRemoteNotification:notification];
        });
    }
}

+ (void)unityDidRegisterForRemoteNotifications:(NSNotification *)notification
{
   NSObject *userInfo = [notification userInfo];
   if (userInfo != nil) {
 
     NSData *data = (NSData *) userInfo;
     NSUInteger length = data.length;
     
     if (length > 0){
       const unsigned char *buffer = data.bytes;
       NSMutableString *tokenString = [NSMutableString stringWithCapacity:(length * 2)];
       for (int i = 0; i < length; ++i) {
         [tokenString appendFormat:@"%02x", buffer[i]];
       }
       UnitySendMessage("DeltaDNA.IosNotifications", "DidRegisterForPushNotifications", [tokenString UTF8String]);
     }
   }
}

+ (void)unityDidFailToRegisterForRemoteNotifications:(NSNotification *)notification
{
    NSString *errorMsg = @"";
    NSError *error = (NSError *)[notification userInfo];
    if (error != nil) {
        NSLog(@"Failed to register for push notifications: %@", error);
        errorMsg = [error localizedDescription];
    }

    UnitySendMessage("DeltaDNA.IosNotifications", "DidFailToRegisterForPushNotifications",
        [errorMsg UTF8String]);
}

- (void)unityLoaded
{
    _sdkLoaded = YES;
    dispatch_resume(_taskQueue);
}

- (BOOL)applicationDidLaunchWithRemoteNotification
{
    return _didLaunchWithNotification;
}

- (NSDictionary *)getRemoteNotification
{
    return _remoteNotification;
}

@end
