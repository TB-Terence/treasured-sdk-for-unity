# Treasured SDK for Unity
[![LinkedIn](https://img.shields.io/badge/LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white)](https://www.linkedin.com/company/treasured/)
[![Instagram](https://img.shields.io/badge/Instagram-E4405F?style=for-the-badge&logo=instagram&logoColor=white)](https://www.instagram.com/treasuredteam/)
[![Youtube](https://img.shields.io/badge/YouTube-FF0000?style=for-the-badge&logo=youtube&logoColor=white)](https://www.youtube.com/channel/UCe7PPx_Gn7rq3Wfl1MO9NEQ)

## Overview

This package provides tools for creating and exporting files in [Unity](https://unity.com/) used by [Treasured Web Viewer](https://treasured.ca/). The package is currently under development. Issues and feature requests can be submit at [here](https://github.com/TB-Terence/treasured-sdk-for-unity/issues).

## Minimum Unity Version
- 2020.3+, older version may work but not guaranteed

## Before you start

This package relies on [UnityMeshSimplifier](https://openupm.com/packages/com.whinarn.unitymeshsimplifier/). You must set up your scoped registry for your project `BEFORE` installing the package.

Open your Unity Editor and go to `Edit > Project Settings... > Package Manager > Scoped Registries`. Create a new scope registry and enter the following:

**Name**
```
package.openupm.com
```
**URL**
```
https://package.openupm.com
```
**Scope(s)**
```
com.whinarn.unitymeshsimplifier
```

If you already using [OpenUPM](https://openupm.com/) simply add `com.whinarn.unitymeshsimplifier` to the scope(s) and click `Save`.

Next, go to `Edit > Project Settings... > Player > Api Compatibility Level` and set it to `.NET 4.x`.

## Installing the Package
Currently the package can only be installed using git. Eventually the package will be move to scoped registry (e.g., [OpenUPM](https://openupm.com/)) or asset store.

There are two versions of the package:
1. Stable - This is the most stable version.
```
https://github.com/TB-Terence/treasured-sdk-for-unity.git#upm
```
2. Experimental - This is an experimental version with all the new features and changes. This version doesn't guarantee to generate valid files.
```
https://github.com/TB-Terence/treasured-sdk-for-unity.git#exp
```

### Install via GIT URL
The guide on how to add a package from a git URL to your project can be found [here](https://docs.unity3d.com/Manual/upm-ui-giturl.html). Use the GIT URL above for different version.

### Install via local folder
You must [clone](https://docs.github.com/en/repositories/creating-and-managing-repositories/cloning-a-repository) the project first and then follow the [guide](https://docs.unity3d.com/Manual/upm-ui-local.html) on how to add a package from local folder. Use the GIT URL above for different version.

## Troubleshooting

If you are getting the following errors, see [Before you start](#before-you-start) section.
```
Packages\com.treasured.unitysdk has invalid dependencies or related test packages
```
```
error CS7069: Reference to type 'Image' claims it is defined in 'System.Drawing', but it could not be found
```

If you are getting the this error, update your `Version Control` package to `1.15.12` or above.
```
unityplastic references strong named Newtonsoft.Json Assembly references: 12.0.0.0 Found in project: 13.0.0.0.
```

## Getting Started
- [Creating an Exhibit](Documentation~/Creating-an-Exhibit.md)
- [Exporting an Exhibit](Documentation~/Exporting-an-Exhibit.md)