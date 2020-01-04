git subtree push --prefix=Assets/Mirror origin upmtest 
git fetch
git tag $1 origin/upmtest
git push --tags
