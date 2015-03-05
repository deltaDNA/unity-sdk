#include <Foundation/Foundation.h>

// Unity native interface
void _registerForPushNotifications();
void _unregisterForPushNotifications();

@interface DDNAUnityNotificationsPlugin : NSObject

+ (instancetype)sharedPlugin;

- (void)registerForPushNotifications;
- (void)unregisterForPushNotifications;
- (BOOL)applicationDidLaunchWithRemoteNotification;
- (NSDictionary *)getRemoteNotification;

@end
