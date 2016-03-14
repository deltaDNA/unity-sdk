#!/usr/bin/python

import argparse
import json
import logging as log
import shutil
import subprocess
from os import name,sys

LIBS = 'Assets/DeltaDNAAds/Plugins/Android'
CONFIG = 'config.json'
NETWORKS = [
'adcolony',
'admob',
'amazon',
'applovin',
'chartboost',
'flurry',
'inmobi',
'mopub',
'supersonic',
'unity',
'vungle']
GRADLE = './gradlew' if name is 'posix' else 'gradlew.bat'

def clean():
    log.info('cleaning downloaded libraries in %s', LIBS)

    code = execute(GRADLE + ' clean')

    if code is 0:
        log.info("cleaned libraries in %s", LIBS)
    else:
        log.error('failed to clean libraries')

    return code

def download(args, notifications, smartads, networks):
    if notifications:
        log.info('requesting notifications')
    if smartads:
        log.info('requesting smartads')
    if len(networks) > 0:
        log.info("requesting networks %s", ', '.join(networks))

    log.info('downloading libraries')
    code = execute("{} {} {} {} clean download {} {} -Pnetworks={}".format(
        GRADLE,
        '--stacktrace' if args.stacktrace else '',
        '--info' if args.info else '',
        '--debug' if args.debug else '',
        '-Pnotifications' if notifications else '',
        '-Psmartads' if smartads else '',
        ','.join(networks)))

    if code is 0:
        log.info("downloaded libraries to %s", LIBS)
        log.info("if using Unity 4 then libraries from %s need to be moved to the 'Assets/Plugins/Android' directory", LIBS)
    else:
        log.error('failed to download libraries')

    return code

def execute(cmd):
    log.debug("executing `%s`", cmd)

    process = subprocess.Popen(cmd, shell = True)
    process.wait()

    log.debug("got return code %d", process.returncode)
    return process.returncode

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description = "downloads libraries needed for the DeltaDNA Unity SDK on Android as specified in {} to {}".format(CONFIG, LIBS))
    parser.add_argument('-c', '--clean', action = 'store_true', help = 'cleans the downloaded libraries only')
    parser.add_argument('--stacktrace', action = 'store_true', help = 'show full stacktrace or error')
    group = parser.add_mutually_exclusive_group()
    group.add_argument('--info', action = 'store_true', help = 'show info logging')
    group.add_argument('--debug', action = 'store_true', help = 'show debug logging')
    args = parser.parse_args()

    if args.debug:
        log.basicConfig(level = log.DEBUG)
    else:
        log.basicConfig(level = log.INFO)

    if args.clean:
        exit(clean())
    else:
        try:
            with open(CONFIG) as config_file:
                config = json.load(config_file)
                smartads = True
                networks = config['networks']

                if smartads and len(networks) is 0:
                    networks = NETWORKS
                elif smartads and len(set(networks) - set(NETWORKS)) > 0:
                    log.error("only %s are valid networks", NETWORKS)
                    exit(1)
                elif not smartads and len(networks) > 0:
                    log.error('cannot request networks without smartads')
                    exit(1)

                exit(download(args,
                              False,
                              smartads,
                              networks))
        except IOError:
            log.error("missing configuration %s", CONFIG)
            exit(1)
