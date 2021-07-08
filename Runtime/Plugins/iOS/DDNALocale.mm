#include <stdlib.h>
#include <string.h>

extern "C" {

    const char *DDNA_current_culture_language_code() {
        NSString *languageCode = [NSLocale currentLocale].languageCode;
        if (languageCode == NULL) {
            return NULL;
        }
        char *cString = (char*)malloc(strlen(languageCode.UTF8String) + 1);
        strcpy(cString, languageCode.UTF8String);
        return cString;
    }

    const char *DDNA_current_culture_country_code() {
        NSString *countryCode = [NSLocale currentLocale].countryCode;
        if (countryCode == NULL) {
            return NULL;
        }
        char *cString = (char*)malloc(strlen(countryCode.UTF8String) + 1);
        strcpy(cString, countryCode.UTF8String);
        return cString; 
    }
}



