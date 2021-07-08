#include <AppTrackingTransparency/ATTrackingManager.h>
#include <Foundation/Foundation.h>
#include <AdSupport/ASIdentifierManager.h>

int ddna_get_tracking_status() {
    if (@available(iOS 14, *)) {
        return [ATTrackingManager trackingAuthorizationStatus];
    } else {
        // As 0 is a valid tracking status (status not checked) then we return a negative here to show the value is not a valid status.
        return -1;
    }
}

bool ddna_is_tracking_authorized() {
    if (@available(iOS 14, *)) {
        return [ATTrackingManager trackingAuthorizationStatus] == 3;
    } else {
        return [[ASIdentifierManager sharedManager] isAdvertisingTrackingEnabled];
    }
}
