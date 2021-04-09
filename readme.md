# geokassa

## dotnet core links  

| Site | Url |
| ----------- | ----------- |
| [.NET CLI overview] | https://docs.microsoft.com/en-us/dotnet/core/tools/ |
| [Deploying .Net apps with .NET CLI] | https://docs.microsoft.com/en-us/dotnet/core/deploying/deploy-with-cli |
| [Publish profiles with .NET CLI]  | https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/visual-studio-publish-profiles?view=aspnetcore-5.0 |
 

## Installing dotnet SDK

### Installing dotnet SDK Windows

>Instructions  
>>https://docs.microsoft.com/en-us/dotnet/core/install/windows?tabs=netcore21

>Scripts  
>>https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.ps1

### Installing dotnet SDK Linux

>Instructions  
>>https://docs.microsoft.com/en-us/dotnet/core/install/linux

>Scripts  
>>https://dotnet.microsoft.com/download/dotnet/script

#### Installing dotnet SDK Centos 8
`sudo yum install dotnet-sdk-2.1`

#### Installing dotnet SDK Conda

## Nice dotnet commands  

#### dotnet restore
`dotnet clean`

#### dotnet clean
`dotnet clean`  
`dotnet clean -c Release`

#### dotnet build
`dotnet build`  
`dotnet build -c Release`

#### dotnet publish (deploy)

`dotnet publish -c Release`  
`dotnet publish -c Release ./geokassa/geokassa.csproj /p:PublishProfile=FolderProfile`

## Building geokassa

```
git clone https://github.com/himsve/geokassa.git geokassa
cd geokassa
dotnet restore
dotnet build -c Release
dotnet publish -c Release ./geokassa/geokassa.csproj /p:PublishProfile=FolderProfile
cd .\geokassa\bin\Release\netcoreapp2.1\publish
```

### Running geokassa

`dotnet geokassa.dll`  

### geokassa commands

```
Usage:
  geokassa [options] [command]

Options:
  --version         Show version information
  -?, -h, --help    Show help and usage information

Commands:
  jsontin <input> <output>                          Makes triangulated TIN from point clouds
  lsc2geotiff <inputsource> <inputtarget>           Converts GeoTiff translations based on Helmert
  <output>                                          +Least Squares Collocation
  bin2geotiff <input> <output>                      Converts bin file to GeoTiff
  gri2geotiff <output>                              Converts gri file(s) to GeoTiff
  gtx2geotiff <input> <output>                      Converts gtx file to GeoTiff
  ct2gtx2geotiff <output>                           Converts gtx or ct2 files to GeoTiff
```

## NuGet

### Help

### Contribute
 
