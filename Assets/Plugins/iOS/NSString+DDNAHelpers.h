//
//  NSString+Helpers.h
//  DeltaDNASDK
//
//  Created by David White on 18/07/2014.
//  Copyright (c) 2014 deltadna. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface NSString (DDNAHelpers)

+ (BOOL) stringIsNilOrEmpty: (NSString*) aString;

+ (NSString *) jsonStringWithContentsOfDictionary: (NSDictionary *) aDictionary;

- (NSString *) md5;

@end
