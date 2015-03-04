#include "DDNANotificationManager.h"
#include "NSString+Helpers.h"


void UnitySendMessage(const char *className, const char *methodName, const char *parameter);
UIViewController *UnityGetGLViewController();

// Unity custom notifications
extern NSString *kUnityDidReceiveRemoteNotification;
extern NSString *kUnityDidRegisterForRemoteNotificationsWithDeviceToken;
extern NSString *kUnityDidFailToRegisterForRemoteNotificationsWithError;

@interface DDNANotificationManager ()
{

}

+ (void)applicationDidFinishLaunchingNotification:(NSNotification *)notification;
+ (void)unityDidReceiveRemoteNotification:(NSNotification *)notification;
+ (void)unityDidRegisterForRemoteNotifications:(NSNotification *)notification;
+ (void)unityDidFailToRegisterForRemoteNotifications:(NSNotification *)notification;

@end


@implementation DDNANotificationManager

static BOOL _didLaunchWithNotification = NO;
static NSDictionary *_remoteNotification = nil;

+ (void)load
{
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
    NSDictionary *launchOptions = [notification userInfo];
    if (launchOptions != nil) {
        NSMutableDictionary *payload = [launchOptions objectForKey:UIApplicationLaunchOptionsRemoteNotificationKey];
        if (payload != nil) {
            payload[@"_ddLaunch"] = @YES;
            UnitySendMessage("DeltaDNA.IOSPluginManager", "didLaunchWithPushNotification",
                [[NSString jsonStringWithContentsOfDictionary:payload] UTF8String]);
        }
    }
}

+ (void)unityDidReceiveRemoteNotification:(NSNotification *)notification
{
    NSMutableDictionary *payload = [NSMutableDictionary dictionaryWithDictionary:[notification userInfo]];
    if (payload != nil) {
        UIApplication *application = [UIApplication sharedApplication];
        payload[@"_ddLaunch"] = [NSNumber numberWithBool:application.applicationState != UIApplicationStateActive];
        UnitySendMessage("DeltaDNA.IOSPluginManager", "didReceivePushNotification",
            [[NSString jsonStringWithContentsOfDictionary:payload] UTF8String]);
    }
}

+ (void)unityDidRegisterForRemoteNotifications:(NSNotification *)notification
{
    NSObject *userInfo = [notification userInfo];
    if (userInfo != nil) {
        NSString *deviceToken = [userInfo description];
        deviceToken = [deviceToken stringByTrimmingCharactersInSet:[NSCharacterSet characterSetWithCharactersInString:@"<>"]];
        deviceToken = [deviceToken stringByReplacingOccurrencesOfString:@" " withString:@""];
        UnitySendMessage("DeltaDNA.IOSPluginManager", "didRegisterForPushNotifications",
            [deviceToken UTF8String]);
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

    UnitySendMessage("DeltaDNA.IOSPluginManager", "didFailToRegisterForPushNotifications",
        [errorMsg UTF8String]);
}

+ (BOOL)applicationDidLaunchWithRemoteNotification
{
    return _didLaunchWithNotification;
}

+ (NSDictionary *)getRemoteNotification
{
    return _remoteNotification;
}


@end
