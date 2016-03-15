//
// Copyright (c) 2016 deltaDNA Ltd. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
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
