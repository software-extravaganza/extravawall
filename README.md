# extravawall

To manually build: 
```
cd ExtravaWall
dotnet tool restore
dotnet publish --os linux --arch x64 /t:PublishContainer -c Release
podman-compose up -d --force-recreate

```

Troubleshoot if needed:
```
export DOCKER_HOST='unix:///home/<user>/.local/share/containers/podman/machine/qemu/podman.sock'
```

Database Scaffolding
```
dotnet ef dbcontext scaffold "Server=localhost;User=root;Password=;Database=extravawall" "Pomelo.EntityFrameworkCore.MySql"
```

Database Migrations
```
dotnet ef migrations add InitialCreate
dotnet ef database update
```