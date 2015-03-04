#include <Foundation/Foundation.h>

@interface DDNANotificationManager

+ (BOOL) applicationDidLaunchWithRemoteNotification;
+ (NSDictionary *) getRemoteNotification;

@end
