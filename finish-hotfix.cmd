set AppMajorVersion=2
set AppMinorVersion=5
set AppBuild=0
set "AppRevision=%~1"
set /a AppOldRevision=%AppRevision%-2

(echo Version %AppMajorVersion%.%AppMinorVersion%.%AppBuild%.%AppRevision%) > Moras.Net\release-notes.txt
call git-release-notes -f make-release.json master..HEAD textfile.ejs >> Moras.Net\release-notes.txt
copy /B Moras.Net\release-notes.txt + Moras.Net\history_en.txt Moras.Net\history_en_new.txt
move Moras.Net\history_en_new.txt Moras.Net\history_en.txt
del Moras.Net\release-notes.txt
msbuild.exe Moras.Net.sln /m /target:publish /property:VisualStudioVersion=14.0 /property:ApplicationRevision=%AppRevision% /p:platform="any cpu" /p:configuration="release"

git add .
git rm -r -- "Moras.Net/publish/Application Files/Moras.Net_%AppMajorVersion%_%AppMinorVersion%_%AppBuild%_%AppOldRevision%"
git commit -m "Publish Release %AppMajorVersion%.%AppMinorVersion%.%AppBuild%.%AppRevision%"
git checkout master
git merge --no-ff hotfix-%AppMajorVersion%.%AppMinorVersion%.%AppBuild%.%AppRevision%
git tag -a -m "Tag for %AppMajorVersion%.%AppMinorVersion%.%AppBuild%.%AppRevision% release" release/%AppMajorVersion%.%AppMinorVersion%/%AppMajorVersion%.%AppMinorVersion%.%AppBuild%.%AppRevision%
git checkout devel
git merge --no-ff hotfix-%AppMajorVersion%.%AppMinorVersion%.%AppBuild%.%AppRevision%
git branch -d hotfix-%AppMajorVersion%.%AppMinorVersion%.%AppBuild%.%AppRevision%