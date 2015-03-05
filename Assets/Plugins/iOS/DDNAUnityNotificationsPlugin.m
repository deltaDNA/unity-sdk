#include "DDNAUnityNotificationsPlugin.h"
#include "NSString+DDNAHelpers.h"


void UnitySendMessage(const char *className, const char *methodName, const char *parameter);

// Unity's internal custom notifications to listen to
extern NSString *kUnityDidReceiveRemoteNotification;
extern NSString *kUnityDidRegisterForRemoteNotificationsWithDeviceToken;
extern NSString *kUnityDidFailToRegisterForRemoteNotificationsWithError;

// Extenal functions called from Unity
void _registerForPushNotifications()
{
    [[DDNAUnityNotificationsPlugin sharedPlugin] registerForPushNotifications];
}

void _unregisterForPushNotifications()
{
    [[DDNAUnityNotificationsPlugin sharedPlugin] unregisterForPushNotifications];
}

@interface DDNAUnityNotificationsPlugin ()
{

}

+ (void)applicationDidFinishLaunchingNotification:(NSNotification *)notification;
+ (void)unityDidReceiveRemoteNotification:(NSNotification *)notification;
+ (void)unityDidRegisterForRemoteNotifications:(NSNotification *)notification;
+ (void)unityDidFailToRegisterForRemoteNotifications:(NSNotification *)notification;

@end


@implementation DDNAUnityNotificationsPlugin

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
        NSMutableDictionary *payload = [NSMutableDictionary dictionaryWithDictionary:[launchOptions objectForKey:UIApplicationLaunchOptionsRemoteNotificationKey]];
        if (payload != nil) {
            payload[@"_ddLaunch"] = @YES;
            UnitySendMessage("DeltaDNA.NotificationsPlugin", "DidLaunchWithPushNotification",
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
        UnitySendMessage("DeltaDNA.NotificationsPlugin", "DidReceivePushNotification",
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
        UnitySendMessage("DeltaDNA.NotificationsPlugin", "DidRegisterForPushNotifications",
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

    UnitySendMessage("DeltaDNA.NotificationsPlugin", "DidFailToRegisterForPushNotifications",
        [errorMsg UTF8String]);
}

- (void)registerForPushNotifications
{
    NSLog(@"DDNA Registering for push notifications");

    UIUserNotificationType types = UIUserNotificationTypeBadge |
                 UIUserNotificationTypeSound | UIUserNotificationTypeAlert;

    UIUserNotificationSettings *mySettings =
                [UIUserNotificationSettings settingsForTypes:types categories:nil];

    [[UIApplication sharedApplication] registerUserNotificationSettings:mySettings];

}

- (void)unregisterForPushNotifications
{
    NSLog(@"DDNA Unregistering for push notifications");

    [[UIApplication sharedApplication] unregisterForRemoteNotifications];
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
