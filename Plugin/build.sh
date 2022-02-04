#!/bin/sh

LIB="KlakSyphon"
DST="../jp.keijiro.klak.syphon/Internal"

set -e
set -x

make clean
make all TARGET="x86_64-apple-macos10.12"
mv ${LIB}.dylib x86_64.dylib

make clean
make all TARGET="arm64-apple-macos10.12"
mv ${LIB}.dylib arm64.dylib

lipo -create -output ${LIB}.bundle x86_64.dylib arm64.dylib

DST_FILE="${DST}/${LIB}.bundle"
[ -e $DST_FILE ] && rm $DST_FILE
cp ${LIB}.bundle $DST_FILE
