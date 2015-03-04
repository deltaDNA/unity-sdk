#import "DDNAManager.h"

#define GetStringParam( _x_ ) ( _x_ != NULL ) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]

void _registerForPushNotifications()
{
    [[DDNAManager sharedInstance] registerForPushNotifications];
}

void _unregisterForPushNotifications()
{
    [[DDNAManager sharedInstance] unregisterForPushNotifications];
}

@implementation DDNAManager

+ (DDNAManager *)sharedInstance
{
    static DDNAManager *sharedInstance = nil;

    if (!sharedInstance) {
        sharedInstance = [[DDNAManager alloc] init];
    }

    return sharedInstance;
}

- (void)registerForPushNotifications
{
    NSLog(@"Registering for push notifications");

    UIUserNotificationType types = UIUserNotificationTypeBadge |
                 UIUserNotificationTypeSound | UIUserNotificationTypeAlert;

    UIUserNotificationSettings *mySettings =
                [UIUserNotificationSettings settingsForTypes:types categories:nil];

    [[UIApplication sharedApplication] registerUserNotificationSettings:mySettings];

}

- (void)unregisterForPushNotifications
{
    NSLog(@"Unregistering for push notifications");
}

@end
