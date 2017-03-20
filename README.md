### CompatTelOverride v2 - more persistant than ever!

## Intro
CompatTelRunner telemetry runs very frequently and drains **a lot** of system resources. Even if the file is deleted, Windows will re-install it in a while.

## Description
This project has three modules: an installer, an override (dummy exe), and a service.

The installer (*'CompatTelOverride.exe'*) will manage installation and uninstallation of this project.
Once installed, the service (*'CompatTelWatch.exe'*) will replace the original *'CompatTelRunner.exe'* with a dummy one that, if started, will sleep forever, prevening telemetry.
This will happen every boot, so even if Windows replaces the file, it will get reverted. The service also locks the telemetry runner, so it doesn't get modified while Windows is running.

## Features
* Automatic ownership/permission handling
* All modified files retain high security permissions
* Automatic backup
* Uninstallation support

## How to install
1. Run *'CompatTelOverride.exe'* as Administrator
2. Click 'Yes' when prompted to install.

## How to uninstall
1. If already installed, re-run *'CompatTelOverride.exe'* as Administrator
2. Click 'Yes' when prompted to uninstall.
3. Click 'No' when prompted to re-install.

## How to force uninstall
1. Run *'CompatTelOverride.exe'*, as Administrator, with argument: *'/uninstall'*

---

## Comparison

#### Before:
![](https://i.imgur.com/6Saqe6T.png)

#### After:
![](https://i.imgur.com/9o0Kp7x.png)

---

## Note to developers
The two dependancy DLLs are intended to be merged into the main executables before deployment. If you wish to use them as-is, some code tweaks will be required.