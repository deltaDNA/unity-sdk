#!/usr/bin/python

import json
import logging as log
import shutil
import subprocess
from os import name,sys

DEBUG = False
CONFIG = 'config.json'
NETWORKS = [
'admob',
'mobfox',
'mopub',
'adcolony',
'amazon',
'chartboost',
'flurry',
'inmobi',
'unity',
'vungle']
GRADLE = './gradlew' if name is 'posix' else 'gradlew.bat'

def download(notifications, smartads, networks):
    if notifications:
        log.info('requesting notifications')
    if smartads:
        log.info('requesting smartads')
    if len(networks) > 0:
        log.info("requesting %s networks", networks)

    return execute("{} {} download {} {} -Pnetworks={}".format(
        GRADLE,
        '--info' if DEBUG else '',
        '-Pnotifications' if notifications else '',
        '-Psmartads' if smartads else '',
        ','.join(networks)))

def execute(cmd):
    process = subprocess.Popen(cmd, shell = True)
    process.wait()
    return process.returncode

if __name__ == '__main__':
    DEBUG = '-d' in sys.argv
    log.basicConfig(level = log.DEBUG if DEBUG else log.INFO)

    if '-c' in sys.argv:
        exit(execute(GRADLE + ' clean'))
    else:
        try:
            with open(CONFIG) as config_file:
                config = json.load(config_file)
                smartads = config['smartads']
                networks = config['networks']

                if smartads and len(networks) is 0:
                    networks = NETWORKS
                elif smartads and len(set(networks) - set(NETWORKS)) > 0:
                    log.error("only %s are valid networks", NETWORKS)
                    exit(1)
                elif not smartads and len(networks) > 0:
                    log.error('cannot request networks without smartads')
                    exit(1)

                exit(download(config['notifications'],
                              smartads,
                              networks))
        except IOError:
            log.error("missing configuration %s", CONFIG)
            exit(1)
