#include <Foundation/Foundation.h>

@interface DDNAUnityNotificationsPlugin

+ (BOOL) applicationDidLaunchWithRemoteNotification;
+ (NSDictionary *) getRemoteNotification;

@end
