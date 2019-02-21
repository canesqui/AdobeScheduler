# AdobeScheduler
## 6/18/2018 - Update AdobeScheduler
## .Net calendar for Adobe connect.

Credentials - SAU

- IntraNet
- carrera.southern.edu:8080
- Adobe connect
- turner.southern.edu


### SDK

- Adobe Connect Web Services API reference - https://helpx.adobe.com/adobe-connect/webservices/topics/action-reference.html
- Adobe Connect SDK - scidec - DimitriStroganov
https://github.com/DmitryStroganov/AdobeConnectSDK

- JQuery version 2.0.3
- JQuery SignalR-1.0.0
- SignalR.net v2.0
- Fullcalendar.js - https://fullcalendar.io/

AdobeConnect Sdk customized on the following functions:
Funtions found in AdobeConnectXmlAPI

- public bool IsAdmin (String acl_id)
- public List<List<string>> GetSharedList()

Logon, User management
- public bool Login(string userName, string userPassword, out StatusInfo iStatus)

and others..

Updates Needed
- AdobeConnectSdk merge
- Update bootstrap from 2.3.2 to latest.
