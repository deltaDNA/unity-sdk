#include <stdlib.h>
#include <string.h>

extern "C" {

  const char *DDNA_current_culture() {
    NSLocale *locale = [NSLocale currentLocale];

    unsigned long len = locale.localeIdentifier.length;

    char *locale_str = 0;
    locale_str = (char*)malloc(len + 1);

    // iOS reports en_US, but we expect en-US..
    for (unsigned long i = 0; i < len; ++i) {
      char c = [locale.localeIdentifier characterAtIndex:i];
      if (c == '_') {
        locale_str[i] = '-';
      }
      else {
        locale_str[i] = c;
      }
    }
    locale_str[len] = 0;

    return locale_str;
  }

  const char *DDNA_system_culture() {
    NSLocale *locale = [NSLocale systemLocale];

    unsigned long len = locale.localeIdentifier.length;

    char *locale_str = 0;
    locale_str = (char*)malloc(len + 1);

    // iOS reports en_US, but we expect en-US..
    for (unsigned long i = 0; i < len; ++i) {
      char c = [locale.localeIdentifier characterAtIndex:i];
      if (c == '_') {
        locale_str[i] = '-';
      }
      else {
        locale_str[i] = c;
      }
    }
    locale_str[len] = 0;

    return locale_str;
  }
}



