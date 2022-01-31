#!/bin/bash                    

# Download the latest Raspberry pi lite image
# Instructions from https://geoffhudik.com/tech/2020/04/27/scripting-raspberry-pi-image-builds/
loop_1_device=/dev/loop2p1
loop_2_device=/dev/loop2p2
image_path=./sdBuild/download
mount_path=./sdBuild/mount
netcore_gzip=$image_path/netcore.tar.gz
image_zip=$image_path/osImage.zip
image_iso=$image_path/osImage.img
image_output=$image_path/image.tar.gz
tally_app_zip=./SpyderTallyApp.zip

# Ensure the app is built before continuing
if [ ! -f "$tally_app_zip" ]; then
    echo "SpyderTallyApp.zip not found. Please build the app before continuing."
    exit -1
fi

# Download the latest .Net Core image
if [ ! -f $image_zip ]; then
  mkdir -p $image_path
  echo "Downloading .Net core framework"
  # curl often gave "error 18 - transfer closed with outstanding read data remaining"
  #wget -O $netcore_gzip "https://download.visualstudio.microsoft.com/download/pr/72888385-910d-4ef3-bae2-c08c28e42af0/59be90572fdcc10766f1baf5ac39529a/dotnet-sdk-6.0.101-linux-arm.tar.gz"
  wget -O $netcore_gzip "https://download.visualstudio.microsoft.com/download/pr/ff3b2714-0dee-4cf9-94ee-cb9f5ded285f/d6bfe8668428f9eb28acdf6b6f5a81bc/aspnetcore-runtime-6.0.1-linux-arm.tar.gz"
 
  if [ $? -ne 0 ]; then
    echo "Download failed" ; exit -1;
  fi
fi
 
# Download Raspberry Pi image
if [ ! -f $image_zip ]; then
  mkdir -p $image_path
  echo "Downloading latest Raspbian lite image"
  # curl often gave "error 18 - transfer closed with outstanding read data remaining"
  wget -O $image_zip "https://downloads.raspberrypi.org/raspios_lite_armhf/images/raspios_lite_armhf-2022-01-28/2022-01-28-raspios-bullseye-armhf-lite.zip"
 
  if [ $? -ne 0 ]; then
    echo "Download failed" ; exit -1;
  fi
fi
 
echo "Extracting ${image_zip} ISO"
unzip -p $image_zip > $image_iso
 
if [ $? -ne 0 ]; then
    echo "Unzipping image ${image_zip} failed" ; exit -1;
fi

# Now we're going to have to extend the image size to make room for the app
# 400Mb added below
echo "Extending image size..."
parted $image_iso print
dd if=/dev/zero bs=1M count=1000 >> $image_iso
parted $image_iso resizepart 2 100%
                             
# Find partitions on this image and many them available to the 
# build server.                                                 
losetup --find -P $image_iso

# Resize partition (take two)
echo "Resizing root partition to add space"
e2fsck -f $loop_2_device
resize2fs $loop_2_device

echo "Completed extending image size"
parted $image_iso print

# Mount the boot partition
echo "Mounting boot partition"
mkdir -p $mount_path
mount $loop_1_device $mount_path

# turn on I2C for front panel display control
echo "Enabling I2C in config.txt"
cp $mount_path/config.txt $image_path/config.txt
echo "dtparam=i2c_arm=on" >> $image_path/config.txt
echo "dtparam=i2c1=on" >> $image_path/config.txt
cp $image_path/config.txt $mount_path/config.txt

# Add default config files (experimental)
cp -r ../../docs/Linux/appConfig.json $image_path/appConfig.json
cp -r ../../docs/Linux/deviceConfig.json $image_path/deviceConfig.json

echo "Unmounting boot partition"
umount $mount_path
                       
# Mount the root partition, and copy any files from filesToAdd                                                        
# to the partition. 
echo "Mounting root partition"
mount $loop_2_device $mount_path

# Install .Net Core
echo "Installing .Net Core to ~/dotnet"
local_dotnet_path=/home/pi/dotnet
dotnet_path=$mount_path$local_dotnet_path
mkdir -p $dotnet_path
tar -zxf $netcore_gzip -C $dotnet_path
echo "export PATH=$PATH:\${local_dotnet_path}" >> $mount_path/home/pi/.bashrc
echo "export DOTNET_ROOT=${local_dotnet_path}" >> $mount_path/home/pi/.bashrc

# Install the app
local_app_path=/home/pi/SpyderTallyApp
app_path=$mount_path$local_app_path
echo "Installing Spyder Tally app to ${local_app_path}"
mkdir -p $app_path
unzip -q $tally_app_zip -d $app_path

# Add default config files (will be used if not found in /boot)
cp -r ../../docs/Linux/appConfig.json $app_path/appConfig.json
cp -r ../../docs/Linux/deviceConfig.json $app_path/deviceConfig.json
chmod ugo+rw $app_path/*.json

# Sending app parameter of /boot to the app, which will be where we try to load the config from
echo "Creating Crontab entry to run app on boot"
(crontab -l; echo "@reboot ${local_dotnet_path}/dotnet ${local_app_path}/SpyderTallyControllerWebApp.dll /boot &") | sort -u | crontab -

echo "Unounting root partition"
umount $mount_path

# Don't need the loopback device anymore, disconnect it.                                                                                
losetup -D

# Zip the output image
echo "Compressing image to ${image_output}"
tar -czf $image_output $image_iso
                 
echo "Finished building the image!"