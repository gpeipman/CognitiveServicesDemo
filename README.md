# CognitiveServicesDemo

This solution contains ASP.NET MVC web application that demonstrates Microsoft Cognitive Services. This is on-going work and in case of any bugs or feature requests please report the new issue.

## Getting started

1. Log in to Azure Portal
2. Create new Cognitive Services Face API and Computer Vision API accounts
3. Clone or download solution
4. At web application root folder create file keys.config
5. Add your API keys to this file
6. Set API end-points in web.config file
7. Run web application

## Example keys.config

```xml
<?xml version="1.0"?>
<appSettings>
	<add key="CognitiveServicesFaceApiKey" value="adadadsadasdasdasdasd" />
	<add key="CognitiveServicesVisionApiKey" value="adfsdfsdasqefsdsadas" />
</appSettings>
```
