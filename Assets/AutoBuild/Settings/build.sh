#!/bin/bash
buildXmlPath=$1
target=$2

/usr/local/bin/ant -f $buildXmlPath $target