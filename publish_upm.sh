#!/bin/bash -e

git subtree push --prefix=Assets/Mirror origin upmtest 
git tag $1 upmtest
git push origin --tags
