## Intro
CompatTelRunner telemetry runs very frequently and drains **a lot** of system resources. Even if the file is deleted, Windows will re-install it in a while.

## Description
This executable has two functions - an installer, when run outside of *'System32'*, and a 'dummy' that sleeps forever once it's there.

When installing, this file will replace *'CompatTelRunner.exe'* with itself in order to prevent telemetry. The new file will have the same permissions as other system files except TrustedInstaller won't be allowed to change it, in hopes that Windows won't automatically install the original file again. The new file is also added to start-up to make it even harder for Windows, by having it replace a running executable.

## Features
* Automatic ownership/permission handling
* Proper replace with security settings restored
* Deny TrustedInstaller write access
* Automatic start-up
* Automatic backup
* Proper error handling and helpful messages


## Comparison

#### Before:
![](https://i.imgur.com/6Saqe6T.png)

#### After:
![](https://i.imgur.com/9o0Kp7x.png)
