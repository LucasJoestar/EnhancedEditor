# Enhanced Editor - Enhanced Engine, Part I

This repository is part of the "Enhanced Engine", an advanced collection of systems for the [Unity](https://unity.com/) game engine.


The Enhanced Editor features a large toolset aiming to improve the use of the editor itself. <br/>
This is the base component of the Enhanced Engine, and as so is required by all other parts for use.

## Unity Versions

Minimum tested Unity version: 2019.4.x <br/>
Minimum required Unity version: 2020.3.x

## Installation

You can install this library by two means: either by .zip extraction, or from the Unity built-in *Package Manager*.

---

### Installation via .zip extraction into your Assets/ folder - Editable Project

1. Download the EnhancedEditor.zip file from the GitHub repository.
2. Unzip the contents into your [Your Project Directory]/Assets/... folder.

This method allows you to modify this library contents depending on your need/wheterver-bug-you-may-find.

---

### Installation via Git URL from the Package Manager - Readonly Project [Requires Git]

If Git is not yet installed on your machine, you can download it for Windows from here: https://git-scm.com/download/win/


1. Open the *Package Manager* in your Unity project from *Window > Package Manager*.
2. Click on the icon *+* to add packages, and select *Add package from git URL...*

![Add a package from Git in Unity](./Documentation~/Images/package-manager-add.png)

3. In the text field, enter the URL of this Git repository, and click on *Add*

```txt
https://github.com/LucasJoestar/EnhancedEditor.git
```

![Enter the Git URL](./Documentation~/Images/package-manager-url.png)

Unity will download the module and add it as a dependency.

### Updates

You check and download updated of the package by doing the same thing as the package installation (see the above section). After you click on *Add*, Unity will check for package changes, and install the new version if needed.
