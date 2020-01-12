#!/bin/sh -e

BRANCH=$1
TAG=$2
echo "Showing remotes 1"
git remote -v

git subtree split --prefix=Assets/Mirror -b $BRANCH
echo "Showing remotes 2"
git remote -v

#git filter-branch --prune-empty --tree-filter 'rm -rf Tests' upm
git gc
echo "Showing remotes 3"
git remote -v

git filter-repo --force --invert-paths --path Tests --path Tests.meta --path Examples.meta --path-rename "Examples:Samples~" --refs $BRANCH

echo "Showing remotes 5"
git remote -v
git tag $TAG $BRANCH
git push origin $BRANCH --tags
