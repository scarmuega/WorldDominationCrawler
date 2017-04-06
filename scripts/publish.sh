cd ..
dotnet publish -r ubuntu.14.10-x64 -c Release
dotnet publish -r win10-x64 -c Release
dotnet publish -r osx.10.12-x64 -c Release
rm ./docs/ubuntu.14.10-x64.zip
rm ./docs/win10-x64.zip
rm ./docs/osx.10.12-x64.zip
zip -r -j ./docs/ubuntu.14.10-x64.zip ./bin/Release/netcoreapp1.1/ubuntu.14.10-x64/publish
zip -r -j ./docs/win10-x64.zip ./bin/Release/netcoreapp1.1/win10-x64/publish
zip -r -j ./docs/osx.10.12-x64.zip ./bin/Release/netcoreapp1.1/osx.10.12-x64/publish