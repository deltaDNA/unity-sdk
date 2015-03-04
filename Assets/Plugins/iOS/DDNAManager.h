#import <Foundation/Foundation.h>

void _registerForPushNotifications();
void _unregisterForPushNotifications();

@protocol DDNAManagerDelegate;

@interface DDNAManager : NSObject
{

}

@property (nonatomic, strong) id<DDNAManagerDelegate> delegate;

+ (DDNAManager *)sharedInstance;

- (void)registerForPushNotifications;
- (void)unregisterForPushNotifications;

@end

@protocol DDNAManagerDelegate

@optional

- (void)didRegisterForPushNotificationsWithDeviceToken:(NSString *)deviceToken;

- (void)didFailToRegisterForPushNotificationsWithError:(NSError *)error;

@end
