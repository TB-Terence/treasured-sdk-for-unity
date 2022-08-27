#!/usr/bin/env sh
## Loop through all the .png files from the given directory folder
## Encode to KTX2 format and delete .png files

executable=$1
build_root=$2

usage() {
	echo "Usage: $0 <executable> <build_root>"
	echo "  <executable> - path to the executable"
	echo "  <build_root> - path to the build root"
	exit 1
}

if [ -z "$executable" ]; then
	echo "Please provide the executable path"
	echo ""
	usage
	exit 1
fi

if [ -z "$build_root" ]; then
	echo "Please provide the build root path"
	echo ""
	usage
	exit 1
fi

FILES=$(find $build_root -name "*.png")

for file in $FILES; do
	echo "Converting $file"
	sh -c "$executable --bcmp ${file%.*} $file"
	rm $file
done
