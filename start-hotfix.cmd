set AppMajorVersion=2
set AppMinorVersion=5
set AppBuild=0
set "AppRevision=%~1"

git checkout -b hotfix-%AppMajorVersion%.%AppMinorVersion%.%AppBuild%.%AppRevision% master
