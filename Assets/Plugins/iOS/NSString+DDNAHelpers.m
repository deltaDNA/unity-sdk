//
//  NSString+Helpers.m
//  DeltaDNASDK
//
//  Created by David White on 18/07/2014.
//  Copyright (c) 2014 deltadna. All rights reserved.
//

#import "NSString+DDNAHelpers.h"
#import <CommonCrypto/CommonDigest.h>

@implementation NSString (DDNAHelpers)

+ (BOOL) stringIsNilOrEmpty: (NSString*) aString
{
    return !(aString && aString.length);
}

+ (NSString *) jsonStringWithContentsOfDictionary: (NSDictionary *) aDictionary
{
    NSString * result = nil;
    NSError * error = nil;
    NSData * data = [NSJSONSerialization dataWithJSONObject:aDictionary
                                                    options:kNilOptions
                                                      error:&error];
    if (error == 0)
    {
        result = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
    }

    return result;
}

- (NSString *) md5
{
    if (!self) return nil;

    const char* str = [self UTF8String];
    unsigned char result[CC_MD5_DIGEST_LENGTH];
    CC_MD5(str, (CC_LONG)strlen(str), result);

    NSMutableString *ret = [NSMutableString stringWithCapacity:CC_MD5_DIGEST_LENGTH*2];
    for (int i = 0; i < CC_MD5_DIGEST_LENGTH; i++) {
        [ret appendFormat:@"%02x", result[i]];
    }
    return ret;
}

@end
