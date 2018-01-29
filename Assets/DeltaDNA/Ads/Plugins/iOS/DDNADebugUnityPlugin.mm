#if DDNA_DEBUG_NOTIFICATIONS

#import "UnityAppController.h"
#import <DeltaDNA/DeltaDNA.h>
#import <DeltaDNAAds/DeltaDNAAds.h>
#import <UserNotifications/UserNotifications.h>

@interface DDNAUnityAppController : UnityAppController <UNUserNotificationCenterDelegate>

@end

IMPL_APP_CONTROLLER_SUBCLASS(DDNAUnityAppController)

@implementation DDNAUnityAppController

-(BOOL)application:(UIApplication*) application didFinishLaunchingWithOptions:(NSDictionary*) options
{
    NSLog(@"[OverrideAppDelegate application:%@ didFinishLaunchingWithOptions:%@]", application, options);

    [self setupPushNotifications];

    return [super application:application didFinishLaunchingWithOptions:options];
}

- (void)setupPushNotifications
{
    UNUserNotificationCenter *center = [UNUserNotificationCenter currentNotificationCenter];
    center.delegate = self;

    [center requestAuthorizationWithOptions:(UNAuthorizationOptionAlert | UNAuthorizationOptionSound | UNAuthorizationOptionBadge) completionHandler:^(BOOL granted, NSError * _Nullable error) {
        if (granted) {

            UNNotificationAction *stopAction = [UNNotificationAction actionWithIdentifier:@"com.deltadna.stopAction" title:@"Stop Notifications" options:UNNotificationActionOptionDestructive];

            UNNotificationCategory *diagnosticCategory = [UNNotificationCategory categoryWithIdentifier:@"com.deltadna.diagnosticCategory" actions:@[stopAction] intentIdentifiers:@[] options:UNNotificationCategoryOptionNone];

            UNUserNotificationCenter *center = [UNUserNotificationCenter currentNotificationCenter];
            [center setNotificationCategories:[NSSet setWithObjects:diagnosticCategory, nil]];

            dispatch_async(dispatch_get_main_queue(), ^(void) {
                [[UIApplication sharedApplication] registerForRemoteNotifications];
            });
        }
    }];
}

#pragma mark - UNUserNotificationCenterDelegate

- (void)userNotificationCenter:(UNUserNotificationCenter *)center willPresentNotification:(UNNotification *)notification withCompletionHandler:(void (^)(UNNotificationPresentationOptions))completionHandler
{
    // Allow diagnostic notifications to appear when in the foreground.
    if ([notification.request.content.categoryIdentifier isEqualToString:@"com.deltadna.diagnosticCategory"]) {
        completionHandler(UNNotificationPresentationOptionAlert);
    }
}

- (void)userNotificationCenter:(UNUserNotificationCenter *)center didReceiveNotificationResponse:(UNNotificationResponse *)response withCompletionHandler:(void (^)(void))completionHandler
{
    if ([response.actionIdentifier isEqualToString:@"com.deltadna.stopAction"]) {
        [[DDNADebugListener sharedInstance] disableNotifications];
    }
    // Must call the completion handler.
    completionHandler();
}

@end

#endif // DDNA_DEBUG_NOTIFICATIONS
