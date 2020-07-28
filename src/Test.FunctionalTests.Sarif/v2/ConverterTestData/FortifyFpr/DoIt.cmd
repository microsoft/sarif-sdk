git checkout main
git pull
git branch -D test
git push origin HEAD:main
git checkout -b test
git branch --set-upstream-to=origin/test test
git pull
copy /Y e:\repros\WebGoat.sarif
git commit -am "Update test files."
git push --set-upstream origin test
