# Building the Linux application

This document is mostly quick notes to help me remember how to build a disk image for the Raspbery pi.  I have high hopes to get this image build process into a script that will run as a Github action when a new release branch is created, but I'm not there yet.

## Stuff to install on a new Raspian Image
- dotnet ([Scripted Install](https://learn.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual#scripted-install))
- nginx (apt-get package install)
- git (sort of optional, depending on whether or not you clone the repo)
- From Raspi-Config
    - Enable I2C
    - Enable SSH

## Files to copy over from this repo
- nginx.conf (copy to /etc/nginx/nginx.conf)
- SpyderTallies.service (copy to /etc/systemd/system/SpyderTallies.service)

## Installing the app
- Clone the git repo:  `git clone https://github.com/dsmithson/SpyderTallyController.git`
- cd to the '~/git/SpyderTallyController/src/Linux/SpyderTallyControllerLinux' folder (assuming you cloned into the ~/git folder)
- run `mkdir ~/app` to create a place to publish
- run `dotnet publish -p:PublishDir=/home/dsmithson/app -c Release ./SpyderTallyControllerLinux.sln` to build in release and publish to the ~/app directory

## Installing the app as a service
- Edit SpyderTallies.service as needed (in repo username is dsmithson at the time of this writing)
- `sudo cp SpyderTallies.service /etc/systemd/system/SpyderTallies.service`
- `sudo systemctl daemon-reload`
- (Make sure the app is running with `sudo systemctl status SpyderTallies`)
- `sudo systemctl enable SpyderTallies`

As needed, run `sudo systemctl [stop|start|restart] SpyderTallies` to manage service.

## Cleaning up before/during building an SD card image after configuring
- Delete SSH keys (if added to edit/push code)
- Make sure your user password is 'spyder'
- Make sure the app starts automatically on reboot before sealing an image
- Delete, or at least clean, the repo cloned (if done) to save space
- DD clone the disk image
- Run [PiShrink](https://github.com/Drewsif/PiShrink) to shrink down the image size
