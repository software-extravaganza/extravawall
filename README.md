# extravawall

To manually build: 
```
cd ExtravaWall
dotnet publish --os linux --arch x64 /t:PublishContainer -c Release
podman compose up -d --force-recreate
```