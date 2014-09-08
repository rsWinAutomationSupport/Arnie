Arnie
=====

WCF Web Service to receive github webhook notifications and do "it" now (rather than on the next run of the scheduled task)


###Installation###

Clone this repo to the location of your choice, for these instructions, assume that we cloned to D:\Website\Arnie. Then, set up a  Application using one of the following methods

via GUI:
* Open IIS Manager
* Select Application Pools
* Click Add Application Pool in the Actions Pane
* Type a name for the AppPool (e.g. API) and click OK
* Optional: change AppPool Identity by going to Advanced Settings => Process Model => Identity
* Right-Click "Default Web Site" and Click on "Add Application"
* Type an Alias (e.g. API), change the Application Pool to the one created earlier (e.g. API) and specify the Physical Path (e.g. D:\Website\Arnie\Arnie) and click OK

via Powershell:
```PoSh
$pool = New-WebAppPool -Name API
$pool.processModel.identityType = 0
$pool | Set-Item
New-WebApplication -Site "Default Web Site" -Name "API" -PhysicalPath D:\Website\Arnie\Arnie -ApplicationPool "API"
```

via DSC:
```
Configuration InstallAPI {
    Import-DscResource -ModuleName msWebAdministration
    Node localhost {
        xWebAppPool API {
            Ensure = "Present"
            Name = "API"
        }

        Script changeIdentity {
            DependsOn = "[xWebAppPool]API"
            GetScript = {return @{"IdentityType"=(Get-Item IIS:\AppPools\API).processModel.identityType}}
            SetScript = {Set-ItemProperty -Path IIS:\AppPools\API -Name processModel.identityType -Value 0}
            TestScript = { if ( (Get-Item IIS:\AppPools\API).processModel.identityType -eq 0 ) {return $true} else {return $false}}
        }

        xWebApplication API {
            DependsOn = "[Script]changeIdentity"
            Ensure = "Present"
            Name = "API"
            Website = "Default Web Site"
            WebAppPool = "API"
            PhysicalPath = "D:\Website\Arnie\Arnie"
        }
    }
}

InstallAPI
Start-DscConfiguration .\InstallAPI -Wait -Verbose
```


Either of the three methods above will leave you with an Application called API under the Default Web Site. The only endpoint of this web service will be API/Arnie.svc/DoItNow.

###Configuration###

The JSON data posted by github will contain a sub-document called repository looking similar to the below:

```
  "repository": {
    "id": 22758137,
    "name": "WebHook_Test",
    "full_name": "nick-o/WebHook_Test",
    "owner": {
      "name": "nick-o",
      "email": "<redacted>"
    },
    "private": false,
    "html_url": "https://github.com/nick-o/WebHook_Test",
    "description": "",
    "fork": false,
    "url": "https://github.com/nick-o/WebHook_Test",
    "forks_url": "https://api.github.com/repos/nick-o/WebHook_Test/forks"
    ...
  }
```

Each of the above keys can be used to define actions/commands. The Configuration is inside a JSON file called Configuration.json located in Arnie/App_Data. This file needs to look like the following:
```
{
    "repos": [
        {
            "repo_key": "full_name",
            "repo_value": "nick-o/WebHook_Test",
            "commands": [
                {
                    "command": "Get-Process | Out-File D:\\process.txt",
                    "order": 1
                },
                {
                    "command": "Get-Service | Out-File D:\\service.txt",
                    "order": 2
                }
            ]
        }
    ]
}
```





###Setting up a github webhook###

Create a github webhook under the repository settings to point at http://<public IP>/API/Arnie.svc/DoItNow
